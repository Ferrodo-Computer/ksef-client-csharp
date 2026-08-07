#nullable enable
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

[Collection("BatchSessionScenario")]
public partial class BatchSessionE2ETests : TestBase
{
    private const int TotalInvoices = 20;
    private const int PartQuantity = 11;
    private const int ExpectedFailedInvoiceCount = 0;
    private const int ExpectedSessionStatusCode = 200;
    private const int TotalInvoices10k = 10_000;
    private const int SessionMaxAttempts10k = 120;
    private const int ExportMaxAttempts10k = 120;
    private static readonly TimeSpan SessionTimeout10k = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan ExportTimeout10k = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ExportMetadataPollingDelay10k = TimeSpan.FromSeconds(10);

    private readonly string accessToken = string.Empty;
    private readonly string sellerNip = string.Empty;

    public BatchSessionE2ETests()
    {
        // Autoryzacja do testów – jednorazowa, dane zapisane w readonly properties
        string nip = MiscellaneousUtils.GetRandomNip();
        AuthenticationOperationStatusResponse authInfo = AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient, nip)
            .GetAwaiter().GetResult();

        accessToken = authInfo.AccessToken.Token;
        sellerNip = nip;
    }
}
