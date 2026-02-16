namespace KSeF.Client.Api.Services;

/// <summary>
/// Określa sposób wykonania procesu pobrania certyfikatów publicznych:
/// - Disabled: nie uruchamiaj w tle; aplikacja startuje dalej
/// - NonBlocking: uruchom w tle; aplikacja startuje dalej
/// - Blocking: uruchom i czekaj na zakończenie; jeśli nie wyjdzie, aplikacja się nie uruchomi
/// </summary>
/// <remarks>Określa jak ma zostać zainicjalizowana aplikacja oraz jakie ma być jej zachowanie w zależności od przekazanej opcji oraz wyniku pobrania certyfikatów publicznych.</remarks>
public enum CryptographyServiceWarmupMode
{
    Disabled,       // Nie uruchamiaj w tle; aplikacja startuje dalej
    NonBlocking,    // Uruchom w tle; aplikacja startuje dalej
    Blocking        // Uruchom i czekaj na zakończenie; jeżeli nie wyjdzie, aplikacja się nie uruchomi
}
