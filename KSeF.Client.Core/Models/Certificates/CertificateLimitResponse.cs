namespace KSeF.Client.Core.Models.Certificates
{
    public class CertificateLimitResponse
    {
        public bool CanRequest { get; set; }
        public Enrollment Enrollment { get; set; }
        public Certificate Certificate { get; set; }
    }
    public class Enrollment
    {
        public int Remaining { get; set; }
        public int Limit { get; set; }
    }
    public class Certificate
    {
        public int Remaining { get; set; }
        public int Limit { get; set; }
    }
}
