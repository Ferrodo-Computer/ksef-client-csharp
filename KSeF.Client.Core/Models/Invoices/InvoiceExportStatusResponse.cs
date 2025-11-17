using System;

namespace KSeF.Client.Core.Models.Invoices
{
    public class InvoiceExportStatusResponse
    {
        /// <summary>
        /// Aktualny status operacji (np. IN_PROGRESS, READY, ERROR).
        /// </summary>
        public StatusInfo Status { get; set; }

        /// <summary>
        /// Data zakończenia przetwarzania żądania.
        /// </summary>
        public DateTimeOffset? CompletedDate { get; set; }

        /// <summary>
        /// Data wygaśnięcia paczki faktur przygotowanej do pobrania. Po upływie tej daty paczka nie będzie już dostępna do pobrania.
        /// </summary>
        public DateTimeOffset? PackageExpirationDate { get; set; }

        /// <summary>
        /// Paczka faktur (InvoicePackage) – obecna tylko jeśli status = READY.
        /// </summary>
        public InvoiceExportPackage Package { get; set; }
    }
}