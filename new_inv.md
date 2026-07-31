# Plan poprawy wyglądu nowego inventory

## Cel

Uczytelnić `INVENTORY_NEW`, zachowując jego charakter klasycznego interfejsu Amiga/AMOS, obecną rozdzielczość `320×190` oraz istniejące działanie ekwipowania, plecaka, ziemi i drag-and-drop.

Najważniejsze cele wizualne:

- od razu widzieć, która postać jest wybrana i jaki ma stan;
- rozróżniać wyposażenie, plecak, przedmioty na ziemi i statystyki;
- ograniczyć wrażenie przypadkowego rozmieszczenia tekstów i ramek;
- zapewnić czytelny feedback podczas najechania, zaznaczenia i przeciągania przedmiotu;
- utrzymać spójną paletę i styl z ekranem `WYBOR`.

## Stan obecny i główne problemy

Punktem wyjścia jest `src/AmigaNet.Legion/AmigaNet.Legion/LegionInventoryNew.cs`:

1. Ekran ma trzy kolumny, ale lewa lista drużyny jest praktycznie „wolną przestrzenią”, bez wyraźnego panelu i bez mocnego podziału na nazwę, HP oraz stan postaci (`INVENTORY_NEW_DRAW_BACKGROUND`, `INVENTORY_NEW_DRAW_ROSTER`).
2. Środkowy panel łączy paperdoll i plecak, lecz nie pokazuje wystarczająco wyraźnie sylwetki ani znaczenia pustych slotów. Wszystkie sloty są podobne wizualnie, więc hierarchia wyposażenia i plecaka jest słaba.
3. Prawy panel zawiera ziemię, stronicowanie, statystyki i dwa przyciski. Przy takiej ilości treści łatwo traci się podział na sekcje (`INVENTORY_NEW_DRAW_GROUND`, `INVENTORY_NEW_DRAW_STATS`).
4. Kolory są w większości zgodne z `WYBOR`, ale przyda się konsekwentny podział: kolor nagłówków/etykiet, kolor wartości, kolor zaznaczenia i kolor ostrzeżeń.
5. Informacja o przedmiocie pojawia się głównie podczas drag-and-drop. Brakuje lekkiego, przewidywalnego podglądu przedmiotu przy zwykłym najechaniu.
6. Przyciski przewijania ziemi nie pokazują jasno, czy są aktywne, a użytkownik nie widzi numeru strony ani liczby przedmiotów.
7. Stan pustego slotu, poprawnego celu upuszczenia i niepoprawnego celu nie jest dostatecznie widoczny.

Dokument `docs/implementation/rendering-patterns.md` potwierdza właściwe kierunki: używanie standardowych kombinacji `GADGET`, kolor 16 dla wartości, kolor 3 dla etykiet, kolor 20 dla alarmów oraz selektywny redraw regionów.

## Docelowa hierarchia ekranu

Układ trzech kolumn zostaje, ale każda z nich powinna mieć jedną główną funkcję:

```text
┌──────────────┬──────────────────┬────────────────┐
│ DRUŻYNA       │ POSTAĆ            │ ZIEMIA         │
│ lista 1–10    │ nazwa + paperdoll │ grid + strona  │
│ HP / status   │                   ├────────────────┤
│               │ PLECAK            │ STATYSTYKI     │
│               │ 4 × 2 sloty       │ etykiety/wartości│
│               │                   ├────────────────┤
│               │                   │ AUTO  ZAMKNIJ  │
└──────────────┴───────────────────┴────────────────┘
```

Nie chodzi o dodanie większej liczby dekoracji. Priorytetem jest czytelny rytm: nagłówek → zawartość → podsumowanie/akcje.

## Faza 0 — przygotowanie i punkt odniesienia

1. Uruchomić ekran w języku polskim i angielskim oraz wykonać zrzuty dla kilku stanów: pusta drużyna, pełny ekwipunek, przedmioty na ziemi, przeciążona postać.
2. Spisać dostępne boby dla sylwetki i ewentualnych ikon pustych miejsc. Nie zakładać nowych grafik, dopóki nie zostanie potwierdzone, że istnieją w banku bobów.
3. W jednym miejscu zdefiniować stałe layoutu: szerokości kolumn, pozycje nagłówków, rozmiary slotów, pozycje przycisków oraz kolory semantyczne.

**Rezultat:** mamy referencyjne obrazy i stały język wizualny, dzięki czemu kolejne zmiany można porównywać zamiast oceniać je wyłącznie „na oko”.

## Faza 1 — szybkie poprawki wizualne

### 1.1. Uporządkowanie nagłówków

- Zmienić `LEGION (10)` na krótszy i jednoznaczny nagłówek, np. `DRUŻYNA`.
- W środkowym panelu rozdzielić nazwę postaci od zawartości: nazwa jako nagłówek, a pod nią paperdoll.
- W prawym panelu pokazywać `ZIEMIA` wraz z licznikiem, np. `ZIEMIA 3/9` lub `ZIEMIA 3 PRZEDM.`.
- Dodać mały status strony przy strzałkach, np. `1/2`; jeśli mieści się w dostępnej przestrzeni.

### 1.2. Ujednolicenie palety

Stosować istniejącą semantykę `GADGET`, zamiast wprowadzać nowe kombinacje kolorów:

| Rola | Kolor | Zastosowanie |
|---|---:|---|
| tło ekranu | `0` | puste tło poza panelami |
| panel ciemny | `8` | ziemia, przyciski, elementy drugorzędne |
| panel jasny | `19` | aktywna postać, stats, zaznaczony wiersz |
| etykiety/nagłówki | `3` | nazwy sekcji i opisy statystyk |
| wartości | `16` | HP, statystyki, licznik strony, ceny |
| alarm | `20` | przeciążenie, krytycznie niski HP, błąd akcji |
| aktywne zaznaczenie | `31` | tekst aktualnie wybranej postaci/przycisku |

W szczególności nie używać koloru 16 do nagłówków oraz nie używać jaskrawego koloru wartości tylko po to, aby przyciągnąć uwagę.

### 1.3. Lżejsze ramki

- Zachować ramkę dla trzech głównych paneli.
- Wewnątrz paneli preferować wypełnienia `Bar` i krótkie separatory zamiast kolejnych ciężkich ramek `Box`.
- Pusty slot powinien być widoczny, ale nie konkurować z przedmiotem: ciemne tło + subtelny obrys.

**Kryterium akceptacji:** po jednym spojrzeniu widać trzy obszary funkcjonalne, ale ekran nie wygląda jak zbiór wielu zagnieżdżonych okien.

## Faza 2 — lewa kolumna: czytelna lista drużyny

1. Nadać lewej kolumnie dyskretne tło/panel albo stałą linię podziału, bez dodawania dużej ramki.
2. Ustalić stały format wiersza:
   - numer postaci;
   - skrócona nazwa;
   - HP jako wartość lub krótki pasek;
   - opcjonalny symbol stanu, jeśli nie zabiera miejsca.
3. Dla aktywnej postaci zastosować wyraźne, ale jednoznaczne zaznaczenie całego wiersza oraz marker `>`.
4. Postacie nieaktywne/puste wyszarzyć i wyraźnie odróżnić od aktywnych, ale nie pozwalać, by wyglądały jak możliwe do wybrania.
5. Dłuższe nazwy skracać przewidywalnie, np. przez limit znaków lub elipsę; nie dopuszczać do nachodzenia tekstu na HP.
6. Opcjonalnie użyć koloru 20 dla krytycznie niskiego HP, po ustaleniu progu w logice gry.

**Kryterium akceptacji:** nazwa, HP i aktywne zaznaczenie są czytelne także dla 10. postaci i dla nazw maksymalnej długości.

## Faza 3 — środkowa kolumna: paperdoll i plecak

### 3.1. Paperdoll

- Dodać sylwetkę postaci albo delikatne znaczniki miejsc, jeśli odpowiednie boby są dostępne.
- Zachować pusty slot bez tekstowych skrótów typu `H`, `C`, `LHand`; w tym interfejsie grafika i położenie są czytelniejsze niż etykiety.
- Rozróżnić wizualnie sloty wyposażenia od plecaka: wyposażenie może używać jaśniejszego tła lub większego odstępu, plecak pozostaje regularną siatką.
- Dla każdego miejsca zachować stałe położenie i identyczny sposób centrowania boba.

### 3.2. Plecak

- Dodać mały nagłówek `PLECAK` nad siatką, o ile nie wymusi ścisku z paperdollem.
- Utrzymać siatkę `4×2`, ale zwiększyć wizualny odstęp między paperdollem a plecakiem zamiast zwiększać sloty ponad rozmiar dostępnych ikon.
- Pusty slot powinien mieć ten sam obrys co slot z przedmiotem, ale ciemniejsze wnętrze.
- W stanie hover podświetlać tylko slot, nie całą sekcję.

### 3.3. Podgląd przedmiotu

- Wprowadzić podgląd przy najechaniu na przedmiot, oparty na istniejącym wzorcu tooltipa.
- Tooltip powinien zawierać: nazwę, typ, najważniejsze statystyki i wagę/cenę zależnie od kontekstu.
- Pozycjonować go w bezpiecznym, stałym obszarze, aby nie wychodził poza ekran i nie zasłaniał celu drag-and-drop.
- Nie dublować informacji o wadze w kilku miejscach; obciążenie postaci pozostaje w panelu statystyk.

**Kryterium akceptacji:** użytkownik może rozpoznać przedmiot bez rozpoczynania przeciągania, a tooltip nie niszczy układu po zmianie slotu.

## Faza 4 — prawa kolumna: ziemia, statystyki i akcje

### 4.1. Grid ziemi

- Zostawić układ `2×4`, ale dodać licznik przedmiotów oraz numer strony.
- Strzałkę niedostępną na pierwszej/ostatniej stronie rysować w przygaszonym wariancie i wyłączyć jej strefę kliknięcia albo jasno oznaczyć brak akcji.
- Puste sloty drugiej strony powinny wyglądać identycznie jak puste sloty pierwszej strony.
- Po zmianie strony odświeżać wyłącznie region ziemi.

### 4.2. Statystyki

- Zachować układ dwóch kolumn: etykieta po lewej, wartość wyrównana do prawej.
- Utrzymać kolor 3 dla etykiet i 16 dla wartości.
- Wyróżniać wyłącznie stany wymagające reakcji: przeciążenie kolorem 20, ewentualnie niski HP.
- Dodać krótką linię podziału między ziemią a statystykami; nie obudowywać każdej statystyki osobną ramką.
- Rozważyć prosty pasek `obciążenie / limit`, jeśli mieści się bez zmniejszania czytelności liczb.

### 4.3. Przyciski

- Ujednolicić wysokość, obrys, kolory i sposób wyrównania tekstu.
- Zachować `AUTO`/`Auto` jako skrót tylko wtedy, gdy w tooltipie lub opisie ekranu jest jasne, że chodzi o automatyczne założenie przedmiotów; w przeciwnym razie użyć krótkiego polskiego `ZAŁÓŻ`.
- Zostawić `Zamknij`, ale sprawdzić, czy tekst nie jest zbyt blisko krawędzi przy aktualnym foncie.
- Dodać stan aktywnego wciśnięcia oraz stan niedostępności, jeśli auto-equip nie ma nic do zrobienia.

## Faza 5 — feedback interakcji

1. Hover na postaci, slocie, strzałce i przycisku powinien mieć osobny, subtelny stan wizualny.
2. Podczas drag-and-drop:
   - podświetlać poprawne sloty jako możliwe cele;
   - odróżniać slot zajęty, który spowoduje zamianę, od slotu niedozwolonego;
   - zachować widoczny przedmiot pod kursorem i czytelny tooltip;
   - po niepoprawnym upuszczeniu przywrócić przedmiot oraz wyczyścić podświetlenie.
3. Po auto-equip pokazać krótkie potwierdzenie w stałym obszarze komunikatu, bez zasłaniania statystyk.
4. Sprawdzić, czy kursor systemowy i sprite przeciąganego przedmiotu nie są widoczne jednocześnie; wykorzystać istniejące `HideOn()`/`ShowOn()`.

## Faza 6 — uporządkowanie implementacji pod wygląd

Zmiany wizualne powinny być wsparte prostą strukturą kodu:

- stałe pozycji i kolorów zamiast powtarzania liczb w wielu funkcjach;
- osobne funkcje `DRAW_*` dla nagłówków, rosteru, paperdolla, plecaka, ziemi, statystyk i tooltipa;
- każdy redraw zaczynać od wyczyszczenia całego własnego regionu;
- wszystkie grupy operacji rysowania wykonywać w `BeginBatch()`/`EndBatch()`;
- po zmianie postaci odświeżać roster + jednostkę + statystyki, a po zmianie ziemi tylko grid ziemi;
- nie zmieniać modelu danych `ARMIA`, `GLEBA` ani `BRON` w ramach prac nad wyglądem.

Źródłem wzorców pozostaje `docs/implementation/rendering-patterns.md`, szczególnie sekcje 11–16 i 18.

## Kolejność wdrażania

1. **Szybki polish:** nagłówki, paleta, odstępy, przyciski, licznik strony i liczba przedmiotów.
2. **Czytelność danych:** roster, status HP, paperdoll/sylwetka, rozdzielenie wyposażenia i plecaka.
3. **Informacja kontekstowa:** tooltip hover, stany pustych slotów, podświetlenia poprawnych celów.
4. **Polerowanie techniczne:** stałe layoutu, redraw regionów, batchowanie i usunięcie powtarzalnych fragmentów rysowania.
5. **Weryfikacja wizualna:** porównanie zrzutów referencyjnych i testy wszystkich stanów.

## Kryteria końcowe

Prace można uznać za zakończone, gdy:

- aktywna postać jest widoczna natychmiast po otwarciu ekranu;
- każda sekcja ma jeden jasno rozpoznawalny cel;
- nazwy, HP, statystyki i liczniki nie nachodzą na siebie przy skrajnych danych;
- paleta jest spójna z `WYBOR`: nagłówki/etykiety `3`, wartości `16`, alarmy `20`;
- puste, zajęte, zaznaczone i niedozwolone sloty są rozróżnialne;
- przeciąganie daje informację o możliwym celu i nie zostawia artefaktów;
- ekran wygląda poprawnie dla pustej i pełnej ziemi, pierwszej i ostatniej strony, postaci przeciążonej oraz postaci nieaktywnej;
- działa wersja polska i angielska, a `dotnet build src/AmigaNet.Legion/AmigaNet.Legion.sln` przechodzi bez nowych błędów.

## Poza zakresem

- zmiana rozdzielczości lub przejście na nowy framework UI;
- przebudowa modelu przedmiotów i statystyk;
- tworzenie dużego zestawu nowych grafik bez wcześniejszej weryfikacji istniejących bobów;
- zmiana zasad auto-equip, drag-and-drop lub przechowywania przedmiotów.
