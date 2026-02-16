# language: pl
Potrzeba biznesowa: Operacje na sesji wsadowej

@smoke @Batch @regresja
Scenariusz: Wysłanie dokumentów w jednoczęściowej paczce
  Zakładając, że mam 5 faktur z poprawnym NIP
  Jeżeli wyślę je w jednoczęściowej zaszyfrowanej paczce
  Wtedy wszystkie faktury powinny być przetworzone pomyślnie
  Oraz powinienem móc pobrać UPO

@smoke @Batch @regresja @Negative
Scenariusz: Wysłanie jednoczęściowej paczki dokumentów ze złym NIP
  Zakładając, że mam 5 faktur z niepoprawnym NIP
  Jeżeli wyślę je w jednoczęściowej zaszyfrowanej paczce
  Wtedy proces powinien zakończyć się błędem 445
  Oraz wszystkie faktury powinny być odrzucone

@Batch @regresja @Negative
Scenariusz: Przekroczenie limitu liczby faktur
  Zakładając, że mam 10001 faktur
  Jeżeli wyślę je w paczce
  Wtedy system powinien zwrócić błąd 420

@Batch @regresja @Negative
Scenariusz: Przekroczenie maksymalnego rozmiaru całej paczki
  Zakładając, że przygotowałem paczkę o deklarowanym rozmiarze większym niż 5 GiB
  Jeżeli spróbuję nawiązać sesję wsadową
  Wtedy system odrzuci żądanie ze względu na przekroczenie limitu fileSize

@Batch @regresja @Negative
Scenariusz: Przekroczenie rozmiaru paczki
  Zakładając, że mam paczkę o rozmiarze 101 MiB
  Jeżeli spróbuję otworzyć sesję wsadową
  Wtedy system powinien odrzucić żądanie

@Batch @regresja @Negative
Scenariusz: Zamknięcie sesji bez wysłania wszystkich części
  Zakładając, że zadeklarowałem 3 części paczki
  Jeżeli wyślę tylko 1 część
  Wtedy system powinien odrzucić próbę wysłania

@Batch @regresja @Negative 
Scenariusz: Przekroczenie limitu liczby części
  Zakładając, że zadeklarowałem 51 części paczki
  Jeżeli spróbuję otworzyć sesję wsadową
  Wtedy system powinien odrzucić żądanie

@Batch @regresja @Encryption @Negative
Scenariusz: Nieprawidłowy zaszyfrowany klucz
  Zakładając, że mam paczkę z uszkodzonym kluczem szyfrowania
  Jeżeli wyślę paczkę i zamknę sesję
  Wtedy system powinien zwrócić błąd 415

@Batch @regresja @Encryption @Negative
Scenariusz: Uszkodzone zaszyfrowane dane
  Zakładając, że mam paczkę z uszkodzonymi danymi
  Jeżeli wyślę paczkę i zamknę sesję
  Wtedy system powinien zwrócić błąd 405

@Batch @regresja @Encryption @Negative
Scenariusz: Nieprawidłowy wektor inicjujący
  Zakładając, że mam paczkę z nieprawidłowym IV
  Jeżeli wyślę paczkę i zamknę sesję
  Wtedy system powinien zwrócić błąd 430
