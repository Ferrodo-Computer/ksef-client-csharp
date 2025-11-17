namespace KSeF.Client.Core.Models.TestData
{
    public sealed class ChangeSessionLimitsInCurrentContextRequest
    {
        public TestDataSessionLimitsBase OnlineSession { get; set; }
        public TestDataSessionLimitsBase BatchSession { get; set; }
    }
}
