namespace KSeF.Client.Core.Models.TestData
{
    /// <summary>Odebranie uprawnień nadanych w testach.</summary>
    public sealed class TestDataPermissionsRevokeRequest
    {
        /// <summary>Kontekst — jeśli dotyczy.</summary>
        public ContextIdentifier ContextIdentifier { get; set; }

        /// <summary>Identyfikator podmiotu upoważniającego.</summary>
        public AuthorizedIdentifier AuthorizedIdentifier { get; set; }
    }
}
