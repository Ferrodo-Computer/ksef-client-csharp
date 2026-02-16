using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.Entity
{
    public class EntityAuthorizationsQueryRequest
    {
        public EntityAuthorizationsAuthorizingEntityIdentifier AuthorizingIdentifier { get; set; }
        public EntityAuthorizationsAuthorizedEntityIdentifier AuthorizedIdentifier { get; set; }
        public QueryType QueryType { get; set; }
        public List<InvoicePermissionType> PermissionTypes { get; set; }
    }
    public enum QueryType
    {
        Granted,
        Received
    }
    public enum InvoicePermissionType
    {
        SelfInvoicing,
        TaxRepresentative,
        RRInvoicing,
        PefInvoicing
    }
}
