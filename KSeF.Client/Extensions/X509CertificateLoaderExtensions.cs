using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace KSeF.Client.Extensions
{
    public static class X509CertificateLoaderExtensions
    {
        private static X509KeyStorageFlags GetFlags()
        {
            // macOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return X509KeyStorageFlags.Exportable;
            }

            // Windows / Linux 
            return X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet;
        }

        public static X509Certificate2 LoadCertificate(this byte[] certBytes)
        {
            ArgumentNullException.ThrowIfNull(certBytes);

#if NET9_0_OR_GREATER
            return X509CertificateLoader.LoadCertificate(certBytes);
#else
            return new X509Certificate2(
                certBytes,
                "",
                GetFlags());
#endif
        }

        public static X509Certificate2 LoadPkcs12(this byte[] certBytes)
        {
            ArgumentNullException.ThrowIfNull(certBytes);

#if NET9_0_OR_GREATER
            return X509CertificateLoader.LoadPkcs12(
                certBytes,
                password: string.Empty,
                GetFlags());
#else
            return new X509Certificate2(
                certBytes,
                string.Empty,
                GetFlags());
#endif
        }

        public static X509Certificate2 LoadCertificateFromFile(string certificatePath)
        {
            ArgumentNullException.ThrowIfNull(certificatePath);

#if NET9_0_OR_GREATER
            return X509CertificateLoader.LoadCertificateFromFile(certificatePath);
#else
            byte[] certBytes = File.ReadAllBytes(certificatePath);

            return new X509Certificate2(
                certBytes,
                "",
                GetFlags());
#endif
        }

        /// <summary>
        /// Łączy publiczny certyfikat X.509 z kluczem prywatnym zakodowanym w formacie PEM w celu utworzenia certyfikatu zawierającego
        /// klucz prywatny. Obsługuje klucze typu RSA i ECDSA.
        /// </summary>
        /// <remarks>Zwracany certyfikat jest kopią oryginału z dołączonym kluczem prywatnym.
        /// Obsługiwane są tylko algorytmy RSA i ECDSA. Metoda nie modyfikuje oryginalnej instancji certyfikatu. </remarks>
        /// <param name="publicCert">Publiczna instancja X509Certificate2, do której zostanie dołączony klucz prywatny. Nie może zawierać już
        /// klucza prywatnego.</param>
        /// <param name="privateKeyPem">Ciąg znaków zawierający klucz prywatny zakodowany w formacie PEM. Może być zaszyfrowany lub niezaszyfrowany. Nie może być null, pusty ani
        /// zawierać spacji.</param>
        /// <param name="password">Hasło używane do odszyfrowania klucza prywatnego, jeśli format PEM jest zaszyfrowany. Jeśli klucz nie jest zaszyfrowany, ten
        /// parametr jest ignorowany.</param>
        /// <returns>Nowa instancja X509Certificate2 zawierająca zarówno certyfikat publiczny, jak i powiązany klucz prywatny. </returns>
        /// <exception cref="ArgumentNullException">Rzucane, jeśli <paramref name="publicCert"/> jest null lub jeśli <paramref name="privateKeyPem"/> jest null, puste lub
        /// składa się wyłącznie ze spacji. </exception>
        /// <exception cref="InvalidOperationException">Rzucane, jeśli <paramref name="publicCert"/> zawiera już klucz prywatny. </exception>
        /// <exception cref="NotSupportedException">Rzucane, jeśli algorytm klucza publicznego certyfikatu nie jest RSA lub ECDSA lub jeśli brakuje identyfikatora OID klucza publicznego. </exception>
        /// <exception cref="ArgumentException">Rzucane, jeśli klucz prywatny PEM jest zaszyfrowany, a <paramref name="password"/> jest pusty lub ma wartość null. </exception>
        public static X509Certificate2 MergeWithPemKey(
            this X509Certificate2 publicCert,
            string privateKeyPem,
            string password = null)
        {
            ArgumentNullException.ThrowIfNull(publicCert);
            if (string.IsNullOrWhiteSpace(privateKeyPem))
            {
                throw new ArgumentNullException(nameof(privateKeyPem));
            }

            if (publicCert.HasPrivateKey)
            {
                throw new InvalidOperationException("Certyfikat zawiera już klucz prywatny.");
            }

            const string RsaOid = "1.2.840.113549.1.1.1";
            const string EcOid = "1.2.840.10045.2.1";

            string oid = publicCert.PublicKey.Oid?.Value
                ?? throw new NotSupportedException("Certyfikat nie zawiera klucza publicznego OID.");

            bool isEncrypted = privateKeyPem.Contains(
                "ENCRYPTED PRIVATE KEY",
                StringComparison.OrdinalIgnoreCase);

            if (isEncrypted && string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(
                    "Zaszyfrowany klucz prywatny wymaga podania hasła.",
                    nameof(password));
            }

            if (oid == RsaOid)
            {
                using RSA rsa = RSA.Create();
                if (isEncrypted)
                {
                    rsa.ImportFromEncryptedPem(privateKeyPem, password);
                }
                else
                {
                    rsa.ImportFromPem(privateKeyPem);
                }

                return publicCert.CopyWithPrivateKey(rsa);
            }
            else if (oid == EcOid)
            {
                using ECDsa ecdsa = ECDsa.Create();
                if (isEncrypted)
                {
                    ecdsa.ImportFromEncryptedPem(privateKeyPem, password);
                }
                else
                {
                    ecdsa.ImportFromPem(privateKeyPem);
                }

                return publicCert.CopyWithPrivateKey(ecdsa);
            }

            throw new NotSupportedException(
                $"Algorytym o OID '{oid}' nie jest wspierany.");
        }
    }
}