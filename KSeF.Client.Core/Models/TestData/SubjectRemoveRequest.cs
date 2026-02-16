namespace KSeF.Client.Core.Models.TestData
{
    /// <summary>Żądanie usunięcia podmiotu testowego.</summary>
    public sealed class SubjectRemoveRequest
    {
        /// <summary>Identyfikator podmiotu do usunięcia.</summary>
        public string SubjectNip { get; set; }
    }
}
