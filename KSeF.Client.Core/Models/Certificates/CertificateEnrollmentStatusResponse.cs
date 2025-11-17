using System;

namespace KSeF.Client.Core.Models.Certificates
{
    public class CertificateEnrollmentStatusResponse
    {
        public DateTime RequestDate { get; set; }
        public StatusInfo Status { get; set; }
        public string CertificateSerialNumber { get; set; }
    }
}
