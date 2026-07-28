using AmigaNet.Amos.Screens;
using System;
using System.Collections.Generic;

namespace AmigaNet.Legion
{
    public partial class Legion
    {
        /// <summary>
        /// Modern Squad Inventory System (INVENTORY_NEW)
        /// Manages equipment for up to 10 squad members in a legion, including paperdoll equipment slots,
        /// individual unit backpacks, and a Diablo 2 style ground stash grid.
        /// Rendering pattern follows WYBOR: draw once, then only update changed regions.
        /// </summary>
        public void INVENTORY_NEW(int armiaIndex)
        {
            ARM = armiaIndex;

            // In Legion, SEK represents sector/ground item index based on unit's tile position
            int xb = ARMIA[ARM, 1, TX];
            int yb = ARMIA[ARM, 1, TY];
            int sek = (xb / 16) + (yb / 16) * 10;
            if (sek < 0) sek = 0;
            if (sek > 110) sek = 110;

            // Select first living unit in the legion (1-10)
            int selectedUnit = 1;
            for (int i = 1; i <= 10; i++)
            {
                if (ARMIA[ARM, i, TE] > 0)
                {
                    selectedUnit = i;
                    break;
                }
            }

            // Load item bob graphics (not available in map/army context)
            int savedBobCount = screens.GetBobCount();
            int savedGoby = GOBY;
            _LOAD("dane/gad", 0);
            _LOAD("dane/glowny", 1);
            GOBY = savedBobCount;

            int groundScroll = 0;

            screens.ScreenOpen(1, 320, 140, 32, PixelMode.Lowres);
            screens.ReserveZone(100);
            screens.ScreenHide();
            screens.View();
            screens.ScreenDisplay(1, 130, 162, 320, 140);
            screens.Colour(0, 2, 1, 0);

            // Initial full draw (like WYBOR: draw everything once, then show)
            INVENTORY_NEW_DRAW_BACKGROUND();
            INVENTORY_NEW_DRAW_ROSTER(ARM, selectedUnit);
            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
            INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
            screens.ScreenShow();
            screens.View();

            // Drag and drop state
            int dragItem = 0;
            int dragFromZone = 0;
            int dragFromSlot = 0; // actual data slot index (TPLECAK+b, TGLOWA, etc. or ground slot)
            bool dragFromGround = false;

            // Main loop: only redraw changed panels on interaction
            while (true)
            {
                // --- Drag and drop handling ---
                if (dragItem > 0)
                {
                    if (screens.MouseKey() == 1)
                    {
                        // Still holding: draw item bob at cursor
                        int mx = screens.XMouse();
                        int my = screens.YMouse();
                        screens.PasteBob(mx - 8, my - 8, BRON[dragItem, B_BOB] + BROBY + GOBY);
                    }
                    else
                    {
                        // Released: drop on target zone
                        int dropZone = screens.MouseZone();
                        INVENTORY_NEW_DROP_ITEM(ARM, selectedUnit, sek,
                            ref dragItem, dragFromZone, dragFromSlot, dragFromGround, dropZone);
                        WAGA(ARM, selectedUnit);
                        INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                        INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                        dragItem = 0;
                    }
                }
                else if (screens.MouseClick() == 1)
                {
                    int zone = screens.MouseZone();

                    // Roster Unit Selection (Zones 1-10)
                    if (zone >= 1 && zone <= 10)
                    {
                        if (ARMIA[ARM, zone, TE] > 0 && zone != selectedUnit)
                        {
                            selectedUnit = zone;
                            WAGA(ARM, selectedUnit);
                            INVENTORY_NEW_DRAW_ROSTER(ARM, selectedUnit);
                            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                        }
                    }
                    // Paperdoll Equipment Slots (Zones 11-15): pick up item for dragging
                    else if (zone >= 11 && zone <= 15)
                    {
                        int slotIdx = INVENTORY_NEW_ZONE_TO_SLOT(zone);
                        if (slotIdx > 0)
                        {
                            int item = ARMIA[ARM, selectedUnit, slotIdx];
                            if (item > 0)
                            {
                                dragItem = item;
                                dragFromZone = zone;
                                dragFromSlot = slotIdx;
                                dragFromGround = false;
                                ARMIA[ARM, selectedUnit, slotIdx] = 0;
                                INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                            }
                        }
                    }
                    // Backpack Slots (Zones 16-23): pick up item for dragging
                    else if (zone >= 16 && zone <= 23)
                    {
                        int bpSlot = zone - 16;
                        int item = ARMIA[ARM, selectedUnit, TPLECAK + bpSlot];
                        if (item > 0)
                        {
                            dragItem = item;
                            dragFromZone = zone;
                            dragFromSlot = TPLECAK + bpSlot;
                            dragFromGround = false;
                            ARMIA[ARM, selectedUnit, TPLECAK + bpSlot] = 0;
                            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                        }
                    }
                    // Ground Stash Slots (Zones 30-37): pick up item for dragging
                    else if (zone >= 30 && zone <= 37)
                    {
                        int gSlot = (zone - 30) + groundScroll;
                        if (gSlot < 9)
                        {
                            int item = GLEBA[sek, gSlot];
                            if (item > 0)
                            {
                                dragItem = item;
                                dragFromZone = zone;
                                dragFromSlot = gSlot;
                                dragFromGround = true;
                                GLEBA[sek, gSlot] = 0;
                                INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                            }
                        }
                    }
                    // Ground Scroll Prev/Next (Zones 50, 51)
                    else if (zone == 50 && groundScroll > 0)
                    {
                        groundScroll -= 4;
                        INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                    }
                    else if (zone == 51 && groundScroll + 4 < 8)
                    {
                        groundScroll += 4;
                        INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                    }
                    // Quick Action: Auto-Equip selected unit (Zone 60)
                    else if (zone == 60)
                    {
                        INVENTORY_NEW_AUTO_EQUIP(ARM, selectedUnit, sek);
                        WAGA(ARM, selectedUnit);
                        INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                        INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                    }
                    // Exit Button (Zone 99)
                    else if (zone == 99)
                    {
                        break;
                    }
                }

                if (screens.MouseKey() == PRAWY) break;
            }

            screens.ScreenClose(1);

            // Restore bob bank state (remove temporarily loaded item bobs)
            screens.TrimBobs(savedBobCount);
            GOBY = savedGoby;
        }

        /// <summary>
        /// Draws static background and action buttons (called once).
        /// </summary>
        private void INVENTORY_NEW_DRAW_BACKGROUND()
        {
            // Clear / Fill background
            screens.Ink(31, 30);
            screens.Bar(0, 0, 319, 139);

            // Panel backgrounds
            // Left panel (roster)
            screens.Ink(1, 30);
            screens.Text(6, 10, "LEGION (10)");

            // Center panel background
            int cx = 90;
            int cy = 4;
            screens.Ink(19, 30);
            screens.Bar(cx, cy, cx + 115, cy + 130);

            // Right panel background
            int gx = 210;
            int gy = 4;
            screens.Ink(19, 30);
            screens.Bar(gx, gy, gx + 105, gy + 130);

            screens.Ink(31, 19);
            screens.Text(gx + 6, gy + 10, "ZIEMIA");

            // Ground Scroll Controls (Zones 50 & 51)
            GADGET(gx + 6, gy + 70, 42, 14, " < ", 5, 0, 8, 1, 50);
            GADGET(gx + 54, gy + 70, 42, 14, " > ", 5, 0, 8, 1, 51);

            // Auto-Equip Button (Zone 60)
            GADGET(gx + 6, gy + 92, 92, 16, "Auto-Equip", 8, 2, 6, 31, 60);

            // Exit Button (Zone 99)
            GADGET(gx + 6, gy + 112, 92, 16, "  Zamknij  ", 8, 2, 6, 31, 99);
        }

        /// <summary>
        /// Draws the left roster panel (unit list with selection highlight).
        /// </summary>
        private void INVENTORY_NEW_DRAW_ROSTER(int arm, int selectedUnit)
        {
            for (int i = 1; i <= 10; i++)
            {
                int py = 14 + (i - 1) * 12;
                bool isAlive = ARMIA[arm, i, TE] > 0;
                bool isSelected = (i == selectedUnit);

                int bgCol = isSelected ? 19 : (isAlive ? 8 : 0);
                int fgCol = isSelected ? 31 : (isAlive ? 1 : 6);

                // Set zone for unit selection (Zones 1-10)
                screens.SetZone(i, 4, py - 2, 85, py + 9);
                screens.Ink(bgCol, 30);
                screens.Bar(4, py - 2, 85, py + 9);

                if (isAlive)
                {
                    string unitName = ARMIA_S[arm, i];
                    if (string.IsNullOrEmpty(unitName)) unitName = "Wojownik " + i;
                    if (unitName.Length > 8) unitName = unitName.Substring(0, 8);

                    int hp = ARMIA[arm, i, TE];
                    string prefix = isSelected ? ">" : " ";
                    screens.Ink(fgCol, bgCol);
                    screens.Text(6, py + 7, $"{prefix}{i}.{unitName} HP:{hp}");
                }
                else
                {
                    screens.Ink(6, bgCol);
                    screens.Text(6, py + 7, $" {i}. --- (Puste)");
                }
            }
        }

        /// <summary>
        /// Draws the center panel: unit name, paperdoll slots, backpack, and stats.
        /// </summary>
        private void INVENTORY_NEW_DRAW_UNIT(int arm, int selectedUnit)
        {
            int cx = 90;
            int cy = 4;

            // Clear center panel area
            screens.Ink(19, 30);
            screens.Bar(cx, cy, cx + 115, cy + 130);

            string activeName = ARMIA_S[arm, selectedUnit];
            if (string.IsNullOrEmpty(activeName)) activeName = "Wojownik " + selectedUnit;
            screens.Ink(31, 19);
            screens.Text(cx + 6, cy + 10, $"{selectedUnit}. {activeName}");

            // Paperdoll Slots
            // Head Slot (Zone 11)
            GADGET(cx + 46, cy + 16, 22, 20, "H", 5, 5, 0, 16, 11);
            int headItem = ARMIA[arm, selectedUnit, TGLOWA];
            if (headItem > 0)
            {
                screens.PasteBob(cx + 48, cy + 18, BRON[headItem, B_BOB] + BROBY + GOBY);
            }

            // Chest Slot (Zone 12)
            GADGET(cx + 46, cy + 38, 22, 20, "C", 5, 5, 0, 16, 12);
            int chestItem = ARMIA[arm, selectedUnit, TKORP];
            if (chestItem > 0)
            {
                screens.PasteBob(cx + 48, cy + 40, BRON[chestItem, B_BOB] + BROBY + GOBY);
            }

            // Legs Slot (Zone 13)
            GADGET(cx + 46, cy + 60, 22, 20, "L", 5, 5, 0, 16, 13);
            int legsItem = ARMIA[arm, selectedUnit, TNOGI];
            if (legsItem > 0)
            {
                screens.PasteBob(cx + 48, cy + 62, BRON[legsItem, B_BOB] + BROBY + GOBY);
            }

            // Left Hand (Zone 14)
            GADGET(cx + 20, cy + 38, 22, 20, "LHand", 5, 5, 0, 16, 14);
            int leftItem = ARMIA[arm, selectedUnit, TLEWA];
            if (leftItem > 0)
            {
                screens.PasteBob(cx + 22, cy + 40, BRON[leftItem, B_BOB] + BROBY + GOBY);
            }

            // Right Hand (Zone 15)
            GADGET(cx + 72, cy + 38, 22, 20, "RHand", 5, 5, 0, 16, 15);
            int rightItem = ARMIA[arm, selectedUnit, TPRAWA];
            if (rightItem > 0)
            {
                screens.PasteBob(cx + 74, cy + 40, BRON[rightItem, B_BOB] + BROBY + GOBY);
            }

            // Backpack Slots (Zones 16-23, 2 rows of 4)
            screens.Ink(1, 19);
            screens.Text(cx + 6, cy + 88, "PLECAK:");
            for (int b = 0; b < 8; b++)
            {
                int bx = cx + 6 + (b % 4) * 26;
                int by = cy + 92 + (b / 4) * 22;
                GADGET(bx, by, 22, 20, "", 0, 5, 0, 16, 16 + b);

                int bpItem = ARMIA[arm, selectedUnit, TPLECAK + b];
                if (bpItem > 0)
                {
                    screens.PasteBob(bx + 2, by + 2, BRON[bpItem, B_BOB] + BROBY + GOBY);
                }
            }

            // Unit Stats Summary
            int sil = ARMIA[arm, selectedUnit, TSI];
            int spd = ARMIA[arm, selectedUnit, TSZ];
            int weight = ARMIA[arm, selectedUnit, TWAGA];
            screens.Ink(31, 19);
            screens.Text(cx + 6, cy + 138, $"Sila:{sil} Szyb:{spd} Waga:{weight}");
        }

        /// <summary>
        /// Draws the right ground stash panel with current scroll offset.
        /// </summary>
        private void INVENTORY_NEW_DRAW_GROUND(int sek, int groundScroll)
        {
            int gx = 210;
            int gy = 4;

            // Clear ground slot area only (below the "ZIEMIA" label)
            screens.Ink(19, 30);
            screens.Bar(gx, gy + 14, gx + 105, gy + 66);

            // Ground Stash Slots (Zones 30-37, 2 rows of 4 per page)
            for (int g = 0; g < 8; g++)
            {
                int actualGSlot = g + groundScroll;
                int slotX = gx + 6 + (g % 4) * 24;
                int slotY = gy + 18 + (g / 4) * 24;

                GADGET(slotX, slotY, 22, 20, "", 0, 5, 0, 16, 30 + g);

                if (actualGSlot < 9)
                {
                    int groundItem = GLEBA[sek, actualGSlot];
                    if (groundItem > 0)
                    {
                        screens.PasteBob(slotX + 2, slotY + 2, BRON[groundItem, B_BOB] + BROBY + GOBY);
                    }
                }
            }
        }

        /// <summary>
        /// Maps a paperdoll zone (11-15) to the ARMIA slot index.
        /// </summary>
        private int INVENTORY_NEW_ZONE_TO_SLOT(int zone)
        {
            if (zone == 11) return TGLOWA;
            if (zone == 12) return TKORP;
            if (zone == 13) return TNOGI;
            if (zone == 14) return TLEWA;
            if (zone == 15) return TPRAWA;
            return -1;
        }

        /// <summary>
        /// Handles dropping a dragged item onto a target zone.
        /// If the target is invalid or occupied, returns item to its origin.
        /// </summary>
        private void INVENTORY_NEW_DROP_ITEM(int arm, int unit, int sek,
            ref int dragItem, int fromZone, int fromSlot, bool fromGround, int dropZone)
        {
            int item = dragItem;
            bool placed = false;

            // Drop on paperdoll slot (Zones 11-15)
            if (dropZone >= 11 && dropZone <= 15)
            {
                int targetSlot = INVENTORY_NEW_ZONE_TO_SLOT(dropZone);
                if (targetSlot > 0 && ARMIA[arm, unit, targetSlot] == 0)
                {
                    // Check item type matches slot
                    int place = BRON[item, B_PLACE];
                    bool valid = false;
                    if (targetSlot == TGLOWA && place == 1) valid = true;
                    else if (targetSlot == TKORP && place == 2) valid = true;
                    else if (targetSlot == TNOGI && place == 3) valid = true;
                    else if (targetSlot == TPRAWA && (place == 4 || place == 6)) valid = true;
                    else if (targetSlot == TLEWA && place == 4) valid = true;

                    if (valid)
                    {
                        ARMIA[arm, unit, targetSlot] = item;
                        placed = true;
                    }
                }
            }
            // Drop on backpack slot (Zones 16-23)
            else if (dropZone >= 16 && dropZone <= 23)
            {
                int bpSlot = dropZone - 16;
                if (ARMIA[arm, unit, TPLECAK + bpSlot] == 0)
                {
                    ARMIA[arm, unit, TPLECAK + bpSlot] = item;
                    placed = true;
                }
            }
            // Drop on ground slot (Zones 30-37)
            else if (dropZone >= 30 && dropZone <= 37)
            {
                int gSlot = (dropZone - 30);
                // Use the ground scroll offset that was active; approximate via zone
                // Since we cleared the source already, just find first free ground slot
                for (int g = 0; g < 9; g++)
                {
                    if (GLEBA[sek, g] == 0)
                    {
                        GLEBA[sek, g] = item;
                        placed = true;
                        break;
                    }
                }
            }

            // If not placed, return to origin
            if (!placed)
            {
                if (fromGround)
                {
                    GLEBA[sek, fromSlot] = item;
                }
                else
                {
                    ARMIA[arm, unit, fromSlot] = item;
                }
            }

            dragItem = 0;
        }

        /// <summary>
        /// Auto-equips ground items onto the SELECTED unit only.
        /// Tries to equip directly, then falls back to backpack.
        /// </summary>
        private void INVENTORY_NEW_AUTO_EQUIP(int arm, int selectedUnit, int sek)
        {
            for (int g = 0; g < 9; g++)
            {
                int item = GLEBA[sek, g];
                if (item == 0) continue;

                int place = BRON[item, B_PLACE];
                bool taken = false;

                // Try to equip directly
                if (place == 1 && ARMIA[arm, selectedUnit, TGLOWA] == 0)
                {
                    ARMIA[arm, selectedUnit, TGLOWA] = item;
                    taken = true;
                }
                else if (place == 2 && ARMIA[arm, selectedUnit, TKORP] == 0)
                {
                    ARMIA[arm, selectedUnit, TKORP] = item;
                    taken = true;
                }
                else if (place == 3 && ARMIA[arm, selectedUnit, TNOGI] == 0)
                {
                    ARMIA[arm, selectedUnit, TNOGI] = item;
                    taken = true;
                }
                else if ((place == 4 || place == 6) && ARMIA[arm, selectedUnit, TPRAWA] == 0)
                {
                    ARMIA[arm, selectedUnit, TPRAWA] = item;
                    taken = true;
                }
                else if (place == 4 && ARMIA[arm, selectedUnit, TLEWA] == 0)
                {
                    ARMIA[arm, selectedUnit, TLEWA] = item;
                    taken = true;
                }

                // If can't equip, try backpack
                if (!taken)
                {
                    for (int b = 0; b < 8; b++)
                    {
                        if (ARMIA[arm, selectedUnit, TPLECAK + b] == 0)
                        {
                            ARMIA[arm, selectedUnit, TPLECAK + b] = item;
                            taken = true;
                            break;
                        }
                    }
                }

                if (taken)
                {
                    GLEBA[sek, g] = 0;
                }
            }
        }
    }
}
