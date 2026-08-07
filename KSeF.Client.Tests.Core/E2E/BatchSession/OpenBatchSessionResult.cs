using KSeF.Client.Core.Models.Sessions.BatchSession;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    private sealed record OpenBatchSessionResult(
        string ReferenceNumber,
        OpenBatchSessionRequest OpenBatchSessionRequest,
        OpenBatchSessionResponse OpenBatchSessionResponse,
        List<BatchPartSendingInfo> EncryptedParts
    );

    /// <summary>
    /// Wynik wspólnego przebiegu sesji wsadowej (wysyłka, zamknięcie, UPO).
    /// </summary>
    private sealed record BatchSessionFlowResult(
        string BatchSessionReferenceNumber,
        string KsefNumber,
        string UpoReferenceNumber
    );
}
