# Serializacja XML modeli KSeF

Opcjonalny pakiet do serializacji i deserializacji modeli, żądań i odpowiedzi z `KSeF.Client.Core` do XML.

Pakiet stanowi osobną warstwę umożliwiającą zapis lub odczyt obiektów KSeF w formacie XML.

## Instalacja

```bash
dotnet add package KSeF.Client.XmlSerialization
```

## Użycie

### Serializacja do stringa

```csharp
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.XmlSerialization;

IXmlModelSerializer serializer = new XmlModelSerializer();

string xml = serializer.Serialize(sessionInvoice);
```

### Deserializacja ze stringa

```csharp
SessionInvoice invoice = serializer.Deserialize<SessionInvoice>(xml);
```

### Serializacja do strumienia

```csharp
serializer.Serialize(stream, sessionInvoice);

stream.Position = 0;
SessionInvoice invoice = serializer.Deserialize<SessionInvoice>(stream);
```

## Konfiguracja

Opcje serializacji można przekazać przez `XmlSerializationOptions`.

```csharp
IXmlModelSerializer serializer = new XmlModelSerializer(new XmlSerializationOptions
{
    RootElementName = "SessionInvoice",
    Namespace = "urn:ksef:models",
    Indent = true,
    Encoding = Encoding.UTF8,
    EmitNullValues = false,
    CollectionItemElementName = "Item",
    DictionaryItemElementName = "Item",
    DictionaryKeyElementName = "Key",
    DictionaryValueElementName = "Value",
    FormatCulture = CultureInfo.InvariantCulture
});
```

Najważniejsze opcje:

- `RootElementName` - własna nazwa elementu głównego
- `Namespace` - przestrzeń nazw XML
- `Indent` - formatowanie XML z wcięciami
- `Encoding` - kodowanie dokumentu
- `EmitNullValues` - zapis wartości `null` jako `xsi:nil="true"`
- `CollectionItemElementName` - nazwa elementu dla pozycji kolekcji
- `DictionaryItemElementName`, `DictionaryKeyElementName`, `DictionaryValueElementName` - nazwy elementów dla słowników
- `FormatCulture` - kultura używana przy zapisie liczb i dat

## Obsługiwane typy

Serializer obsługuje modele KSeF, które są problematyczne dla standardowego `XmlSerializer`.

### Uri

Wartości `Uri` są zapisywane jako tekst:

```xml
<UpoDownloadUrl>https://example.test/upo/123</UpoDownloadUrl>
```

### Kolekcje

Kolekcje, w tym `ICollection<T>`, są zapisywane jako lista elementów:

```xml
<Details>
  <Item>Duplikat faktury. Faktura o numerze KSeF: 3400864125-20260707-64B07F400000-AE została już prawidłowo przesłana do systemu w sesji: 20260707-SO-2A4C4B6000-DC79898348-43</Item>
</Details>
```

Podczas deserializacji właściwości typu `ICollection<T>` są domyślnie odtwarzane jako `List<T>`.

### Słowniki

Słowniki, w tym `IDictionary<TKey, TValue>`, są zapisywane jako lista par klucz-wartość:

```xml
<Extensions>
  <Item>
    <Key>originalSessionReferenceNumber</Key>
    <Value>20260707-SO-2A4C4B6000-DC79898348-43</Value>
  </Item>
  <Item>
    <Key>originalKsefNumber</Key>
    <Value>3400864125-20260707-64B07F400000-AE</Value>
  </Item>
</Extensions>
```

Podczas deserializacji właściwości typu `IDictionary<TKey, TValue>` są domyślnie odtwarzane jako `Dictionary<TKey, TValue>`.

### Tablice bajtów

Właściwości `byte[]` są zapisywane jako tekst Base64:

```xml
<CipherKey>AQIDBA==</CipherKey>
```

## Wartości null i puste kolekcje

Domyślnie właściwości o wartości `null` są pomijane.

Po ustawieniu `EmitNullValues = true` wartości `null` są zapisywane jako elementy z atrybutem `xsi:nil="true"`.

Puste kolekcje i słowniki są zapisywane jako puste elementy kontenerów, np.:

```xml
<Details />
```

Brakujący element XML pozostawia właściwości referencyjne i nullable jako `null`. Dla typów wartościowych bez `Nullable<T>` pozostaje wartość domyślna.

## Błędy deserializacji

Niepoprawny adres URI powoduje `XmlModelSerializationException`.

Zduplikowany klucz w słowniku również powoduje `XmlModelSerializationException`.

## Ograniczenia

Serializer jest przeznaczony dla publicznych DTO z `KSeF.Client.Core`, czyli modeli, żądań i odpowiedzi z publicznymi właściwościami `get; set;`.

Nie jest zamiennikiem dla dowolnego serializera grafów obiektów .NET. Nie obsługuje pełnej polimorfii dla właściwości interfejsowych, cykli referencji, prywatnych pól ani pełnej semantyki atrybutów XML z `System.Xml.Serialization`.

Jeżeli dany payload ma już własny serializer protokołowy, należy nadal używać tej dedykowanej ścieżki. Przykład: `AuthenticationTokenRequest.SerializeToXmlString()` zachowuje wymagany element główny, namespace, wartości enumów i specjalny kształt identyfikatora kontekstu.

Serializacja JSON oraz działanie klientów API pozostają bez zmian.
