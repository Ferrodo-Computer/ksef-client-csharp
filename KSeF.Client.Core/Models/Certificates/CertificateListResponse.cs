using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Certificates
{
    public class CertificateListResponse
    {
        public ICollection<CertificateResponse> Certificates { get; set; }
    }
    public class CertificateResponse
    {
        public string Certificate { get; set; }
        public string CertificateName { get; set; }
        public string CertificateSerialNumber { get; set; }
        public CertificateType CertificateType { get; set; }
    }
}
