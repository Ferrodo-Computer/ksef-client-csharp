using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.CollectiveIdentifiers;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.OnlineSession;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.CollectiveIdentifiers;

public class CollectiveIdentifiersE2ETests : TestBase
{
    private const string InvoiceTemplate = "invoice-template-fa-3-with-custom-Subject2.xml";
    private const int MaxPollingAttempts = 30;
    private const int InvoicesCount = 5;
    private const int PermissionPropagationMaxAttempts = 30;
    private static readonly TimeSpan PermissionPropagationDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Weryfikuje pełny cykl generowania identyfikatora zbiorczego dla kilku faktur sprzedawcy
    /// (API wymaga co najmniej 2 faktur w identyfikatorze zbiorczym) oraz jego odnajdywania
    /// przez oba dostępne zapytania (po numerze KSeF i po numerze identyfikatora).
    /// Kroki:
    /// 1) Sprzedawca wystawia kilka faktur i czeka na ich dostępność w systemie
    /// 2) Generowanie identyfikatora zbiorczego dla wszystkich wystawionych faktur
    /// 3) Weryfikacja odnalezienia identyfikatora po numerze KSeF pierwszej faktury
    /// 4) Weryfikacja odnalezienia wszystkich faktur w ramach identyfikatora zbiorczego
    /// 5) Weryfikacja odnalezienia identyfikatora w wynikach zapytania listującego
    /// </summary>
    [Fact]
    public async Task GenerateCollectiveIdentifier_ThenFindItByKsefNumberAndByNumber()
    {
        string sellerNip = MiscellaneousUtils.GetRandomNip();
        AuthenticationOperationStatusResponse sellerAuth = await AuthenticationUtils.AuthenticateAsync(
            AuthorizationClient, sellerNip);
        string sellerToken = sellerAuth.AccessToken.Token;

        string buyerNip = MiscellaneousUtils.GetRandomNip();
        List<string> ksefNumbers = [];
        for (int i = 0; i < InvoicesCount; i++)
        {
            ksefNumbers.Add(await SendInvoiceAndGetKsefNumberAsync(sellerNip, buyerNip, sellerToken));
        }

        DateTimeOffset dateFrom = DateTimeOffset.UtcNow.AddMinutes(-5);

        GenerateCollectiveIdentifierResponse generateResponse = await CollectiveIdentifiersClient.GenerateCollectiveIdentifierAsync(
            new GenerateCollectiveIdentifierRequest
            {
                Invoices = ksefNumbers.Select(ksefNumber => new CollectiveIdentifierInvoice { KsefNumber = ksefNumber }).ToList()
            },
            sellerToken, CancellationToken);

        Assert.NotNull(generateResponse);
        Assert.False(string.IsNullOrWhiteSpace(generateResponse.CollectiveIdentifierNumber));
        string collectiveIdentifierNumber = generateResponse.CollectiveIdentifierNumber;

        CollectiveIdentifiersByKsefNumberQueryResponse byKsefNumber = await AsyncPollingUtils.PollAsync(
            action: () => CollectiveIdentifiersClient.GetCollectiveIdentifiersByKsefNumberAsync(
                ksefNumbers[0], sellerToken, pageSize: 10, cancellationToken: CancellationToken),
            condition: r => r?.CollectiveIdentifiers is not null && r.CollectiveIdentifiers.Any(ci => ci.CollectiveIdentifierNumber == collectiveIdentifierNumber),
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: MaxPollingAttempts,
            cancellationToken: CancellationToken);

        Assert.NotNull(byKsefNumber);
        Assert.Contains(byKsefNumber.CollectiveIdentifiers, ci => ci.CollectiveIdentifierNumber == collectiveIdentifierNumber);

        CollectiveIdentifierInvoicesQueryResponse invoicesResponse = await AsyncPollingUtils.PollAsync(
            action: () => CollectiveIdentifiersClient.GetCollectiveIdentifierInvoicesAsync(
                collectiveIdentifierNumber, sellerToken, pageSize: 10, cancellationToken: CancellationToken),
            condition: r => r?.Invoices is not null && ksefNumbers.All(ksefNumber => r.Invoices.Any(inv => inv.KsefNumber == ksefNumber)),
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: MaxPollingAttempts,
            cancellationToken: CancellationToken);

        Assert.NotNull(invoicesResponse);
        foreach (string ksefNumber in ksefNumbers)
        {
            Assert.Contains(invoicesResponse.Invoices, inv => inv.KsefNumber == ksefNumber);
        }

        CollectiveIdentifiersQueryResponse queryResponse = await AsyncPollingUtils.PollAsync(
            action: () => CollectiveIdentifiersClient.QueryCollectiveIdentifiersAsync(
                new CollectiveIdentifiersQueryRequest
                {
                    DateCreatedFrom = dateFrom,
                    DateCreatedTo = DateTimeOffset.UtcNow.AddMinutes(5)
                },
                sellerToken,
                pageSize: 10,
                cancellationToken: CancellationToken),
            condition: r => r?.CollectiveIdentifiers is not null && r.CollectiveIdentifiers.Any(ci => ci.CollectiveIdentifierNumber == collectiveIdentifierNumber),
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: MaxPollingAttempts,
            cancellationToken: CancellationToken);

        Assert.NotNull(queryResponse);
        Assert.Contains(queryResponse.CollectiveIdentifiers, ci => ci.CollectiveIdentifierNumber == collectiveIdentifierNumber);
    }

    /// <summary>
    /// Weryfikuje, że osoba, której nadano w kontekście sprzedawcy uprawnienia `InvoiceRead` i `CollectiveIdentifierManage`
    /// przez rzeczywisty endpoint nadawania uprawnień (`POST /permissions/persons/grants`), może w tym kontekście
    /// wygenerować identyfikator zbiorczy (zgodnie z wymaganymi uprawnieniami endpointu).
    /// Kroki:
    /// 1) Sprzedawca wystawia kilka faktur
    /// 2) Sprzedawca nadaje osobie (PESEL) uprawnienia `InvoiceRead` i `CollectiveIdentifierManage` w swoim kontekście
    /// 3) Oczekiwanie na zakończenie operacji nadania uprawnień
    /// 4) Uwierzytelnienie tej osoby w kontekście sprzedawcy
    /// 5) Wygenerowanie identyfikatora zbiorczego przy użyciu tokenu tej osoby — powinno się udać
    /// </summary>
    [Fact]
    public async Task GrantCollectiveIdentifierManagePermission_ThenGenerateCollectiveIdentifier()
    {
        string sellerNip = MiscellaneousUtils.GetRandomNip();
        string authorizedPesel = MiscellaneousUtils.GetRandomPesel();

        AuthenticationOperationStatusResponse sellerAuth = await AuthenticationUtils.AuthenticateAsync(
            AuthorizationClient, sellerNip);
        string sellerToken = sellerAuth.AccessToken.Token;

        string buyerNip = MiscellaneousUtils.GetRandomNip();
        List<string> ksefNumbers = [];
        for (int i = 0; i < InvoicesCount; i++)
        {
            ksefNumbers.Add(await SendInvoiceAndGetKsefNumberAsync(sellerNip, buyerNip, sellerToken));
        }

        GrantPermissionsPersonSubjectIdentifier subject = new()
        {
            Type = GrantPermissionsPersonSubjectIdentifierType.Pesel,
            Value = authorizedPesel
        };

        PersonPermissionSubjectDetails subjectDetails = new()
        {
            SubjectDetailsType = PersonPermissionSubjectDetailsType.PersonByIdentifier,
            PersonById = new PersonPermissionPersonById { FirstName = "Jan", LastName = "Testowy" }
        };

        OperationResponse grantResponse = await PermissionsUtils.GrantPersonPermissionsAsync(
            KsefClient,
            sellerToken,
            subject,
            [PersonPermissionType.InvoiceRead, PersonPermissionType.CollectiveIdentifierManage],
            subjectDetails,
            "E2E CollectiveIdentifierManage test");

        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrWhiteSpace(grantResponse.ReferenceNumber));

        PermissionsOperationStatusResponse grantStatus = await AsyncPollingUtils.PollAsync(
            action: () => KsefClient.OperationsStatusAsync(grantResponse.ReferenceNumber, sellerToken),
            condition: s => s?.Status?.Code == OperationStatusCodeResponse.Success,
            delay: PermissionPropagationDelay,
            maxAttempts: PermissionPropagationMaxAttempts,
            cancellationToken: CancellationToken);

        Assert.Equal(OperationStatusCodeResponse.Success, grantStatus.Status.Code);

        AuthenticationOperationStatusResponse authorizedAuth = await AuthenticationUtils.AuthenticateAsync(
            AuthorizationClient, authorizedPesel, sellerNip);
        string authorizedToken = authorizedAuth.AccessToken.Token;

        GenerateCollectiveIdentifierResponse generateResponse = await CollectiveIdentifiersClient.GenerateCollectiveIdentifierAsync(
            new GenerateCollectiveIdentifierRequest
            {
                Invoices = ksefNumbers.Select(ksefNumber => new CollectiveIdentifierInvoice { KsefNumber = ksefNumber }).ToList()
            },
            authorizedToken, CancellationToken);

        Assert.NotNull(generateResponse);
        Assert.False(string.IsNullOrWhiteSpace(generateResponse.CollectiveIdentifierNumber));
    }

    private async Task<string> SendInvoiceAndGetKsefNumberAsync(string sellerNip, string buyerNip, string sellerToken)
    {
        EncryptionData encryptionData = CryptographyService.GetEncryptionData();
        OpenOnlineSessionResponse session = await OnlineSessionUtils.OpenOnlineSessionAsync(
            KsefClient, encryptionData, sellerToken).ConfigureAwait(false);

        SendInvoiceResponse invoiceResponse = await OnlineSessionUtils.SendInvoiceAsync(
            KsefClient, session.ReferenceNumber, sellerToken, sellerNip, buyerNip,
            InvoiceTemplate, encryptionData, CryptographyService).ConfigureAwait(false);

        await OnlineSessionUtils.CloseOnlineSessionAsync(KsefClient, session.ReferenceNumber, sellerToken).ConfigureAwait(false);

        SessionInvoice processedInvoice = await AsyncPollingUtils.PollAsync(
            action: () => OnlineSessionUtils.GetSessionInvoiceStatusAsync(
                KsefClient, session.ReferenceNumber, invoiceResponse.ReferenceNumber, sellerToken),
            condition: inv => inv?.PermanentStorageDate is not null,
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: MaxPollingAttempts,
            cancellationToken: CancellationToken).ConfigureAwait(false);

        Assert.NotNull(processedInvoice?.KsefNumber);
        string ksefNumber = processedInvoice.KsefNumber;

        await AsyncPollingUtils.PollAsync(
            action: () => KsefClient.GetInvoiceAsync(ksefNumber, sellerToken),
            condition: xml => !string.IsNullOrWhiteSpace(xml),
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: MaxPollingAttempts,
            cancellationToken: CancellationToken).ConfigureAwait(false);

        return ksefNumber;
    }
}
