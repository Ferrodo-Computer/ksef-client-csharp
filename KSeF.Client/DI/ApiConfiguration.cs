namespace KSeF.Client.DI
{
    /// <summary>
    /// Konfiguracja API KSeF
    /// </summary>
    public sealed class ApiConfiguration
    {     
        public string ApiPrefix { get; private set; } 
        public string ApiVersion { get; private set; }        
    }
}