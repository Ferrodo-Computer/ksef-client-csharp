using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace KSeF.Client.Clients;

/// <inheritdoc />
public class SessionStatusClient(IRestClient restClient, IRouteBuilder routeBuilder)
    : ClientBase(restClient, routeBuilder), ISessionStatusClient
{
    /// <inheritdoc />
    public Task<SessionsListResponse> GetSessionsAsync(SessionType sessionType, string accessToken, int? pageSize, string continuationToken, SessionsFilter sessionsFilter = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new StringBuilder($"{Routes.Sessions.Root}?sessionType={sessionType}");

        if (pageSize.HasValue)
        {
            urlBuilder.Append($"&pageSize={pageSize.Value}");
        }

        sessionsFilter?.AppendAsQuery(urlBuilder);

        string endpoint = urlBuilder.ToString();

        return ExecuteAsync<SessionsListResponse>(
            endpoint,
            HttpMethod.Get,
            accessToken,
            !string.IsNullOrEmpty(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SessionStatusResponse> GetSessionStatusAsync(string sessionReferenceNumber, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        string endpoint = Routes.Sessions.ByReference(Uri.EscapeDataString(sessionReferenceNumber));
        return ExecuteAsync<SessionStatusResponse>(endpoint, HttpMethod.Get, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SessionInvoicesResponse> GetSessionInvoicesAsync(string sessionReferenceNumber, string accessToken, int? pageSize = null, string continuationToken = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new StringBuilder(Routes.Sessions.Invoices(Uri.EscapeDataString(sessionReferenceNumber)));

        if (pageSize.HasValue)
        {
            urlBuilder.Append($"?pageSize={pageSize.Value}");
        }

        string endpoint = urlBuilder.ToString();

        return ExecuteAsync<SessionInvoicesResponse>(
            endpoint,
            HttpMethod.Get,
            accessToken,
            !string.IsNullOrEmpty(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SessionInvoice> GetSessionInvoiceAsync(string sessionReferenceNumber, string invoiceReferenceNumber, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        string endpoint = Routes.Sessions.Invoice(Uri.EscapeDataString(sessionReferenceNumber), Uri.EscapeDataString(invoiceReferenceNumber));
        return ExecuteAsync<SessionInvoice>(endpoint, HttpMethod.Get, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SessionInvoicesResponse> GetSessionFailedInvoicesAsync(string sessionReferenceNumber, string accessToken, int? pageSize, string continuationToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        StringBuilder urlBuilder = new StringBuilder(Routes.Sessions.FailedInvoices(Uri.EscapeDataString(sessionReferenceNumber)));

        if (pageSize.HasValue)
        {
            urlBuilder.Append($"?pageSize={pageSize.Value}");
        }

        string endpoint = urlBuilder.ToString();

        return ExecuteAsync<SessionInvoicesResponse>(
            endpoint,
            HttpMethod.Get,
            accessToken,
            !string.IsNullOrEmpty(continuationToken)
                ? new Dictionary<string, string> { { "x-continuation-token", Regex.Unescape(continuationToken) } }
                : null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetSessionInvoiceUpoByKsefNumberAsync(string sessionReferenceNumber, string ksefNumber, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(ksefNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        string endpoint = Routes.Sessions.UpoByKsefNumber(Uri.EscapeDataString(sessionReferenceNumber), Uri.EscapeDataString(ksefNumber));
        return ExecuteAsync<string>(endpoint, HttpMethod.Get, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetSessionInvoiceUpoByReferenceNumberAsync(string sessionReferenceNumber, string invoiceReferenceNumber, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        string endpoint = Routes.Sessions.UpoByInvoiceReference(Uri.EscapeDataString(sessionReferenceNumber), Uri.EscapeDataString(invoiceReferenceNumber));
        return ExecuteAsync<string>(endpoint, HttpMethod.Get, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetSessionUpoAsync(string sessionReferenceNumber, string upoReferenceNumber, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(upoReferenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        string endpoint = Routes.Sessions.Upo(Uri.EscapeDataString(sessionReferenceNumber), Uri.EscapeDataString(upoReferenceNumber));
        return ExecuteAsync<string>(endpoint, HttpMethod.Get, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetUpoAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        return ExecuteAsync<string>(uri, HttpMethod.Get, cancellationToken);
    }
}
