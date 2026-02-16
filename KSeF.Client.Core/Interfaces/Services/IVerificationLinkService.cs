using KSeF.Client.Core.Models.QRCode;
using System;
using System.Security.Cryptography.X509Certificates;


namespace KSeF.Client.Core.Interfaces.Services
{
    public interface IVerificationLinkService
    {
        /// <summary>
        /// Tworzy adres URL służący do weryfikacji faktury przy użyciu podanego numeru identyfikacyjnego podatnika, daty wystawienia i
        /// skrótu faktury.
        /// </summary>
        /// <remarks>Zwrócony adres URL może służyć do weryfikacji autentyczności faktury w systemach zewnętrznych.</remarks>
        /// <param name="nip">Numer identyfikacji podatkowej (NIP) będący kontekstem wystawcy faktury.</param>
        /// <param name="issueDate">Data wystawienia faktury.</param>
        /// <param name="invoiceHash">Unikalna wartość skrótu reprezentująca fakturę. Nie może być null ani pusta.</param>
        /// <returns>Ciąg znaków zawierający adres URL do weryfikacji określonej faktury.</returns>
        string BuildInvoiceVerificationUrl(string nip, DateTime issueDate, string invoiceHash);

        /// <summary>
        /// Tworzy adres URL służący do weryfikacji certyfikatu wystawcy (CERTYFIKAT, KOD II).
        /// </summary>
        /// <param name="sellerNip">Numer identyfikacji podatkowej sprzedawcy (NIP) używany do identyfikacji podmiotu, którego certyfikat jest weryfikowany. Nie może
        /// być pusty ani mieć wartości null. </param>
        /// <param name="contextIdentifierType">Typ identyfikatora kontekstu używanego do weryfikacji.</param>
        /// <param name="contextIdentifierValue">Wartość identyfikatora kontekstu odpowiadająca określonemu typowi. Musi być prawidłowa dla wybranego typu identyfikatora kontekstu.</param>
        /// <param name="certificateSerial">Numer seryjny certyfikatu. Nie może być pusty lub null.</param>
        /// <param name="invoiceHash">Wartość skrótu faktury, która ma zostać zweryfikowana. Służy do zapewnienia integralności i autentyczności danych faktury.</param>
        /// <param name="signingCertificate">Certyfikat X.509 używany do podpisania żądania weryfikacji. Musi być ważnym certyfikatem powiązanym ze sprzedawcą.</param>
        /// <param name="privateKey">Klucz prywatny odpowiadający certyfikatowi podpisu, używany do podpisu kryptograficznego. Jeśli nie zostanie podany, metoda
        /// może użyć klucza prywatnego z certyfikatu, jeśli jest dostępny.</param>
        /// <returns>Ciąg znaków zawierający adres URL do weryfikacji certyfikatu.</returns>
        string BuildCertificateVerificationUrl(
            string sellerNip,
            QRCodeContextIdentifierType contextIdentifierType,
            string contextIdentifierValue,
            string certificateSerial,
            string invoiceHash,
            X509Certificate2 signingCertificate,
            string privateKey = ""
        );

        /// <summary>
        /// Tworzy adres URL służący do weryfikacji certyfikatu wystawcy (CERTYFIKAT, KOD II).
        /// </summary>
        /// <param name="sellerNip">Numer identyfikacji podatkowej sprzedawcy (NIP) używany do identyfikacji podmiotu, którego certyfikat jest weryfikowany. Nie może
        /// być pusty ani mieć wartości null. </param>
        /// <param name="contextIdentifierType">Typ identyfikatora kontekstu używanego do weryfikacji.</param>
        /// <param name="contextIdentifierValue">Wartość identyfikatora kontekstu odpowiadająca określonemu typowi. Musi być prawidłowa dla wybranego typu identyfikatora kontekstu.</param>
        /// <param name="invoiceHash">Wartość skrótu faktury, która ma zostać zweryfikowana. Służy do zapewnienia integralności i autentyczności danych faktury.</param>
        /// <param name="signingCertificate">Certyfikat X.509 używany do podpisania żądania weryfikacji. Musi być ważnym certyfikatem powiązanym ze sprzedawcą.</param>
        /// <param name="privateKey">Klucz prywatny odpowiadający certyfikatowi podpisu, używany do podpisu kryptograficznego. Jeśli nie zostanie podany, metoda
        /// może użyć klucza prywatnego z certyfikatu, jeśli jest dostępny.</param>
        /// <returns>Ciąg znaków zawierający adres URL do weryfikacji certyfikatu.</returns>
        string BuildCertificateVerificationUrl(
            string sellerNip,
            QRCodeContextIdentifierType contextIdentifierType,
            string contextIdentifierValue,
            string invoiceHash,
            X509Certificate2 signingCertificate,
            string privateKey = ""
        );
    }
}
