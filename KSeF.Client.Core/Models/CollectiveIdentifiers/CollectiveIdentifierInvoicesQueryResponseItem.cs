namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifierInvoicesQueryResponseItem
    {
        public string KsefNumber { get; set; }
        public string CollectiveIdentifierNumber { get; set; }
        public CollectiveIdentifierInvoicesQueryResponseItemPayment Payment { get; set; }
        public string Description { get; set; }
        public bool DetailsHidden { get; set; }
    }
}
