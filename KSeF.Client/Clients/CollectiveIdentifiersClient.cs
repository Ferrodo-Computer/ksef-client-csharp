using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.CollectiveIdentifiers;
using KSeF.Client.Http.Helpers;
using System.Text;
using System.Text.RegularExpressions;

namespace KSeF.Client.Clients;

/// <inheritdoc />
public sealed class CollectiveIdentifiersClient(IRestClient restClient, IRouteBuilder routeBuilder) : ClientBase(restClient, routeBuilder), ICollectiveIdentifiersClient
{
    /// <inheritdoc />
    public Task<GenerateCollectiveIdentifierResponse> GenerateCollectiveIdentifierAsync(GenerateCollectiveIdentifierRequest request, string accessToken, CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);
        Guard.ThrowIfNullOrWhiteSpace(accessToken);

        return ExecuteAsync<GenerateCollectiveIdentifierResponse, GenerateCollectiveIdentifierRequest>(Routes.CollectiveIdentifiers.Root, request, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CollectiveIdentifiersQueryResponse> QueryCollectiveIdentifiersAsync(
        CollectiveIdentifiersQueryRequest request,
        string accessToken,
        int? pageSize = null,
        string continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);
        Guard.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new(Routes.CollectiveIdentifiers.Query);
        PaginationHelper.AppendPagination(null, pageSize, urlBuilder);

        return ExecuteAsync<CollectiveIdentifiersQueryResponse, CollectiveIdentifiersQueryRequest>(
            urlBuilder.ToString(),
            request,
            accessToken,
            !string.IsNullOrWhiteSpace(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CollectiveIdentifiersByKsefNumberQueryResponse> GetCollectiveIdentifiersByKsefNumberAsync(
        string ksefNumber,
        string accessToken,
        string continuationToken = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNullOrWhiteSpace(ksefNumber);
        Guard.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new(Routes.CollectiveIdentifiers.ByKsefNumber(Uri.EscapeDataString(ksefNumber)));
        PaginationHelper.AppendPagination(null, pageSize, urlBuilder);

        return ExecuteAsync<CollectiveIdentifiersByKsefNumberQueryResponse>(
            urlBuilder.ToString(),
            HttpMethod.Get,
            accessToken,
            !string.IsNullOrWhiteSpace(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CollectiveIdentifierInvoicesQueryResponse> GetCollectiveIdentifierInvoicesAsync(
        string collectiveIdentifierNumber,
        string accessToken,
        string continuationToken = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNullOrWhiteSpace(collectiveIdentifierNumber);
        Guard.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new(Routes.CollectiveIdentifiers.InvoicesByCollectiveIdentifierNumber(Uri.EscapeDataString(collectiveIdentifierNumber)));
        PaginationHelper.AppendPagination(null, pageSize, urlBuilder);

        return ExecuteAsync<CollectiveIdentifierInvoicesQueryResponse>(
            urlBuilder.ToString(),
            HttpMethod.Get,
            accessToken,
            !string.IsNullOrWhiteSpace(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }
}
