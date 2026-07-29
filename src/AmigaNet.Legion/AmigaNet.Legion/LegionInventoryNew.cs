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

            // Select first living unit in the legion (1-10)
            int selectedUnit = 0;
            for (int i = 1; i <= 10; i++)
            {
                if (ARMIA[ARM, i, TE] > 0)
                {
                    selectedUnit = i;
                    break;
                }
            }
            if (selectedUnit == 0) return;
            NUMER = selectedUnit;

            // Calculate sector based on selected unit's tile position
            int sek = SEKTOR(ARMIA[ARM, selectedUnit, TX], ARMIA[ARM, selectedUnit, TY]);

            // Load item bob graphics (not available in map/army context)
            int savedBobCount = screens.GetBobCount();
            int savedGoby = GOBY;
            _LOAD("dane/gad", 0);
            _LOAD("dane/glowny", 1);
            GOBY = savedBobCount;

            int groundScroll = 0;

            screens.ScreenOpen(1, 320, 190, 32, PixelMode.Lowres);
            screens.ReserveZone(100);
            screens.ScreenHide();
            screens.View();
            screens.ScreenDisplay(1, 130, 120, 320, 190);
            int screenOffX = 130;
            int screenOffY = 120;
            screens.Colour(0, 2, 1, 0);

            // Initial full draw (like WYBOR: draw everything once, then show)
            INVENTORY_NEW_DRAW_BACKGROUND();
            INVENTORY_NEW_DRAW_ROSTER(ARM, selectedUnit);
            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
            INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
            INVENTORY_NEW_DRAW_STATS(ARM, selectedUnit);
            screens.ScreenShow();
            screens.View();

            // Main loop
            while (true)
            {
                if (screens.MouseClick() == 1)
                {
                    int zone = screens.MouseZone();

                    // Roster Unit Selection (Zones 1-10)
                    if (zone >= 1 && zone <= 10)
                    {
                        if (ARMIA[ARM, zone, TE] > 0 && zone != selectedUnit)
                        {
                            selectedUnit = zone;
                            NUMER = selectedUnit;
                            WAGA(ARM, selectedUnit);
                            INVENTORY_NEW_DRAW_ROSTER(ARM, selectedUnit);
                            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                            INVENTORY_NEW_DRAW_STATS(ARM, selectedUnit);
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
                                PRZELICZ(slotIdx - TGLOWA, -1);
                                ARMIA[ARM, selectedUnit, slotIdx] = 0;
                                INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                                INVENTORY_NEW_PICK(item, ARM, selectedUnit, sek, slotIdx, false, groundScroll);
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
                            ARMIA[ARM, selectedUnit, TPLECAK + bpSlot] = 0;
                            INVENTORY_NEW_DRAW_UNIT(ARM, selectedUnit);
                            INVENTORY_NEW_PICK(item, ARM, selectedUnit, sek, TPLECAK + bpSlot, false, groundScroll);
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
                                GLEBA[sek, gSlot] = 0;
                                INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
                                INVENTORY_NEW_PICK(item, ARM, selectedUnit, sek, gSlot, true, groundScroll);
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
                        INVENTORY_NEW_DRAW_STATS(ARM, selectedUnit);
                    }
                    // Exit Button (Zone 99)
                    else if (zone == 99)
                    {
                        break;
                    }
                }

                if (screens.MouseKey() == PRAWY) break;
                screens.WaitVbl();
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
            // Dark screen background (like original WYBOR)
            screens.Ink(0, 30);
            screens.Bar(0, 0, 319, 189);

            // Left panel area (no panel - just open space for roster)
            screens.Ink(1, 0);
            screens.Text(6, 10, "LEGION (10)");

            // Center panel: paperdoll + backpack (style: old paperdoll panel, bg=19)
            int cx = 90;
            int cy = 4;
            GADGET(cx, cy, 115, 180, "", 0, 5, 19, 19, -1);

            // Right panel: ground stash + stats (style: old ground panel, bg=8)
            int gx = 210;
            int gy = 4;
            GADGET(gx, gy, 105, 180, "", 5, 0, 8, 8, -1);

            screens.Ink(16, 8);
            screens.Text(gx + 6, gy + 10, "ZIEMIA");

            // Ground Scroll Controls (Zones 50 & 51)
            GADGET(gx + 50, gy + 2, 24, 10, "<", 5, 0, 8, 1, 50);
            GADGET(gx + 76, gy + 2, 24, 10, ">", 5, 0, 8, 1, 51);

            // Auto-Equip Button (Zone 60)
            GADGET(gx + 4, gy + 158, 48, 14, "Auto", 8, 2, 6, 31, 60);

            // Exit Button (Zone 99)
            GADGET(gx + 56, gy + 158, 48, 14, "Zamknij", 8, 2, 6, 31, 99);
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

            // Clear center panel area (paperdoll + backpack only)
            screens.Ink(19, 30);
            screens.Bar(cx + 1, cy + 1, cx + 114, cy + 130);

            string activeName = ARMIA_S[arm, selectedUnit];
            if (string.IsNullOrEmpty(activeName)) activeName = "Wojownik " + selectedUnit;
            screens.Ink(16, 19);
            screens.Text(cx + 6, cy + 10, $"{selectedUnit}. {activeName}");

            // Paperdoll Slots (match original WYBOR style: K1=5, K2=5, K3=0, K4=16)
            // Head Slot (Zone 11) - uses bg=19 like original head slot
            GADGET(cx + 46, cy + 16, 22, 20, "H", 5, 5, 19, 19, 11);
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

            // Backpack Slots (Zones 16-23, 2 rows of 4) - original style: K1=0, K2=5, K3=0, K4=16
            screens.Ink(16, 19);
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

            // Unit Stats Summary (compact, at bottom of center panel)
            int weight = ARMIA[arm, selectedUnit, TWAGA];
            screens.Ink(20, 19);
            screens.Text(cx + 6, cy + 138, $"Waga:{weight}");
        }

        /// <summary>
        /// Draws the right ground stash panel with current scroll offset.
        /// </summary>
        private void INVENTORY_NEW_DRAW_GROUND(int sek, int groundScroll)
        {
            int gx = 210;
            int gy = 4;

            // Clear ground slot area (below label, above stats) with panel bg=8
            screens.Ink(8, 30);
            screens.Bar(gx + 1, gy + 14, gx + 104, gy + 58);

            // Ground Stash Slots (Zones 30-37, 2 rows of 4) - original style: K1=0, K2=5, K3=0, K4=16
            for (int g = 0; g < 8; g++)
            {
                int actualGSlot = g + groundScroll;
                int slotX = gx + 4 + (g % 4) * 25;
                int slotY = gy + 16 + (g / 4) * 21;

                GADGET(slotX, slotY, 22, 18, "", 0, 5, 0, 16, 30 + g);

                if (actualGSlot < 9)
                {
                    int groundItem = GLEBA[sek, actualGSlot];
                    if (groundItem > 0)
                    {
                        screens.PasteBob(slotX + 2, slotY + 1, BRON[groundItem, B_BOB] + BROBY + GOBY);
                    }
                }
            }
        }

        /// <summary>
        /// Draws character stats in the right panel below the ground grid.
        /// </summary>
        private void INVENTORY_NEW_DRAW_STATS(int arm, int unit)
        {
            int gx = 210;
            int gy = 4;
            int sy = gy + 62;

            // Clear stats area with panel bg=19 (like original stats panel)
            screens.Ink(19, 30);
            screens.Bar(gx, sy - 2, gx + 105, sy + 90);
            // Stats sub-panel border
            screens.Ink(5);
            screens.Box(gx + 1, sy - 2, gx + 104, sy + 90);

            // Race name
            int rasa = ARMIA[arm, unit, TRASA];
            if (rasa < 0 || rasa >= RASY_S.Length) rasa = 0;
            screens.Ink(3, 19);
            screens.Text(gx + 4, sy + 6, RASY_S[rasa]);

            // Separator line
            screens.Ink(8, 19);
            screens.Bar(gx + 4, sy + 9, gx + 100, sy + 9);

            // Stats labels - left column
            screens.Ink(3, 19);
            screens.Text(gx + 4, sy + 18, "Energia:");
            screens.Text(gx + 4, sy + 28, "Sila:");
            screens.Text(gx + 4, sy + 38, "Szybkosc:");
            screens.Text(gx + 4, sy + 48, "Odpornosc:");
            screens.Text(gx + 4, sy + 58, "Magia:");
            screens.Text(gx + 4, sy + 68, "Doswiad:");
            screens.Text(gx + 4, sy + 78, "Obciazenie:");

            // Stats values - right aligned
            screens.Ink(16, 19);
            screens.Text(gx + 62, sy + 18, $"{ARMIA[arm, unit, TE]}/{ARMIA[arm, unit, TEM]}");
            screens.Text(gx + 62, sy + 28, amos.Str_S(ARMIA[arm, unit, TSI]));
            screens.Text(gx + 62, sy + 38, amos.Str_S(ARMIA[arm, unit, TSZ]));
            screens.Text(gx + 62, sy + 48, amos.Str_S(ARMIA[arm, unit, TP]));
            screens.Text(gx + 62, sy + 58, $"{ARMIA[arm, unit, TMAG]}/{ARMIA[arm, unit, TMAGMA]}");
            screens.Text(gx + 62, sy + 68, amos.Str_S(ARMIA[arm, unit, TDOSW]));

            // Weight - red if overloaded
            if (ARMIA[arm, unit, TWAGA] > ARMIA[arm, unit, TEM])
            {
                screens.Ink(20, 19);
            }
            screens.Text(gx + 62, sy + 78, amos.Str_S(ARMIA[arm, unit, TWAGA]));
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
        /// Redraws all dynamic panels (roster, active unit paperdoll, ground stash, stats).
        /// </summary>
        private void INVENTORY_NEW_DRAW_ALL(int arm, int unit, int sek, int groundScroll)
        {
            INVENTORY_NEW_DRAW_ROSTER(arm, unit);
            INVENTORY_NEW_DRAW_UNIT(arm, unit);
            INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
            INVENTORY_NEW_DRAW_STATS(arm, unit);
        }

        /// <summary>
        /// Drag-and-drop handler (transplanted from original WYBOR_PICK logic).
        /// Handles PRZELICZ for equipment, ammo consumption, potion/herb usage, item info display.
        /// </summary>
        private void INVENTORY_NEW_PICK(int item, int arm, int unit, int sek,
            int fromSlot, bool fromGround, int groundScroll)
        {
            int bb = BRON[item, B_BOB] + BROBY;
            int place = BRON[item, B_PLACE];
            int typ = BRON[item, B_TYP];
            bool done = false;

            screens.HideOn();
            screens.HotSpot(bb, 11);
            screens.NoMask(bb + GOBY);

            // Item info bar (center panel bottom)
            int cx = 90, cy = 4;
            screens.Ink(19);
            screens.Bar(cx + 6, cy + 145, cx + 110, cy + 160);
            screens.Ink(3, 19);
            screens.Text(cx + 8, cy + 150, BRON2_S[typ] + " " + BRON_S[item]);
            screens.Text(cx + 8, cy + 158, "Waga:" + BRON[item, B_WAGA]);

            do
            {
                int xm = screens.XScreen(screens.XMouse());
                int ym = screens.YScreen(screens.YMouse());
                screens.Sprite(53, screens.XMouse(), screens.YMouse(), bb + GOBY);
                screens.WaitVbl();

                if (screens.MouseClick() == 1)
                {
                    screens.SpriteOff(53);
                    screens.WaitVbl();
                    screens.HotSpot(bb, 0);

                    int dropZone = screens.Zone(xm, ym);

                    // Equipment slots: must be empty (same as original WYBOR)
                    if (dropZone >= 11 && dropZone <= 15)
                    {
                        int targetSlot = INVENTORY_NEW_ZONE_TO_SLOT(dropZone);
                        if (targetSlot > 0)
                        {
                            int oldItem = ARMIA[arm, unit, targetSlot];
                            if (oldItem == 0)
                            {
                                bool valid = false;
                                if (targetSlot == TGLOWA && place == 1) valid = true;
                                else if (targetSlot == TKORP && place == 2) valid = true;
                                else if (targetSlot == TNOGI && place == 3) valid = true;
                                else if (targetSlot == TPRAWA && (place == 4 || place == 6)) valid = true;
                                else if (targetSlot == TLEWA && (place == 4 || (place == 6 && ARMIA[arm, unit, TPRAWA] == 0))) valid = true;

                                if (valid)
                                {
                                    ARMIA[arm, unit, targetSlot] = item;
                                    PRZELICZ(targetSlot - TGLOWA, 1);

                                    // Potions/herbs: apply effects then remove from slot
                                    if (typ == 13 || typ == 18)
                                    {
                                        ARMIA[arm, unit, targetSlot] = 0;
                                    }
                                    done = true;
                                }
                                else
                                {
                                    ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                                    done = true;
                                }
                            }
                            else
                            {
                                ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                                done = true;
                            }
                        }
                        else
                        {
                            ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                            done = true;
                        }
                    }
                    // Backpack slots
                    else if (dropZone >= 16 && dropZone <= 23)
                    {
                        int bpSlot = dropZone - 16;

                        // Ammo: consume into ammo pool
                        if (typ == 17)
                        {
                            amos.Add(ref ARMIA[arm, 0, TAMO], BRON[item, B_DOSW], ARMIA[arm, 0, TAMO], 320);
                            done = true;
                        }
                        else
                        {
                            int oldItem = ARMIA[arm, unit, TPLECAK + bpSlot];
                            ARMIA[arm, unit, TPLECAK + bpSlot] = item;
                            if (oldItem == 0)
                            {
                                done = true;
                            }
                            else
                            {
                                item = oldItem;
                                place = BRON[item, B_PLACE];
                                typ = BRON[item, B_TYP];
                                bb = BRON[item, B_BOB] + BROBY;
                                screens.HotSpot(bb, 11);
                                screens.NoMask(bb + GOBY);
                                // Update info bar for swapped item
                                screens.Ink(19);
                                screens.Bar(cx + 6, cy + 145, cx + 110, cy + 160);
                                screens.Ink(3, 19);
                                screens.Text(cx + 8, cy + 150, BRON2_S[typ] + " " + BRON_S[item]);
                                screens.Text(cx + 8, cy + 158, "Waga:" + BRON[item, B_WAGA]);
                            }
                        }
                    }
                    // Ground stash slots
                    else if (dropZone >= 30 && dropZone <= 37)
                    {
                        int gSlot = (dropZone - 30) + groundScroll;
                        if (gSlot < 9)
                        {
                            int oldItem = GLEBA[sek, gSlot];
                            GLEBA[sek, gSlot] = item;
                            if (oldItem == 0)
                            {
                                done = true;
                            }
                            else
                            {
                                item = oldItem;
                                place = BRON[item, B_PLACE];
                                typ = BRON[item, B_TYP];
                                bb = BRON[item, B_BOB] + BROBY;
                                screens.HotSpot(bb, 11);
                                screens.NoMask(bb + GOBY);
                                // Update info bar for swapped item
                                screens.Ink(19);
                                screens.Bar(cx + 6, cy + 145, cx + 110, cy + 160);
                                screens.Ink(3, 19);
                                screens.Text(cx + 8, cy + 150, BRON2_S[typ] + " " + BRON_S[item]);
                                screens.Text(cx + 8, cy + 158, "Waga:" + BRON[item, B_WAGA]);
                            }
                        }
                        else
                        {
                            ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                            done = true;
                        }
                    }
                    else
                    {
                        ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                        done = true;
                    }
                }
                else if (screens.MouseKey() == PRAWY)
                {
                    screens.SpriteOff(53);
                    screens.WaitVbl();
                    screens.HotSpot(bb, 0);
                    ReturnItemToSource(item, arm, unit, sek, fromSlot, fromGround);
                    done = true;
                }
            }
            while (!done);

            WAGA(arm, unit);
            INVENTORY_NEW_DRAW_UNIT(arm, unit);
            INVENTORY_NEW_DRAW_GROUND(sek, groundScroll);
            INVENTORY_NEW_DRAW_STATS(arm, unit);
            screens.ShowOn();
        }

        /// <summary>
        /// Returns an item to its original slot (undoes the pick-up).
        /// </summary>
        private void ReturnItemToSource(int item, int arm, int unit, int sek, int fromSlot, bool fromGround)
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

        /// <summary>
        /// Auto-equips ground items onto the SELECTED unit only.
        /// Uses the same logic as SKLEP_AUTO_EQUIP/WYBOR_AUTO_EQUIP with PRZELICZ.
        /// </summary>
        private void INVENTORY_NEW_AUTO_EQUIP(int arm, int selectedUnit, int sek)
        {
            // Ensure NUMER is set for PRZELICZ
            NUMER = selectedUnit;

            for (int g = 0; g < 9; g++)
            {
                int item = GLEBA[sek, g];
                if (item == 0) continue;

                int place = BRON[item, B_PLACE];
                int typ = BRON[item, B_TYP];
                bool taken = false;

                // Ammo: consume into ammo pool
                if (typ == 17)
                {
                    amos.Add(ref ARMIA[arm, 0, TAMO], BRON[item, B_DOSW], ARMIA[arm, 0, TAMO], 320);
                    GLEBA[sek, g] = 0;
                    continue;
                }

                // Try to equip directly (potions/herbs also equipped to apply effects)
                if (place == 1 && ARMIA[arm, selectedUnit, TGLOWA] == 0)
                {
                    ARMIA[arm, selectedUnit, TGLOWA] = item;
                    PRZELICZ(0, 1);
                    // Potions/herbs: apply effects then remove from slot
                    if (typ == 13 || typ == 18)
                    {
                        ARMIA[arm, selectedUnit, TGLOWA] = 0;
                    }
                    taken = true;
                }
                else if (place == 2 && ARMIA[arm, selectedUnit, TKORP] == 0)
                {
                    ARMIA[arm, selectedUnit, TKORP] = item;
                    PRZELICZ(1, 1);
                    // Potions: apply effects then remove from slot
                    if (typ == 13)
                    {
                        ARMIA[arm, selectedUnit, TKORP] = 0;
                    }
                    taken = true;
                }
                else if (place == 3 && ARMIA[arm, selectedUnit, TNOGI] == 0)
                {
                    ARMIA[arm, selectedUnit, TNOGI] = item;
                    PRZELICZ(2, 1);
                    taken = true;
                }
                else if (place == 4) // One-handed
                {
                    if (ARMIA[arm, selectedUnit, TLEWA] == 0)
                    {
                        ARMIA[arm, selectedUnit, TLEWA] = item;
                        PRZELICZ(3, 1);
                        taken = true;
                    }
                    else if (ARMIA[arm, selectedUnit, TPRAWA] == 0)
                    {
                        ARMIA[arm, selectedUnit, TPRAWA] = item;
                        PRZELICZ(4, 1);
                        taken = true;
                    }
                }
                else if (place == 6 && ARMIA[arm, selectedUnit, TLEWA] == 0 && ARMIA[arm, selectedUnit, TPRAWA] == 0)
                {
                    ARMIA[arm, selectedUnit, TLEWA] = item;
                    PRZELICZ(3, 1);
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
