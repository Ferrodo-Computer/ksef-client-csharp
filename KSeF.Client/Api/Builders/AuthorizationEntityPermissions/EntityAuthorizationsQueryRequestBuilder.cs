using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.Client.Api.Builders.AuthorizationPermissions
{
    /// <summary>
    /// Buduje zapytanie o OTRZYMANE uprawnienia podmiotowe jako WŁAŚCICIEL w kontekście NIP.
    /// </summary>
    public static class EntityAuthorizationsQueryRequestBuilder
    {
        public static IOwnerNipStep Create() => new Impl();

        public interface IOwnerNipStep
        {
            IOptionalStep ReceivedForOwnerNip(string ownerNip);
        }

        public interface IOptionalStep
        {
            IOptionalStep WithPermissionTypes(IEnumerable<InvoicePermissionType> types);
            EntityAuthorizationsQueryRequest Build();
        }

        private sealed class Impl : IOwnerNipStep, IOptionalStep
        {
            private readonly EntityAuthorizationsQueryRequest _request = new EntityAuthorizationsQueryRequest();

            public IOptionalStep ReceivedForOwnerNip(string ownerNip)
            {
                if (string.IsNullOrWhiteSpace(ownerNip))
                {
                    throw new ArgumentException("ownerNip is required.", nameof(ownerNip));
                }

                _request.AuthorizedIdentifier = new EntityAuthorizationsAuthorizedEntityIdentifier
                {
                    Type = EntityAuthorizationsAuthorizedEntityIdentifierType.Nip,
                    Value = ownerNip
                };
                _request.QueryType = QueryType.Received;

                return this;
            }

            public IOptionalStep WithPermissionTypes(IEnumerable<InvoicePermissionType> types)
            {
                _request.PermissionTypes = types is null ? null : new List<InvoicePermissionType>(types);
                return this;
            }

            public EntityAuthorizationsQueryRequest Build() => _request;
        }
    }
}
