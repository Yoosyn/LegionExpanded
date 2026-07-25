# Projekt: amiga-dotnet-legion

Port gry **Legion** (1994) na Amigę z AMOS BASICA do C#/.NET 6 + MonoGame.
Licencja: GPL v3

---

## Originalna gra (Amiga)

**Legion** to polskie RPG wydane w 1994 roku na komputer Amiga. Gra napisana została w języku AMOS BASIC. Akcja toczy się w fantasy świecie — gracz dowodzi armią, eksploruje mapę, walczy z potworami, odwiedza miasta, handluje i rozwija swoją drużynę.

### Oryginalny kod źródłowy (AMOS BASIC)

Katalog `original/` zawiera trzy wersje kodu źródłowego:

| Plik | Opis |
|------|------|
| `legion_3_1.Asc` | Główny kod źródłowy (polski), ~9015 linii, wersja 3.1 z komentarzami operacji |
| `legion_nop.Asc` | Kod źródłowy bez komentarzy operacji |
| `legion_english.Asc` | Angielskie tłumaczenie kodu źródłowego |

Kod AMOS zawiera ~117 procedur/funkcji, z czego 107 zostało bezpośrednio przetłumaczonych na metody C#. Gra używa systemu pamięci bankowej AMOS (Bank 1 – obrazki/sprajty, Bank 4/5 – próbki audio), ekranów (Screens), obiektów Bob i Sprite, animacji AMAL, czcionek Amiga, formatu IFF ILBM dla grafik oraz modułów trackerowych (ProTracker MOD) dla muzyki.

### Zasoby binarne — `original/legion/`

#### Grafika (IFF ILBM / raw)
- `kam.pic`, `kam_eng.pic` — obrazki jaskini/kamienia
- `szkielend.pic` — obrazek zakończenia (szkielet)
- `napeng.pic` — obrazek (angielski)
- `grob.hb`, `pobieda.hb` — grafika w trybie HalfBrite
- `czacha.32`, `gobilog.16` — surowe dane graficzne (czaszka, logo goblina)
- `title2`, `intro` — ekran tytułowy i intro
- `miecz`, `mieczyk` — obrazki mieczy

#### Muzyka (tracker modules ProTracker)
- `mod.legion` — główny motyw muzyczny
- `mod.intro` — muzyka intro
- `mod.end2` — muzyka końcowa
- `mod.2sample+` — dodatkowy moduł
- `dane/muzyka/` — muzyka regionalna (8 plików: bagna, góry, grota, jaskinia, las, pustynia, step, zima)

#### Mapy i dane leveli
- `mapa`, `mapa_eng`, `mapa2.org` — dane mapy świata
- `t1`, `t2`, `t3` — tilesety map
- `dane/scen-*` — dane scenerii dla różnych terenów (12 plików: bagno, domy, grobowiec, grota, jaskinia, las, lodowiec, pustynia, skały, stara grota, step)

#### Dane gry
- `dane/glowny`, `dane/glowny2`, `dane/glowny3` — główne dane gry
- `dane/mur1`, `mur2`, `mur3` — dane kolizji/ścian
- `dane/gad`, `dane/gad3` — dane interfejsu (gadżety)
- `dane/sound`, `dane/sound2`, `dane/old_sound` — banki próbek dźwiękowych (format AmBk)
- `dane/potwory/` — 12 potworów (sprite + próbki dźwiękowe):
  boss, dzik (dzik), gargoil (gargulec), gloom, humanoid, pająk (pająk), pterodon, skirial, szkielet (szkielet), warpun, wilk (wilk)

#### Pliki tekstowe
- `opis.txt`, `opis_eng.txt` — opisy (PL/EN)
- `rozmowa.txt`, `rozmowa2.txt`, `conversation.txt` — dialogi NPC
- `przygody.txt`, `przygody2.txt`, `adventures.txt` — teksty przygód/questów

#### Fonty Amiga
- `fonts/garnet/` — 9px i 16px
- `fonts/defender2/` — 8px
- `fonts/Bodacious/` — 42px

#### Save'y
- `Archiwum/zapis 1`, `Archiwum/Zapis 5` — przykładowe zapisy stanu gry

---

## Port do C# (.NET 6 + MonoGame)

### Struktura rozwiązania (`src/AmigaNet.Legion/`)

```
AmigaNet.Legion.sln
├── AmigaNet.Legion/              (główna biblioteka gry)
├── AmigaNet.Amos/                (warstwa emulacji runtime'u AMOS)
├── AmigaNet.IO/                  (odczyt/zapis formatów Amiga)
├── AmigaNet.Types/               (wspólne typy danych)
├── AmigaNet.Legion.DesktopApp/   (aplikacja desktop MonoGame)
└── SharpMod.Core/                (odtwarzacz trackerów MOD/XM/S3M)
```

### Warstwy architektury

```
Program.cs
  └─ LegionGame (MonoGame Game, implementuje IGameEngine)
       ├─ Legion — główna logika gry, tłumaczenie procedur AMOS
       │    ├─ LegionData / LegionDataLoader — dane gry
       │    ├─ LegionArmia — zarządzanie armią
       │    ├─ LegionGadka — system dialogów
       │    ├─ LegionMainAction — walki i potyczki
       │    ├─ LegionMapaAkcja — mapa świata
       │    ├─ LegionMiasto — zarządzanie miastami
       │    ├─ LegionRysujScenerie — renderowanie scenerii (Mode 7)
       │    ├─ LegionSklep — system sklepów
       │    ├─ LegionStrings — lokalizacja (PL/EN)
       │    ├─ LegionWybor — menu wyboru
       │    └─ Pathfinding — A* dla AI w walkach
       ├─ ScreensManager — system ekranów AMOS (1739 linii)
       ├─ MemoryBanksManager — banki pamięci AMOS
       ├─ AmosBase — funkcje wbudowane AMOS BASIC
       └─ ModulePlayer — odtwarzacz trackerów (SharpMod.Core)
```

### Pliki źródłowe C#

#### `AmigaNet.Legion/AmigaNet.Legion/`

| Plik | Opis |
|------|------|
| `Legion.cs` | Główny silnik gry (partial class), ~57k znaków, tłumaczenie procedur AMOS |
| `LegionData.cs` | Tablice danych gry (rasy, broń, przedmioty, mapy) |
| `LegionDataLoader.cs` | Wczytywanie danych z plików tekstowych |
| `LegionArmia.cs` | Zarządzanie armią (rekrutacja, ekwipunek, statystyki) |
| `LegionArchive.cs` | Zapis/odczyt stanu gry |
| `LegionGadka.cs` | System dialogów i konwersacji |
| `LegionMainAction.cs` | Główna pętla akcji, system walki |
| `LegionMapaAkcja.cs` | Akcje na mapie świata |
| `LegionMiasto.cs` | Zarządzanie miastami |
| `LegionRysujScenerie.cs` | Renderowanie scenerii (tryb Mode 7, pomieszczenia) |
| `LegionSklep.cs` | System sklepów |
| `LegionStrings.cs` | Słowniki zlokalizowanych stringów (PL/EN) |
| `LegionWybor.cs` | Menu wyboru i selekcji |
| `Pathfinding/Pathfinder.cs` | Pathfinding A* (216 linii) |
| `Pathfinding/NavGrid.cs` | Siatka nawigacyjna dla walk (90 linii) |
| `data/pl/WCZYTAJ_ROZMOWE` | Dane dialogów (PL) |
| `data/pl/WCZYTAJ_RASY` | Statystyki ras/potworów |
| `data/pl/WCZYTAJ_PRZYGODY` | Teksty przygód (13 questów) |
| `data/pl/WCZYTAJ_GULE` | Deskryptory nastroju/postaw |
| `data/pl/WCZYTAJ_BUDYNKI` | Dane budynków |
| `data/pl/WCZYTAJ_BRON` | Bronie, zbroje, przedmioty (172 linie) |

#### `AmigaNet.Amos/` (warstwa emulacji AMOS)

| Plik | Opis |
|------|------|
| `AmosBase.cs` | Funkcje wbudowane AMOS BASIC (Wait, Str$, Left$, Right$, Mid$, Instr, Val, Rnd, Timer, Exist, TrackPlay) (248 linii) |
| `Data.cs` | Parsowanie Data/Restore/Read AMOS (85 linii) |
| `IGameEngine.cs` | Interfejs abstrakcyjny dla usług platformowych (49 linii) |
| `MemoryBanks/MemoryBank.cs` | Generyczny kontener banku (7 linii) |
| `MemoryBanks/MemoryBanksManager.cs` | Zarządzanie bankami pamięci (Bank 1, 4, 5), odtwarzanie SamPlay (131 linii) |
| `Screens/ScreensManager.cs` | Pełny system ekranów AMOS: ScreenOpen/Close, LoadIff, Bob/Sprite, strefy, tekst, fonty, paleta/fade, rysowanie, AMAL, mysz/klawiatura (1739 linii) |
| `Screens/Block.cs` | Blok ekranu (GetBlock/PutBlock) (15 linii) |
| `Screens/Bob.cs` | Obiekt Bob (Blitter Object) (17 linii) |
| `Screens/Display.cs` | Sprzętowy wyświetlacz z 64 slotami sprite'ów (17 linii) |
| `Screens/Drawing.cs` | Algorytm Bresenhama (87 linii) |
| `Screens/FadeState.cs` | Stan płynnego wygaszania palety (28 linii) |
| `Screens/IGraphicElement.cs` | Interfejs elementów rysowalnych (19 linii) |
| `Screens/IScreensManager.cs` | Interfejs ScreensManager (15 linii) |
| `Screens/PixelMode.cs` | Enum trybów: Lowres/Hires/HiresAndLaced (9 linii) |
| `Screens/Screen.cs` | Deskryptor ekranu (paleta, boby, bloki, strefy, fade) (66 linii) |
| `Screens/Shape.cs` | Element kształtu (Bar/Box/Draw) (19 linii) |
| `Screens/Sprite.cs` | Sprzętowy sprite (15 linii) |
| `Screens/Zone.cs` | Prostokąt strefy kolizji (18 linii) |
| `Screens/Amal/AmalAnim.cs` | Instrukcja animacji (sekwencja obrazków z opóźnieniami) (17 linii) |
| `Screens/Amal/AmalBuilder.cs` | Budowa łańcuchów instrukcji AMAL (47 linii) |
| `Screens/Amal/AmalInfo.cs` | Stan kanału AMAL (15 linii) |
| `Screens/Amal/AmalInstruction.cs` | Klasa bazowa (4 linie) |
| `Screens/Amal/AmalJump.cs` | Skok do etykiety (7 linii) |
| `Screens/Amal/AmalLabel.cs` | Etykieta (7 linii) |
| `Screens/Amal/AmalMove.cs` | Instrukcja płynnego ruchu (12 linii) |

#### `AmigaNet.IO/` (odczyt/zapis formatów Amiga)

| Plik | Opis |
|------|------|
| `BytesReader.cs` | Big-endian binary reader (71 linii) |
| `BytesWriter.cs` | Big-endian binary writer (56 linii) |
| `Audio/Amos/SampleBanksReader.cs` | Odczyt banków próbek AmBk (PCM 8-bit signed) (90 linii) |
| `Fonts/FontInfo.cs` | Metadane czcionki Amiga + renderowanie znaków (74 linie) |
| `Fonts/FontsReader.cs` | Parsowanie plików czcionek Amiga (52 linie) |
| `Graphics/IImagesReader.cs` | Interfejs readerów obrazków (11 linii) |
| `Graphics/Amos/SpriteBanksReader.cs` | Odczyt banków sprite'ów AmSp/Amlc (bitplane → indexed pixels) (101 linii) |
| `Graphics/Iff/IffImage.cs` | Descriptor obrazka IFF ILBM (41 linii) |
| `Graphics/Iff/IffImagesReader.cs` | Pełny reader IFF ILBM z obsługą EHB/HAM i dekompresją (221 linii) |

#### `AmigaNet.Types/` (wspólne typy)

| Plik | Opis |
|------|------|
| `Graphics/Pixel.cs` | RGBA pixel z opcjonalnym indeksem palety (49 linii) |
| `Graphics/ImagesContainer.cs` | Kontener na obrazki + wspólna paleta (9 linii) |
| `Graphics/ImageData.cs` | Dane obrazka (wymiary, hotspot, maska) (49 linii) |
| `Audio/AudioSampleData.cs` | Mono próbka PCM 8-bit signed (15 linii) |

#### `AmigaNet.Legion.DesktopApp/` (entry point)

| Plik | Opis |
|------|------|
| `Program.cs` | Punkt wejścia: ścieżki, wybór języka, uruchomienie LegionGame (36 linii) |
| `LegionGame.cs` | Klasa Game MonoGame — implementuje IGameEngine: rendering, audio, input, kursor, VBL (614 linii) |
| `FrameCounter.cs` | Licznik FPS (43 linie) |
| `KeyInfo.cs` | Stan klawiatury (13 linii) |
| `MonoGameLibLoader.cs` | Ładowarka bibliotek SDL2 (26 linii) |
| `XnaSoundRenderer.cs` | Bridge SharpMod.Core → XNA/MonoGame (57 linii) |
| `libs/` | Biblioteki natywne SDL2 + soft_oal (linux, osx, windows x64/x86) |

#### `SharpMod.Core/` (~40 plików)

Odtwarzacz modułów trackerowych (MOD/XM/S3M/IT). Zawiera loadery formatów, mikser (4-kanałowy Paula-style), DSP/FFT, obsługę envelope'ów, samplerów, patternów, instrumentów, eksport do WAV.

---

## Stan portu (z PORTING_PLAN.md)

### Rozwiązane
1. Działanie animacji płynnego wygaszania palety (fade)
2. Animacja miecza w intro (AMAL Anim 0 = nieskończona pętla)

### Otwarte (10 znanych problemów)
1. Brak regionalnej muzyki scenerii (Mode 7 — muzyka zależna od terenu)
2. Stan regeneracji po spotkaniu (leczenie między walkami)
3. Renderowanie paska zdrowia w pasku statusu
4. Latające stworzenia niszczące kamienne ściany (interakcja z terenem)
5. Kupiec ignorujący widoczny filtr (AI NPC)
6. Hack ruchu rysowania linii ekwipunku (artefakt wizualny)
7. Kolizja z bramą/drzwiami
8. Brak interakcji z ciałem pokonanego wroga
9. Duchy łuczników na ścianach (pathfinding — edge case)
10. Problem przezroczystości w slocie ekwipunku (tło sklepu)

---

## Oryginalny kod źródłowy AMOS — szczegółowa analiza

Trzy pliki źródłowe AMOS znajdują się w `original/`:

### `legion_3_1.Asc`
Główny, najpełniejszy plik źródłowy. Około 9015 linii. Zawiera pełną implementację gry z komentarzami opisującymi operacje. To jest wersja 3.1, czyli finalna/późna wersja gry.

Zawiera:
- Inicjalizację gry i banków pamięci
- System ekranów AMOS (wielowarstwowe przewijanie)
- Obsługę sprite'ów i bobów (obiekty gry)
- Animacje AMAL (np. animacja miecza w intro)
- Pętlę główną gry
- System walki (w tym pathfinding)
- Ekran mapy świata z przewijaniem
- System miast i sklepów
- Dialogi z NPC
- Zarządzanie armią (rekrutacja, ekwipunek, statystyki)
- Zapis/odczyt stanu gry
- 117 procedur/funkcji

### `legion_nop.Asc`
Kod źródłowy bez komentarzy operacji — czystsza wersja, prawdopodobnie używana do rzeczywistej kompilacji.

### `legion_english.Asc`
Angielskie tłumaczenie kodu źródłowego. Zawiera te same procedury, ale z angielskimi stringami i komentarzami.

### Kluczowe elementy kodu AMOS przetłumaczone na C#:
- **AMOS Banks** → `MemoryBanksManager` (Bank 1: sprite'y, Bank 4/5: dźwięki)
- **Screen system** → `ScreensManager` (ScreenOpen, ScreenClose, LoadIff, Display, Bob, Sprite)
- **AMAL animacje** → `AmalAnim`, `AmalMove`, `AmalJump`, `AmalLabel`
- **AMOS BASIC functions** → `AmosBase` (Str$, Val, Instr, Left$, Mid$, Right$, Rnd, Timer, itd.)
- **Data/Restore/Read** → `Data.cs`
- **IFF ILBM** → `IffImagesReader`
- **Sprite banks AmSp/Amlc** → `SpriteBanksReader`
- **Sample banks AmBk** → `SampleBanksReader`
- **Fonts** → `FontsReader` + `FontInfo`
- **ProTracker MOD** → `SharpMod.Core`
- **Pathfinding walki** → `Pathfinder.cs` (A*)

### Uwagi techniczne dot. oryginału:
- Gra używa trybów graficznych: Lowres, Hires, HiresAndLaced
- Obsługa trybu HalfBrite (EHB) i HAM w IFF ILBM
- System palet z płynnym przejściem (fade)
- 64 sprzętowe sprite'y
- 4-kanałowy dźwięk Paula (Amiga chipset)
- Muzyka w formacie ProTracker MOD
- Próbki dźwiękowe 8-bit signed PCM
- Teksty w kodowaniu ASCII/znakowym (polskie znaki w kodzie Amiga)
- Pliki czcionek w formacie Amiga Font
