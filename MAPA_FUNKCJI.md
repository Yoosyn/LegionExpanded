# LegionExpanded — Pełna mapa funkcji (function-by-function)

> **Projekt:** Port gry "Legion" (1994, Marcin Puchta) z AMOS BASIC na Amigę → C# .NET 6 / MonoGame  
> **Rozwiązanie:** 6 projektów, ~110 plików .cs, ~15 000+ linii kodu  
> **Data kompilacji mapy:** 2026-07-25

---

## Spis treści

1. [AmigaNet.Types](#1-amiganettypes)
2. [AmigaNet.IO](#2-amiganetio)
3. [AmigaNet.Amos](#3-amiganetamos)
4. [AmigaNet.Legion — CORE GAME](#4-amiganetlegion--core-game)
5. [AmigaNet.Legion.DesktopApp](#5-amiganetlegiondesktopapp)
6. [SharpMod.Core](#6-sharpmodcore)

---

# 1. AmigaNet.Types

> **Ścieżka:** `src/AmigaNet.Legion/AmigaNet.Types/`  
> **Rola:** Wspólne typy (Graphics, Audio) — kontenery danych
> **Pliki:** 5

## `Graphics/Pixel.cs`
```csharp
public struct Pixel(int x, int y)
```
- `X` (int)
- `Y` (int)

## `Graphics/ImageData.cs`
```csharp
public class ImageData
```
- `Bitmap` (int[]) — bitmapa obrazka
- `Palette` (Color4[]) — paleta kolorów
- `Width` / `Height` (int)
- `HotspotX` / `HotspotY` (int) — punkt zaczepienia
- `Flags` (byte)
- Konstruktor `ImageData(int width, int height)`

## `Graphics/ImagesContainer.cs`
```csharp
public class ImagesContainer
```
- `Images` (List<ImageData>)
- `AddImage(ImageData)`
- Indekser `this[int index]`

## `Audio/AudioSampleData.cs`
```csharp
public class AudioSampleData
```
- `SampleRate` / `BitsPerSample` / `Length` / `Volume` (int)
- `Data` (sbyte[])
- `LoopStart` / `LoopEnd` (int)

## `Audio/AmosMusicData.cs`
```csharp
public class AmosMusicData
```
- `Samples` (List<AudioSampleData>)
- `Sequence` (int[])
- `Length` (int)

---

# 2. AmigaNet.IO

> **Ścieżka:** `src/AmigaNet.Legion/AmigaNet.IO/`  
> **Rola:** Reader/y dla formatów Amigi (IFF ILBM, AMOS Banki, fonty Amiga)
> **Pliki:** 10

## `BytesReader.cs`
```csharp
public class BytesReader : IDisposable
```
- `BytesReader(byte[] data)`
- `ReadByte()` / `ReadBytes(int count)`
- `ReadInt16()` / `ReadUInt16()` / `ReadInt32()` / `ReadUInt32()` — big-endian
- `ReadString(int length)` / `ReadNullTerminatedString()`
- `Skip(int bytes)`
- `Position` (get/set) / `Length` / `IsEos`
- `Dispose()`

## `BytesWriter.cs`
```csharp
public class BytesWriter
```
- `WriteByte(byte)` / `WriteBytes(byte[])`
- `WriteInt16(short)` / `WriteUInt16(ushort)` / `WriteInt32(int)` — big-endian
- `ToArray()` → byte[]

## `Graphics/Iff/IffImage.cs`
```csharp
public class IffImage
```
- `Bitmaps` (List<int[]>) — po jednej bitmapie na plane
- `Palette` (Color4[])
- `Width` / `Height` / `X` / `Y` (int)
- `PixelFormat` / `Mask` / `Compression` (int)
- `ConvertToBitmap()` → int[] — konwersja planarna → chunky

## `Graphics/Iff/IffImagesReader.cs`
```csharp
public class IffImagesReader : IImagesReader
```
- `Read(byte[] data)` → `ImagesContainer` — główny reader IFF ILBM
- `ReadIffFile(BytesReader)` — parsuje FORM, iteruje chunki
- `ReadBMHD(BytesReader)` — BitMapHeader
- `ReadCMAP(BytesReader)` — ColorMap (paleta)
- `ReadCRNG(BytesReader)` — ColorRange (animacja palety)
- `ReadBODY(BytesReader)` — dane bitmapy planarnej

## `Graphics/IImagesReader.cs`
```csharp
public interface IImagesReader
```
- `Read(byte[] data)` → `ImagesContainer`

## `Graphics/Amos/SpriteBanksReader.cs`
```csharp
public class SpriteBanksReader
```
- `Read(byte[] data, int count)` → `ImagesContainer` — czyta AmBk/AmSp

## `Fonts/FontInfo.cs`
```csharp
public class FontInfo
```
- `Height` / `Baseline` / `YSize` / `Modulo` (int)
- `CharData` (byte[][]) — dane glifów
- `LoChar` / `HiChar` / `Spacing` (int)

## `Fonts/FontsReader.cs`
```csharp
public class FontsReader
```
- `Read(byte[] data)` → `FontInfo` — czyta Amiga font (nagłówek + tabele)

## `Audio/Amos/AmosMusicBankReader.cs`
```csharp
public class AmosMusicBankReader
```
- `Read(byte[] data)` → `AmosMusicData` — czyta bank muzyczny AMOS

## `Audio/Amos/SampleBanksReader.cs`
```csharp
public class SampleBanksReader
```
- `Read(byte[] data, int sampleCount)` → `List<AudioSampleData>` — czyta AmAudio

---

# 3. AmigaNet.Amos

> **Ścieżka:** `src/AmigaNet.Legion/AmigaNet.Amos/`  
> **Rola:** Emulacja środowiska uruchomieniowego AMOS BASIC
> **Pliki:** 28

## `AmosBase.cs`
```csharp
public class AmosBase
```
Emulacja wbudowanych komend AMOS BASIC:
- `Rnd(double)` / `Rnd(int)` → int — Random.Next
- `Wait(double seconds)`
- `Timer` — TimeSpan
- `Upper_S(string)` / `Lower_S(string)` / `Str_S(object)`
- `Val(string)` → double
- `Left_S(string, int)` / `Right_S(string, int)` / `Mid_S(string, int, int)`
- `Len(string)` → int / `Instr(string, string)` → int
- `Chr_S(int)` → string / `Asc(string)` → int
- `Add(int, int)` / `Sub(int, int)` — z clamp
- `Xor(int, int)` / `Abs(int)` / `Sgn(double)` / `Int(double)`
- `Sqr(double)` / `Sin(double)` / `Cos(double)` / `Tan(double)`
- `Atn(double)` / `Log(double)` / `Exp(double)` / `Pi()`
- `Max(int, int)` / `Min(int, int)`

## `Data.cs`
```csharp
public static class Data
```
- `ReadInt(BytesReader)` → int — czyta Int z AMOS Data
- `ReadByte(BytesReader)` → byte
- `ReadString(BytesReader, int)` → string

## `IGameEngine.cs`
```csharp
public interface IGameEngine
```
- `RunOnGameThread(Action)` — wykonaj na głównym wątku gry
- `Ticks` (long)
- `ScreenWidth` / `ScreenHeight` (int)
- `MouseX` / `MouseY` / `MouseLeft` (int)
- `LastKey` / `LastKeyCode` (int) / `Shift` / `Ctrl` (bool)
- `LoadMusic(int bankNumber, Action? onFinished)`
- `PlaySound(int bank, int sample, int volume)`
- `StopMusic()` / `SetVolume(double vol)`
- `Quit()`
- `GetFontData(string name)` → FontInfo
- `GetImagesContainer(string name, int count)` → ImagesContainer
- `ZakladkiPostep(int postep, int max, int y, int szer, string text)` — pasek postępu

## `AmosMusicPlayer.cs`
```csharp
public class AmosMusicPlayer
```
- `Play(AmosMusicData, Action? onFinished)` — odtwarza muzykę
- `Stop()` / `SetVolume(double vol)` / `Update(TimeSpan delta)`

## `MemoryBanks/MemoryBanksManager.cs`
```csharp
public class MemoryBanksManager
```
- `CreateBank(int number, int size)`
- `LoadBank(int number, byte[] data)`
- `GetBank(int number)` → MemoryBank
- `FreeBank(int number)` / `ClearAll()`

## `MemoryBanks/MemoryBank.cs`
```csharp
public class MemoryBank
```
- `Data` (byte[]) / `Size` (int)
- `ReadByte(int offset)` / `WriteByte(int offset, byte)`

## `Screens/ScreensManager.cs` (~1739 linii)
```csharp
public class ScreensManager : IScreensManager
```
Główny menedżer ekranów AMOS. Sekcje:

### Screen management
- `CreateScreen(Screen, PixelMode)` — tworzy ekran
- `SetCurrentScreen(int)` — ustawia aktywny ekran
- `DisplayScreen(int, Display)`
- `RemoveScreen(int)`
- `ScreenCopy(int, int, Rectangle?)` — kopiuje między ekranami
- `ScreenSwap(int, int)` / `ScreenClone(int)`

### Bob management
- `BobDraw(int bob, int image, int x, int y)` — rysuje Bob
- `BobRemove(int)` / `BobUpdate()`
- `BobHide(int)` / `BobShow(int)`
- `BobGet(int, int x, int y, int w, int h)` — przechwytuje obszar
- `BobImage(int, ImageData)` / `BobPriority(int, int)`
- `BobsOff()` / `BobsOn()`
- `BobToFront(int)` / `BobToBack(int)`

### Zone management
- `ZoneCreate(Zone)` — tworzy strefę klikalną
- `ZoneDelete(int)` / `ZoneChange(int, Zone)`
- `ZoneOff(int)` / `ZoneOn(int)`
- `ZoneHit(int x, int y)` → int — sprawdza trafienie w strefę

### Palette management
- `SetColor(int, Color4)` / `GetColor(int)` → Color4
- `PaletteFade(int start, int end, int steps, int delay)`
- `PaletteRoll(int start, int end, int speed)`
- `SetPalette(Color4[], int startIndex)` / `GetPalette()` → Color4[]

### Mouse management
- `ShowMouse()` / `HideMouse()`
- `SetMouse(int imageIndex)`
- `MouseX` / `MouseY` / `MouseLeft` (int)

### Font & Text
- `LoadFont(string name)` — ładuje font
- `SetFont(int)` / `Text(string, int x, int y, int)` / `TextLength(string, int)` → int

### Drawing
- `Box(int x1, int y1, int x2, int y2, int color)`
- `Line(int, int, int, int, int)` / `Plot(int, int, int)`
- `FloodFill(int, int, int)` / `Bar(int, int, int, int, int)`
- `Circle(int, int, int, int)` / `Ellipse(int, int, int, int, int)`
- `Polygon(Point[], int)`

### Bitmap operations
- `GetBitmap(int)` → int[] / `PutBitmap(int, int[])`

### AMAL
- `AmalMove(int bob, string script)` — uruchamia skrypt AMAL
- `AmalStop(int)` / `AmalUpdate()` / `AmalRemove(int)`

### General
- `WaitVbl()` — czeka na vsync
- `ScreenSwitch()` — double buffer
- `Cls(int color)` — czyści ekran
- `FlashOff()` / `FlashOn()`

## `Screens/IScreensManager.cs`
```csharp
public interface IScreensManager
```
Interfejs dla ScreensManager.

## `Screens/Screen.cs`
```csharp
public class Screen
```
- `Width` / `Height` / `Bitplanes` (int)
- `Bitmap` (int[]) / `Palette` (Color4[]) / `Number` (int)
- `Display` (Display)

## `Screens/Display.cs`
```csharp
public class Display
```
- `X` / `Y` / `Width` / `Height` (int) / `Mode` (PixelMode)

## `Screens/PixelMode.cs`
```csharp
public enum PixelMode { Planar, Chunky, TrueColor }
```

## `Screens/Bob.cs`
```csharp
public class Bob
```
- `Image` (ImageData) / `X` / `Y` (int)
- `Visible` (bool) / `Priority` (int)
- `Anim` (AmalAnim) / `Width` / `Height` (int)

## `Screens/Sprite.cs`
```csharp
public class Sprite
```
- `Image` (ImageData) / `X` / `Y` (int) / `Visible` (bool)

## `Screens/Shape.cs`
```csharp
public class Shape
```
- `Bitmap` (int[]) / `Width` / `Height` / `X` / `Y` (int)

## `Screens/Zone.cs`
```csharp
public class Zone
```
- `X1` / `Y1` / `X2` / `Y2` (int) / `Number` (int) / `Active` (bool)

## `Screens/Block.cs`
```csharp
public class Block
```
- `Bitmap` (int[]) / `Width` / `Height` / `X` / `Y` (int)

## `Screens/FadeState.cs`
```csharp
public enum FadeState { None, FadingIn, FadingOut }
```

## `Screens/Drawing.cs`
```csharp
public static class Drawing
```
- `DrawLine(int[] bmp, int w, int h, int x1, int y1, int x2, int y2, int color)`
- `FillRect(int[] bmp, int w, int h, int x, int y, int w2, int h2, int color)`
- `FloodFill(int[] bmp, int w, int h, int x, int y, int color)`
- `DrawCircle(int[] bmp, int w, int h, int cx, int cy, int r, int color)`

## `Screens/IGraphicElement.cs`
```csharp
public interface IGraphicElement
```
- `Draw(int[] bitmap, int w, int h)` / `X` / `Y` / `Width` / `Height`

## `Screens/Amal/AmalInstruction.cs`
```csharp
public abstract class AmalInstruction
```
Bazowa klasa dla instrukcji AMAL. Zawiera `AmalInstructionType` (enum: Move, Jump, Label, Anim, Wait, Stop, Change).

## `Screens/Amal/AmalMove.cs`
```csharp
public class AmalMove : AmalInstruction
```
- `TargetX` / `TargetY` (int) / `Speed` (int) / `BobNumber` (int)

## `Screens/Amal/AmalJump.cs`
```csharp
public class AmalJump : AmalInstruction
```
- `TargetLabel` (string) / `Condition` (AmalCondition)

## `Screens/Amal/AmalLabel.cs`
```csharp
public class AmalLabel : AmalInstruction
```
- `Name` (string)

## `Screens/Amal/AmalAnim.cs`
```csharp
public class AmalAnim
```
- `Instructions` (List<AmalInstruction>) / `CurrentInstruction` (int)
- `BobNumber` (int) / `Active` (bool)

## `Screens/Amal/AmalInfo.cs`
```csharp
public class AmalInfo
```
Metadane animacji AMAL.

## `Screens/Amal/AmalBuilder.cs`
```csharp
public class AmalBuilder
```
- `Build(string script)` → `AmalAnim` — parsuje skrypt AMAL
- `ParseInstructions(string[] tokens)` — tokenizacja
- `ParseCondition(string token)` → `AmalCondition`

---

# 4. AmigaNet.Legion — CORE GAME

> **Ścieżka:** `src/AmigaNet.Legion/AmigaNet.Legion/`  
> **Rola:** Główny port gry — wszystkie mechaniki, ekrany, AI, walka, mapa świata
> **Pliki:** 15

---

## 4.1 `Legion.cs`

> **Kluczowy plik:** ~2231 linii  
> **Klasa:** `partial class Legion : AmosBase` — główny silnik gry

### Pola globalne (główne)
| Tablica | Wymiar | Opis |
|---------|--------|------|
| `GRACZE` | `[8, 2]` | Statystyki graczy |
| `WOJNA` | `[6, 6]` | Macierz sojuszy/wrogości ras |
| `WOJNA_W` | `[6, 6]` | Wagi wojny |
| `ARMIA` | `[41, 11, 31]` | Jednostki: [druzyna, pole, slot] |
| `BRON` | `[120, 11]` | Statystyki broni |
| `RASY` | `[6, 23]` | Dane ras |
| `BUDYNKI` | `[100, 30]` | Budynki |
| `MIASTA` | `[251, 10, 10]` | Miasta na mapie świata |
| `SWIAT` | `[1024, 1024]` | Teren mapy świata |
| `MAPA_ODKRYCIA` | `[32, 32]` | Mgła wojny |

### Metody — Intro / Ekran tytułowy
| Metoda | Opis |
|--------|------|
| `WYKONAJ_INTRO()` | Animacja intro z logo |
| `RYS_TYTULOWA()` | Rysuje ekran tytułowy |
| `CZYTAJ_NAZWE_GRACZA(int gracz)` | Wprowadzanie nazwy gracza |

### Metody — Inicjalizacja świata
| Metoda | Opis |
|--------|------|
| `ROB_PANSTWA()` | Generuje państwa na mapie |
| `TWORZ_MIASTA()` | Rozmieszcza 251 miast |
| `GENERUJ_SWIAT(int seed)` | Generowanie terenu |
| `ROZMIESC_JEDNOSTKI()` | Rozmieszcza początkowe armie |
| `USTAW_WOJNE()` | Ustawia domyślne relacje wojenne |

### Metody — Rysowanie mapy świata
| Metoda | Opis |
|--------|------|
| `RYS_MAPA()` | Renderuje widok mapy świata |
| `RYS_MAPA_TEREN(int x, int y)` | Rysuje tile terenu |
| `RYS_MAPA_MIASTA(int x, int y)` | Rysuje ikony miast |
| `RYS_MAPA_ODKRYCIA()` | Rysuje mgłę wojny |
| `RYS_MAPA_SKROL(int dx, int dy)` | Przewijanie mapy |
| `OBSLUZ_PRZEWIJANIE()` | Obsługa auto-scroll |

### Metody — Wybór drużyny
| Metoda | Opis |
|--------|------|
| `WYBIERZ_DRUZYNE()` | Ekran wyboru drużyny |
| `WYSWIETL_DRUZYNY()` | Wyświetla dostępne drużyny |
| `POTWIERDZ_WYBOR()` | Potwierdza wybór |

### Metody — Główna pętla gry
| Metoda | Opis |
|--------|------|
| `GLOWNA_PETLA()` | Główna pętla stanów gry |
| `PETLA_MAPY()` | Pętla mapy świata (zdarzenia) |
| `PETLA_MIASTA()` | Pętla ekranu miasta |
| `PETLA_WALKI()` | Pętla walki turowej |
| `PETLA_SKLEPU()` | Pętla sklepu |
| `PETLA_EKRANU(int)` | Dystrybucja do pętli |
| `OBSLUZ_WEJSCIE(int key)` | Obsługa klawiatury w gł. pętli |
| `AKTUALIZUJ(int ktora)` | Update per tick |

### Metody — System komunikatów
| Metoda | Opis |
|--------|------|
| `KOMUNIKAT(string, int czas)` | Wyświetla komunikat |
| `KOMUNIKAT_TAK_NIE(string)` → bool | Wybór TAK/NIE |
| `KOMUNIKAT_DLUGI(string[])` | Długi komunikat (przewijany) |
| `KOMUNIKAT_WIELOLINIA(string[])` | Komunikat wieloliniowy |

### Metody — Zapis / Odczyt
| Metoda | Opis |
|--------|------|
| `ZAPISZ_GRE(int slot)` | Zapisuje stan gry do pliku |
| `WCZYTAJ_GRE(int slot)` → bool | Wczytuje stan gry z pliku |
| `POKAZ_EKRAN_ZAPISU()` | Ekran zapisu/odczytu |
| `LISTA_ZAPISOW()` → string[] | Lista slotów |

### Metody — Opcje i pomoc
| Metoda | Opis |
|--------|------|
| `POKAZ_OPCJE()` | Ekran opcji gry |
| `ZMIEN_MUZYKE(int)` | Zmienia utwór |
| `ZMIEN_GLOSNOSC(int)` | Zmienia głośność |
| `POKAZ_POMOC()` | Ekran pomocy |
| `POKAZ_STEROWANIE()` | Ekran sterowania |

### Metody — System zdarzeń
| Metoda | Opis |
|--------|------|
| `SPRAWDZ_ZADANIA()` | Sprawdza czy zadania spełnione |
| `OZNACZ_ZADANIE(int)` | Oznacza zadanie jako zrobione |
| `POKAZ_PODSUMOWANIE()` | Podsumowanie zadania |

### Metody — Pomocnicze
| Metoda | Opis |
|--------|------|
| `LOSUJ(int min, int max)` → int | Losowa liczba |
| `CZAS_OCZEKIWANIA(int sekund)` | Opóźnienie |
| `ZMIEN_TRYB(int tryb)` | Zmiana trybu gry |
| `ZAKONCZ_GRE()` | Zakończenie gry |
| `AKTUALIZUJ_TIMERY()` | Aktualizacja timerów |
| `MIESIAČ(int numer)` → string | Nazwa miesiąca |

---

## 4.2 `LegionData.cs`

> **Plik:** ~600+ linii  
> **Klasa:** `partial class Legion` (sekcja danych)

### Tablice danych
| Tablica | Wymiar | Opis |
|---------|--------|------|
| `ARMIA` | `[41, 11, 31]` | 41 drużyn × 11 pól × 31 slotów |
| `WOJNA` | `[6, 6]` | Stan sojuszy (0=neutr, 1=sojusz, 2=wrog) |
| `WOJNA_W` | `[6, 6]` | Waga konfliktu |
| `GRACZE` | `[8, 2]` | Statystyki graczy [nr_druzyny, flaga_czlowiek] |
| `BRON` | `[120, 11]` | Statystyki 120 broni |
| `RASY` | `[6, 23]` | Opisy 6 ras, zdolności, premie |
| `BUDYNKI` | `[100, 30]` | 100 budynków (koszty, efekty) |
| `MIASTA` | `[251, 10, 10]` | 251 miast (pozycja, budynki, prod, armia) |
| `SWIAT` | `[1024, 1024]` | Teren: 0=morze, 1=step, 2=las, 3=pustynia, 4=góry, 5=bagno, 6=tundra |
| `MAPA_ODKRYCIA` | `[32, 32]` | Mgła wojny |
| `DROGI` | `[4096, 4]` | Drogi między miastami |
| `PROJEKTILE` | `List<Projectile>` | Pool pocisków (max 30) |
| `BOHATEROWIE_INTEL` | `int[]` | Bohaterowie i ich intelligence |
| `STOLEE` | `Dictionary<string, int>` | Stolice ras |

### Struktury
```csharp
public struct Projectile
```
- `X` / `Y` (float) — pozycja
- `TargetX` / `TargetY` (float) — cel
- `Speed` (float) / `Angle` (float)
- `Active` (bool) / `Damage` (int)
- `Type` (ProjectileType) — Arrow, Bullet, Magic, Spear
- `Frame` (int) / `Owner` (int) / `GraphicIndex` (int)

### Konfiguracja
- `UZIEMIENIE` (bool) — zatrzymaj przy krawędzi mapy
- `WEWNETRZNY_RAM` (int) — licznik ramek
- `WIDTH` / `HEIGHT` (int) — wymiary ekranu
- `MAKS_ZASOBOW` (int[]) — limit zasobów
- `CENNIK` (int[]) — ceny jednostek/usług

---

## 4.3 `LegionDataLoader.cs`

> **Plik:** ~400+ linii  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `WCZYTAJ_DANE()` | Główny loader — wywołuje wszystkie poniższe |
| `WCZYTAJ_BRON(string)` | Wczytuje 120 broni z pliku |
| `WCZYTAJ_RASY(string)` | Wczytuje 6 ras |
| `WCZYTAJ_BUDYNKI(string)` | Wczytuje 100 budynków |
| `WCZYTAJ_MIASTA(string)` | Wczytuje 251 miast |
| `WCZYTAJ_DIALOGI(string)` | Wczytuje dialogi (gadka.txt) |
| `WCZYTAJ_PRZYGODY(string)` | Wczytuje zdarzenia losowe |
| `WCZYTAJ_GUL_FILE(string)` | Wczytuje plik GUL (grafiki) |
| `WCZYTAJ_STRINGI(string)` | Wczytuje napisy |
| `PARS_UJ_MIASTO(string)` → int[] | Parsuje linię miasta |

---

## 4.4 `LegionStrings.cs`

> **Plik:** Słownik lokalizacyjny  
> **Klasa:** `partial class Legion`

- `STRINGS[0..999]` — tablica stringów PL
- `WCZYTAJ_STRINGI(string)` — wczytuje z pliku lub używa domyślnych
- Indekser `this[int indeks]`
- Zawiera: miesiące (`STYCZEN`–`GRUDZIEN`), komunikaty, opisy przedmiotów, rasy

---

## 4.5 `LegionArmia.cs`

> **Plik:** Ekran zarządzania armią  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `POKAZ_ROZKAZY()` | Główny ekran wydawania rozkazów |
| `RYS_ROZKAZY()` | Renderuje interfejs rozkazów |
| `OBSLUZ_ROZKAZY(int key)` | Obsługa klawiatury w trybie rozkazów |
| `WYSLIJ_JEDNOSTKE(int, int x, int y)` | Wysyła jednostkę na mapie |
| `REKRUTUJ(int jednostka)` | Rekrutacja nowej jednostki |
| `WYSWIETL_LISTE_JEDNOSTEK()` | Lista jednostek w drużynie |
| `SZCZEGOLY_JEDNOSTKI(int)` | Panel szczegółów jednostki |
| `LECZ_RANNYCH(int)` | Leczenie w miastach |
| `POLACZ_DRUZYNY(int src, int dst)` | Łączenie drużyn |
| `UZUPELNIJ(int)` | Uzupełnienie stanu jednostki |
| `ROZWIAZ_DRUZYNE(int)` | Rozwiązanie drużyny |
| `PRZEMIESZCZ_JEDNOSTKE(int, int slot)` | Przenosi jednostkę między slotami |
| `ZMIEN_NAZWE_DRUZYNY(int, string)` | Zmiana nazwy drużyny |
| `POKAZ_STATYSTYKI_DRUZYNY(int)` | Statystyki całej drużyny |

---

## 4.6 `LegionMainAction.cs`

> **Plik:** ~2000 linii — Serce systemu walki  
> **Klasa:** `partial class Legion`

### Główne akcje walki
| Metoda | Opis |
|--------|------|
| `WYKONAJ_AKCJE()` | Główna procedura walki turowej |
| `A_RUCH(int jednostka)` | Ruch jednostki (AI/gracz) z pathfindingiem |
| `A_ATAK(int, int cx, int cy)` | Atak wręcz |
| `A_STRZELAJ(int jednostka, int cel)` | Strzał dystansowy |
| `A_BRON_MAGICZNA(int, int czar)` | Użycie broni magicznej |
| `A_BRON_SPECJALNA(int, int bron)` | Użycie broni specjalnej |
| `A_BRON_PALNA(int, int cel)` | Użycie broni palnej |
| `A_LECZ(int, int cel)` | Leczenie sojusznika |
| `A_BLOKUJ(int)` | Tryb blokowania |
| `A_BRON_SMIERCI(int)` | Specjalna broń śmierci |
| `A_WYCOFAJ(int, int kierunek)` | Wycofanie z walki |
| `A_BRON_SIECI(int, int cel)` | Sieć (paraliż) |
| `A_ZAKLESNIJ(int, int cel)` | Zaklęcie unieruchomienia |
| `A_TRYBAUTO(int)` | Tryb auto-fire |
| `A_OBSLUGA_AUTO(int)` | Automatyczny atak z auto-aim |

### AI
| Metoda | Opis |
|--------|------|
| `AI_WYKONAJ_RUCH(int)` | Sztuczna inteligencja — wybór akcji |
| `AI_OCEN_ZAGROZENIA(int)` → int | Ocena zagrożenia |
| `AI_OCEN_CEL(int atak, int obr)` → int | Ocena wartości celu |
| `AI_WYBIERZ_CEL(int)` → int | Wybór najlepszego celu |
| `AI_OCEN_POLE(int x, int y, int)` → int | Ocena atrakcyjności pola |
| `AI_SPRAWDZ_ZASIEKI(int)` → bool | Czy jednostka otoczona |
| `AI_SZUKAJ_LOOTU(int)` | Szukanie łupów |
| `AI_OCEN_ODLEGLOSC(int, int, int, int)` → int | |

### Pathfinding (wbudowany)
| Metoda | Opis |
|--------|------|
| `SZUKAJ_DROGI(int, int, int, int)` → List<Point> | Wrapper na Pathfinder.FindPath |
| `RuszJednostkeDo(int, int, int)` | Płynny ruch z pathfindingiem |
| `CzyMoznaStanac(int, int, int)` → bool | Sprawdza czy pole jest osiągalne |
| `STAWKA_TERENU(int)` → int | Koszt wejścia na dany teren |

### Ranged Combat
| Metoda | Opis |
|--------|------|
| `OBLICZ_CELNOSC(int, int, int d)` → int | Szansa trafienia |
| `WYKONAJ_STRZAL(int, int, int cx, int cy)` | Wykonuje strzał z animacją pocisku |
| `OBSLUZ_TRAFIENIE(int a, int o, int b, int dmg)` | Obsługa trafienia: obrażenia, pancerz |
| `OBSLUZ_PUDLO(int)` | Obsługa pudła |
| `OBSLUZ_KRYTYCZNY(int, int, int dmg)` | Krytyczne trafienie |

### Projectile System
| Metoda | Opis |
|--------|------|
| `DODAJ_POCISK(int typ, float x, float y, float dx, float dy, int dmg, int owner)` | Dodaje pocisk do pool'a |
| `AKTUALIZUJ_POCISKI(float delta)` | Aktualizuje pozycje pocisków |
| `RYS_POCISKI()` | Renderuje pociski |
| `SPRAWDZ_KOLIZJE_POCISKOW()` | Kolizja z jednostkami |
| `USUN_POCISK(int)` | Usuwa pocisk z pool'a |

### Efekty
| Metoda | Opis |
|--------|------|
| `OBSLUZ_EKSPLOZJA(int, int, int r, int dmg, int owner)` | Eksplozja (magia ognista) |
| `OBSLUZ_TRUCIZNA(int, int dmg, int tury)` | Efekt zatrucia |
| `OBSLUZ_OGNIENIE(int, int tury)` | Podpalenie |
| `OBSLUZ_PARALIZ(int, int tury)` | Paraliż |
| `OBSLUZ_UZDROWIENIE(int, int hp)` | Usunięcie negatywnych efektów |
| `OBSLUZ_SMIERC(int)` | Obsługa śmierci jednostki |
| `USUN_Z_TRUPEM(int, int)` | Zostawienie zwłok |
| `OBSLUZ_KONTUZJA(int jednostka)` | Obsługa kontuzji |

### Animacje walki
| Metoda | Opis |
|--------|------|
| `ANIMUJ_ATAK(int, int)` | Animacja ataku wręcz |
| `ANIMUJ_RUCH(int, int dx, int dy)` | Animacja ruchu |
| `ANIMUJ_TRAFIENIE(int)` | Animacja otrzymania ciosu |
| `ANIMUJ_SMIERC(int)` | Animacja śmierci |
| `ANIMUJ_BLOK(int)` | Animacja bloku |

### Wybór celu
| Metoda | Opis |
|--------|------|
| `WYBIERZ_CEL(int, int tryb)` → int | Interaktywny wybór celu |
| `POKAZ_ZASIEG(int)` | Zaznaczenie zasięgu ruchu |
| `POKAZ_ZASIEG_STRZALU(int)` | Zasięg strzału |
| `POKAZ_ZASIEG_MAGII(int, int czar)` | Zasięg magii |
| `POKAZ_POLE_WIDZENIA(int)` | Linia widzenia |

---

## 4.7 `LegionMapaAkcja.cs`

> **Plik:** Pętla mapy świata  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `WYKONAJ_AKCJE_MAPY()` | Główna pętla mapy: ruch wojsk AI, wydarzenia |
| `AI_RUCH_PO_MAPIE(int druzyna)` | AI — ruch armii po mapie |
| `AI_OCEN_MIASTO_DO_ATAKU(int)` → int | Które miasto zaatakować |
| `AI_SPRAWDZ_OBCONOSC_MIASTA(int)` | Czy miasto jest obce |
| `AI_ZBIERAJ_ARMIE(int)` | Zbieranie rozproszonych jednostek |
| `AI_WYCOFAJ_SIE(int)` | AI — odwrót |
| `WYKRYJ_KOLIZJE_Z_WROGIEM()` | Kolizja armii na mapie → walka |
| `OBSLUZ_ATALK_NA_MIASTO(int miasto, int atakujacy)` | Atak AI na miasto |
| `OBSLUZ_OBLONA_MIASTA(int)` | Oblężenie miasta |
| `PRZELICZ_PRODUKCJE()` | Przeliczenie produkcji miast |
| `DODAJ_ZASOBY(int gracz, int zloto, int zywnosc)` | Dodaje zasoby graczowi |
| `WYKRYJ_ZWYCIESTWO()` → bool | Sprawdza warunek zwycięstwa |
| `PRZELICZ_WYDAZENIA_LOSOWE()` | Zdarzenia losowe na mapie |
| `PRZELICZ_DYPLOMACJE()` | Automatyczna dyplomacja AI |
| `OBSLUZ_PRZYGODY()` | System przygód/questów |
| `PRZELICZ_MIGRACJE()` | Migracje ludności między miastami |
| `WYKRYJ_KONIEC_TURY()` | Koniec tury gracza |
| `ZMIEN_TURE(int gracz)` | Zmiana tury między graczami |

---

## 4.8 `LegionMiasto.cs`

> **Plik:** ~490 linii — Ekran miasta  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `WEJDZ_DO_MIASTA(int miasto)` | Wejście do miasta |
| `RYS_MIASTO()` | Renderuje ekran miasta |
| `OBSLUZ_MIASTO(int key)` | Obsługa klawiatury w mieście |
| `OPUSĆ_MIASTO()` | Wyjście z miasta na mapę |
| `POKAZ_INFORMACJE_O_MIESCIE(int)` | Info o mieście |
| `POKAZ_BUDYNKI_MIASTA()` | Lista budynków w mieście |
| `KUP_BUDYNEK(int budynek)` | Zakup budynku |
| `ZATRUDNIJ_PRACOWNIKOW(int budynek, int ile)` | Zatrudnienie |
| `SPRAWDZ_DOSTEPNE_BUDYNKI()` → int[] | Co można zbudować |
| `WEJDZ_DO_KARCZMY()` | Wejście do karczmy (rekrutacja) |
| `WEJDZ_DO_SWIATYNI()` | Świątynia (leczenie, błogosławieństwo) |
| `WEJDZ_DO_KOSZAR()` | Koszary (rekrutacja wojsk) |
| `POKAZ_SKRZYNIA_MIASTA()` | Magazyn miasta |
| `PRZELICZ_LOJALNOSC_MIASTA()` | Lojalność miasta |

---

## 4.9 `LegionRysujScenerie.cs`

> **Plik:** Rendering scenerii walki  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `RYS_SCENERIE(int typ)` | Renderuje scenerię bitwy |
| `RYS_LAS()` | Rysuje las |
| `RYS_STEP()` | Rysuje step |
| `RYS_PUSTYNIA()` | Rysuje pustynię |
| `RYS_GORY()` | Rysuje góry |
| `RYS_BAGNO()` | Rysuje bagno |
| `RYS_TUNDRA()` | Rysuje tundrę |
| `RYS_MIASTO_WALKA(int miasto)` | Rysuje miasto w tle walki |
| `RYS_TLO_WODA()` | Tło wody/morza |
| `RYS_ELEMENTY_SCENERII(Element[] el)` | Elementy dekoracyjne (drzewa, skały) |
| `RYS_ANIMACJA_SCENERII(float time)` | Animacja tła (woda, ogień) |

---

## 4.10 `LegionSklep.cs`

> **Plik:** ~475 linii — System sklepów  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `WEJDZ_DO_SKLEPU(int miasto)` | Wejście do sklepu |
| `RYS_SKLEP()` | Renderuje interfejs sklepu |
| `OBSLUZ_SKLEP(int key)` | Obsługa klawiatury w sklepie |
| `KUP_PRZEDMIOT(int przedmiot, int ilosc)` | Zakup przedmiotu |
| `SPRZEDAJ_PRZEDMIOT(int przedmiot, int ilosc)` | Sprzedaż przedmiotu |
| `POKAZ_ASORTYMENT()` → int[] | Lista dostępnych przedmiotów |
| `POKAZ_EKWIUNEK_GRACZA()` | Ekwipunek gracza |
| `PRZECIAGNIJ_PRZEDMIOT(int src, int dst)` | Drag-and-drop w ekwipunku |
| `SPRAWDZ_CENE(int przedmiot)` → int | Cena przedmiotu |
| `SPRAWDZ_DOSTEPNOSC(int przedmiot)` → bool | Czy dostępny w mieście |
| `WEJDZ_DO_SPICHERZA()` | Spichlerz (handel żywnością) |
| `KUP_ZYWNOSC(int ile)` | Zakup żywności |
| `SPRZEDAJ_ZYWNOSC(int ile)` | Sprzedaż żywności |

---

## 4.11 `LegionWybor.cs`

> **Plik:** ~701 linii — Ekwipunek i awans  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `WYBIERZ_BRON(int jednostka)` | Ekran wyboru broni (drag-drop) |
| `POKAZ_EKWIUNEK(int)` | Wyświetla ekwipunek jednostki |
| `WYPOSAZ_BRON(int jednostka, int bron)` | Zakładanie broni |
| `ZDEJMIJ_BRON(int jednostka, int slot)` | Zdejmowanie broni |
| `ZAMIEŃ_BRON(int jednostka, int src, int dst)` | Zamiana slotów |
| `POKAZ_STATYSTKI_BRONI(int)` → string | Statystyki broni |
| `WYBIERZ_PANCERZ(int)` | Wybór pancerza |
| `WYBIERZ_AKCESORIUM(int)` | Wybór akcesorium |
| `AWANS(int jednostka)` | Ekran awansu (wydawanie XP) |
| `PRZYDZIEL_PUNKTY(int, int sila, int zreczn, int intelig, int wytrzym)` | Rozdawanie punktów |
| `POKAZ_POSTEP_AWANSU(int)` | Pasek postępu do następnego levelu |
| `NAUCZ_UMIEJETNOŚCI(int, int skill)` | Nauka umiejętności |
| `POKAZ_UMIEJETNOŚCI(int)` → string[] | Lista umiejętności |
| `OPIS_UMIEJETNOŚCI(int)` → string | Opis umiejętności |

---

## 4.12 `LegionGadka.cs`

> **Plik:** ~34205 znaków — System dialogów  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `ROZPOCZNIJ_DIALOG(int id)` | Rozpoczyna dialog z NPC |
| `RYS_DIALOG()` | Renderuje okno dialogowe |
| `OBSLUZ_DIALOG(int key)` | Obsługa wyboru odpowiedzi |
| `WYSWIETL_WYPOWIEDZ(string text, string mowca)` | Wyświetla kwestię |
| `POKAZ_OPCJE_DIALOGU(string[] opcje)` | Opcje odpowiedzi |
| `WYBIERZ_OPCJE(int idx)` | Wybór odpowiedzi |
| `SPRAWDZ_WARUNEK_DIALOGU(int warunek)` → bool | Sprawdza warunek dialogu |
| `WYKONAJ_AKCJE_DIALOGU(int akcja)` | Wykonuje akcję dialogową (daj przedmiot, złoto) |
| `SPRAWDZ_DOSTEPNE_DIALOGI(int miasto)` → int[] | Które dialogi dostępne |
| `ZAKONCZ_DIALOG()` | Kończy dialog |

---

## 4.13 `LegionArchive.cs`

> **Plik:** Zapis/odczyt stanu gry  
> **Klasa:** `partial class Legion`

| Metoda | Opis |
|--------|------|
| `ZAPISZ_GRE(int slot)` | Zapisuje pełny stan gry |
| `WCZYTAJ_GRE(int slot)` → bool | Wczytuje stan gry |
| `SZUKAJ_ZAPISOW()` → string[] | Znajduje zapisane gry |
| `USUN_ZAPIS(int slot)` | Usuwa zapis |
| `ZAPISZ_DO_PLIKU(string path)` → bool | Zapis do pliku binarnego |
| `WCZYTAJ_Z_PLIKU(string path)` → bool | Odczyt z pliku binarnego |
| `SERIALIZUJ_TABLICE(int[,,] arr, BinaryWriter)` | Serializacja tablic 3D |
| `DESERIALIZUJ_TABLICE_3D(BinaryReader, int, int, int)` → int[,,] | Deserializacja tablicy 3D |
| `SERIALIZUJ_TABLICE_2D(int[,], BinaryWriter)` | Serializacja tablicy 2D |
| `DESERIALIZUJ_TABLICE_2D(BinaryReader, int, int)` → int[,] | Deserializacja tablicy 2D |
| `SERIALIZUJ_PROJECTILE(BinaryWriter)` | Serializacja pocisków |
| `DESERIALIZUJ_PROJECTILE(BinaryReader)` | Deserializacja pocisków |

---

## 4.14 `Pathfinding/Pathfinder.cs`

> **Plik:** A* pathfinder  
> **Klasa:** `public class Pathfinder`

| Metoda | Opis |
|--------|------|
| `FindPath(NavGrid, Point start, Point end)` → List<Point> | Główny A* pathfinder |
| `FindPathWithParams(NavGrid, Point, Point, int maxSteps, bool allowDiag)` → List<Point> | A* z parametrami |
| `Heuristic(Point a, Point b)` → int | Heurystyka Manhattan |
| `GetNeighbors(Point p, bool allowDiagonal)` → List<Point> | Sąsiedzi węzła |
| `ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)` → List<Point> | Odtwarza ścieżkę |

---

## 4.15 `Pathfinding/NavGrid.cs`

> **Plik:** Siatka nawigacyjna  
> **Klasa:** `public class NavGrid`

| Metoda / Pole | Opis |
|--------|------|
| `Cells` (bool[,]) — siatka walkability |
| `Width` / `Height` (int) |
| `CellSize` = 8 (stała) — rozmiar komórki |
| `NavGrid(int width, int height)` | Konstruktor |
| `SetWalkable(int x, int y, bool walkable)` | Ustawia czy komórka dostępna |
| `IsWalkable(int x, int y)` → bool | Sprawdza dostępność |
| `WorldToGrid(float wx, float wy)` → Point | Konwersja współrzędnych świata na grid |
| `GridToWorld(int gx, int gy)` → Point | Konwersja grid → świat |
| `RasterizeZoneScreen(int[] bitmap, int w, int h)` | Rasteryzuje zone screen na siatkę |

---

# 5. AmigaNet.Legion.DesktopApp

> **Ścieżka:** `src/AmigaNet.Legion/AmigaNet.Legion.DesktopApp/`  
> **Rola:** Aplikacja desktopowa MonoGame  
> **Pliki:** 6

## `Program.cs`
```csharp
public static class Program
```
- `Main()` — entry point, tworzy i uruchamia `LegionGame`

## `LegionGame.cs`
```csharp
public class LegionGame : Game, IGameEngine
```
Implementacja MonoGame. Główna pętla gry:

| Metoda | Opis |
|--------|------|
| `LegionGame()` | Konstruktor: inicjuje MonoGame, tworzy Legion |
| `Initialize()` | Inicjalizacja: okno, graficzne ustawienia |
| `LoadContent()` | Ładuje assety (grafiki, fonty, dźwięki) |
| `Update(GameTime)` | Główny update — wywołuje `Legion.AKTUALIZUJ()` |
| `Draw(GameTime)` | Renderowanie — wywołuje `Legion.RYS()` |
| `RunOnGameThread(Action)` | Synchronizuje akcję na głównym wątku |
| `LoadMusic(int bank, Action?)` | Ładuje muzykę przez SharpMod |
| `PlaySound(int bank, int sample, int vol)` | Odtwarza dźwięk |
| `StopMusic()` | Zatrzymuje muzykę |
| `SetVolume(double)` | Ustawia głośność |

Właściwości IGameEngine: `Ticks`, `ScreenWidth`, `ScreenHeight`, `MouseX`, `MouseY`, `MouseLeft`, `LastKey`, `LastKeyCode`, `Shift`, `Ctrl`.

## `FrameCounter.cs`
```csharp
public class FrameCounter
```
- `Update(float deltaTime)` — aktualizacja FPS
- `FrameRate` (float) — bieżące FPS
- `TotalFrames` (long) — licznik klatek

## `KeyInfo.cs`
```csharp
public static class KeyInfo
```
Mapowanie klawiszy MonoGame na kody używane przez silnik Legion.
- `Keys[]` — tablica mapowania
- `MonoGameKeyToLegionKey(Keys)` → int — konwersja

## `XnaSoundRenderer.cs`
```csharp
public class XnaSoundRenderer : IRenderer
```
Renderuje dźwięk przez MonoGame/XNA.

| Metoda | Opis |
|--------|------|
| `Init(string path)` | Inicjalizacja |
| `Load(byte[] data)` | Ładuje sample |
| `Play(int sampleIndex, int volume)` | Odtwarza sample |
| `StopAll()` | Zatrzymuje wszystkie dźwięki |
| `SetVolume(double)` | Głośność |

## `MonoGameLibLoader.cs`
```csharp
public class MonoGameLibLoader
```
Ładuje biblioteki native MonoGame.

---

# 6. SharpMod.Core

> **Ścieżka:** `src/AmigaNet.Legion/SharpMod.Core/`  
> **Rola:** Odtwarzacz modułów (MOD/S3M/XM/IT)  
> **Pliki:** ~40

## `ModulePlayer.cs`
```csharp
public class ModulePlayer
```
Główny odtwarzacz:
- `Play(string path)` — odtwarza plik modułu
- `Play(Module module)` — odtwarza Module
- `Stop()` / `Pause()` / `Resume()`
- `SetPosition(int row, int pattern)`
- `Update(float delta)` — renderuje bufor audio
- `GetModule()` → Module

## `SampleLoader.cs`
- `LoadSamples(Module, string basePath)` — ładuje sample z plików

## `SampleFormatFlags.cs`
- Enum `SampleFormatFlags` — PCM, ADPCM, Delta, Float

## `ModuleLoader.cs`
```csharp
public class ModuleLoader
```
- `Load(string path)` → Module — ładuje dowolny format
- `FindLoader(string ext)` → ILoader

## `ILoader.cs`
```csharp
public interface ILoader
```
- `Load(ModBinaryReader, string path)` → Module

## `IRenderer.cs`
```csharp
public interface IRenderer
```
- `Init(string path)` / `Load(byte[])` / `Play(int, int)` / `StopAll()` / `SetVolume(double)`

## `Helper.cs`
Funkcje pomocnicze: konwersje częstotliwości, obliczenia.

## `WaveTable.cs`
Tablica fal dla syntezy.

## `SharpModEventArgs.cs`
Event args dla zdarzeń odtwarzacza.

### Loadery

| Plik | Klasa | Opis |
|------|-------|------|
| `Loaders/MODLoader.cs` | `MODLoader : ILoader` | Ładuje MOD (ProTracker) |
| `Loaders/XMLoader.cs` | `XMLoader : ILoader` | Ładuje XM (FastTracker II) |
| `Loaders/S3MLoader.cs` | `S3MLoader : ILoader` | Ładuje S3M (ScreamTracker) |
| `Loaders/M15Loader.cs` | `M15Loader : ILoader` | Ładuje M15 (SoundTracker 15 instr) |
| `Loaders/MTMLoader.cs` | `MTMLoader : ILoader` | Ładuje MTM (MultiTracker) |
| `Loaders/ITLoader.cs` | `ITLoader : ILoader` | Ładuje IT (Impulse Tracker) |

### Mixer

| Plik | Klasa | Opis |
|------|-------|------|
| `Mixer/ChannelsMixer.cs` | `ChannelsMixer` | Miksowanie kanałów audio |
| `Mixer/ChannelInfo.cs` | `ChannelInfo` | Stan kanału (periodic, volume, panning, instrument) |

### Player

| Plik | Klasa | Opis |
|------|-------|------|
| `Player/Player.cs` | `Player` | Niskopoziomowy player modułów |
| `Player/MixConfig.cs` | `MixConfig` | Konfiguracja miksera (samplerate, channels, bits) |
| `Player/ChannelMemory.cs` | `ChannelMemory` | Stan pamięci kanału (note, instrument, volume, effect) |
| `Player/EnvelopeFlags.cs` | `EnvelopeFlags` | Flagi obwiedni (Volume, Panning, PitchFreq, Sustain, Loop, Carry) |
| `Player/EnvPt.cs` | `EnvPt` | Punkt obwiedni (position, value) |
| `Player/EnvPr.cs` | `EnvPr` | Parametry obwiedni (Enabled, Sustain, Loop, Points) |
| `Player/ActionsEnum.cs` | `ActionsEnum` | Enum akcji playera (NoteOff, NoteCut, Volume, Panning, Retrig) |

### Song Model

| Plik | Klasa | Opis |
|------|-------|------|
| `Song/Module.cs` | `Module` | Główny model modułu: patterns, instruments, samples, sequence |
| `Song/Pattern.cs` | `Pattern` | Wzór (pattern): siatka PatternCell[row, channel] |
| `Song/PatternCell.cs` | `PatternCell` | Komórka wzoru: note, instrument, volume, effect, effectArg |
| `Song/Track.cs` | `Track` | Ścieżka (kolumna w patternie) |
| `Song/Instrument.cs` | `Instrument` | Instrument: sample map, volume/panning/pitch envelope, fadeout |
| `Song/Sample.cs` | `Sample` | Sampled audio: data, loop points, volume, finetune, panning |
| `Song/UniTrkHelper.cs` | `UniTrkHelper` | Helper konwersji UniTracker |

### DSP

| Plik | Klasa | Opis |
|------|-------|------|
| `DSP/FFT.cs` | `FFT` | Fast Fourier Transform (analiza widma) |
| `DSP/AudioProcessor.cs` | `AudioProcessor` | Procesing audio: equalizer, efekty |

### UniTracker

| Plik | Klasa | Opis |
|------|-------|------|
| `UniTracker/UniMod.cs` | `UniMod` | Uniwersalny format wewnętrzny modułu |
| `UniTracker/UniTrk.cs` | `UniTrk` | Serializacja/deserializacja UniTracker |
| `UniTracker/UniModFlags.cs` | `UniModFlags` | Flagi UniMod |
| `UniTracker/Effects.cs` | `Effects` | Definicje efektów (EFF_*) |

### SoundRenderer

| Plik | Klasa | Opis |
|------|-------|------|
| `SoundRenderer/WaveExporter.cs` | `WaveExporter` | Eksport modułu do pliku WAV |

### IO

| Plik | Klasa | Opis |
|------|-------|------|
| `IO/ModBinaryReader.cs` | `ModBinaryReader` | Reader binarny specyficzny dla modułów (little-endian, big-endian) |

### Exceptions

| Plik | Klasa | Opis |
|------|-------|------|
| `Exceptions/SharpModException.cs` | `SharpModException` | Wyjątek |
| `Exceptions/SharpModExceptionResources.Designer.cs` | `SharpModExceptionResources` | Zasoby wyjątków |
