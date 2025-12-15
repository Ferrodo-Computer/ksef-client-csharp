namespace KSeF.Client.Validation
{
    /// <summary>
    /// Zawiera stałe długości i zakresy długości dla różnych wartości tekstowych używanych w aplikacji.
    /// </summary>
    public static class ValidValues
    {
        /// <summary>
        /// Wymagana długość wartości „challenge” używanej np. w procesie uwierzytelniania.
        /// </summary>
        public const int RequiredChallengeLength = 36;

        /// <summary>
        /// Minimalna dozwolona długość nazwy certyfikatu.
        /// </summary>
        public const int CertificateNameMinLength = 5;

        /// <summary>
        /// Maksymalna dozwolona długość nazwy certyfikatu.
        /// </summary>
        public const int CertificateNameMaxLength = 100;

        /// <summary>
        /// Minimalna dozwolona długość nazwy podjednostki (np. wydziału, sekcji).
        /// </summary>
        public const int SubunitNameMinLength = 5;

        /// <summary>
        /// Maksymalna dozwolona długość nazwy podjednostki (np. wydziału, sekcji).
        /// </summary>
        public const int SubunitNameMaxLength = 256;

        /// <summary>
        /// Minimalna dozwolona długość opisu uprawnienia.
        /// </summary>
        public const int PermissionDescriptionMinLength = 5;

        /// <summary>
        /// Maksymalna dozwolona długość opisu uprawnienia.
        /// </summary>
        public const int PermissionDescriptionMaxLength = 256;
    }
}
