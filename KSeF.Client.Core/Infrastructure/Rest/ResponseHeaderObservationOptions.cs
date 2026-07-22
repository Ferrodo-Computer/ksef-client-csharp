using System;
using System.Collections.Generic;

namespace KSeF.Client.Core.Infrastructure.Rest;

/// <summary>
/// Konfiguruje, które nagłówki odpowiedzi HTTP mają być zgłaszane przez
/// <see cref="System.EventHandler{ResponseHeaderObservedEventArgs}"/> zdarzenie
/// <c>RestClient.ResponseHeaderObserved</c>.
/// </summary>
public sealed class ResponseHeaderObservationOptions
{
    /// <summary>
    /// Włącza obserwację nagłówków odpowiedzi. Domyślnie wyłączone (brak narzutu wydajnościowego).
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Nazwy nagłówków odpowiedzi, o których zgłaszane będą zdarzenia. Porównanie bez uwzględniania
    /// wielkości liter. Domyślnie zawiera nagłówki ostrzeżeń systemowych API KSeF; można dodać
    /// kolejne bez zmian w kodzie klienta.
    /// </summary>
    public ISet<string> HeaderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "X-System-Warning"
    };
}
