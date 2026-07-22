using System;

namespace KSeF.Client.Core.Models.TestData
{
    /// <summary>Aktualizacja danych certyfikatu (tylko na środowiskach testowych).</summary>
    public sealed class TestDataUpdateCertificateRequest
    {
        /// <summary>Nowa data ważności certyfikatu, nie może być późniejsza niż obecna.</summary>
        public DateTimeOffset ValidTo { get; set; }
    }
}
