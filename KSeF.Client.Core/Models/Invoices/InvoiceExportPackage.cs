using System;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Invoices
{
    /// <summary>
    /// Dane paczki faktur przygotowanej do pobrania z eksportu KSeF.
    /// </summary>
    public class InvoiceExportPackage
    {
        /// <summary>
        /// Liczba faktur w paczce.
        /// </summary>
        public int InvoiceCount { get; set; }

        /// <summary>
        /// Rozmiar paczki w bajtach.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Lista części paczki do pobrania.
        /// </summary>
        public ICollection<InvoiceExportPackagePart> Parts { get; set; }

        /// <summary>
        /// Czy paczka została obcięta (nie zawiera wszystkich faktur z zakresu).
        /// </summary>
        public bool IsTruncated { get; set; }

        /// <summary>
        /// Data wystawienia ostatniej faktury w paczce.
        /// </summary>
        public DateTimeOffset? LastIssueDate { get; set; }

        /// <summary>
        /// Data sprzedaży ostatniej faktury w paczce.
        /// </summary>
        public DateTimeOffset? LastInvoicingDate { get; set; }

        /// <summary>
        /// Data trwałego przechowywania ostatniej faktury w paczce.
        /// </summary>
        public DateTimeOffset? LastPermanentStorageDate { get; set; }

        /// <summary>
        /// Dotyczy wyłącznie zapytań filtrowanych po typie daty PermanentStorage. Jeśli zapytanie dotyczyło najnowszego okresu, wartość ta może być wartością nieznacznie skorygowaną względem górnej granicy podanej w warunkach zapytania.
        /// </summary>
        public DateTimeOffset? PermanentStorageHwmDate { get; set; }
    }
}
