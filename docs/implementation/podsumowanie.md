# `PODSUMOWANIE_BITWY` — plan refaktora

> **Plik docelowy kodu:** `src/AmigaNet.Legion/AmigaNet.Legion/LegionMainAction.cs` (funkcja `PODSUMOWANIE_BITWY` linia 3374)
> **Powiązany dokument:** `docs/implementation/rendering-patterns.md` (sekcje #11–#18)
> **Wniosek z iteracji `INVENTORY_NEW` (`LegionInventoryNew.cs`):** podział na regiony redrawu, paleta GADGET z WYBOR, kolor 16 = wartości, eliminacja redundancji i obramowań `Box`, save/restore bob bank.

## Status wdrożenia (decyzje z 2026-07-31)

| Plan | Decyzja | Stan |
|---|---|---|
| A1–A6 (batch, HideOn/ShowOn, kolory, GADGET, rozmiar ekranu) | ✅ zatwierdzone | **wdrożone** |
| B1–B2 (pojedynczy ekran + redraw-regionu) | ❌ odrzucone — **układ 2 ekranów zostaje** | — |
| C1 (paleta: wartości 16, nazwy 3, alarmy czerwone 20) | ✅ zatwierdzone | **wdrożone** |
| C2 (przyciski → paleta WYBOR 8/2/6/31) | ✅ zatwierdzone | **wdrożone** |
| C3 (bob tła sylwetek) | ✅ zatwierdzone | **wdrożone** (bez `_LOAD` — boby `GOBY+1` są już załadowane z bitwy) |
| C4 (usunięcie pól klasy) / C5 (ScreenHide/Show) | ❓ bez decyzji | odłożone |
| D1 (podsumowanie bez łupów) | ✅ zatwierdzone | **wdrożone** |
| D2 (ekran porażki) | ❌ odrzucone — **bez ekranu porażki** | — |
| D3 (auto-wyjście po zabraniu wszystkiego) | ✅ zatwierdzone | **wdrożone** |

---

## 1. Cel i zakres

Refaktor ekranu podsumowania bitwy ("łupy") tak, aby był zgodny ze wzorcami z `rendering-patterns.md`, z zachowaniem dotychczasowej funkcjonalności (drag&drop, ctrl+click auto-equip, ammo, paginacja, discard, wyjście).

**PLAN D** (§7) wychodzi poza rendering — domyka luki funkcjonalne: ekran wyniku przy wygranej bez łupów. **D2 (ekran porażki) odrzucony decyzją.**

**Nie scoujemy:**
- Przeniesienia funkcji do osobnego pliku — pozostaje w `LegionMainAction.cs`
- Zmiany modelu danych (`GLEBA`, `ARMIA`, `BRON`)
- Zmiany i18n (`TR("BATTLE_*")`)

---

## 2. Mapa istniejącego kodu

| Funkcja / stała | Plik:Linia | Odpowiedzialność |
|---|---|---|
| `PODSUMOWANIE_BITWY()` | `LegionMainAction.cs:3374` | Główna funkcja ekranu łupów, wywoływana z `LegionMainAction.cs:138` |
| `ZONE_*` (stałe) | `3288-3296` | Strefy: 1=up, 2=down, 3=next, 4=take-all, 5/6=page, 10+=grid, 40+=backpack |
| `AMMO_ITEM_TYPE = 17` | `3296` | Typ amunicji (amunicja → `ARMIA[*,0,TAMO]`) |
| `MAX_AMMO_CAPACITY = 320` | `3297` | Limit puli amunicji |
| `GetLootMouseZone()` | `3299-3310` | Robi ręczny switch Screen(1)/Screen(2) aby scalić mysz-strefy z dwóch ekranów |
| `TryAddLootItemToWarrior()` | `3312-3334` | Ammo → pula; inny → pierwszy wolny slot `TPLECAK+0..7` |
| `TakeAllLoot()` | `3336-3372` | Pętla: ammo do puli, inne rozdzielane po 10 wojownikach |
| `OUTLINE()` | `373-383` | Helper tekstu: 4 cienie `Ink(K2)` + foreground `Ink(K1)` |
| `varTakenCount`, `varNoSpace` (pola klasy!) | `3804-3805` | Stan shared między wywołaniami — zapach; reset w ciele `PODSUMOWANIE_BITWY` (3429-3430) |
| Wejście / wyjście bitwy | `LegionMainAction.cs:138-149`, `3374` | `PODSUMOWANIE_BITWY()` jest wołane gdy `WYNIK_AKCJI == 1` |

**Wykorzystywane stany:**
- `GLEBA[0..110, 0..7]` — sektory z itemami na ziemi (skanowane do `lupItems`)
- `ARMIA[ARM, 0..10, ...]` — wojownicy + plecaki `TPLECAK+0..7`
- `ARMIA[WRG, *, TE]` — wrogowie (do liczenia `enemiesKilled`)
- `BRON[BR, B_*]` — staty przedmiotów: `B_BOB`, `B_CENA`, `B_TYP`, `B_PLACE`, `B_SI`, `B_PAN`, `B_SZ`, `B_WAGA`, `B_DOSW`
- `BROBY + B_BOB` — offset bob-a do `PasteBob`
- `ARMIA_S`, `RASY_S`, `BRON2_S`, `BRON_S` — nazwy do wyświetlania
- Sprite 53 — kursor drag (kompatybilny z `WYBOR_PICK`)
- `WAGA(ARM, NR)` — przeliczenie wagi wojownika
- `SKLEP_AUTO_EQUIP(BR, slot, ARM, NR)` — auto-equip z Ctrl+Klik na plecaku

**Konwencje z `rendering-patterns.md` do zastosowania:**

| Wzorzec nr | Tytuł | Bezpośrednie zastosowanie |
|---|---|---|
| #1 | Batch rendering | Otoczyć `Cls(0)` + redraw batchem |
| #5 | Screen lifecycle | `ScreenClose` + `ScreenOpen` (nie samo `ScreenDisplay`) |
| #10 | Wzorzec okna | `ScreenHide/Show/View` sekwencja |
| #11 | Redraw regionu | Podział na `DRAW_*` wywoływane selektywnie |
| #12 | GADGET palette | Stałe kombinacje `K1/K2/K3/K4` wg semantyki slotu |
| #13 | Etykiety | Sloty bez tekstu, same bob-y |
| #14 | Kolor 16 = wartości | Nagłówki → 3, wartości → 16, alarmy → 20/25 |
| #15 | Usuń redundancję | Nie dublować wagi jeśli w plecaku |
| #16 | Bar > Box | GADGET zamiast ręcznego `Bar`+`Box` |
| #17 | Save/restore bob bank | `_LOAD`/`TrimBobs` jeśli bob tła |
| #18 | Roster vs < > | Konsekwentny wybór UX |

---

## 3. Identyfikacja problemów (vs rendering-patterns)

| # | Problem | Łamany wzorzec |
|---|---|---|
| 1 | **2 screeny zamiast 1** — Screen 2 (320×244) + Screen 1 (320×160) | #11: każdy rebuild robi `Cls(0)` Screen 2 + `Bar` Screen 1. **Świadomie akceptowane** — decyzja: układ 2 ekranów zostaje (B1 odrzucone) |
| 2 | **Pełny `Cls(0)` Screen 2 w pętli** po każdej akcji | #11: redraw-all zamiast redraw-regionu. `Cls(0)` + ~30 operacji na klik |
| 3 | **Brak batcha** (`Begin/EndBatch`) wokół pełnego redrawu | #1: każda operacja flaguje `IsModified`. *Szczegół techniczny:* `IsModified` to bool — rebuild tekstury i tak odbywa się **raz na klatkę Draw** (`LegionGame.cs:224-228`), więc nie ma „30 relebuildów", ale batch daje: jeden pass flag, spójność ze wzorcem i ochronę gdy między redrawami trafi się `View()`/`autoView` |
| 4 | **OUTLINE** = 4 cienie + 1 foreground, bez batcha | #1: pojedynczy OUTLINE = 5×`Text` = 5× zapis do `screen.Data[]` + flag; 8×OUTLINE na klik = ~40 zapisów pikseli (koszt CPU, nie GPU) |
| 5 | **`Ink(0); Bar(...); Ink(5); Box(...)` dla paneli** zamiast `GADGET` | #12, #16: GADGET istnieje, ręczne `Bar`+`Box` to overhead + dublowanie stylu |
| 6 | **Ceny pod ikoną na kolor 30** (jasny żółty) | #14: WYBOR używa 16 (niebieski) dla wartości liczbowych; 30 zarezerwowany dla alarmów |
| 7 | **Brak `HideOn()/ShowOn()`** wokół drag-loop | Podczas dragu widoczne 2 kursory (OS + sprite 53); `WYBOR_PICK_2` chowa kursor OS: `HideOn` `LegionWybor.cs:666`, `ShowOn` `:660` (także `LegionInventoryNew.cs:443,641`) |
| 8 | **`varTakenCount`/`varNoSpace` to pola klasy** (3804) | Stan shared między wywołaniami; reset ręczny w ciele funkcji (3429-3430) |
| 9 | **Grid 2×10 z ręcznym `Box`/`Bar`** | #12, #16: grid loot może być `GADGET`: `K1=0,K2=5,K3=8,K4=16` — to samo co sloty WYBOR |
| 10 | **PPM = discard** (ale PPM też default wyjścia w WYBOR) | UX rozbieżność — do udokumentowania w kodzie |
| 11 | **Status/komunikaty bez tła** (`3515-3523`) | #11: `OUTLINE(statusMsg)` i `OUTLINE(BATTLE_NO_SPACE)` bez `Bar` tła — bezpieczne tylko dzięki pełnemu `Cls` w pętli. **Utrzymane świadomie** — B2 (redraw-regionu) odrzucone, pełny redraw zostaje; przy ewentualnym powrocie do B2 wymagane `Bar` tła |
| 12 | **Screen 2 otwarty jako 640×512, a wyświetlany 320×244** (`3408-3409`) | ✅ **naprawione (A6):** `ScreenOpen(2, 320, 244, ...)` — `Cls` i redrawy piszą 4× mniej pikseli |
| 13 | **Brak podsumowania przy wygranej bez łupów** (`3390: return` gdy `lupItems.Count == 0`) i **brak ekranu po porażce** (guard `WYNIK_AKCJI != 1` na `3376`) | ✅ **D1 wdrożone** (wygrana bez łupów → podsumowanie); ❌ **D2 odrzucone** (porażka wraca na mapę jak dotychczas) |

---

## 4. PLAN A — quick wins (niskie ryzyko)

> ✅ **WDROŻONE** (A1–A6). Tylko `LegionMainAction.cs`. Brak zmiany struktury pętli.

### A1. Batch wokół pełnego redrawu Screen 2

**Lokalizacja:** `3447-3451` (`Screen(2); Cls(0); SetFont(FON1);`) do `screens.View()` na linii `3551`.

**Edycja:**
```csharp
screens.Screen(2);
screens.BeginBatch();
screens.Cls(0);
screens.SetFont(FON1);
// ... cały redraw Screen 2 ...
screens.EndBatch();
screens.View();

screens.Screen(1);
screens.BeginBatch();
// ...
screens.EndBatch();
screens.View();
```

(Uwaga: jeden batch może objąć **oba** screeny — `EndBatch` oznacza wszystkie screeny jako zmodyfikowane (`ScreensManager.cs:79`), a `Screen(N)` nie resetuje `batchDepth`. Osobne batche mają sens tylko gdy chcesz `View()` pomiędzy ekranami.)

**Weryfikacja wizualna:** brak zmian wizualnych, spadek flickeru.
**Weryfikacja kodu:** `dotnet build src/AmigaNet.Legion`, uruchom bitwę → wejdź do podsumowania → sprawdź konsolę.

### A2. HideOn/ShowOn wokół drag-loop

**Lokalizacja:** `3676-3698` (drag onset + drop) oraz `3729-3736` (cancel PPM).

**Edycja:**
```csharp
// przed Sprite(53, ...):  screens.HideOn();
screens.Sprite(53, ...);
// po SpritOff(53):       screens.ShowOn();
```

W **dwóch miejscach** (drop na LPM oraz cancel na PPM).

**Weryfikacja:** sprite 53 nie przebija się przez Screen 2 gdy drag zaczyna/kończy się w innych screen-context.

### A3. Zmiana koloru ceny z 30 → 16

**Lokalizacja:** `3493` — `screens.Ink(30); screens.Text(X + 2, Y + 26, amos.Str_S(BRON[BR, B_CENA]));`

**Zamiana:** `screens.Ink(16, ...)` (niebieski = wartości, zgodny z WYBOR_WYPISZ).

**Weryfikacja:** ceny w gridzie — kolor 16 czytelny na czarnym `Bar(0)` pod spodem.

### A4. Statystyki panelu → GADGET

**Lokalizacja:** `3501-3504`
```csharp
screens.Ink(0);
screens.Bar(8, 130, 310, 195);
screens.Ink(5);
screens.Box(8, 130, 310, 195);
```

**Zamiana:**
```csharp
GADGET(7, 129, 304, 70, "", 5, 0, 8, 8, -1);
```

Paleta "Panel główny" (`K1=5, K2=0, K3=8, K4=8`) — ciemny fill. **Korekta po wdrożeniu:** początkowo użyto jasnego `K3=19` (jak panel postaci WYBOR), ale kolor 19 w palecie to `(238,170,0)` — pomarańczowy, który nie pasował do ciemnego ekranu; zmieniono na ciemny `8`.

**Weryfikacja:** ciemny panel 8 z delikatną ramką 5, spójny z panelem ZIEMIA w INVENTORY_NEW.

### A5. Sloty grid loot → GADGET

**Lokalizacja:** `3472-3497` (pętla rysująca sloty).

**Aktualnie:**
```csharp
screens.Ink(0);
screens.Bar(X, Y, X + 26, Y + 30);
screens.Ink(5);
screens.Box(X, Y, X + 26, Y + 30);
// ... PasteBob + Text(cena) ...
screens.SetZone(ZONE_GRID_START + I, X, Y, X + 28, Y + 32);
```

**Zamiana:**
```csharp
GADGET(X, Y, 28, 32, "", 0, 5, 8, 16, ZONE_GRID_START + I);
// usuń osobne SetZone (GADGET to robi gdy STREFA > 0)
// zostaw PasteBob + Text(cena)
```

**Uwaga:** `K4=16` efektu wizualnego nie ma (TX_S puste), ale stylistycznie spójne z paletą WYBOR dla slotów.

**Weryfikacja:** sloty z delikatną obrysówką 5 i ciemnym tłem 8, jak backpack/ground w WYBOR.

### A6. Zmniejszenie Screen 2 z 640×512 do 320×244

**Lokalizacja:** `3408-3409`.

**Motywacja:** screen 2 jest otwarty jako 640×512, a wyświetlany tylko jako 320×244 (problem #12). `Cls(0)` i pełne redrawy piszą 4× więcej pikseli do `screen.Data[]` niż potrzeba.

**Edycja:**
```csharp
screens.ScreenClose(2);
screens.ScreenOpen(2, 320, 244, 32, PixelMode.Lowres);  // zamiast 640, 512
screens.ScreenDisplay(2, 130, 40, 320, 244);
```

**Bezpieczeństwo:** restore po ekranie (`3793-3796`) i tak tworzy screen 2 od nowa (80×50) — nic poza obszarem 320×244 nie jest rysowane ani czytane.

**Relacja do B1:** A6 to tymczasowe łatanie; B1 (pojedynczy ekran) eliminuje problem całkowicie. Jeśli Faza B ma być wdrożona wkrótce — A6 można pominąć.

### Podsumowanie Fazy A

- **Pliki edytowane:** `LegionMainAction.cs` (5 miejsc)
- **Ryzyko:** niskie (izolowane zmiany, brak zmiany struktury pętli)
- **Build:** musi przejść bez nowych warningów
- **Czas:** ~30 minut + manualny test bitwy

---

## 5. PLAN B — refaktoryzacja pojedynczego ekranu

> ❌ **ODRZUCONY.** Decyzja: układ dwóch ekranów zostaje. Sekcja zachowana jako dokumentacja alternatywy i uzasadnienia.
>
> **Jeśli kiedyś wrócić do tematu:** największy zysk wydajnościowy i uproszczenie pętli. Średnie ryzyko — **backup przed commit**.

### B1. Pojedynczy ekran zamiast 2

**Motywacja:** eliminacja Screen 2 = jedna pętla redraw, jeden `MouseZone`, jeden batch.

**Nowy układ:**
- Jeden ekran `320×240` (lub `320×220`) z `ScreenDisplay(1, 130, 30, 320, 220)`
- **Layout:**
  - `(0,0)-(319,4)` — pasek przycisków (Dalej/Zabierz wszystko)
  - `(0,6)-(319, ~130)` — tytuł + stats bitwy + grid loot (paginacja)
  - `(~130, ~150)-(319, ~195)` — panel info przedmiotu + panel plecaka aktualnej postaci
  - `(0, 206)-(319, 219)` — nazwa/rasa aktualnej postaci

**Zastąpienia w kodzie:**
1. Usunąć `screens.ScreenClose(2); ScreenOpen(2, 640, 512, ...)` (`3407-3413`)
2. Usunąć `screens.ScreenClose(1); ScreenOpen(1, 320, 160, ...)` (`3415-3420`)
3. Dodać:
   ```csharp
   screens.ScreenClose(1);
   screens.ScreenOpen(1, 320, 240, 32, PixelMode.Lowres);
   screens.ReserveZone(100);
   screens.SetFont(FON1);
   screens.ScreenHide();
   screens.View();
   screens.ScreenDisplay(1, 130, 30, 320, 220);
   screens.Colour(0, 3, 1, 0);
   // ... initial redraw ...
   screens.ScreenShow();
   screens.View();
   ```
4. Usunąć wszystkie `screens.Screen(1)` / `screens.Screen(2)` wewnętrzne w pętli (pojedynczy `Screen(1) = default`)
5. Usunąć `GetLootMouseZone()` (`3299-3310`) — bezpośrednio `screens.MouseZone()`
6. **Restore ekranu** (`3793-3801`):
   ```csharp
   screens.ScreenClose(1);
   screens.ScreenOpen(1, 320, 160, 32, PixelMode.Lowres);
   screens.ScreenDisplay(1, 130, 275, 320, 25);
   screens.ReserveZone(100);
   ```

**Uwaga na kompatybilność:** ekran po `PODSUMOWANIE_BITWY` używany w `LegionMainAction.cs:140-205` (`ScreenDisplay(1, 130, 275+J, ...)`) jest `320×160` — musi wrócić do tego stanu. Aktualnie `PODSUMOWANIE_BITWY` ma własne open/close i restore — ta izolacja musi zostać.

### B2. Podział na funkcje redraw-regionu

**Nowe funkcje prywatne (pozostają w `LegionMainAction.cs`):**

| Funkcja | Co rysuje | Kiedy |
|---|---|---|
| `PODSUMOWANIE_DRAW_BACKGROUND()` | Tytuł + statsy bitwy + przyciski + ramy paneli | raz na wejściu |
| `PODSUMOWANIE_DRAW_GRID(lupItems, page)` | 2×10 slotów GADGET + bob + cena | gdy `lupItems` lub `page` zmieniony |
| `PODSUMOWANIE_DRAW_ITEM_INFO(item)` | Clear panelu info + OUTLINE statów | gdy klik na przedmiocie |
| `PODSUMOWANIE_DRAW_BACKPACK(NR)` | Clear paska plecaka + PasteBob 8 slotów | gdy `NR` lub plecak zmieniony |
| `PODSUMOWANIE_DRAW_STATUS(msg, noSpace, overweight)` | Status-bar update | gdy komunikat |
| `PODSUMOWANIE_DRAW_PAGINATION(page, totalPages)` | Strzałki < > + "Strona X/Y" | gdy `page` zmieniony |

**Idempotentność (wzorzec #11):** każda funkcja zaczyna od `screens.Ink(bg, ...); screens.Bar(x1, y1, x2, y2)` — pełne wyczyszczenie swojej strefy, nie zakłada stanu poprzedniej klatki.

**Alternatywa dla `PODSUMOWANIE_DRAW_ITEM_INFO` (opcjonalnie, wzorzec #2):** zamiast redraw-regionu można użyć overlay `GetBlock`/`PutBlock` — zapisz tło panelu info przed rysowaniem statów i przywróć po. Sensowne tylko jeśli panel info ma być *chwilowy* (tooltip). W obecnym layoutcie panel jest stały — redraw-regionu jest prostszy.

### B3. Szkielet nowej pętli

```csharp
bool redrawGrid = true, redrawInfo = false, redrawBackpack = true, redrawStatus = false;
bool redrawPagination = true;

PODSUMOWANIE_DRAW_BACKGROUND();

while (!KONIEC)
{
    if (redrawGrid)       { PODSUMOWANIE_DRAW_GRID(lupItems, page);          redrawGrid = false; }
    if (redrawInfo)       { PODSUMOWANIE_DRAW_ITEM_INFO(currentItem);         redrawInfo = false; }
    if (redrawBackpack)   { PODSUMOWANIE_DRAW_BACKPACK(NR);                   redrawBackpack = false; }
    if (redrawStatus)     { PODSUMOWANIE_DRAW_STATUS(statusMsg, ...);         redrawStatus = false; }
    if (redrawPagination)  { PODSUMOWANIE_DRAW_PAGINATION(page, totalPages);   redrawPagination = false; }

    screens.View();

    // --- Input ---
    var KEY = screens.Inkey_S();
    if (KEY != "" && (KEY == ESC || KEY == ENTER)) { KONIEC = true; break; }
    // ...

    var click = screens.MouseClick();
    if (click == 1)
    {
        var STREFA = screens.MouseZone();  // jeden screen, jeden MouseZone
        // ... obsługa STREFA — ustawia odpowiednie flagi true ...
        // np. klik w grid → redrawInfo = true; lupItems.RemoveAt(...) → redrawGrid = true;
    }

    screens.WaitVbl();
}
```

### Podsumowanie Fazy B

- **Pliki edytowane:** `LegionMainAction.cs`
- **Ryzyko:** średnie — wymaga testu wszystkich ścieżek inputu (drag&drop, ctrl+klik, klawisz A, paginacja, discard)
- **Czas:** ~2-4 godziny + testy manualne
- **Backup git przed commit** zalecany

---

## 6. PLAN C — kosmetyka i spójność

> C1–C3 ✅ **WDROŻONE**. C4/C5 ❓ bez decyzji — odłożone (możliwe jako osobne małe commity).

### C1. Konwersja palety (wzorzec #14) — ✅ wdrożone

| Element | Aktualnie | Wdrożone |
|---|---|---|
| "Zwycięstwo!" | `OUTLINE(..., 31, 0)` | 31 (żółty nagłówek, bez zmian) |
| "Nasi: x/10" / "Pokonani wrogowie" / "Przedmioty na ziemi" / "Zabrano" | `30, 0` | `16, 0` |
| OUTLINE statów przedmiotu (DMG/ARM/SPD/WGT/PRICE) | `30, 0` | `16, 0` |
| Nazwa itemu (`BRO1_S + BRO2_S`) | `31, 0` | `3, 0` |
| `BATTLE_OVERWEIGHT`, `BATTLE_NO_SPACE` | `25, 0` (różowy) | `20, 0` (czerwony=alarm) |

**Uwaga implementacyjna:** panel statów itemu ma ciemne tło `8` (A4, korekta po wdrożeniu) — w handlerze kliku obszar wewnętrzny czyszczony `Ink(8); Bar(...)`, teksty `3/16` z czarnym cieniem `0`.

### C2. Przyciski "Dalej/Zabierz wszystko" — ✅ wdrożone

**Lokalizacja:** `3423-3427`
```csharp
GADGET(6, 2, 100, 38, TR("BATTLE_NEXT"),      8, 2, 6, 31, ZONE_NEXT_BUTTON);
GADGET(110, 2, 96, 38, TR("BATTLE_TAKE_ALL"), 8, 2, 6, 31, ZONE_TAKE_ALL_BUTTON);
```

Paleta WYBOR "Auto/Zamknij" (`8/2/6/31`). **Bonus (D1):** przycisk "Zabierz wszystko" jest rysowany tylko gdy `lupItems.Count > 0`.

### C3. Bob tła (wzorce #17 + WYBOR) — ✅ wdrożone

Wdrożone bez `_LOAD`/`TrimBobs` — po bitwie boby `GOBY+1..+50` (sylwetki wojowników z `dane/gad`) są już załadowane, `GOBY` pozostaje `0`. Wzorzec jak w WYBOR (`LegionWybor.cs:11-14`):

```csharp
// w redrawie Screen 2, zaraz po Cls(0):
for (var I = 0; I <= 3; I++)
{
    screens.PasteBob(0, I * 50, GOBY + 1);
}
```

Sylwetki wzdłuż lewej krawędzi, przykrywane przez panele (grid, panel statów) — klimat oryginalnego WYBOR. Zweryfikować wizualnie, czy sylwetka `I=0` nie koliduje z tytułem "Zwycięstwo!" (tekst rysowany później — powinien być czytelny; w razie potrzeby przesunąć tytuł w prawo lub pominąć `I=0`).

### C4. Usunięcie pól klasy `varTakenCount`/`varNoSpace`

**Lokalizacja:** `3804-3805` (deklaracja pól klasy).

**Edycja:**
1. Przenieść deklaracje do wnętrza `PODSUMOWANIE_BITWY()` jako locals
2. Usunąć inicjalizację na `3429-3430` (zbędną — locals są default 0/false)
3. `TakeAllLoot` i `TryAddLootItemToWarrior` już przyjmują `ref` — tylko usunąć deklarację klasy
4. Weryfikacja: brak pozostałych referencji pól poza `PODSUMOWANIE_BITWY`

### C5. Sekwencja `ScreenHide/ScreenShow/View` (wzorzec #10)

Gładkie wejście jak w `INVENTORY_NEW`:
```csharp
screens.ScreenOpen(1, ...);
screens.ReserveZone(100);
screens.ScreenHide();
screens.View();
screens.ScreenDisplay(1, ...);
// ... pełny redraw ...
screens.ScreenShow();
screens.View();
```

---

## 7. PLAN D — funkcjonalne rozszerzenia (poza renderingiem)

> **D1 ✅ wdrożone, D3 ✅ wdrożone, D2 ❌ odrzucone** (decyzja: bez ekranu porażki).
> Obserwacja z analizy: `PODSUMOWANIE_BITWY` pokazywał się **tylko przy zwycięstwie z łupami**. Lukę domyka D1.

### D1. Podsumowanie przy wygranej bez łupów — ✅ wdrożone

Usunięty wczesny `return` (dawna linia 3390). Przy pustym `lupItems`: tytuł + statystyki (`BATTLE_SURVIVORS`, `BATTLE_ENEMIES_KILLED`) + przycisk "Dalej" (grid i "Zabierz wszystko" nie są rysowane — `gridCount = 0`, przycisk warunkowy w setupie).

### D2. Ekran porażki (`WYNIK_AKCJI == 2`) — ❌ odrzucony

> Decyzja: **bez ekranu porażki** — gra wraca na mapę jak dotychczas. Sekcja zachowana jako dokumentacja możliwego przyszłego rozszerzenia.

Analogiczny ekran, inny tytuł (`BATTLE_DEFEAT` — nowy klucz i18n) + statystyki strat: polegli nasi (`ARMIA[ARM, *, TE] <= 0`), ocaleni. Bez gridu łupów i drag&drop. Po "Dalej" → dotychczasowy flow wyjścia (`140-205`).

**Uwaga na flow:** `PODSUMOWANIE_BITWY` jest wywoływane z `MAIN_ACTION:138` bezwarunkowo; obecny guard `WYNIK_AKCJI != 1` robi early-return. D2 = zamiana guarda na `WYNIK_AKCJI == 2` → osobna ścieżka renderu (lub parametr trybu w wspólnej funkcji). Nie zmienia to wywołania w `MAIN_ACTION`.

### D3. "Zabierz wszystko" przy pustej liście — ✅ wdrożone

Po `TakeAllLoot`, gdy `lupItems.Count == 0` — automatyczny `KONIEC = true` (zamiast zostawiania gracza na pustym ekranie). Dotyczy obu ścieżek: klawisz "A" i przycisk "Zabierz wszystko".

---

## 8. Kolejność wdrożenia — stan: Fazy A, C1–C3, D1, D3 wdrożone

1. ✅ **Faza A** (A1–A6) — `refactor(podsumowanie): batch redraw + HideOn/ShowOn + GADGET panels + screen size`
2. ⏳ **Manualny test** bitew (mała/duża/przygoda) — do wykonania
3. ❌ **Faza B** (B1–B2) — odrzucona (układ 2 ekranów zostaje)
4. ⏳ **Manualny test** — szczególnie drag&drop sprite 53, paginacja, ctrl+klik, PPM discard
5. ✅ **Faza C** (C1–C3) — wdrożona; C4/C5 bez decyzji, odłożone
6. ✅ **Faza D** (D1, D3) — wdrożona; D2 odrzucony

---

## 9. Mapa weryfikacji

| Co | Komenda / Akcja | Spodziewany rezultat |
|---|---|---|
| Build | `dotnet build src/AmigaNet.Legion` | 0 errors, 0 new warnings |
| Mała bitwa | Walka pojedyncza → `PODSUMOWANIE_BITWY` | Działa jak przed Fazą A |
| Duża bitwa | Bitwa z >20 lupitems → paginacja | `<` `>` działają, strony się zmieniają |
| Ammunition | Bitwa z arrow = typ 17 | Trafia do `ARMIA[ARM, 0, TAMO]` (pula) |
| Auto-equip | Ctrl+click item w plecaku | `SKLEP_AUTO_EQUIP` + status `BATTLE_EQUIPPED` |
| Discard | PPM na item w gridzie | Usunięty, status `BATTLE_DISCARDED` |
| Exit | ESC / Enter / "Dalej" | Powrót do ekranu mapy, Screen 1 wraca do `320×160` |
| No space | Wszystkie plecaki pełne | `varNoSpace=true`, komunikat `BATTLE_NO_SPACE` |
| Performance wizualnie | Brak flickeru przy redraw | Faza A: redukcja; Faza B: brak całkowity |
| Restore ekranu | Po wyjściu z `PODSUMOWANIE_BITWY` | `ScreenDisplay(1, 130, 275, 320, 25)` przywrócone |
| Wygrana bez łupów (D1) | Bitwa, po której wrogowie nic nie zostawili | Ekran wyniku z samymi statystykami + "Dalej" |
| Take-all pusta lista (D3) | "Zabierz wszystko" gdy `lupItems` puste | Automatyczne wyjście z ekranu |
| Sylwetki tła (C3) | Wejście w podsumowanie | 4 sylwetki wzdłuż lewej krawędzi, panele przykrywają; tytuł czytelny |

---

## 10. Pytania otwarte — rozstrzygnięte 2026-07-31

1. **C1 alarmy (`OVERWEIGHT`, `NO_SPACE`)** → **czerwone (20)** ✅
2. **C2 przyciski ("Dalej"/"Zabierz wszystko")** → **paleta WYBOR (`8/2/6/31`)** ✅
3. **C3 bob tła** → **wdrażamy** ✅ (bez `_LOAD` — boby już załadowane z bitwy)
4. **Faza B (pojedynczy ekran)** → **zostajemy przy układzie 2 ekranów**; A6 wdrożone
5. **D2 ekran porażki** → **bez ekranu porażki** ❌
6. **D1 / D3** → **wdrożone** ✅

**Pozostałe do rozstrzygnięcia:** C4 (pola klasy `varTakenCount`/`varNoSpace`), C5 (sekwencja `ScreenHide/Show`).

---

## 11. Referencje

- `docs/implementation/rendering-patterns.md` — wzorce #1–#18 (źródło inspiracji z `INVENTORY_NEW`)
- `src/AmigaNet.Legion/AmigaNet.Legion/LegionInventoryNew.cs` — implementacja wzorców #11/#12/#14/#16; `HideOn/ShowOn` na `443,641`
- `src/AmigaNet.Legion/AmigaNet.Legion/LegionWybor.cs` — oryginalne `WYBOR` / `WYBOR_PICK_2` / `WYBOR_WYPISZ` (kanoniczna paleta i UX; `HideOn:666`, `ShowOn:660`)
- `src/AmigaNet.Legion/AmigaNet.Legion/LegionStrings.cs` — klucze i18n `BATTLE_*` (linia 56–74 PL, 124–142 EN)
- `src/AmigaNet.Legion/AmigaNet.Legion/Legion.cs:1072` — definicja `GADGET` (K1/K2/K3/K4)
- `src/AmigaNet.Legion/AmigaNet.Legion.DesktopApp/LegionGame.cs:218-228` — `DrawScreens()`: rebuild tekstury raz na klatkę (gdzie faktycznie dzieje się koszt redrawu)
