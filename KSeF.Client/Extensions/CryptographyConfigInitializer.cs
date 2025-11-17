using System.Security.Cryptography;

namespace KSeF.Client.Extensions
{
    /// <summary>
    /// Inicjalizuje konfigurację kryptograficzną algorytmu ECdsa.
    /// </summary>
    public static class CryptographyConfigInitializer
    {
        private static readonly Lazy<bool> _initialized = new Lazy<bool>(InitializeInternal, isThreadSafe: true);

        /// <summary>
        /// Zapewnia, że algorytm kryptograficzny dla ECDSA z SHA-256 jest zainicjalizowany i dostępny do użycia.
        /// Rzuca wyjątek InvalidOperationException, jeśli inicjalizacja się nie powiedzie.
        /// </summary>
        public static void EnsureInitialized()
        {
            try
            {
                if (!_initialized.Value)
                {
                    // Ten scenariusz teoretycznie nie powinien wystąpić, jeśli InitializeInternal rzuca wyjątki zamiast zwracać false.
                    // Dodane jako zabezpieczenie.
                    throw new InvalidOperationException("Inicjalizacja CryptoConfig zakończona niepowodzeniem.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Nie udało się zainicjalizować niestandardowych algorytmów kryptograficznych.", ex);
            }
        }

        private static bool InitializeInternal()
        {
            const string algorithmName = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256";

            if (CryptoConfig.CreateFromName(algorithmName) != null)
            {
                return true;
            }

            CryptoConfig.AddAlgorithm(typeof(Ecdsa256SignatureDescription), algorithmName);
            return true;
        }
    }
}
