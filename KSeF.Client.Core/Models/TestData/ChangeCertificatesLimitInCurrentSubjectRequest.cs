namespace KSeF.Client.Core.Models.TestData
{
    public sealed class ChangeCertificatesLimitInCurrentSubjectRequest
    {
        public TestDataSubjectIdentifierType SubjectIdentifierType { get; set; }
        public TestDataEnrollment Enrollment { get; set; }
        public TestDataCertificate Certificate { get; set; }
    }
}
