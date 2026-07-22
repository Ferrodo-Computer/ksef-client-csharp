using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Http;
using KSeF.Client.Tests.WarningTestApp;

Console.WriteLine("KSeF.Client.Tests.WarningTestApp - demonstracja odczytu nagłówka X-System-Warning");
Console.WriteLine();

// Kontekst demo udostępnia gotowych klientów SDK skonfigurowanych przez AddKSeFClient.
using DemoAppContext context = new(forcedWarningMessage: "Testowe-ostrzezenie-z-demo-aplikacji");

// Rejestracja pojedynczej subskrypcji.
// Od tego momentu aplikacja otrzymuje zdarzenia dla wszystkich skonfigurowanych
// nagłówków obserwacyjnych (np. X-System-Warning), niezależnie od używanego klienta SDK.
Console.WriteLine("[1] Subskrypcja RestClient.ObserveResponseHeaders (raz, na starcie)");
using IDisposable subscription = RestClient.ObserveResponseHeaders((sender, e) =>
{
    Console.WriteLine($"    [OSTRZEŻENIE] {e.HeaderName}: {string.Join(" | ", e.Values)}");
    Console.WriteLine($"                  (dla {e.RequestMethod} {e.RequestUri})");
});

try
{
	// 2) Zwykłe wywołanie klienta SDK – bez dodatkowego kodu związanego z obsługą nagłówków.
	Console.WriteLine("[2] AuthorizationClient.GetAuthChallengeAsync()...");
    AuthenticationChallengeResponse challengeResponse = await context.AuthorizationClient
        .GetAuthChallengeAsync()
        .ConfigureAwait(false);
    Console.WriteLine($"    Challenge: {challengeResponse.Challenge}");
    Console.WriteLine();

	// 3) Wywołanie innego klienta SDK.
	// Ta sama subskrypcja nadal odbiera zdarzenia, ponieważ mechanizm obserwacji
	// działa dla wszystkich klientów korzystających ze wspólnego IRestClient.
	Console.WriteLine("[3] CryptographyClient.GetPublicCertificatesAsync()...");
    ICollection<PemCertificateInfo> certificates = await context.CryptographyClient
        .GetPublicCertificatesAsync()
        .ConfigureAwait(false);
    Console.WriteLine($"    Liczba pobranych certyfikatów: {certificates.Count}");
    Console.WriteLine();

	Console.WriteLine("Gotowe.");
	Console.WriteLine("Powyższe ostrzeżenia zostały przechwycone przez pojedynczą subskrypcję");
	Console.WriteLine("pomimo użycia różnych klientów SDK.");
}
catch (Exception ex)
{
    Console.WriteLine("Wystąpił błąd.");
    Console.WriteLine(ex.ToString());
}

Console.WriteLine();
if (!Console.IsInputRedirected)
{
    Console.WriteLine("Naciśnij dowolny klawisz, aby zakończyć...");
    Console.ReadKey();
}
