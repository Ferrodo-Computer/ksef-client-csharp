## Przykładowe szablony faktur
W bibliotece znajdują się pliki z przykładowymi szablonami faktur, między innymi:

* invoice-template-fa-2.xml
* invoice-template-fa-3.xml
* invoice-template-fa-3-pef.xml
* invoice-template-fa-3-pef-correction.xml
* invoice-template-fa-3-pef-with-attachment.xml

**Uwaga:** Szablony mają wyłącznie charakter poglądowy i służą do testów technicznych biblioteki `ksef-client-csharp`.  
Nie są to gotowe faktury do użycia na środowisku produkcyjnym. 
Ich zadaniem jest pokazanie struktury dokumentu i umożliwienie weryfikacji poprawności integracji na środowisku testowym.

### Testowanie wysyłki do Aplikacji Podatnika

Jeżeli chcesz przetestować wysyłkę faktury przez Aplikację Podatnika (AP), zalecamy użyć szablonu:

**`invoice-template-fa-3.xml`**

Jest to najprostszy przykład faktury, który powinien przejść walidację w AP.

#### Wymagane modyfikacje przed wysłaniem w Aplikacji Podatnika

Aby faktura została zaakceptowana, trzeba zmienić w niej trzy rzeczy:

1. Pole P_1 (data wystawienia) – musi być ustawione na dzisiejszą datę.
2. Pole NIP dla Podmiotu1, z wartością „#nip#” – należy wpisać tam NIP podmiotu, 
   w kontekście którego jesteśmy uwierzytelnieni w Aplikacji Podatnika.
3. Pole P_2 - zastąpić #invoice_number# losowym ciągiem znaków

Po wprowadzeniu tych trzech zmian faktura powinna zostać poprawnie przyjęta przez AP.