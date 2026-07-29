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

## 1. Nowe metody w `Legion.cs` (lub `LegionWybor.cs`)

```csharp
// Blok dla tooltipa — używamy numeru spoza zakresu używanego przez istniejący kod
private const int TOOLTIP_BLOCK = 99;
private int lastTooltipItem = -1;
private int lastTooltipZone = -1;
private int lastTooltipScreen = -1;
private int lastTooltipX, lastTooltipY;

void DRAW_TOOLTIP(int itemId, int x, int y)
{
    if (itemId <= 0) return;

    lastTooltipScreen = screens.Screen();

    const int W = 120;
    const int H = 52;

    // Zapisz tło
    screens.GetBlock(TOOLTIP_BLOCK, x, y, W, H);

    // Tło tooltipa
    screens.Ink(0);
    screens.Bar(x, y, x + W, y + H);
    screens.Ink(19);
    screens.Bar(x + 1, y + 1, x + W - 1, y + H - 1);
    screens.Ink(0);
    screens.Bar(x + 2, y + 2, x + W - 2, y + H - 2);

    // Nazwa przedmiotu
    screens.Ink(31, 0);
    screens.Text(x + 4, y + 3, BRON_S[itemId]);

    // Typ i waga
    screens.Ink(16, 0);
    screens.Text(x + 4, y + 16, BRON2_S[BRON[itemId, B_TYP]]);
    screens.Text(x + 80, y + 16, "W:" + BRON[itemId, B_WAGA]);

    // Statystyki: Siła, Pancerz, Szybkość, Energia
    screens.Ink(20, 0);
    var stats = "S:" + BRON[itemId, B_SI] + " P:" + BRON[itemId, B_PAN]
              + " Sz:" + BRON[itemId, B_SZ] + " E:" + BRON[itemId, B_EN];
    screens.Text(x + 4, y + 29, stats);

    // Cena
    screens.Ink(21, 0);
    screens.Text(x + 4, y + 42, "Cena: " + BRON[itemId, B_CENA]);

    lastTooltipX = x;
    lastTooltipY = y;
}

void CLEAR_TOOLTIP()
{
    if (lastTooltipScreen < 0) return;

    var prevScreen = screens.Screen();
    screens.Screen(lastTooltipScreen);
    screens.PutBlock(TOOLTIP_BLOCK);
    screens.Screen(prevScreen);

    lastTooltipItem = -1;
    lastTooltipZone = -1;
    lastTooltipScreen = -1;
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
        int mx = screens.XMouse();
        int my = screens.YMouse();

        // Tooltip przesunięty względem kursora
        int tx = mx + 16;
        int ty = my + 16;

        // Clamp do granic ekranu (screen 1 = 320×140)
        if (tx + 120 > 320) tx = mx - 124;
        if (ty + 52 > 140) ty = my - 56;
        if (tx < 0) tx = 0;
        if (ty < 0) ty = 0;

        DRAW_TOOLTIP(item, tx, ty);
    }

    lastTooltipItem = item;
    lastTooltipZone = zone;
}
```

### Uwagi do WYBOR

- Tooltip rysowany na **screen 1** (320×140) — `screens.Screen(1)` jest ustawione na starcie `WYBOR`.
- `CLEAR_TOOLTIP` sama przełącza screen, więc nie trzeba tego robić ręcznie.
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
        int mx = screens.XMouse();
        int my = screens.YMouse();

        int tx = mx + 16;
        int ty = my + 16;

        // Clamp do granic ekranu (screen 2 = 320×244)
        if (tx + 120 > 320) tx = mx - 124;
        if (ty + 52 > 244) ty = my - 56;
        if (tx < 0) tx = 0;
        if (ty < 0) ty = 0;

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

## Uwagi końcowe

- **Performance**: `GetBlock`/`PutBlock` kopiują piksel po pikselu, ale obszar tooltipa to tylko ~120×52 pixeli — pomijalny koszt.
- **Przeciąganie**: Przed `WYBOR_PICK` / `SKLEP_PICK` czyścimy tooltip (`CLEAR_TOOLTIP`), bo pick może nadpisać jego obszar. Po wyjściu z picka następna iteracja pętli odświeży tooltip automatycznie.
- **Blok nr 99**: Nieużywany w istniejącym kodzie. Jeśli w przyszłości ktoś doda blok o tym numerze, zmienić stałą `TOOLTIP_BLOCK`.
- **Stany**: `lastTooltipItem` i `lastTooltipZone` zapobiegają niepotrzebnemu przeklejaniu pikseli co klatkę — tooltip odświeża się tylko gdy zmieni się slot lub przedmiot. `lastTooltipScreen` umożliwia `CLEAR_TOOLTIP` przywrócenie tła na właściwym screenie.
- **Czyszczenie przed rysowaniem**: `CLEAR_TOOLTIP` wewnętrznie przełącza screen (`screens.Screen(lastTooltipScreen)`) przed `PutBlock`, a potem wraca do poprzedniego. Dzięki temu działa poprawnie z obu screenów bez dodatkowego przełączania w kodzie wywołującym.
