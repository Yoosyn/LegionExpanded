# Tooltip — implementacja podpowiedzi dla przedmiotów

Podpowiedzi (tooltip) dla przedmiotów w ekwipunku (`WYBOR`) i sklepie (`SKLEP_`).

---

## Założenia

- System renderowania to bitmapy (`screen.Data[]` — piksel po pikselu), **nie ma warstwy overlay**.
- Tooltip rysujemy bezpośrednio w bitmapę ekranu, zapisując uprzednio tło przez `GetBlock`.
- Gdy tooltip ma zniknąć, przywracamy tło przez `PutBlock` — dokładnie ten sam wzorzec co `SKLEP_NAPISZ`.
- Żadnych zmian w systemie renderowania, strukturach danych, ani w pętli głównej gry.

---

## Przegląd

| Krok | Opis |
|------|------|
| 1 | Dodaj metodę `DRAW_TOOLTIP` — zapis tła, rysowanie boxa i statystyk |
| 2 | Dodaj `CLEAR_TOOLTIP` — przywrócenie tła |
| 3 | W `WYBOR` — w głównej pętli `while(true)` dodaj hover detection |
| 4 | W `SKLEP_` — w pętli `do...while` dodaj hover detection |

---

## 1. Nowe metody (implementacja: `LegionSklep.cs`)

```csharp
// Blok dla tooltipa — używamy numeru spoza zakresu używanego przez istniejący kod
private const int TOOLTIP_BLOCK = 99;
private bool tooltipActive;
private int lastTooltipItem = -1;
private int lastTooltipZone = -1;
private int lastTooltipScreen = -1;
private int lastTooltipX = -1;
private int lastTooltipY = -1;

void DRAW_TOOLTIP(int itemId, int x, int y, bool showPrice = true)
{
    if (itemId <= 0) return;

    tooltipActive = true;
    lastTooltipScreen = screens.Screen();  // ZAPAMIĘTAJ screen — CLEAR_TOOLTIP wróci tu z PutBlock
    lastTooltipX = x;
    lastTooltipY = y;

    const int W = 120;
    int H = showPrice ? 52 : 40;

    var baseline = screens.TextBase();

    // Zapisz tło (Bar jest inkluzywny o 1 px — patrz rendering-patterns.md §4)
    screens.GetBlock(TOOLTIP_BLOCK, x, y - baseline, W + 1, H + baseline + 1);

    // Tło tooltipa
    screens.Ink(0);
    screens.Bar(x, y, x + W, y + H);
    screens.Ink(19);
    screens.Bar(x + 1, y + 1, x + W - 1, y + H - 1);
    screens.Ink(0);
    screens.Bar(x + 2, y + 2, x + W - 2, y + H - 2);

    // Nazwa przedmiotu — baseline + N, żeby tekst był WIZUALNIE N px od góry ramki
    screens.Ink(31, 0);
    screens.Text(x + 4, y + baseline + 3, BRON_S[itemId]);

    // Typ i waga
    screens.Ink(16, 0);
    screens.Text(x + 4, y + baseline + 16, BRON2_S[BRON[itemId, B_TYP]]);
    screens.Text(x + 80, y + baseline + 16, "W:" + BRON[itemId, B_WAGA]);

    // Statystyki: Siła, Pancerz, Szybkość, Energia
    screens.Ink(20, 0);
    var stats = "S:" + BRON[itemId, B_SI] + " P:" + BRON[itemId, B_PAN]
              + " Sz:" + BRON[itemId, B_SZ] + " E:" + BRON[itemId, B_EN];
    screens.Text(x + 4, y + baseline + 29, stats);

    // Cena (w WYBOR pokazujemy — showPrice = true)
    if (showPrice)
    {
        screens.Ink(21, 0);
        screens.Text(x + 4, y + baseline + 42, "Cena: " + BRON[itemId, B_CENA]);
    }
}

void CLEAR_TOOLTIP()
{
    if (!tooltipActive) return;

    tooltipActive = false;
    var prevScreen = screens.Screen();
    screens.Screen(lastTooltipScreen);  // screen zapisany w DRAW_TOOLTIP
    try { screens.PutBlock(TOOLTIP_BLOCK); } catch { }
    screens.Screen(prevScreen);

    lastTooltipItem = -1;
    lastTooltipZone = -1;
    lastTooltipScreen = -1;
    lastTooltipX = -1;
    lastTooltipY = -1;
}
```

---

## 2. Hover detection — `WYBOR()` w `LegionWybor.cs`

Wewnątrz głównej pętli `while (true)` (linia 68), **na końcu pętli** (po bloku `if (screens.MouseKey() == PRAWY)`, ale przed `}` zamykającym pętlę), dodaj:

```csharp
// Tooltip: hover nad slotami
var zone = screens.MouseZone();
var item = 0;

if (zone >= 1 && zone <= 8)
    item = ARMIA[ARM, NUMER, TPLECAK + zone - 1];
else if (zone >= 9 && zone <= 12)
    item = GLEBA[SEK, zone - 9];
else if (zone >= 30 && zone <= 33)
    item = GLEBA[SEK, zone - 30 + 4];
else if (zone == 13) item = ARMIA[ARM, NUMER, TGLOWA];
else if (zone == 14) item = ARMIA[ARM, NUMER, TKORP];
else if (zone == 15) item = ARMIA[ARM, NUMER, TNOGI];
else if (zone == 16) item = ARMIA[ARM, NUMER, TLEWA];
else if (zone == 17) item = ARMIA[ARM, NUMER, TPRAWA];

if (item != lastTooltipItem || zone != lastTooltipZone)
{
    if (lastTooltipItem > 0) CLEAR_TOOLTIP();

    if (item > 0)
    {
        // STAŁA pozycja nad oknem WYBOR (monitor (230,110)), nie przy kursorze:
        // okno ma 320×140, tooltip 120×52. XScreen/YScreen kompensują pozycję
        // screena 0 (X=130, Y=40) i scroll mapy (OffsetX/OffsetY) — patrz
        // rendering-patterns.md §19 (wcześniej twarde (100,100) = bug, §5).
        screens.Screen(0);
        int tx = screens.XScreen(230); // monitor X = 130 + 100
        int ty = screens.YScreen(110); // monitor Y = 162 - 52
        DRAW_TOOLTIP(item, tx, ty, true);
        screens.Screen(1);
    }

    lastTooltipItem = item;
    lastTooltipZone = zone;
}
```

### Uwagi do WYBOR (stan po 2026-07-31)

- Tooltip rysowany jest na **screen 0** (mapa/pole bitwy), a nie na screen 1 — screen 1 (okno WYBOR) jest wyświetlany NAD screenem 0 (z-order, patrz `rendering-patterns.md` §19), więc rysowanie na screen 1 skończyłoby się przykryciem przez własne sloty okna.
- Pozycja jest **stała na monitorze**: (230,110)..(350,162) — okno WYBOR zaczyna się na monitor Y=162, więc tooltip jest w całości nad jego górną krawędzią.
- `XScreen`/`YScreen` przeliczają współrzędne **monitora** na lokalne **aktualnego** screena (tu: 0) i kompensują `OffsetX`/`OffsetY` (scroll mapy) — bez nich pozycja „dryfowałaby" po mapie.
- Ten sam wzorzec obowiązuje w `WYBOR_PICK` (drag) — tooltip trzymanego przedmiotu (`LegionWybor.cs:517`).
- `CLEAR_TOOLTIP` sama przełącza na screen zapisany w `DRAW_TOOLTIP` (`lastTooltipScreen` = 0), więc przywraca tło tam, gdzie rysowaliśmy — nie trzeba tego robić ręcznie.
- Przed wejściem w `WYBOR_PICK` (przeciąganie) wywołaj `CLEAR_TOOLTIP()` — pick może nadpisać obszar tooltipa, a po wyjściu z picka tooltip i tak zostanie odświeżony przez hover detection w następnej iteracji.

---

## 3. Hover detection — `SKLEP_()` w `LegionSklep.cs`

Wewnątrz pętli `do...while (!KONIEC)` (linia 104), **na końcu pętli** (po bloku `if (screens.Inkey_S() == "q" ...)`, ale przed `}` zamykającym `do`), dodaj:

```csharp
// Tooltip: hover nad slotami sklepu i plecaka
var zone = screens.MouseZone();
var item = 0;

if (zone > 9 && zone < 30)        // sklep: zonety 10-29
    item = SKLEP[SNR, zone - 10];
else if (zone > 39 && zone < 48)   // plecak: zonety 40-47
    item = ARMIA[A, NR, TPLECAK + zone - 40];

if (item != lastTooltipItem || zone != lastTooltipZone)
{
    if (lastTooltipItem > 0) CLEAR_TOOLTIP();

    if (item > 0)
    {
        // STAŁA pozycja na tle sklepu (screen 2 = 320×244) — lewy górny róg:
        // screen 2 ma X=130, Y=40, więc lokalne (2,96) = monitor (132,136).
        int tx = 2;
        int ty = (244 - 52) / 2;

        // W sklepie główna zawartość jest na screen 2
        screens.Screen(2);
        DRAW_TOOLTIP(item, tx, ty);
        screens.Screen(1);
    }

    lastTooltipItem = item;
    lastTooltipZone = zone;
}
```

### Uwagi do SKLEP_

- W sklepie główna zawartość jest na **screen 2** (320×244) — trzeba przełączyć się przed `DRAW_TOOLTIP`.
- Pozycja stała: lokalne (2, 96) na screen 2 = monitor (132, 136) — screen 2 jest wyświetlany od (130,40). Tu nie jest potrzebna konwersja `XScreen/YScreen`, bo współrzędne są już lokalne dla screena 2 (screen nie ma scrolla).
- Po narysowaniu tooltipa wracamy na screen 1 (dolny pasek).
- `CLEAR_TOOLTIP` sama przełącza na właściwy screen (zapisany przez `DRAW_TOOLTIP`), więc nie ma ryzyka przywrócenia tła na złym screenie.
- Przed `SKLEP_PICK` (kupno/sprzedaż) wywołaj `CLEAR_TOOLTIP()`.

---

## 4. Czyszczenie tooltipa

Wywołaj `CLEAR_TOOLTIP()` w tych miejscach:

| Miejsce | Kiedy |
|---------|-------|
| `WYBOR` — przed `WYBOR_PICK` | Przeciąganie przedmiotu może nadpisać obszar tooltipa |
| `WYBOR` — przed `break` z pętli | Wyjście z ekranu wyboru |
| `SKLEP_` — przed `SKLEP_PICK` | Przed zakupem/sprzedażą |
| `SKLEP_` — przed `SKLEP_OVER` / `KONIEC = true` | Wyjście ze sklepu |

```csharp
if (lastTooltipItem > 0) CLEAR_TOOLTIP();
```

`CLEAR_TOOLTIP` sama przełącza na właściwy screen, więc działa niezależnie od kontekstu.

---

## 5. Lekcja z buga — tooltip WYBOR niewidoczny (2026-07-31)

**Objaw:** tooltip w starym inventory (`WYBOR`) „się nie rysuje"; w sklepie (`SKLEP_`) działa.

**Przyczyna:** rysowanie na screen 0 z twardymi współrzędnymi lokalnymi:

```csharp
screens.Screen(0);
DRAW_TOOLTIP(item, 100, 100, true); // ŹLE
screens.Screen(1);
```

Screen 0 jest wyświetlany od monitora (130,40) (`ScreenDisplay(0, 130, 40, 320, 234)`), więc rysunek w lokalnych (100,100) lądował na monitorze (230,140), a okno WYBOR (screen 1, od monitora Y=162) przykrywało dolną część tooltipa — zostawał tylko cienki pasek nad krawędzią okna. Zero wyjątków, zero logów — „cichy" brak rysowania (z-order, patrz `rendering-patterns.md` §19).

**Fix:** konwersja współrzędnych monitora na lokalne aktualnego screena (kompensuje pozycję screena i scroll):

```csharp
screens.Screen(0);
int tx = screens.XScreen(230); // monitor X = 130 + 100
int ty = screens.YScreen(110); // monitor Y = 162 - 52
DRAW_TOOLTIP(item, tx, ty, true);
screens.Screen(1);
```

**Wnioski:**

- Rysunek trafia do współrzędnych lokalnych aktualnego screena, nie monitora.
- Pozycję nakładki licz przez `XScreen`/`YScreen` — kompensują `screen.X/Y` i `OffsetX/OffsetY` (scroll).
- Nakładka pod innym screenem jest niewidoczna bez błędu — sprawdzaj kolejność listy `ScreensManager.Screens` (późniejszy = na wierzchu).
- Odchodząc od wzorca z dokumentacji (tu: „screen 1" → „screen 0"), zaktualizuj od razu ten dokument — dewiacja bez aktualizacji docs wprowadziła buga.

---

## Uwagi końcowe

- **Performance**: `GetBlock`/`PutBlock` kopiują piksel po pikselu, ale obszar tooltipa to tylko ~120×52 pixeli — pomijalny koszt.
- **Przeciąganie**: Przed `WYBOR_PICK` / `SKLEP_PICK` czyścimy tooltip (`CLEAR_TOOLTIP`), bo pick może nadpisać jego obszar. Po wyjściu z picka następna iteracja pętli odświeży tooltip automatycznie.
- **Blok nr 99**: Nieużywany w istniejącym kodzie. Jeśli w przyszłości ktoś doda blok o tym numerze, zmienić stałą `TOOLTIP_BLOCK`.
- **Stany**: `lastTooltipItem` i `lastTooltipZone` zapobiegają niepotrzebnemu przeklejaniu pikseli co klatkę — tooltip odświeża się tylko gdy zmieni się slot lub przedmiot. `lastTooltipScreen` umożliwia `CLEAR_TOOLTIP` przywrócenie tła na właściwym screenie.
- **Czyszczenie przed rysowaniem**: `CLEAR_TOOLTIP` wewnętrznie przełącza screen (`screens.Screen(lastTooltipScreen)`) przed `PutBlock`, a potem wraca do poprzedniego. Dzięki temu działa poprawnie z obu screenów bez dodatkowego przełączania w kodzie wywołującym.
