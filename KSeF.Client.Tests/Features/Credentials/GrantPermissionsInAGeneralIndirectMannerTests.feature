@smoke
Scenariusz: Nadanie pośrednich uprawnień przez dwa podmioty pośrednikowi i dalszemu podmiotowi końcowemu
Zakładając, że istnieją dwa podmioty gospodarcze A i B
Oraz każdy z tych podmiotów nadał pośrednikowi uprawnienia 'InvoiceRead'
Oraz pośrednik został uwierzytelniony w systemie KSeF
Jeżeli pośrednik nada w sposób pośredni uprawnienia 'InvoiceRead' dla podmiotu końcowego o identyfikatorze PESEL bądź NIP
Wtedy podmiot końcowy (zarówno PESEL jak i NIP)
będzie mógł zalogować się w kontekście podmiotu A
oraz posiadać dokładnie jedno uprawnienie 'InvoiceRead'
I również podmiot końcowy będzie mógł zalogować się w kontekście podmiotu B
oraz posiadać dokładnie jedno uprawnienie 'InvoiceRead'