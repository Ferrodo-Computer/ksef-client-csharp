using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Invoices
{
    /// <summary>
    /// Reprezentuje plik _metadata.json znajdujący się w paczce eksportu faktur.
    /// Zawiera szczegółowe informacje o fakturach w paczce.
    /// </summary>
    public class InvoicePackageMetadata
    {
        /// <summary>
        /// Lista faktur w paczce.
        /// </summary>
        public List<InvoiceSummary> Invoices { get; set; } = new List<InvoiceSummary>();

        /// <summary>
        /// Zachowane dla wstecznej kompatybilności z dotychczasową nazwą właściwości.
        /// </summary>
        [System.Obsolete("InvoiceList jest przestarzały i zostanie usunięty. Zamiast tego użyj Invoices.", error: false)]
        public List<InvoiceSummary> InvoiceList
        {
            get => Invoices;
            set => Invoices = value ?? new List<InvoiceSummary>();
        }
    }
}
