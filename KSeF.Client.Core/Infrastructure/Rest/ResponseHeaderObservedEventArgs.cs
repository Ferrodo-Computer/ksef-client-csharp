using System;
using System.Collections.Generic;
using System.Net.Http;

namespace KSeF.Client.Core.Infrastructure.Rest;

/// <summary>
/// Dane zdarzenia zgłaszanego, gdy odpowiedź HTTP zawiera jeden z obserwowanych nagłówków
/// (patrz <see cref="ResponseHeaderObservationOptions.HeaderNames"/>).
/// </summary>
public sealed class ResponseHeaderObservedEventArgs : EventArgs
{
    /// <summary>
    /// Nazwa nagłówka odpowiedzi (np. "X-System-Warning").
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Wartości nagłówka odpowiedzi.
    /// </summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>
    /// Metoda HTTP żądania, którego dotyczy odpowiedź.
    /// </summary>
    public HttpMethod RequestMethod { get; }

    /// <summary>
    /// Adres URL żądania, którego dotyczy odpowiedź.
    /// </summary>
    public Uri RequestUri { get; }

    public ResponseHeaderObservedEventArgs(string headerName, IReadOnlyList<string> values, HttpMethod requestMethod, Uri requestUri)
    {
        HeaderName = headerName;
        Values = values;
        RequestMethod = requestMethod;
        RequestUri = requestUri;
    }
}
