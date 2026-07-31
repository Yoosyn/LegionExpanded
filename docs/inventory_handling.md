# Inventory & Drag-and-Drop System

## Overview

LegionExpanded is a C# MonoGame port of the Amiga RPG *Legion* (1994). There is **no separate UI framework** -- the inventory and shop screens are procedurally rendered using an AMOS emulation layer (`ScreensManager`/`AmosBase`) with direct pixel operations, sprite drawing, and zone-based click detection. Drag-and-drop is implemented as a **modal loop** that follows the mouse and renders the dragged item as an AMOS hardware sprite (#53).

Key files:

| File | Role |
|---|---|
| `LegionWybor.cs` | Inventory/Equipment screen (`WYBOR`) -- backpack, equipment slots, ground items, drag-and-drop |
| `LegionSklep.cs` | Shop screen (`SKLEP_`) -- buying/selling with drag-and-drop |
| `LegionMainAction.cs` | Battle loot drag-and-drop after combat |
| `LegionData.cs` | Data structure definitions (arrays for items, equipment slots, ground items) |

---

## Data Structures

All item and inventory data lives in flat multi-dimensional arrays (matching the original AMOS `Dim` arrays):

```csharp
int[,,] ARMIA = new int[41, 11, 31];   // units: army, unit_index, attribute
int[,] SKLEP  = new int[21, 22];        // shop shelves: shop_id, slot
int[,] GLEBA  = new int[111, 9];        // ground items: sector, slot
int[,] BRON   = new int[121, 12];       // item definitions: item_id, stat
```

Equipment slot offsets within `ARMIA`:

| Constant | Offset | Slot |
|---|---|---|
| `TGLOWA` | 13 | Head |
| `TKORP` | 14 | Body |
| `TNOGI` | 15 | Legs |
| `TLEWA` | 16 | Left hand |
| `TPRAWA` | 17 | Right hand |
| `TPLECAK` | 18-25 | Backpack (8 slots) |

---

## Zone System (Click Target Detection)

Interactive areas are registered via `screens.SetZone(id, x1, y1, x2, y2)` and checked with `screens.MouseZone()` on click. Zones are used for:

- **Backpack slots**: zones 1-8
- **Ground slots**: zones 9-12 (row 1), 30-33 (row 2)
- **Equipment slots**: zones 13-17 (head, body, legs, left hand, right hand)
- **Shop shelves**: zones 10-29 (20 items, 2 rows of 10)
- **Shop backpack**: zones 40-47 (8 slots)

---

## Drag-and-Drop Mechanics

### Core Pattern (all 3 contexts)

The drag-and-drop follows the same structure everywhere:

```
1. Hide the source slot visually (clear the area)
2. Clear the source data array entry
3. Call the pick function (modal drag loop)

   Inside the loop:
   a. Call PICK_2 helper (set up item sprite hotspot, show item name/type/weight)
   b. On each frame:
      - Convert mouse coords to screen coords
      - Move sprite #53 to mouse position (the dragged item graphic)
      - Wait for VBL
   c. On mouse click:
      - Turn off sprite #53
      - Reset hotspot
      - Query zone under mouse cursor
      - If zone is a valid drop target:
          * Place item in target slot (data array)
          * Paste the item bob at target position
          * If the target slot already had an item (swap), pick that one up
          * Set KONIEC = true (exit loop) if target was empty
      - Else: continue loop (item returns to source)
   d. Recalculate weight and redraw stats
```

### Inventory (`LegionWybor.cs`)

**`WYBOR()`** -- main inventory screen loop (line 5)

- Displays 8 backpack slots (zones 1-8), 5 equipment slots (zones 13-17), and 8 ground slots (zones 9-12, 30-33)
- Left-click on an item starts drag; Ctrl+click bypasses drag (auto-equip or ground-to-backpack)
- Hovering over a slot shows an item tooltip overlay (drawn on screen 0 via `DRAW_TOOLTIP` -- see `docs/implementation/tooltip.md`)
- Right-click exits the screen

**`WYBOR_PICK(int BR, int X, int Y, int X2, int Y2, int NUMER, ref int BB, int SEK)`** (line 458)

- The modal drag loop for inventory items
- Calls `WYBOR_PICK_2()` to set up the dragged item visual (hotspot + mask); the item tooltip is drawn in the pick loop via `DRAW_TOOLTIP` on screen 0 (see `docs/implementation/tooltip.md`)
- Dropping on:
  - **Backpack slot** (zones 1-8): Places item in backpack. Ammo (type 17) is consumed into ammo pool instead.
  - **Ground slot** (zones 9-12, 30-33): Places item on ground (`GLEBA` array)
  - **Equipment slot** (zones 13-17): Checks `BRON[BR, B_PLACE]` (equip slot type) against the target zone. Verifies slot is empty. Validates two-handed (place=6) vs one-handed (place=4) weapon rules.
  - If the target slot already has an item (BR1 != 0), it **swaps**: the held item is placed and the displaced item is picked up (re-enters the drag loop)
- After drop, recalculates weight via `WAGA()` and refreshes stats via `WYBOR_WYPISZ()`

**`WYBOR_PICK_2(int BR, int X, ref int BB)`** (line 654)

- Sets `HotSpot(BB, 11)` -- centers the sprite hotspot at the middle-bottom of the item icon
- Item info is no longer drawn here as a bar -- the `DRAW_TOOLTIP` overlay during the drag loop replaced it (see `docs/implementation/tooltip.md`)
- `BB = BRON[BR, B_BOB] + BROBY` -- calculates the bob/sprite index for the item

**`WYBOR_AUTO_EQUIP()`** (line 834) -- Ctrl+click from backpack

- Uses `BRON[BR, B_PLACE]` to auto-determine the correct equipment slot
- Handles ammo consumption, one-handed vs two-handed logic, and race-type bonuses
- No drag loop -- instant equip

**`WYBOR_GROUND_TO_BACKPACK()`** (line 794) -- Ctrl+click from ground

- Finds the first empty backpack slot and moves the item there directly

### Shop (`LegionSklep.cs`)

**`SKLEP_()`** (line 7)

- Renders shop background, shelf items (zones 10-29), backpack (zones 40-47), player info
- Ctrl+click buys directly to first empty backpack slot (no drag)
- Regular click starts drag via `SKLEP_PICK()`

**`SKLEP_PICK(int BRO, int SNR, int A, int NR, int CENA, int ZNAK, ref bool KONIEC)`** (line 318)

- Same modal drag loop pattern
- `ZNAK = -1` for buying (money decreases), `ZNAK = 1` for selling (money increases)
- Drop on **shop shelf** (zone 10-29): places item back on shelf (selling back)
- Drop on **backpack** (zone 40-47): places item in backpack, deducts/credits money
- Unlike inventory, shop pick does **not** support swapping -- only drops on empty slots
- Money updates via `SKLEP_SZMAL()`

**`SKLEP_AUTO_EQUIP()`** (line 524) -- Ctrl+click in shop

- Data-only equip (no equipment slot visuals since shop has no equipment display)
- Same logic as `WYBOR_AUTO_EQUIP()` but without paste operations

### Battle Loot (`LegionMainAction.cs`, lines 3637-3671)

- After battle, items from the loot grid are dragged to backpack zones (40-47)
- Same sprite #53 + HotSpot(11) pattern
- Drop only allowed on empty backpack slots; no swap
- On successful drop, item is removed from `lupItems` list

---

## Sprite-Based Drag Visual

| Aspect | Detail |
|---|---|
| Sprite slot | Always AMOS sprite #53 (`screens.Sprite(53, ...)`) |
| Initialization | Before the loop: `screens.Sprite(53, x, y, BB)` with `screens.HotSpot(BB, 11)` |
| Per-frame update | `screens.Sprite(53, screens.XMouse(), screens.YMouse())` |
| Cleanup | `screens.SpriteOff(53)` followed by `screens.HotSpot(BB, 0)` (reset hotspot) |
| Item graphic | `BB = BRON[BR, B_BOB] + BROBY` -- base bob offset + item bob index |

The hotspot is set to value **11** (centered, middle-bottom of a 16x16 icon) so the item appears under the cursor as if held.

---

## User Interaction Summary

| Input | Context | Behavior |
|---|---|---|
| Left-click on item | Any | Start drag-and-drop with the item following the cursor |
| Left-click on empty slot | During drag | Drop item; if slot was occupied, pick up the swapped item |
| Right-click | During drag | Continues drag (right-click exits the whole screen only from the main loop) |
| Right-click | Main screen loop | Exit inventory/shop |
| Ctrl+Left-click on item | Inventory backpack | Auto-equip to best matching equipment slot |
| Ctrl+Left-click on item | Inventory ground | Move to first empty backpack slot |
| Ctrl+Left-click on item | Shop shelf | Buy directly to first empty backpack slot (no drag) |
| Ctrl+Left-click on item | Shop backpack | Sell directly to first empty shop shelf slot (no drag) |
| `q` key | Shop | Exit shop |

---

## Weight & Stat Recalculation

`WAGA(int A, int NR)` (line 669 in `LegionWybor.cs`)

- Sums `BRON[B, B_WAGA]` for all equipped items (slots TGLOWA through TPRAWA + TPLECAK)
- If weight exceeds `TEM` (max energy), speed is penalized

`PRZELICZ(int I, int ZNAK)` (line 704)

- Applies equipment stat bonuses (strength, defense, speed, energy, magic) when equipping (ZNAK=1) or unequipping (ZNAK=-1)
- Includes a fix for a legacy Amiga bug: temporarily removes equipment bonuses from other slots before applying racial caps, then re-applies them
