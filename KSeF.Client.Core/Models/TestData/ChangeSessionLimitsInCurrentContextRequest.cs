namespace KSeF.Client.Core.Models.TestData
{
    public sealed class ChangeSessionLimitsInCurrentContextRequest
    {
        public SessionLimits OnlineSession { get; set; }
        public SessionLimits BatchSession { get; set; }
    }
}
