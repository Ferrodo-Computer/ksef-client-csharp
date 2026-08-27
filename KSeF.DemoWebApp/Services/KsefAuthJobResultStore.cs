namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Wynik przykładowego joba uwierzytelnienia.
/// </summary>
public sealed class KsefAuthJobResult
{
    public required string Nip { get; init; }

    public required string AccessToken { get; init; }

    public required int ActiveSessionsCount { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Magazyn wyniku joba uwierzytelnienia.
/// </summary>
public interface IKsefAuthJobResultStore
{
    KsefAuthJobResult? LastSuccess { get; }

    Exception? LastError { get; }

    Task<KsefAuthJobResult> WaitForSuccessAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    void Reset();

    void SetSuccess(KsefAuthJobResult result);

    void SetError(Exception exception);
}

/// <summary>
/// Domyślna implementacja magazynu wyniku joba uwierzytelnienia.
/// </summary>
public sealed class KsefAuthJobResultStore : IKsefAuthJobResultStore
{
    private readonly object _gate = new();
    private TaskCompletionSource<KsefAuthJobResult> _completion = CreateCompletionSource();

    public KsefAuthJobResult? LastSuccess { get; private set; }

    public Exception? LastError { get; private set; }

    public Task<KsefAuthJobResult> WaitForSuccessAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        Task<KsefAuthJobResult> completionTask;
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

    public void SetSuccess(KsefAuthJobResult result)
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

    private static TaskCompletionSource<KsefAuthJobResult> CreateCompletionSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task<KsefAuthJobResult> WaitWithTimeoutAsync(
        Task<KsefAuthJobResult> completionTask,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        Task delayTask = Task.Delay(timeout, cancellationToken);
        Task finished = await Task.WhenAny(completionTask, delayTask).ConfigureAwait(false);
        if (finished != completionTask)
        {
            throw new TimeoutException($"Timeout oczekiwania na wynik joba auth ({timeout}).");
        }

        return await completionTask.ConfigureAwait(false);
    }
}
