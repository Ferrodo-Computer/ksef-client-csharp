namespace KSeF.Client.Core.Exceptions
{
    /// <summary>
    /// Standardowa odpowiedź Problem Details (application/problem+json).
    /// Używana dla kodów HTTP, które nie mają własnego, rozszerzonego kontraktu.
    /// </summary>
    public class ProblemDetails
    {
        public string Title { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
        public string Instance { get; set; }
        public string TraceId { get; set; }
        public string Timestamp { get; set; }

        public bool HasProblemDetailsFields =>
            !string.IsNullOrWhiteSpace(Title) ||
            !string.IsNullOrWhiteSpace(Detail) ||
            Status != default ||
            !string.IsNullOrWhiteSpace(TraceId);
    }
}
