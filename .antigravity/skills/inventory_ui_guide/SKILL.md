---
name: inventory-ui-guide
description: Architecture guide and implementation checklist for procedural/frame-based Inventory, Shop, and Equipment UIs with drag-and-drop, zone detection, auto-equip, and stat updates.
---

# Inventory UI Building Guide

A guide for building robust, procedural or frame-based Inventory, Shop, and Equipment systems with drag-and-drop functionality, zone detection, slot validation, and dynamic stat updates.

---

## Key Architecture Phases

### 1. Data Layer & Slot Mapping
- Store inventory state in explicit structures or arrays separating:
  - **Backpack / Storage Slots** (fixed capacity)
  - **Equipment Slots** (head, body, legs, left hand, right hand, etc.)
  - **Ground / Environment Slots** (temporary drop targets)
  - **Shop Shelves / Vendors** (buy/sell containers)
- Maintain clear item attribute definitions (item ID, slot requirement, weight, stat bonuses, two-handed flag, ammo flag).

### 2. Zone & Target Detection
- Define clickable interaction zones `(x1, y1, x2, y2)` mapped to slot indices:
  - Zones 1-8: Backpack
  - Zones 9-12, 30-33: Ground
  - Zones 13-17: Equipment slots
  - Zones 10-29: Shop shelves
- Query the zone under cursor on mouse events (`MouseZone()`).

### 3. Modal Drag-and-Drop Loop Pattern
When a user initiates a drag action on an item:
1. **Source Prep**: Clear or hide the item graphic from the source slot; temporarily untrack or mark source slot as empty.
2. **Hotspot Alignment**: Center mouse cursor relative to the item icon (e.g. middle-bottom hotspot) so the icon moves naturally under the pointer.
3. **Render Loop**:
   - Each frame, update the dragged sprite position to `(XMouse(), YMouse())`.
   - Display dynamic tooltip/info bar showing item details, stats, or price.
4. **Drop Event**: On mouse release/click:
   - Identify target zone under cursor.
   - **Valid Empty Target**: Place item in target slot data array, paste graphic at target coordinate, terminate drag loop.
   - **Valid Occupied Target (Swap)**: If target slot permits, place held item into target, pick up the displaced item, and re-enter modal drag loop with displaced item.
   - **Invalid Target**: Return held item back to original source slot and clean up sprite.

### 4. Special Rules & Shortcuts
- **Auto-Equip (Ctrl+Click)**: Bypass modal drag loop. Look up item target slot requirement and place directly into the first valid empty slot (or auto-swap).
- **Two-Handed Weapon Constraints**: Unequip or block shield/off-hand slot when a 2H weapon is equipped.
- **Stacking & Ammo Pooling**: Handle stackable items (e.g. arrows/bolts) by merging quantities into an ammo pool rather than taking standard slots.
- **Shop Buy/Sell Rules**:
  - Buying: Deduct money, place item in backpack slot. Reject drop if player cash < price or backpack full.
  - Selling: Credit money, place item on shop shelf.

### 5. Stat & Weight Recalculation (Side Effects)
- **Weight Calculation**: Sum total weight of equipped + backpack items after every inventory mutation. Apply speed/movement penalties if weight > max threshold.
- **Stat Modifiers**: Trigger `ApplyStats(item, +1)` on equip and `ApplyStats(item, -1)` on unequip. Handle race caps or stat dependencies cleanly to avoid legacy stack bugs.
