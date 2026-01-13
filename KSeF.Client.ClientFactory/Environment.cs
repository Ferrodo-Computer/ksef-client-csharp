
namespace KSeF.Client.ClientFactory
{
    /// <summary>
    /// Określa środowisko, z którym będzie komunikowała się aplikacja
    /// podczas korzystania z usług KSeF.
    /// </summary>
    /// <remarks>
    /// W zależności od wybranego środowiska należy używać odpowiedniej
    /// bazowej ścieżki URL do usług API KSeF.
    /// </remarks>
    public enum Environment { Test, Demo, Prod}


    /// <summary>
    /// Zawiera konfigurację środowisk KSeF wraz z odpowiadającymi im adresami bazowymi.
    /// </summary>
    /// <remarks>
    /// Klasa udostępnia słownik pozwalający na pobranie właściwego adresu bazowego
    /// usługi KSeF na podstawie wybranego środowiska <see cref="Environment"/>.
    /// Przykład użycia:
    /// <code>
    /// var baseUrl = KsefEnvironmentConfig.BaseUrls[Environment.Test];
    /// </code>
    /// </remarks>
    public static class KsefEnvironmentConfig
    {
        public static readonly Dictionary<Environment, string> BaseUrls = new()
        {
            { Environment.Test, "https://api-test.ksef.mf.gov.pl" },
            { Environment.Demo, "https://api-demo.ksef.mf.gov.pl" },
            { Environment.Prod, "https://api.ksef.mf.gov.pl" }
        };
    }

}
