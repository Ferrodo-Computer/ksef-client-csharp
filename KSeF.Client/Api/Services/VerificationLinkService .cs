using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.QRCode;
using KSeF.Client.DI;
using KSeF.Client.Extensions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KSeF.Client.Api.Services
{
    /// <inheritdoc/>
    public class VerificationLinkService(KSeFClientOptions options) : IVerificationLinkService
    {
        private readonly string BaseUrl = $"{options.BaseUrl}/client-app";

        /// <inheritdoc/>
        public string BuildInvoiceVerificationUrl(string nip, DateTime issueDate, string invoiceHash)
        {
            string date = issueDate.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            byte[] bytes = Convert.FromBase64String(invoiceHash);
            string urlEncoded = bytes.EncodeBase64UrlToString();
            return $"{BaseUrl}/invoice/{nip}/{date}/{urlEncoded}";
        }

        /// <inheritdoc/>
        public string BuildCertificateVerificationUrl(
            string sellerNip,
            QRCodeContextIdentifierType contextIdentifierType,
            string contextIdentifierValue,
            string certificateSerial,
            string invoiceHash,
            X509Certificate2 signingCertificate,
            string privateKey = ""
        )
        {
            byte[] bytes = Convert.FromBase64String(invoiceHash);
            string invoiceHashUrlEncoded = bytes.EncodeBase64UrlToString();

            string pathToSign = $"{BaseUrl}/certificate/{contextIdentifierType}/{contextIdentifierValue}/{sellerNip}/{certificateSerial}/{invoiceHashUrlEncoded}".Replace("https://", "");
            string signedHash = ComputeUrlEncodedSignedHash(pathToSign, signingCertificate, privateKey);

            return $"{BaseUrl}/certificate/{contextIdentifierType}/{contextIdentifierValue}/{sellerNip}/{certificateSerial}/{invoiceHashUrlEncoded}/{signedHash}";
        }

        /// <inheritdoc/>
        public string BuildCertificateVerificationUrl(
            string sellerNip,
            QRCodeContextIdentifierType contextIdentifierType,
            string contextIdentifierValue,
            string invoiceHash,
            X509Certificate2 signingCertificate,
            string privateKey = ""
        )
        {
            return BuildCertificateVerificationUrl(sellerNip, contextIdentifierType, contextIdentifierValue, signingCertificate.SerialNumber, invoiceHash, signingCertificate, privateKey);
        }


        private static string ComputeUrlEncodedSignedHash(string pathToSign, X509Certificate2 cert, string privateKey = "", DSASignatureFormat dSASignatureFormat = DSASignatureFormat.IeeeP1363FixedFieldConcatenation)
        {
            // 1. SHA-256
            byte[] sha;
            sha = SHA256.HashData(Encoding.UTF8.GetBytes(pathToSign));

            if (!string.IsNullOrEmpty(privateKey))
            {
                if (privateKey.StartsWith("-----", StringComparison.Ordinal))
                {
                    privateKey = string.Concat(
                        privateKey
                            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                            .Where(l => !l.StartsWith("-----", StringComparison.Ordinal))
                    );
                }

                byte[] privateKeyBytes = Convert.FromBase64String(privateKey);

                // 1.1 Importujemy tylko, gdy certyfikat nie ma klucza prywatnego
                if (!cert.HasPrivateKey)
                {
                    if (cert.GetRSAPublicKey() != null)
                    {
                        using RSA rsaTemp = RSA.Create();
                        rsaTemp.ImportRSAPrivateKey(privateKeyBytes, out _);
                        cert = cert.CopyWithPrivateKey(rsaTemp);
                    }
                    else if (cert.GetECDsaPublicKey() != null)
                    {
                        using ECDsa ecdsaTemp = ECDsa.Create();
                        ecdsaTemp.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                        cert = cert.CopyWithPrivateKey(ecdsaTemp);
                    }
                    else
                    {
                        throw new InvalidOperationException("Certyfikat nie wspiera RSA ani ECDSA.");
                    }
                }
            }
            // 2. Sign hash
            byte[] signature;
            if (cert.GetRSAPrivateKey() is RSA rsa)
            {
                signature = rsa.SignHash(sha, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
            else if (cert.GetECDsaPrivateKey() is ECDsa ecdsa)
            {
                signature = ecdsa.SignHash(sha, dSASignatureFormat);
            }
            else
            {
                throw new InvalidOperationException("Certyfikat nie wspiera RSA ani ECDsa.");
            }

            // 3. Base64 + URL-encode            
            return signature.EncodeBase64UrlToString();
        }
    }
}
