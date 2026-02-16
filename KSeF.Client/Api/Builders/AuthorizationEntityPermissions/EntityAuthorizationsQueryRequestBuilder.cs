using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Validation;

namespace KSeF.Client.Api.Builders.AuthorizationEntityPermissions
{
    /// <summary>
    /// Buduje zapytanie o uprawnienia podmiotowe otrzymane przez właściciela o wskazanym NIP.
    /// </summary>
    public static class EntityAuthorizationsQueryRequestBuilder
    {
        /// <summary>
        /// Rozpoczyna budowę zapytania dla właściciela identyfikowanego NIP-em.
        /// </summary>
        /// <returns>
        /// Interfejs pozwalający ustawić NIP właściciela i opcjonalnie typy uprawnień.
        /// </returns>
        public static IOwnerNipStep Create() => new Impl();

        /// <summary>
        /// Etap budowy zapytania, w którym ustawiany jest NIP właściciela.
        /// </summary>
        public interface IOwnerNipStep
        {
            /// <summary>
            /// Określa NIP właściciela, dla którego mają zostać pobrane otrzymane uprawnienia.
            /// </summary>
            /// <param name="ownerNip">
            /// NIP właściciela. Nie może być pusty i musi być zgodny z <see cref="RegexPatterns.Nip"/>.
            /// </param>
            /// <returns>
            /// Interfejs umożliwiający ustawienie typów uprawnień lub bezpośrednie zbudowanie zapytania.
            /// </returns>
            IOptionalStep ReceivedForOwnerNip(string ownerNip);
        }

        /// <summary>
        /// Etap budowy zapytania, w którym można ograniczyć wyniki do wybranych typów uprawnień.
        /// </summary>
        public interface IOptionalStep
        {
            /// <summary>
            /// Ustawia listę typów uprawnień, które mają zostać uwzględnione w wyniku zapytania.
            /// </summary>
            /// <param name="types">
            /// Kolekcja typów uprawnień. Gdy null, zwrócone zostaną wszystkie typy.
            /// </param>
            /// <returns>Ten sam interfejs, umożliwiający zbudowanie zapytania.</returns>
            IOptionalStep WithPermissionTypes(IEnumerable<InvoicePermissionType> types);

            /// <summary>
            /// Tworzy finalne zapytanie o uprawnienia podmiotowe.
            /// </summary>
            /// <returns>
            /// Obiekt <see cref="EntityAuthorizationsQueryRequest"/> gotowy do wysłania do KSeF.
            /// </returns>
            EntityAuthorizationsQueryRequest Build();
        }

        /// <inheritdoc />
        private sealed class Impl : IOwnerNipStep, IOptionalStep
        {
            private readonly EntityAuthorizationsQueryRequest _request = new();

            /// <inheritdoc />
            public IOptionalStep ReceivedForOwnerNip(string ownerNip)
            {
                if (string.IsNullOrWhiteSpace(ownerNip))
                {
                    throw new ArgumentNullException(nameof(ownerNip));
                }
                if (!RegexPatterns.Nip.IsMatch(ownerNip))
                {
                    throw new ArgumentException($"Nip: {ownerNip} jest nieprawidłowy.", nameof(ownerNip));
                }

                _request.AuthorizedIdentifier = new EntityAuthorizationsAuthorizedEntityIdentifier
                {
                    Type = EntityAuthorizationsAuthorizedEntityIdentifierType.Nip,
                    Value = ownerNip
                };
                _request.QueryType = QueryType.Received;

                return this;
            }

            /// <inheritdoc />
            public IOptionalStep WithPermissionTypes(IEnumerable<InvoicePermissionType> types)
            {
                _request.PermissionTypes = types is null ? null : [.. types];
                return this;
            }

            /// <inheritdoc />
            public EntityAuthorizationsQueryRequest Build() => _request;
        }
    }
}