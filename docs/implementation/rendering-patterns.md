# Rendering Patterns — czego nauczyły nas implementacje tooltipa i inventory

## 1. Batch rendering (anty-flicker)

Każde `Bar`/`Text`/`GetBlock`/`PutBlock` modyfikuje `screen.Data[]` i ustawia `screen.IsModified = true`.
W następnym `Draw()` (triggerowanym przez `WaitVbl`) MonoGame przebudowuje całą teksturę ekranu.
Bez batchowania seria redrawów wywołuje przebudowę po każdej operacji.

**Wzorzec:**

```csharp
screens.BeginBatch();
// wszystkie operacje rysowania ...
screens.EndBatch();
// w EndBatch wszystkie screeny dostają IsModified = true jednokrotnie
```

Podczas batcha `PutDataIntoScreen` nadal zapisuje do `screen.Data[]`, ale **nie ustawia** `IsModified`.
Działa to na poziomie `ScreensManager.FlagScreenModified()`.

## 2. GetBlock / PutBlock — overlay pattern

Do rysowania nakładek (tooltip, dialogi) które mają zniknąć:

```csharp
// Zapisz tło
screens.GetBlock(BLOCK_NR, x, y, width, height);
// Rysuj nakładkę
screens.Bar(x, y, x + w, y + h);
screens.Text(x + 4, y + baseline + 3, "...");
// Przywróć tło
screens.PutBlock(BLOCK_NR);
```

**Uwagi:**
- Bloki są per-screen — po `ScreenClose` znikają
- `PutBlock` zwraca błąd jeśli bloku nie ma — od wersji z null checkiem jest bezpieczny
- `GetBlock` z danym numerem nadpisuje istniejący blok (reuse)
- Używaj numerów bloków spoza zakresu 1-3 (używane przez `SKLEP_NAPISZ` etc.)

## 3. Font baseline — jak działa Text

```csharp
// Text(x, y, string) rysuje z baseline na pozycji y
// Znaki rozciągają się w górę o font.Baseline pikseli
var cy = y - font.Baseline;  // wewnętrzna implementacja
```

**Konsekwencje:**
- `Text(x+4, y+3, nazwa)` — baseline na y+3, znaki od y+3-baseline do y+3
- Żeby tekst pojawił się **wizualnie** N pikseli od góry ramki: `y + baseline + N`
- Baseline fontu: `screens.TextBase()` — zwraca aktualną wartość

**W tooltipie:**
```csharp
// ŹLE: znaki pojawiają się nad górną krawędzią
screens.Text(x + 4, y + 3, nazwa);

// DOBRZE: znaki są w ramce
screens.Text(x + 4, y + baseline + 3, nazwa);
```

## 4. Bar — inkluzywność

```csharp
screens.Bar(x1, y1, x2, y2);
// Rysuje (|x2-x1|+1) x (|y2-y1|+1) pikseli
```

Jeśli chcesz narysować prostokąt o wymiarach W×H i zapisać tło:
```csharp
// Bar rysuje o 1 piksel więcej w prawo i dół niż GetBlock zapisuje
screens.GetBlock(NR, x, y - baseline, W + 1, H + baseline + 1);
screens.Bar(x, y, x + W, y + H);  // rysuje W+1 × H+1 pikseli
```

## 5. Screen lifecycle — tworzenie i zamykanie

| Metoda | Co robi |
|--------|---------|
| `ScreenOpen(N, w, h, colors, mode)` | **Tworzy** nowy screen N, zastępując istniejący |
| `ScreenClose(N)` | **Usuwa** screen N z listy. Bloki giną. currentScreen → 0 |
| `ScreenDisplay(N, x, y, w, h)` | Tylko **konfiguruje** istniejący screen. Jeśli N nie istnieje — cichy return. |
| `Screen(N)` | Ustawia `currentScreen = N` |

**Pułapka:** po `ScreenClose(N)`, `ScreenDisplay(N, ...)` nic nie robi.
Zawsze używaj:
```csharp
screens.ScreenClose(N);
screens.ScreenOpen(N, w, h, ...);  // tworzy od nowa, daje świeży screen
screens.ScreenDisplay(N, x, y, w, h);
```

## 6. Null safety w PutBlock

`PutBlock` z `FirstOrDefault(b => b.Number == number)` zwraca null jeśli blok nie istnieje.
Wcześniejszy kod tego nie obsługiwał → `NullReferenceException`.

Zawsze:
```
if (block == null) return;  // w PutBlock
```

## 7. Tooltip positioning

Zamiast pozycjonować tooltip przy kursorze (może wychodzić poza ekran lub nachodzić na panele):

```csharp
int tx = 2;                         // lewa krawędź
int ty = (screenHeight - 52) / 2;   // pionowo na środku
```

## 8. WaitVbl — trigger rysowania

```csharp
screens.WaitVbl();  // blokuje do następnego VBL
```

`WaitVbl` używa `Invoke`, które przepuszcza pętlę MonoGame (`Draw → Invoke queue`).
Oznacza to: każde wołanie `WaitVbl` = jedna klatka renderowana. Jeśli między redrawami jest `WaitVbl`, user widzi stan pośredni.

**Zasada:** wszystkie redrawy muszą być kompletne przed `WaitVbl`.

## 9. Pipeline renderowania

```
Bar/Text/PutBlock
  → PutDataIntoScreen → screen.Data[] (piksel po pikselu)
  → screen.IsModified = true
  → następny Draw() → DrawScreens()
  → if (screen.IsModified) → Texture2DFromImageData(screen.Data)
  → spriteBatch.Draw(texture)
```

- `screen.Data` to zwykła tablica w RAM — modyfikacje są natychmiastowe
- `IsModified` to optymalizacja — texture nie jest przebudowywana bez potrzeby
- `UpdateDisplayRequested` — flag która nie jest konsumowana; można ją ignorować

## 10. Wzorzec: otwarcie okna

```csharp
screens.ScreenOpen(1, 320, 190, 32, PixelMode.Lowres);
screens.ReserveZone(100);
screens.ScreenHide();
screens.View();
screens.ScreenDisplay(1, x, y, 320, 190);

screens.BeginBatch();
// draw everything
screens.EndBatch();

screens.ScreenShow();
screens.View();

// main loop
while (true)
{
            if (screens.MouseClick() == 1) HandleClick();
            if (exit) break;
            screens.WaitVbl();
        }

        screens.ScreenClose(1);
        ```

## 11. Wzorzec: redraw regionu zamiast pełnego redrawu

`INVENTORY_NEW` stosuje podział na niezależne funkcje rysujące poszczególne panele (`DRAW_ROSTER`, `DRAW_UNIT`, `DRAW_GROUND`, `DRAW_STATS`), wywoływane selektywnie po zmianie stanu — zamiast jednej funkcji redraw-all.

```csharp
// Pełny redraw tylko raz, przy starcie:
INVENTORY_NEW_DRAW_BACKGROUND();   // raz — ramy + przyciski
INVENTORY_NEW_DRAW_ROSTER(...);
INVENTORY_NEW_DRAW_UNIT(...);
INVENTORY_NEW_DRAW_GROUND(...);
INVENTORY_NEW_DRAW_STATS(...);

// W pętli: redraw TEGO CO SIĘ ZMIENIŁO
if (zmieniono_postac) {
    INVENTORY_NEW_DRAW_ROSTER(...);
    INVENTORY_NEW_DRAW_UNIT(...);
    INVENTORY_NEW_DRAW_STATS(...);
}
if (zmutowano_ziemię) INVENTORY_NEW_DRAW_GROUND(...);
```

**Korzyść:** pojedynczy klik = jedna mała funkcja bar+text+bob, nie cały ekran. Mniej flickeru, szybciej.

**Klucz:** każda funkcja redraw **nadpisuje całą swoją strefę** (`Bar(tło)` na początku) — nie zakłada że poprzednia klatka jest poprawna, więc jest idempotentna.

## 12. GADGET i paleta WYBOR — reużywalne style

`GADGET` (`Legion.cs:1072`) ma 4 kolory i jeden对身体 stałą semantykę w całym kodzie staring od `WYBOR`:

| Kolor roli | Czym jest | Wartości standardowe |
|------------|-----------|---------------------|
| `K1` | zewnętrzny fill (rama) | 5=róż, 0=czarny, 8=ciemny |
| `K2` | polyline (obrys dolna+prawa) | 0, 5, 8 |
| `K3` | wewnętrzny fill (tło slotu) | 19=jasny, 0, 16=niebieski, 8=ciemny |
| `K4` | kolor tekstu (gdy `TX_S` niepuste) | 1, 3, 16, 20, 31 |

**Stałe kombinacje występujące w WYBOR/INVENTORY_NEW:**
- **Slot papierdoll głowa:** `K1=5, K2=5, K3=19, K4=19` (jasny jak sylwetka postaci)
- **Slot plecak/ziemia/ręce:** `K1=0, K2=5, K3=0, K4=16` (ciemny z czerwoną obrysówką)
- **Przycisk (< >):** `K1=5, K2=0, K3=8, K4=1`
- **Panel główny:** `K1=5, K2=0, K3=8, K4=8` (ciemny)
- **Panel postaci/stats:** `K1=0, K2=5, K3=19, K4=19` (jasny)

**Lekcja:** przy tworzeniu nowego ekranu GUI w stylu WYBOR **nie wymyślaj kolorów od zera** — wybierz kombinację z tej tabeli wg semantyki slotu.

## 13. Etykiety slotów — tekst czy gołe sloty?

`WYBOR` używa `TX_S=""` dla slotów papierdoll/plecaku — postać widać z grafiki sylwetki `PasteBob(19,10, GOBY+38)`, a item z bob-a. `INVENTORY_NEW` początkowo miało etykiety `"H"`, `"C"`, `"L"`, `"LHand"`, `"RHand"` które były niebieskie (`K4=16`), nie-suportowane w oryginale i bały u日益 w composite.

**Lekcja:** w gui typu paperdoll slot **bez etykiety + z grafiką postaci/boba** jest bardziej klimatyczny niż słowna etykieta. Jeśli bardzo chcesz oznaczenie — użyj bob-a ikonki (np. "bob86"/"bob42" jak w WYBOR) zamiast tekstu.

## 14. Kolor 16 (niebieski) a konwencje

W palecie Legia kolor 16 = niebieski. W `WYBOR_WYPISZ` używa się go dla **wartości** statystyk na tle `19` (`Ink(16, 19)`), co jest czytelne i klimatyczne. Ale ten sam kolor na grupie innych miejsc wygląda „komputerowo":

- Nagłówki ("ZIEMIA", "PLECAK:") na kolorze 16 wyglądają dziwnie — lepszy **kolor 3** (jasny/nagłówkowy)
- Nazwy postaci/ras na `19` → **kolor 3** (jak w `WYBOR_WYPISZ`)
- Etykiety statów ("Energia:", "Siła:") → **kolor 3**
- Wartości statów → **kolor 16** (roboczy + klimatyczny)
- Obciążenie/Waga przy przekroczeniu → **kolor 20** (czerwony=alarm)

**Lekcja:** kolor 16 rezerwuj dla wartości liczbowych; nagłówki i etykiety → kolor 3.

## 15. Usuwanie redundancji zamiast ukrywania

Pierwotne `INVENTORY_NEW` pokazywało `"Waga:{waga}"` na dole środkowego panelu **oraz** "Obciążenie" w panelu statsów. To samo dwa miejsca = zmieszanie hierarchii informacji.

**Lekcja:** jeśli równolegle istnieją dwa miejsca pokazujące to samo — usuń jedno z nich (zwykle to „mniejsze", mniej strukturalne). Nie staraj się to chować warstwowo — usuń kod.

## 16. Obramowania Box vs prosty Bar

`WYBOR_WYPISZ` rysuje panel statów jako sam `Ink(19,19); Bar(...)` (jasny fill) BEZ obramowania. `INVENTORY_NEW` początkowo dawało dodatkowo `Ink(5); Box(...)`. Efekt był cięższy wizualnie niż w oryginale.

**Lekcja:** w gui Amos/WYBOR liretyczny fill pokryty jednym `Bar` jest kanoniczny. Obramowania `Box` należy rezerwować dla specyficznych podziałów (np. obramowanie eksternego okna) — nie dla każdego sub-panelu.

## 17. Wzorzec: bob bank state — save/restore

`INVENTORY_NEW` tymczasowo ładuje boby (`_LOAD("dane/gad", 0)`, `_LOAD("dane/glowny", 1)`) bo muszą być dostępne w menu inventory, ale są obce mapie/armii. Po wyjściu **przywraca** stan aby nie zanieczyścić pamięci boba:

```csharp
int savedBobCount = screens.GetBobCount();
int savedGoby = GOBY;
_LOAD("dane/gad", 0);
_LOAD("dane/glowny", 1);
GOBY = savedBobCount;
// ... main loop ...
screens.ScreenClose(1);
screens.TrimBobs(savedBobCount);   // usuń tylko tymczasowe boby
GOBY = savedGoby;
```

**Lekcja:** gdy GUI potrzebuje grafik których reszta gry nie wymaga w tym samym momencie: (1) zapisz ilość bobów, (2) załaduj dodatkowe, (3) przy zamknięciu trim od `savedBobCount` + zwróć stan globalny.

## 18. Roster 10 postaci vs przyciski przewijania

`WYBOR` ma jedną widoczną postać + `GADGET(< >)` zmieniający NUMER (+ były scroll do następnej żywej). `INVENTORY_NEW` ma zamiast tego **listę 10 postaci** z zaznaczeniem (`DRAW_ROSTER`).

Oba style są poprawne; wybór to trade-off:
- **Przyciski < >:** oszczędność miejsca, ale wymaga klikania aby zobaczyć kim jest 5-ta postać
- **Lista 10:** więcej miejsca, ale od razu widać kogo masz / HP / nazwę

`INVENTORY_NEW` celowo wybrał listę (roster) by informacja była naskorowidlowana kompresji — lista 10 zajmuje mniej ekranu niż pokazywanie sylwetki każdej postaci w osobnym oknie.

**Lekcja:** wzorzec WYBOR nie jest religiijny; nowa funkcja może świadomie odchodzić od niego jeśli służy innej wartości użytkowej. Ale **palette/kolory/GADGET style** zostawiaj zgodne.

## 19. Współrzędne: screen lokalny vs monitor — lekcja z tooltipa WYBOR

Piszesz nakładkę (tooltip, dialog, ramkę) na istniejący ekran — i „się nie rysuje". W większości przypadków nie ma żadnego błędu: rysunek trafia w złe miejsce i jest przykryty przez inny ekran.

**Zasada 1 — rysunek trafia do bitmapy AKTUALNEGO screena, w jego współrzędnych LOKALNYCH.**

Screen ma własny układ współrzędnych (0,0 = lewy-górny róg bitmapy), a jego pozycja na monitorze to osobna sprawa:
- `ScreenDisplay(N, x, y, w, h)` — gdzie na monitorze leży screen (`screen.X`/`screen.Y`)
- `OffsetX`/`OffsetY` — scroll (przesunięcie widocznego okna w obrębie bitmapy)

Screen 0 w grze **nie jest** wyświetlany od (0,0): `SETUP` robi `ScreenDisplay(0, 130, 40, 320, 234)`, a mapa scrolluje się przez `ScreenOffset`. Twarde współrzędne `(100,100)` na screen 0 lądują na monitorze (230,140), a nie (100,100) — dokładnie to przydarzyło się tooltipowi WYBOR.

**Zasada 2 — `XScreen`/`YScreen` przeliczają wg AKTUALNEGO screena i kompensują scroll.**

```
XScreen(x) = x - screen.X + OffsetX
YScreen(y) = y - screen.Y + OffsetY
```

Konsekwencje:
- Przelicznik używa screena aktualnego w momencie wywołania — najpierw `Screen(N)`, potem konwersja.
- Wynik „goni" scroll: narysowanie w przeliczonych współrzędnych daje **stałą pozycję monitora** niezależnie od `OffsetX`/`OffsetY`.

Wzorzec — tooltip nad oknem WYBOR (screen 0 ma X=130, Y=40; okno WYBOR startuje na monitor Y=162):

```csharp
screens.Screen(0);
int tx = screens.XScreen(230); // monitor X = 130 + 100
int ty = screens.YScreen(110); // monitor Y = 162 - 52
DRAW_TOOLTIP(item, tx, ty, true);
screens.Screen(1);
```

**Zasada 3 — z-order: screen późniejszy w liście = na wierzchu.**

`DrawScreens` (`LegionGame.cs:220`) rysuje screeny w kolejności listy `ScreensManager.Screens`. Nakładka narysowana na screenie „pod" innym ekranem nie generuje żadnego błędu — jest po prostu niewidoczna. To najczęstsza przyczyna „się nie rysuje" bez wyjątków i logów.

**Checklista niewidocznej nakładki:**

1. Który screen jest `current` w momencie rysowania? (sprawdź `Screen(N)` przed rysowaniem)
2. Gdzie ten screen jest wyświetlany? (`ScreenDisplay` — X/Y, `DisplayWidth`/`DisplayHeight`)
3. Co go przykrywa? (kolejność listy `Screens` — ostatni = na wierzchu; okna UI to zwykle screen 1/2)
4. Czy `OffsetX`/`OffsetY` są niezerowe? (scroll mapy — bez `XScreen`/`YScreen` pozycja „dryfuje")
5. Czy nakładka mieści się w widocznym obszarze screena?
