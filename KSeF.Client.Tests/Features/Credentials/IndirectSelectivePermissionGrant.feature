# language: pl
Funkcja: Nadanie uprawnień w sposób pośredni selektywny
  Jako pośrednik posiadający uprawnienia z delegacją od wielu podmiotów
  Chcę nadawać uprawnienia pośrednie w sposób selektywny dla konkretnych podmiotów
  Aby podmiot końcowy miał dostęp tylko do wybranych kontekstów

  @smoke
  Scenariusz: Nadanie selektywnych pośrednich uprawnień przez pośrednika dla podmiotów końcowych w różnych kontekstach
    Zakładając, że istnieją dwa podmioty gospodarcze A i B
    Oraz każdy z tych podmiotów nadał pośrednikowi uprawnienia 'InvoiceRead' i 'InvoiceWrite' z możliwością delegacji
    Oraz pośrednik został uwierzytelniony w systemie KSeF
    Jeżeli pośrednik nada w sposób pośredni selektywne uprawnienia 'InvoiceRead' i 'InvoiceWrite' dla podmiotu końcowego o identyfikatorze PESEL w kontekście podmiotu A
    Oraz pośrednik nada w sposób pośredni selektywne uprawnienia 'InvoiceRead' i 'InvoiceWrite' dla podmiotu końcowego o identyfikatorze NIP w kontekście podmiotu B
    Wtedy podmiot końcowy PESEL będzie mógł zalogować się w kontekście podmiotu A
    Oraz podmiot końcowy PESEL będzie posiadał dokładnie dwa uprawnienia: 'InvoiceRead' i 'InvoiceWrite' w kontekście A
    Oraz podmiot końcowy PESEL NIE będzie mógł zalogować się w kontekście podmiotu B
    I również podmiot końcowy NIP będzie mógł zalogować się w kontekście podmiotu B
    Oraz podmiot końcowy NIP będzie posiadał dokładnie dwa uprawnienia: 'InvoiceRead' i 'InvoiceWrite' w kontekście B
    Oraz podmiot końcowy NIP NIE będzie mógł zalogować się w kontekście podmiotu A
