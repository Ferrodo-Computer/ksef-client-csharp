namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Wynik joba BackgroundService (wysyłka faktury + UPO).
/// </summary>
public sealed class KsefBackgroundWorkResult
{
    public required string Nip { get; init; }

    public required string SessionReferenceNumber { get; init; }

    public required string InvoiceReferenceNumber { get; init; }

    public required string KsefNumber { get; init; }

    public required string InvoiceUpoXml { get; init; }

    public required string SessionUpoReferenceNumber { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Magazyn wyniku joba BackgroundService (do obserwacji z testów / diagnostyki).
/// </summary>
public interface IKsefBackgroundWorkResultStore
{
    KsefBackgroundWorkResult? LastSuccess { get; }

    Exception? LastError { get; }

    /// <summary>
    /// Czeka na sukces lub błąd joba.
    /// </summary>
    Task<KsefBackgroundWorkResult> WaitForSuccessAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    void Reset();

    void SetSuccess(KsefBackgroundWorkResult result);

    void SetError(Exception exception);
}

/// <summary>
/// Domyślna implementacja magazynu wyniku BackgroundService.
/// </summary>
public sealed class KsefBackgroundWorkResultStore : IKsefBackgroundWorkResultStore
{
    private readonly object _gate = new();
    private TaskCompletionSource<KsefBackgroundWorkResult> _completion =
        CreateCompletionSource();

    public KsefBackgroundWorkResult? LastSuccess { get; private set; }

    public Exception? LastError { get; private set; }

    public Task<KsefBackgroundWorkResult> WaitForSuccessAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        Task<KsefBackgroundWorkResult> completionTask;
        lock (_gate)
        {
            if (LastSuccess is not null)
            {
                return Task.FromResult(LastSuccess);
            }

            completionTask = _completion.Task;
        }

        return WaitWithTimeoutAsync(completionTask, timeout, cancellationToken);
    }

    public void Reset()
    {
        lock (_gate)
        {
            LastSuccess = null;
            LastError = null;
            _completion = CreateCompletionSource();
        }
    }

    public void SetSuccess(KsefBackgroundWorkResult result)
    {
        lock (_gate)
        {
            LastSuccess = result;
            LastError = null;
            _completion.TrySetResult(result);
        }
    }

    public void SetError(Exception exception)
    {
        lock (_gate)
        {
            LastError = exception;
            _completion.TrySetException(exception);
        }
    }

    private static TaskCompletionSource<KsefBackgroundWorkResult> CreateCompletionSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task<KsefBackgroundWorkResult> WaitWithTimeoutAsync(
        Task<KsefBackgroundWorkResult> completionTask,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        Task delayTask = Task.Delay(timeout, cancellationToken);
        Task finished = await Task.WhenAny(completionTask, delayTask).ConfigureAwait(false);
        if (finished != completionTask)
        {
            throw new TimeoutException($"Timeout oczekiwania na wynik BackgroundService ({timeout}).");
        }

        return await completionTask.ConfigureAwait(false);
    }
}
