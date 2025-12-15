# KSeF Client Factory

Biblioteka `KSeF.Client.ClientFactory` dostarcza **fabrykę klientów
KSeF**, zintegrowaną z **Dependency Injection**, umożliwiającą łatwe
tworzenie klientów dla różnych środowisk (**Test**, **Demo**, **Prod**)
wraz z obsługą **kryptografii** oraz **pobierania certyfikatów**.

------------------------------------------------------------------------

## Funkcjonalności

-   Rejestracja klientów HTTP dla środowisk:
    -   `Test`
    -   `Demo`
    -   `Prod`
-   Fabryka klientów `IKSeFClientFactory`
-   Fabryka usług kryptograficznych
-   Fabryka pobierania certyfikatów
-   Cache usług per środowisko
-   Pełna integracja z `IHttpClientFactory`
-   Wsparcie dla `Microsoft.Extensions.DependencyInjection`

------------------------------------------------------------------------

## Instalacja

``` bash
dotnet add reference KSeF.Client.ClientFactory.csproj
```


## Rejestracja w Dependency Injection

``` csharp
builder.Services.RegisterKSeFClientFactory();
```


------------------------------------------------------------------------

## Obsługiwane środowiska

  Test         https://ksef-test.mf.gov.pl
  Demo         https://ksef-demo.mf.gov.pl
  Produkcja    https://ksef.mf.gov.pl

------------------------------------------------------------------------

## Użycie -- Tworzenie klienta

``` csharp
var client = factory.KSeFClient(Environments.Prod);

var crypto = await cryptoFactory.CryprographyService(Environments.Demo);

var fetcher = await certFactory.GetOrSetCertificateFetcher(Environments.Prod);
```

