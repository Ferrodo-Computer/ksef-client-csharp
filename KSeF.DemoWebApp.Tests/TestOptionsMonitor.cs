using Microsoft.Extensions.Options;

namespace KSeF.DemoWebApp.Tests;

/// <summary>
/// Prosty IOptionsMonitor zwracający stałą wartość bieżących opcji.
/// </summary>
internal sealed class TestOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T>
{
    public T CurrentValue { get; private set; } = currentValue;

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;

    public void Set(T value) => CurrentValue = value;
}
