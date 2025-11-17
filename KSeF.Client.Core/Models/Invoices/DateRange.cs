
using System;

namespace KSeF.Client.Core.Models.Invoices
{
    public class DateRange
    {
        public DateType DateType { get; set; }
        public DateTime From { get; set; }
        public DateTime? To { get; set; }
    }
    public enum DateType
    {
        Issue,
        Invoicing,
        PermanentStorage
    }
}
