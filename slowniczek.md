# Słowniczek — wyrażenia z programu gry LegionExpanded

> Port gry "Legion" (1994, Marcin Puchta) z AMOS BASIC na C# .NET 6 / MonoGame.
> Program niemal 1:1 zachowuje polskie (i częściowo angielskie) nazwy oryginału —
> poniżej wyjaśnienie skrótów, stałych i zmiennych używanych w kodzie.

---

## Spis treści

1. [Stałe indeksów ARMIA — statystyki wojownika (prefiks `T`)](#1-statystyki-wojownika-t-)
2. [Sloty ekwipunku](#2-sloty-ekwipunku)
3. [Pola broni (prefiks `B`)](#3-pola-broni-b-)
4. [Pola miasta (prefiks `M`)](#4-pola-miasta-m-)
5. [Pola przygód (prefiks `P`)](#5-pola-przygod-p-)
6. [Pola ras (`RASY`)](#6-pola-ras-rasy)
7. [Główne tablice stanu gry](#7-glowne-tablice-stanu-gry)
8. [Pozostałe zmienne i stałe](#8-pozostale-zmienne-i-stale)
9. [Wyrażenia walki](#9-wyrazenia-walki)
10. [Typy terenu](#10-typy-terenu)
11. [Wyrażenia systemowe / grafika](#11-wyrazenia-systemowe--grafika)

---

## 1. Statystyki wojownika (`T*`)

Tablica `ARMIA[41, 11, 31]` = `[drużyna, pole, slot]`, gdzie **slot** to statystyka.
Przedrostek `T` pochodzi z oryginału (pola *tablicy*). Slot `0` to wódz/dowódca drużyny, `1–10` to wojownicy.

| Stała | Wartość | Znaczenie |
|-------|:-------:|-----------|
| `TEM`  | 0 | Maksymalna energia (życie) wojownika |
| `TX`   | 1 | Współrzędna X na ekranie walki |
| `TY`   | 2 | Współrzędna Y na ekranie walki |
| `TSI`  | 3 | **Siła** — obrażenia w walce wręcz |
| `TSZ`  | 4 | **Zręczność** — szybkość ataku (im mniejsza, tym szybciej) |
| `TCELX`| 5 | Cel ataku — współrzędna X |
| `TCELY`| 6 | Cel ataku — współrzędna Y |
| `TTRYB`| 7 | Tryb walki jednostki (0/1/2 — system auto-obrony) |
| `TE`   | 8 | **Energia bieżąca** (aktualne HP) |
| `TP`   | 9 | **Odporność** (ang. Resistance) — obrona, redukcja otrzymywanych obrażeń |
| `TBOB` | 10 | Bazowy numer Boba (grafiki wojownika) |
| `TKLAT`| 11 | Klatka animacji (bieżący frame) |
| `TAMO` | 12 | Amunicja drużyny / ilość strzał |
| `TMAG` | 26 | Magia bieżąca |
| `TDOSW`| 27 | **Doświadczenie** (XP) — waluta treningu i awansu |
| `TRASA`| 28 | Rasa wojownika (indeks do `RASY`) |
| `TWAGA`| 29 | Waga (udźwig) jednostki |
| `TMAGMA`| 30 | **Magia maksymalna** |

**Uwaga:** w oryginale nazwa angielskiego ekranu postaci brzmiała:
*Energy*, *Strength*, *Speed*, *Resistance*, *Magic* → `TE/TEM`, `TSI`, `TSZ`, `TP`, `TMAG/TMAGMA`.

---

## 2. Sloty ekwipunku

Pozycje na ciele wojownika i plecak — te same indeksy co statystyki `T*`:

| Stała | Wartość | Znaczenie |
|-------|:-------:|-----------|
| `TGLOWA` | 13 | Hełm / nakrycie głowy |
| `TKORP` | 14 | Pancerz / ochrona korpusu |
| `TNOGI` | 15 | Nogi / buty |
| `TLEWA` | 16 | Lewa ręka |
| `TPRAWA` | 17 | Prawa ręka |
| `TPLECAK` | 18–25 | Plecak — 8 slotów przedmiotów (0 = puste) |

---

## 3. Pola broni (`B*`)

Tablica `BRON[121, 12]` = statystyki broni/przedmiotów. Przedrostek `B`.

| Stała | Wartość | Znaczenie |
|-------|:-------:|-----------|
| `B_SI` | 1 | Siła / moc obrażeń |
| `B_PAN` | 2 | Pancerz / obrona przedmiotu |
| `B_SZ` | 3 | Szybkość / modyfikator zręczności |
| `B_EN` | 4 | Energia (koszt / modyfikator życia) |
| `B_TYP` | 5 | Typ broni (indeks do `BRON2_S` — "axe", "sword", "bow"…) |
| `B_WAGA` | 6 | Waga przedmiotu |
| `B_PLACE` | 7 | Pole/slot, do którego wkłada się przedmiot |
| `B_DOSW` | 8 | Doświadczenie (np. za trafienie) / parametr zaklęcia |
| `B_MAG` | 9 | Magia (koszt lub ilość magii broni) |
| `B_CENA` | 10 | Cena w złocie |
| `B_BOB` | 11 | Numer Boba (grafika przedmiotu) |

---

## 4. Pola miasta (`M*`)

Tablica `MIASTA[51, 21, 7]` = 50 miast × 20 pól budynków × parametry.

| Stała | Wartość | Znaczenie |
|-------|:-------:|-----------|
| `M_MUR` | 0 | Poziom muru / infrastruktury (podnosi ceny w sklepie) |
| `M_X` | 1 | Współrzędna X miasta na mapie świata |
| `M_Y` | 2 | Współrzędna Y miasta na mapie świata |
| `M_LUDZIE` | 3 | Typ budynku/ludności na polu (np. `9` = spichlerz) |
| `M_PODATEK` | 4 | Podatek / dochód z pola |
| `M_CZYJE` | 5 | Czyje miasto — numer drużyny właściciela |
| `M_MORALE` | 6 | Morale / lojalność miasta |

---

## 5. Pola przygód (`P*`)

Tablica `PRZYGODY[4, 11]` — aktywne przygody/questy.

| Stała | Wartość | Znaczenie |
|-------|:-------:|-----------|
| `P_TYP` | 0 | Typ przygody |
| `P_X` | 1 | Współrzędna X miejsca zdarzenia |
| `P_Y` | 2 | Współrzędna Y |
| `P_TERMIN` | 3 | Termin / czas na wykonanie |
| `P_KIERUNEK` | 4 | Kierunek wyprawy |
| `P_LEVEL` | 5 | Poziom trudności |
| `P_CENA` | 6 | Cena wyprawy |
| `P_NAGRODA` | 7 | Nagroda za wykonanie |
| `P_BRON` | 8 | Broń związana z przygodą (nagroda/znalezisko) |
| `P_TEREN` | 9 | Teren docelowy |
| `P_STAREX` | 10 | Współrzędne startowe (X) |

---

## 6. Pola ras (`RASY`)

Tablica `RASY[21, 8]` — parametry ras (0–19 ras w oryginale).

| Indeks | Znaczenie |
|:------:|-----------|
| `0` | Bazowa energia: `TE = Rnd(20) + RASY(r,0) * 3` |
| `1` | Bazowa siła: `TSI = Rnd(10) + RASY(r,1)/2` |
| `2` | Bazowa zręczność: `TSZ = Rnd(10) + RASY(r,2)` |
| `3` | Bazowa magia: `TMAG = Rnd(5) + RASY(r,3)` |
| `4` | Typ/klasa jednostki danej rasy |
| `5` | Odporność/pancerz rasy (inicjuje `TP` i `TKORP`) |
| `6` | "Mundur" — startowa broń rasy oraz współczynnik zdobywania doświadczenia |
| `7` | (zarezerwowane) |

---

## 7. Główne tablice stanu gry

| Tablica | Wymiar | Znaczenie |
|---------|--------|-----------|
| `ARMIA` | `[41, 11, 31]` | Wszystkie drużyny i wojownicy (drużyna 0 = gracz, 40 = wróg `WRG`) |
| `ARMIA_S` | `[41, 11]` | Nazwy wojowników |
| `WOJNA` | `[6, 6]` | Macierz relacji wojny między drużynami (dni wojny) |
| `GRACZE` | `[5, 4]` | Statystyki graczy: `(I,1)` = złoto, `(I,2)` = siła/moc, `(I,3)` = kolor |
| `BRON` | `[121, 12]` | Statystyki broni i przedmiotów |
| `BRON_S` | `[121]` | Nazwy broni |
| `BRON2_S` | `[26]` | Nazwy typów broni |
| `RASY` | `[21, 8]` | Parametry ras |
| `RASY_S` | `[21]` | Nazwy ras |
| `MIASTA` | `[51, 21, 7]` | Miasta i ich pola |
| `MIASTA_S` | `[51]` | Nazwy miast |
| `MUR` | `[11]` | Wytrzymałość murów podczas oblężenia |
| `SKLEP` | `[21, 22]` | Asortyment i ceny sklepów |
| `STRZALY` | `[11]` | Liczba wystrzelonych pocisków |
| `GLEBA` | `[111, 9]` | Przedmioty leżące na ziemi (łupy, śmieci) |
| `PLAPKI` | `[11, 5]` | Pułapki na polu walki |
| `BUDYNKI` | `[13, 7]` | Definicje budynków w mieście |
| `PRZYGODY` | `[4, 11]` | Aktywne przygody/questy |
| `ROZMOWA_S` / `ROZMOWA2_S` | — | Dialogi NPC |
| `WOJNA` | `[6, 6]` | Relacje wojenne (patrz wyżej) |

---

## 8. Pozostałe zmienne i stałe

| Wyrażenie | Znaczenie |
|-----------|-----------|
| `LEWY` / `PRAWY` | Kody przycisków myszy (1 / 2) |
| `ARM` | Numer drużyny gracza (drużyna 0) |
| `WRG` | Numer drużyny wroga (40) |
| `NUMER` | Indeks wojownika w drużynie |
| `KTO_ATAKUJE` | Która strona wykonuje ruch (`-1` = brak) |
| `KONIEC_AKCJI` | Flaga końca akcji w walce |
| `WYNIK_AKCJI` | Wynik/rezultat wykonanej akcji |
| `REAL_KONIEC` | Flaga prawdziwego końca gry |
| `GAME_OVER` | Flaga przegranej |
| `POWER` | Potęga drużyny wroga (skalowanie siły przy generowaniu) |
| `REZULTAT` | Wynik bitwy / akcji |
| `MUNDRY` | Współczynnik zdobywania doświadczenia (z rasy) |
| `SCENERIA` | Typ terenu/planszy walki (0–6) |
| `DZIEN` | Licznik dni (tury gry) |
| `WIDOCZNOSC` | Zasięg widzenia na mapie |
| `ODLEG` | Odległość do celu (wpływa na obrażenia dystansowe) |
| `CENTER_V` | Centrum wertykalne ekranu walki |
| `BROBY` | Przesunięcie numeru Boba broni (15) |
| `GOBY` | Przesunięcie numeru Boba grafiki (baza grafiki) |
| `PIKIETY` / `POTWORY` | Rodzaje jednostek w walce |
| `SUPERVISOR` | Tryb nadzorcy / testowy |
| `CELOWNIK` | Tryb celowania |
| `MX_WEAPON` | Maksymalna broń (limit) |
| `FONTSZ` | Rozmiar fontu (5) |
| `AN[4]` | Tabela animacji `{0, 1, 0, 2}` |
| `PREFS` | Preferencje gracza |
| `VEKTOR_R` | Tablica wektorów ruchu (walka) |

---

## 9. Wyrażenia walki

| Wyrażenie | Znaczenie |
|-----------|-----------|
| `SILA` | Siła ciosu (z `TSI` lub broni `B_SI`) |
| `ODP` | Obrona (od `TP` — odporność) |
| `OPOR` | Wartość obrony po uwzględnieniu doświadczenia obrońcy |
| `CIOS` | Obliczone obrażenia zadawane ciosem |
| `MOC` | Sprawność atakującego: `100 - TDOSW` |
| `MOC2` | Sprawność obrońcy: `100 - TDOSW` |
| `SPEED` | Szybkość ataku: `(100 - TSZ) / 10` (im więcej zręczności, tym szybciej) |
| `KLATKA` | Bieżąca klatka animacji (`TKLAT`) |
| `BAZA` | Bazowy Bob wojownika (`TBOB`) |
| `EN$` | Łańcuch "energia bieżąca / maksymalna" (`TE`/`TEM`) |
| `MAG$` | Łańcuch "magia bieżąca / maksymalna" (`TMAG`/`TMAGMA`) |
| `PL2` | Numer drużyny właściciela celu |
| `CODP` | Kod obrony AI (sposób reakcji przeciwnika) |
| `GODP` | Poziom agresji AI / gotowość obronna |

**Wzór na obrażenia (wręcz):**
`CIOS = (SILA - Rnd(SILA*MOC/100)) - OPOR`, gdzie
`OPOR = ODP - Rnd(ODP*MOC2/100 + 1)`.

---

## 10. Typy terenu

Wartości w `SCENERIA` i na mapie świata:

| Wartość | Teren |
|:-------:|-------|
| 0 | Morze / woda |
| 1 | Step |
| 2 | Las |
| 3 | Pustynia |
| 4 | Góry |
| 5 | Bagno |
| 6 | Tundra |

---

## 11. Wyrażenia systemowe / grafika

| Wyrażenie | Znaczenie |
|-----------|-----------|
| `Bob` | Sprite AMOS — ruchomy element graficzny |
| `Sprite` | Sprite sprzętowy Amigi (nad Bobami) |
| `Zone` | Strefa klikalna ekranu (myszka) |
| `STREFA` | Numer aktywnej strefy (ekrany klikalne) |
| `GADGET` | Przycisk interfejsu rysowany jako strefa |
| `AMAL` | Język animacji AMOS (skrypty ruchu Bobów) |
| `Rainbow` | Kolorowe tło/pasek z animowaną paletą |
| `PaletteFade` | Płynne przejście palety (Fade) |
| `WaitVbl` | Czekanie na odświeżenie pionowe (vsync) |
| `Double Buffer` | Podwójne buforowanie ekranu |
| `Ink` | Ustawienie koloru atramentu (rysowanie) |
| `Bar` / `Box` | Rysowanie wypełnionego prostokąta / konturu |
| `GLEBA` | "Ziemia" — przedmioty na polu walki |
| `BROBY` | Offset numeru Bobów broni |
| `EKRAN_WYBOR` | Ekran wyboru drużyny |
| `KOMUNIKAT` | Komunikat tekstowy w grze |
| `KOMUNIKAT_TAK_NIE` | Pytanie TAK/NIE |
| `ZAKLADKI_POSTEP` | Pasek postępu (ładowanie) |
| `MAPA_ODKRYCIA` | Mgła wojny (odkryte tereny) |
| `DROGI` | Połączenia drogowe między miastami |

---

## Uwagi ogólne

- Wszystkie nazwy procedur w portcie zachowują polską pisownię oryginału
  (`WYKONAJ_AKCJE`, `A_ATAK`, `AI_WYKONAJ_RUCH`, `WCZYTAJ_BRON`…).
- Skrót `A_` na początku funkcji walki = **akcja** (np. `A_RUCH`, `A_STRZELAJ`).
- Skrót `AI_` = decyzje sztucznej inteligencji.
- Skrót `RYS_` / `RYS` = rysowanie ekranu.
- Skrót `WCZYTAJ_` = wczytywanie danych z plików.
- Skrót `OBSLUZ_` = obsługa zdarzeń (klawiatury, myszy, kolizji).
