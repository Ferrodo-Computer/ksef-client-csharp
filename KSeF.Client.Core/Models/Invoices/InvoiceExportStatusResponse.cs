using System;

namespace KSeF.Client.Core.Models.Invoices
{
    public class InvoiceExportStatusResponse
    {
        /// <summary>
        /// Status eksportu.
        /// </summary>
        public OperationStatusInfo Status { get; set; }

        /// <summary>
        /// Data zakończenia przetwarzania żądania eksportu faktur.
        /// </summary>
        public DateTimeOffset? CompletedDate { get; set; }

        /// <summary>
        /// Data wygaśnięcia paczki faktur przygotowanej do pobrania. Po upływie tej daty paczka nie będzie już dostępna do pobrania.
        /// </summary>
        public DateTimeOffset? PackageExpirationDate { get; set; }

        /// <summary>
        /// Dane paczki faktur przygotowanej do pobrania.
        /// </summary>
        public InvoiceExportPackage Package { get; set; }
    }
}