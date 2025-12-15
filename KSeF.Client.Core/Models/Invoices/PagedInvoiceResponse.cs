
using System;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Invoices
{
    /// <summary>
    /// Odpowiedź z paginacją zawierająca listę faktur.
    /// </summary>
    public class PagedInvoiceResponse
    {
        /// <summary>
        /// Określa, czy istnieją kolejne wyniki zapytania.
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// Określa, czy osiągnięto maksymalny dopuszczalny zakres wyników zapytania (10 000).
        /// </summary>
        public bool IsTruncated { get; set; }

        /// <summary>
        /// Lista faktur spełniających kryteria.
        /// </summary>
        public ICollection<InvoiceSummary> Invoices { get; set; }

        /// <summary>
        /// Górna granica daty PermanentStorage (UTC), do której system uwzględnił dane w ramach tego zapytania - HWM (high water mark).
        /// </summary>
        public DateTimeOffset? PermanentStorageHwmDate { get; set; }
    }
}
