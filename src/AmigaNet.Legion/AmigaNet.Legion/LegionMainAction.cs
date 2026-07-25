using AmigaNet.Amos.Screens;
using AmigaNet.Legion.Pathfinding;
using AmigaNet.Types.Graphics;

namespace AmigaNet.Legion
{
    public partial class Legion
    {
        void MAIN_ACTION()
        {
            KONIEC_AKCJI = false;
            WYNIK_AKCJI = 0;
            //'prze�adowanie �uk�w 
            for (var I = 1; I <= 10; I++) STRZALY[I] = 10;
            screens.Screen(2);
            screens.GetPalette(0);
            screens.Screen(0);
            screens.ChangeMouse(BUBY + 6);
            screens.ShowOn();
            screens.Screen(1);
            USTAW_FONT("defender2", 8);
            screens.ScreenToFront(1);
            NUMER = 1;
            EKRAN1();
            screens.Screen(0);
            MARKERS();
            SELECT(ARM, NUMER);
            var X1 = ARMIA[ARM, NUMER, TX];
            var Y1 = ARMIA[ARM, NUMER, TY];
            CENTER(X1, Y1, 0);
            screens.View();
            screens.BobUpdate();
            screens.WaitVbl();

            while (true)
            {
                screens.Screen(0);
                var HY = screens.YMouse();
                if (screens.MouseClick() == 1)
                {
                    var STREFA0 = screens.MouseZone();
                    if (STREFA0 < 11 && STREFA0 > 0)
                    {
                        NUMER = STREFA0;
                        SELECT(ARM, NUMER);
                    }
                    if (STREFA0 > 10 && STREFA0 < 21)
                    {
                        screens.Screen(1);
                        WYKRESY(WRG, STREFA0 - 10);
                        while (screens.MouseKey() == LEWY) { }
                        if (NUMER > 0)
                        {
                            WYKRESY(ARM, NUMER);
                        }
                    }
                }

                var A_S = screens.Inkey_S();
                var KLAW = screens.Scancode();
                if (screens.MouseKey() == PRAWY)
                {
                    SKROL(0);
                }
                if (KLAW == KeyLeft || KLAW == KeyUp || KLAW == KeyRight || KLAW == KeyDown)
                {
                    KLAWSKROL(KLAW);
                }
                if (KLAW >= KeyF1 && KLAW <= KeyF10)
                {
                    SELECT(ARM, KLAW - KeyF1 + 1);
                    CENTER(ARMIA[ARM, NUMER, TX], ARMIA[ARM, NUMER, TY], 2);
                }
                if (A_S != "") HY = 300;
                if (KONIEC_AKCJI) break;
                while (HY > 275)
                {
                    screens.Screen(1);
                    if (A_S != "" || screens.MouseClick() == 1 && NUMER > 0)
                    {
                        GADUP(LAST_GAD);
                        var STREFA = screens.MouseZone();
                        A_S = A_S.ToUpperInvariant();
                        if (A_S == "R") STREFA = 1;
                        if (A_S == "A") STREFA = 2;
                        if (A_S == "S") STREFA = 3;
                        if (A_S == "G") STREFA = 4;
                        if (A_S == " ") STREFA = 10;
                        if (screens.KeyState(KeyEscape) || ARMIA[ARM, 0, TE] == 0 || KONIEC_AKCJI) goto Exit2;
                        if (STREFA == 20 || STREFA == 21) BRON_INFO(STREFA, NUMER);
                        if (STREFA > 0 && STREFA < 11)
                        {
                            GADDOWN(STREFA);
                            LAST_GAD = STREFA;
                            if (STREFA == 1)
                            {
                                RUCH();
                            }
                            if (STREFA == 2)
                            {
                                _ATAK(2);
                            }
                            if (STREFA == 3)
                            {
                                STRZAL();
                            }
                            if (STREFA == 4)
                            {
                                _ATAK(6);
                            }
                            if (STREFA == 10)
                            {
                                AKCJA();
                            }
                            if (STREFA == 5)
                            {
                                GADGET(170, 2, 20, 20, "bob9", 2, 0, 19, 5, 0);
                                WYBOR(0);
                            }
                            if (STREFA == 9)
                            {
                                GADGET(147, 2, 20, 20, "bob15", 2, 0, 19, 5, 0);
                                ROZKAZ();
                                if (KONIEC_AKCJI) goto Exit2;
                            }
                        }
                    }

                    A_S = screens.Inkey_S();
                    screens.ClearKey();
                    HY = screens.YMouse();
                }
            }
        Exit2:;
            //'------------------
            screens.Screen(0);
            screens.Fade(2);
            screens.Screen(1);
            for (var J = 0; J <= 25; J++)
            {
                screens.ScreenDisplay(1, 130, 275 + J, 320, 25 - J);
                screens.WaitVbl();
                screens.View();
            }
            screens.ScreenClose(1);
            for (var J = 0; J <= 110; J++)
            {
                for (var I = 0; I <= 4; I++)
                {
                    GLEBA[J, I] = 0;
                }
            }
            var WOJ1 = 0;
            var WOJ2 = 0;
            var SILA = 0;
            var SILA2 = 0;
            var SPEED = 0;
            var SPEED2 = 0;
            //'update------------
            for (var I = 1; I <= 10; I++)
            {
                if (ARMIA[WRG, I, TE] > 0)
                {
                    WOJ2++;
                    SILA2 += ARMIA[WRG, I, TSI];
                    SPEED2 += ARMIA[WRG, I, TSZ];
                }
                else
                {
                    ARMIA[WRG, I, TE] = 0;
                }

                if (ARMIA[ARM, I, TE] > 0)
                {
                    WOJ1++;
                    SILA += ARMIA[ARM, I, TSI];
                    SPEED += ARMIA[ARM, I, TSZ];
                }
                else
                {
                    ARMIA[ARM, I, TE] = 0;
                }

                ARMIA[ARM, I, TTRYB] = 0;
                ARMIA[WRG, I, TTRYB] = 0;
                if (WOJ1 > 0) SPEED = ((SPEED / WOJ1) / 5);
                if (WOJ2 > 0) SPEED2 = ((SPEED2 / WOJ2) / 5);
                ARMIA[ARM, 0, TE] = WOJ1;
                ARMIA[ARM, 0, TSI] = SILA;
                ARMIA[ARM, 0, TSZ] = SPEED;
                ARMIA[WRG, 0, TE] = WOJ2;
                ARMIA[WRG, 0, TSI] = SILA2;
                ARMIA[WRG, 0, TSZ] = SPEED2;
                KTO_ATAKUJE = -1;
                _M_FADE(1);
                banks.EraseAll();
                //Sam Stop 
                screens.ScreenClose(2);
                screens.ScreenClose(0);
            }
        }

        void EKRAN1()
        {
            screens.Screen(1);
            screens.Cls(0);
            screens.PasteBob(0, 0, 1);
            GADGET(200, 2, 20, 20, "bob3", 2, 0, 19, 5, 1);
            GADGET(222, 2, 20, 20, "bob4", 2, 0, 19, 5, 2);
            GADGET(244, 2, 20, 20, "bob5", 2, 0, 19, 5, 3);
            GADGET(266, 2, 20, 20, "bob6", 2, 0, 19, 5, 4);
            GADGET(297, 2, 20, 20, "bob7", 2, 0, 19, 5, 10);
            GADGET(170, 2, 20, 20, "bob2", 2, 0, 19, 5, 5);
            GADGET(147, 2, 20, 20, "bob8", 2, 0, 19, 5, 9);
            GADGET(2, 2, 140, 20, "", 0, 1, 19, 16, 0);
        }

        void MARKERS()
        {
            for (var I = 1; I <= 10; I++)
            {
                if (ARMIA[ARM, I, TE] > 0)
                {
                    var X = ARMIA[ARM, I, TX];
                    var Y = ARMIA[ARM, I, TY] - 45;
                    screens.Bob(20 + I, X, Y, PIKIETY + 18 + 10);
                }
            }
            screens.BobUpdate();
            screens.WaitVbl();
        }

        void MARKERS_OFF()
        {
            for (var I = 1; I <= 10; I++)
            {
                screens.BobOff(20 + I);
            }
            screens.BobUpdate();
            screens.WaitVbl();
        }

        void SELECT(int A, int NR)
        {
            screens.Screen(1);
            var TRYB = ARMIA[A, NR, TTRYB];
            NUMER = NR;
            var JEST = false;
            if (ARMIA[A, NR, TE] <= 0)
            {
                JEST = false;
                for (var I = 1; I <= 10; I++)
                {
                    if (ARMIA[A, I, TE] > 0)
                    {
                        NUMER = I;
                        NR = I;
                        I = 10;
                        JEST = true;
                    }
                }
            }
            else
            {
                JEST = true;
            }
            if (!JEST)
            {
                KONIEC_AKCJI = true;
                return;
            }
            var GADE = TRYB;
            if (TRYB == 4 || TRYB == 5)
            {
                GADE = 3;
            }
            if (TRYB == 6)
            {
                GADE = 4;
            }
            GADUP(LAST_GAD);
            GADDOWN(GADE);
            LAST_GAD = GADE;
            var X = ARMIA[A, NR, TX];
            var Y = ARMIA[A, NR, TY];
            var X2 = 0;
            var Y2 = 0;
            var Y22 = 0;
            if (TRYB == 1 || TRYB == 3 || TRYB == 4)
            {
                X2 = ARMIA[ARM, NUMER, TCELX];
                Y2 = ARMIA[ARM, NUMER, TCELY];
            }
            if (TRYB == 2 || TRYB == 5 || TRYB == 6)
            {
                var TARGET = ARMIA[ARM, NUMER, TCELX];
                var B = ARMIA[ARM, NUMER, TCELY];
                X2 = ARMIA[B, TARGET, TX];
                Y2 = ARMIA[B, TARGET, TY];
            }
            if (TRYB == 3 || TRYB == 4)
            {
                Y22 = 12;
            }
            else
            {
                Y22 = 0;
            }
            screens.Screen(0);
            screens.Bob(50, X, Y, 1 + BUBY);
            if (TRYB != 0)
            {
                screens.Bob(51, X2, Y2 + Y22, 2 + BUBY);
            }
            else
            {
                screens.BobOff(51);
                screens.BobUpdate();
                screens.WaitVbl();
            }
            screens.BobUpdate();
            screens.WaitVbl();
            screens.Screen(1);
            WYKRESY(A, NR);
        }

        void GADUP(int GN)
        {
            var SC = screens.Screen();
            screens.Screen(1);
            if (GN == 1 || GN == -1) GADGET(200, 2, 20, 20, "bob3", 2, 0, 19, 5, -1);
            if (GN == 2 || GN == -1) GADGET(222, 2, 20, 20, "bob4", 2, 0, 19, 5, -1);
            if (GN == 3 || GN == -1) GADGET(244, 2, 20, 20, "bob5", 2, 0, 19, 5, -1);
            if (GN == 4 || GN == -1) GADGET(266, 2, 20, 20, "bob6", 2, 0, 19, 5, -1);
            if (GN == 10 || GN == -1) GADGET(297, 2, 20, 20, "bob7", 2, 0, 19, 5, -1);
            screens.Screen(SC);
        }

        void GADDOWN(int GN)
        {
            var SC = screens.Screen();
            screens.Screen(1);
            if (GN == 1) GADGET(200, 2, 20, 20, "bob10", 0, 2, 16, 4, 0);
            if (GN == 2) GADGET(222, 2, 20, 20, "bob11", 0, 2, 16, 4, 0);
            if (GN == 3) GADGET(244, 2, 20, 20, "bob12", 0, 2, 16, 4, 0);
            if (GN == 4) GADGET(266, 2, 20, 20, "bob13", 0, 2, 16, 4, 0);
            if (GN == 10) GADGET(297, 2, 20, 20, "bob14", 0, 2, 16, 4, 0);
            screens.Screen(SC);
        }

        void BRON_INFO(int STREFA, int NUMER)
        {
            var BRO = 0;
            if (STREFA == 20)
            {
                BRO = ARMIA[ARM, NUMER, TLEWA];
            }
            if (STREFA == 21)
            {
                BRO = ARMIA[ARM, NUMER, TPRAWA];
            }
            screens.GetBlock(1, 3, 3, 96, 18);
            screens.GrWriting(0);
            OUTLINE(12, 14, BRON_S[BRO], 31, 19);
            while (screens.MouseKey() == LEWY) { }
            screens.PutBlock(1);
        }

        void OUTLINE(int X, int Y, String A_S, int K1, int K2)
        {
            screens.GrWriting(0);
            screens.Ink(K2);
            screens.Text(X, Y + 1, A_S);
            screens.Text(X + 1, Y, A_S);
            screens.Text(X - 1, Y, A_S);
            screens.Text(X, Y - 1, A_S);
            screens.Ink(K1);
            screens.Text(X, Y, A_S);
        }

        void ROZKAZ()
        {
            var KONIEC = false;
            screens.Screen(1);
            screens.PasteBob(0, 0, 1);
            screens.ResetZone();
            GADGET(8, 4, 70, 15, "Obrona", 26, 24, 25, 30, 1);
            GADGET(82, 4, 70, 15, "Atak", 26, 24, 25, 30, 2);
            GADGET(156, 4, 70, 15, "Odwrót", 26, 24, 25, 30, 3);
            GADGET(230, 4, 70, 15, "Koniec Akcji", 26, 24, 25, 30, 4);
            while (screens.MouseKey() == LEWY) { }

            do
            {
                var HY = screens.YMouse();
                if (HY > 275)
                {
                    var A_S = screens.Inkey_S();
                    var KLAW = screens.Scancode();
                    if (KLAW >= KeyLeft && KLAW <= KeyDown)
                    {
                        KLAWSKROL(KLAW);
                    }
                    var MYSZ = screens.MouseKey();
                    if (MYSZ == LEWY)
                    {
                        var STREFA = screens.MouseZone();
                        if (STREFA == 1)
                        {
                            GADGET(8, 4, 70, 15, "Obrona", 23, 26, 24, 30, 0);
                            GADGET(8, 4, 70, 15, "Obrona", 26, 24, 25, 30, 0);
                            KONIEC = true;
                            for (var I = 1; I <= 10; I++)
                            {
                                ARMIA[ARM, I, TTRYB] = 0;
                            }
                        }
                        if (STREFA == 2)
                        {
                            KONIEC = true;
                            GADGET(82, 4, 70, 15, "Atak", 23, 26, 24, 30, 0);
                            GADGET(82, 4, 70, 15, "Atak", 26, 24, 25, 30, 0);
                            for (var I = 1; I <= 10; I++)
                            {
                                if (ARMIA[ARM, I, TE] > 0)
                                {
                                    var X1 = ARMIA[ARM, I, TX];
                                    var Y1 = ARMIA[ARM, I, TY];
                                    var STARAODL = WIDOCZNOSC;
                                    var WIDAC = false;
                                    var CX = 0;
                                    var CY = 0;
                                    var TARGET = 0;
                                    for (var J = 1; J <= 10; J++)
                                    {
                                        if (ARMIA[WRG, J, TE] > 0)
                                        {
                                            var X2 = ARMIA[WRG, J, TX];
                                            var Y2 = ARMIA[WRG, J, TY];
                                            ODL(X1, Y1, X2, Y2);

                                            if (ODLEG < STARAODL)
                                            {
                                                TARGET = J;
                                                CX = X2;
                                                CY = Y2;
                                                STARAODL = ODLEG;
                                                WIDAC = true;
                                            }
                                        }
                                    }
                                    if (WIDAC)
                                    {
                                        ARMIA[ARM, I, TTRYB] = 2;
                                        ARMIA[ARM, I, TCELX] = TARGET;
                                        ARMIA[ARM, I, TCELY] = WRG;
                                    }
                                }
                            }
                        }

                        if (STREFA == 4)
                        {
                            KONIEC = true;
                            KONIEC_AKCJI = true;
                            WYNIK_AKCJI = 0;
                            for (var I = 1; I <= 10; I++)
                            {
                                if (ARMIA[ARM, I, TE] > 0)
                                {
                                    var X = ARMIA[ARM, I, TX];
                                    var Y = ARMIA[ARM, I, TY];
                                    if (X > 22 && X < 610 && Y > 30 && Y < 500)
                                    {
                                        KONIEC = false;
                                        KONIEC_AKCJI = false;
                                    }
                                }
                            }
                            if (KONIEC == false)
                            {
                                WYNIK_AKCJI = 1;
                                KONIEC = true;
                                KONIEC_AKCJI = true;
                                for (var I = 1; I <= 10; I++)
                                {
                                    if (ARMIA[WRG, I, TE] > 0 && ARMIA[WRG, I, TKORP] > 90)
                                    {
                                        KONIEC = false;
                                        KONIEC_AKCJI = false;
                                        WYNIK_AKCJI = 0;
                                    }
                                }
                            }
                            if (KONIEC_AKCJI)
                            {
                                GADGET(230, 4, 70, 15, "Koniec Akcji", 23, 26, 24, 30, 0);
                                GADGET(230, 4, 70, 15, "Koniec Akcji", 26, 24, 25, 30, 0);
                            }
                        }
                        if (STREFA == 3)
                        {
                            GADGET(156, 4, 70, 15, "Odwrót", 23, 26, 24, 30, 0);
                            GADGET(156, 4, 70, 15, "Odwrót", 26, 24, 25, 30, 0);

                            for (var I = 1; I <= 10; I++)
                            {
                                if (ARMIA[ARM, I, TE] > 0)
                                {
                                    var X = ARMIA[ARM, I, TX];
                                    var Y = ARMIA[ARM, I, TY];
                                    var CX = 0;
                                    var CY = 0;
                                    var ODL = 400;
                                    if (X < ODL)
                                    {
                                        ODL = X;
                                        CY = Y;
                                        CX = 17;
                                    }
                                    if (Y < ODL)
                                    {
                                        ODL = Y;
                                        CY = 22;
                                        CX = X;
                                    }
                                    if (640 - X < ODL)
                                    {
                                        ODL = 640 - X;
                                        CY = Y;
                                        CX = 623;
                                    }
                                    if (512 - Y < ODL)
                                    {
                                        ODL = 512 - Y;
                                        CY = 512;
                                        CX = X;
                                    }
                                    ARMIA[ARM, I, TTRYB] = 1;
                                    ARMIA[ARM, I, TCELX] = CX;
                                    ARMIA[ARM, I, TCELY] = CY;
                                }
                            }
                            KONIEC = true;
                        }
                    }
                    if (MYSZ == PRAWY)
                    {
                        KONIEC = true;
                    }
                }
                else
                {
                    HY = screens.YMouse();
                    var A_S = screens.Inkey_S();
                    var KLAW = screens.Scancode();
                    if (KLAW >= KeyLeft && KLAW <= KeyDown)
                    {
                        KLAWSKROL(KLAW);
                    }
                    if (screens.MouseKey() == PRAWY)
                    {
                        SKROL(0);
                    }
                }
            }
            while (!KONIEC);
            screens.ResetZone();
            EKRAN1();
            SELECT(ARM, NUMER);
        }

        void ODL(int X1, int Y1, int X2, int Y2)
        {
            var DX = X2 - X1;
            var DY = Y2 - Y1;
            ODLEG = (int)Math.Abs(Math.Sqrt(DX * DX + DY * DY)); 
        }

        void AKCJA()
        {
            screens.Screen(0);
            screens.BobOff(50);
            screens.BobOff(51);
            MARKERS_OFF();
            var WOJ = 0;
            MUZYKA = true;
            do
            {
                if (screens.MouseKey() == PRAWY)
                {
                    screens.Screen(0);
                    SKROL(0);
                }
                var A_S = "";
                A_S = screens.Inkey_S();
                var KLAW = screens.Scancode();
                if (KLAW >= KeyLeft && KLAW <= KeyDown)
                {
                    KLAWSKROL(KLAW);
                }
                var HALAS = 0;
                
                for (var I = 1; I <= 10; I++)
                {
                    if (ARMIA[ARM, I, TE] > 0)
                    {
                        WOJ++;
                        var TRYB = ARMIA[ARM, I, TTRYB];
                        if (TRYB == 0)
                        {
                            var BAZA = ARMIA[ARM, I, TBOB];
                            var BNR = 0;
                            if (KTO_ATAKUJE == ARM)
                            {
                                BNR = BAZA + 1;
                            }
                            else
                            {
                                BNR = BAZA + 7;
                            }
                            screens.Bob(I, -1, -1, BNR);
                            goto SKIP1;
                        }
                        if (TRYB == 1)
                        {
                            A_RUCH(ARM, I);
                            goto SKIP1;
                        }
                        if (TRYB == 2)
                        {
                            A_ATAK(ARM, I);
                            HALAS++;
                            goto SKIP1;
                        }
                        if (TRYB == 3)
                        {
                            A_STRZAL(ARM, I);
                            goto SKIP1;
                        }
                        if (TRYB == 4 || TRYB == 5)
                        {
                            A_CZAR(ARM, I);
                            HALAS++;
                            goto SKIP1;
                        }
                        if (TRYB == 6)
                        {
                            A_ROZMOWA(ARM, I);
                            goto SKIP1;
                        }
                        if (TRYB == 7 || TRYB == 8)
                        {
                            A_LOT(ARM, I, TRYB);
                            goto SKIP1;
                        }
                        if (TRYB == 9)
                        {
                            // Auto-fire mode with auto-aim
                            var tgtIdx = AutoFireTargetIdx[I];
                            // Track target position every tick for velocity estimation
                            if (tgtIdx > 0 && ARMIA[WRG, tgtIdx, TE] > 0)
                            {
                                AutoFirePrevTX[I] = ARMIA[WRG, tgtIdx, TX];
                                AutoFirePrevTY[I] = ARMIA[WRG, tgtIdx, TY];
                            }
                            if (FireCooldown[I] > 0)
                            {
                                FireCooldown[I]--;
                            }
                            else
                            {
                                // Check if unit still has ranged capability
                                var BL = ARMIA[ARM, I, TLEWA];
                                var BP = ARMIA[ARM, I, TPRAWA];
                                var BTL = BRON[BL, B_TYP];
                                var BTP = BRON[BP, B_TYP];
                                var canFire = (BTL == 4 && BTP == 5 && STRZALY[I] > 0) ||
                                              (BTL == 5 && BTP == 4 && STRZALY[I] > 0) ||
                                              (BTL == 15 && BTP == 16) ||
                                              (BTL == 16 && BTP == 15) ||
                                              (BTL == 9 && BTP != 12) ||
                                              (BTP == 9 && BTL != 12);
                                if (canFire)
                                {
                                    int aimX, aimY;
                                    if (tgtIdx > 0 && ARMIA[WRG, tgtIdx, TE] > 0)
                                    {
                                        // Auto-aim: extrapolate target movement + spread
                                        var (ax, ay) = ComputeAutoAim(ARM, I, WRG, tgtIdx);
                                        aimX = (int)ax;
                                        aimY = (int)ay;
                                    }
                                    else
                                    {
                                        // No valid target unit, fire at stored position
                                        aimX = ARMIA[ARM, I, TCELX];
                                        aimY = ARMIA[ARM, I, TCELY];
                                    }
                                    FireProjectileFromUnit(ARM, I, aimX, aimY);
                                }
                                else
                                {
                                    ARMIA[ARM, I, TTRYB] = 0;
                                }
                            }
                            goto SKIP1;
                        }
                    SKIP1:;
                    }
                    else
                    {
                        CZEKAJ();
                    }

                    if (ARMIA[WRG, I, TE] > 0)
                    {
                        var TRYB = ARMIA[WRG, I, TTRYB];
                        if (TRYB == 0)
                        {
                            WYDAJ_ROZKAZ(I);
                            goto SKIP2;
                        }
                        if (TRYB == 1)
                        {
                            A_RUCH(WRG, I);
                            if (amos.Rnd(20) == 1)
                            {
                                WYDAJ_ROZKAZ(I);
                            }
                            goto SKIP2;
                        }
                        if (TRYB == 2)
                        {
                            A_ATAK(WRG, I);
                            HALAS++;
                            if (amos.Rnd(10) == 1)
                            {
                                WYDAJ_ROZKAZ(I);
                            }
                            goto SKIP2;
                        }
                        if (TRYB == 3)
                        {
                            A_STRZAL(WRG, I);
                            goto SKIP2;
                        }
                        if (TRYB == 4 || TRYB == 5)
                        {
                            A_CZAR(WRG, I);
                            HALAS++;
                            goto SKIP2;
                        }
                        if (TRYB == 7 || TRYB == 8)
                        {
                            A_LOT(WRG, I, TRYB);
                            goto SKIP2;
                        }
                        if (TRYB == 9)
                        {
                            // Auto-fire mode for enemy with auto-aim
                            var tgtIdx = AutoFireTargetIdxW[I];
                            // Track target position every tick for velocity estimation
                            if (tgtIdx > 0 && ARMIA[ARM, tgtIdx, TE] > 0)
                            {
                                AutoFirePrevTXW[I] = ARMIA[ARM, tgtIdx, TX];
                                AutoFirePrevTYW[I] = ARMIA[ARM, tgtIdx, TY];
                            }
                            if (FireCooldownW[I] > 0)
                            {
                                FireCooldownW[I]--;
                            }
                            else
                            {
                                var BL = ARMIA[WRG, I, TLEWA];
                                var BP = ARMIA[WRG, I, TPRAWA];
                                var BTL = BRON[BL, B_TYP];
                                var BTP = BRON[BP, B_TYP];
                                var canFire = (BTL == 4 && BTP == 5 && ARMIA[WRG, I, TAMO] > 0) ||
                                              (BTL == 5 && BTP == 4 && ARMIA[WRG, I, TAMO] > 0) ||
                                              (BTL == 15 && BTP == 16) ||
                                              (BTL == 16 && BTP == 15) ||
                                              (BTL == 9 && BTP != 12) ||
                                              (BTP == 9 && BTL != 12);
                                if (canFire)
                                {
                                    int aimX, aimY;
                                    if (tgtIdx > 0 && ARMIA[ARM, tgtIdx, TE] > 0)
                                    {
                                        // Auto-aim: extrapolate target movement + spread
                                        var (ax, ay) = ComputeAutoAim(WRG, I, ARM, tgtIdx);
                                        aimX = (int)ax;
                                        aimY = (int)ay;
                                    }
                                    else
                                    {
                                        // No valid target, fire at stored position
                                        aimX = ARMIA[WRG, I, TCELX];
                                        aimY = ARMIA[WRG, I, TCELY];
                                    }
                                    FireProjectileFromUnit(WRG, I, aimX, aimY);
                                }
                                else
                                {
                                    ARMIA[WRG, I, TTRYB] = 0;
                                }
                            }
                            // Periodically re-evaluate orders
                            if (amos.Rnd(10) == 1)
                            {
                                WYDAJ_ROZKAZ(I);
                            }
                            goto SKIP2;
                        }
                    SKIP2:;
                    }
                    else
                    {
                        CZEKAJ();
                    }
                }

                // Update all active projectiles in the pool
                UPDATE_PROJECTILES();

                Thread.Sleep(80); // not included in original Legion, added here to control action speed

                screens.BobUpdate();
                screens.WaitVbl();

                if (HALAS > 3 && MUZYKA)
                {
                    //Music Stop
                    MUZYKA = false;
                }

                if (HALAS <= 3 && MUZYKA == false)
                {
                    MUZYKA = true;
                    //Music 1
                }

            }
            while (!(screens.MouseKey() == LEWY || screens.Inkey_S() == " " || WOJ == 0));

            // Clean up all active projectiles
            ClearAllProjectiles();

            //Sam Loop Off
            if (!MUZYKA)
            {
                //Sam Stop;
                //Music 1;
            }
            MARKERS();
            if (WOJ == 0)
            {
                ARMIA[ARM, 0, TE] = 0;
                WYNIK_AKCJI = 2;
            }
            GADUP(10);
            var STREFA0 = screens.MouseZone();
            if (STREFA0 < 11 && STREFA0 > 0)
            {
                NUMER = STREFA0;
            }
            SELECT(ARM, NUMER);
        }

        void CZEKAJ()
        {
        }

        /// <summary>
        /// Per-unit cached A* path for battle-screen walk orders (see <see cref="A_RUCH"/>).
        /// Indexed the same way as <c>ARMIA</c> (army, unit slot).
        /// </summary>
        private class UnitPathState
        {
            public List<(int X, int Y)> Waypoints = new();
            public int Index;
            public int PlannedForX;
            public int PlannedForY;
            public int PlannedAtEpoch = -1;

            // Local steering commitment: once a sidestep direction is chosen,
            // stick with it for several ticks to prevent oscillation ("dancing").
            // SteerAxis: 0 = none, 1 = vertical (Y sidestep), 2 = horizontal (X sidestep).
            public int SteerAxis;
            public int SteerDir; // -1 or +1
            public int SteerTicks;
        }

        private readonly UnitPathState[,] unitPaths = new UnitPathState[41, 11];

        // How far the unit must be from a waypoint before it's considered "reached" -
        // matches A_RUCH's own arrival threshold (Abs(ROZX)<=4 && Abs(ROZY)<=4).
        private const int WaypointArriveDistance = 6;
        // Replan once the order's target has drifted more than one grid cell since
        // the path was planned (covers WRG's self-reassigned wander targets).
        private const int ReplanTargetDriftDistance = NavGrid.CellSize;

        /// <summary>
        /// Resolves the immediate sub-goal (in pixels) A_RUCH should step toward this
        /// tick, using a cached A* path that's replanned only when the order's target
        /// moves, the path runs out, or the scene's static obstacles changed. Falls
        /// back to the raw target (X2,Y2) if no path can be found, so a fully-enclosed
        /// unit still behaves like the original rather than freezing.
        /// </summary>
        void GetNextWaypoint(int A, int I, int X1, int Y1, int X2, int Y2, out int WPX, out int WPY)
        {
            var state = unitPaths[A, I];
            if (state == null)
            {
                state = new UnitPathState();
                unitPaths[A, I] = state;
            }

            var targetMoved = Math.Abs(state.PlannedForX - X2) > ReplanTargetDriftDistance || Math.Abs(state.PlannedForY - Y2) > ReplanTargetDriftDistance;
            var stale = state.PlannedAtEpoch != screens.ZoneEpoch;
            var exhausted = state.Index >= state.Waypoints.Count;

            if (targetMoved || stale || exhausted)
            {
                navGrid.RebuildIfNeeded(screens);
                var path = pathfinder.FindPath(navGrid, A == ARM, X1, Y1, X2, Y2);

                state.PlannedForX = X2;
                state.PlannedForY = Y2;
                state.PlannedAtEpoch = screens.ZoneEpoch;
                state.Index = 0;
                state.Waypoints = path ?? new List<(int X, int Y)>();
            }

            if (state.Waypoints.Count == 0)
            {
                WPX = X2;
                WPY = Y2;
                return;
            }

            while (state.Index < state.Waypoints.Count - 1
                && Math.Abs(state.Waypoints[state.Index].X - X1) <= WaypointArriveDistance
                && Math.Abs(state.Waypoints[state.Index].Y - Y1) <= WaypointArriveDistance)
            {
                state.Index++;
            }

            (WPX, WPY) = state.Waypoints[state.Index];
        }

        void A_RUCH(int A, int I)
        {
            screens.Screen(0);
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            var X1 = ARMIA[A, I, TX];
            var Y1 = ARMIA[A, I, TY];
            var X2 = ARMIA[A, I, TCELX];
            var Y2 = ARMIA[A, I, TCELY];
            GetNextWaypoint(A, I, X1, Y1, X2, Y2, out var WPX, out var WPY);
            var BAZA = ARMIA[A, I, TBOB];
            var KLATKA = ARMIA[A, I, TKLAT];
            var SPEED = ARMIA[A, I, TSZ] / 10;
            var SPEED2 = 3 - SPEED;
            if (SPEED2 <= 0) SPEED2 = 1;
            if (SPEED <= 0) SPEED = 1;
            if (SPEED > 7) SPEED = 7;
            amos.Add(ref KLATKA, 1, 0, (SPEED2 * 4) - 1);
            ARMIA[A, I, TKLAT] = KLATKA;
            KLATKA = KLATKA / SPEED2;
            KLATKA = AN[KLATKA];
            var BNR = BAZA + 7;
            // ROZX/ROZY (relative to the real order target) drive arrival detection below;
            // WROZX/WROZY (relative to the next A* waypoint) drive the per-tick step direction.
            var ROZX = X2 - X1;
            var ROZY = Y2 - Y1;
            var WROZX = WPX - X1;
            var WROZY = WPY - Y1;
            var STREFA = screens.Zone(X1, Y1 + 1);
            var STREFA2 = screens.Zone(X1, Y1 + 30);
            var RASA = ARMIA[A, I, TRASA];
            var KLIN = 2;
            if (RASA > 9 && amos.Rnd(40) == 1)
            {
                banks.SamBank(5);
                FX(1);
                banks.SamBank(4);
            }
            if (STREFA2 > 20 && STREFA2 < 31)
            {
                //screensManager.LimitBob(I2, 0, 0, 640, 114);
            }
            else
            {
                //screensManager.LimitBob(I2, 0, 0, 640, 512);
            }
            if (STREFA > 100 && STREFA < 120 && A == ARM)
            {
                var MIASTO = ARMIA[A, 0, TNOGI] - 70;
                SKLEP_(MIASTO, STREFA - 100, A, I);
                Y1 += 8;
                goto SKIP;
            }
            if (STREFA > 30 && STREFA < 41)
            {
                var res = PLAPKA(STREFA - 30, A, I, X1, Y1);
                if (res == 1)
                {
                    return;
                }
            }
            if (Math.Abs(WROZX) > 4)
            {
                var ZNX = amos.Sgn(WROZX);
                var T = 0;
                if (ZNX == -1)
                {
                    BNR = BAZA + 4 + KLATKA;
                    T = -17;
                }
                if (ZNX == 1)
                {
                    BNR = BAZA + 10 + KLATKA;
                    T = 17;
                }
                var ST = screens.Zone(X1 + T, Y1);
                if (ST == 0 || (ST > 100 && ST < 120 || ST > 30 && ST < 41) && A == ARM)
                {
                    X1 += ZNX * SPEED;
                    KLIN--;
                }
            }
            if (Math.Abs(WROZY) > 4)
            {
                var ZNY = amos.Sgn(WROZY);
                var T = 0;
                var B2 = 0;
                if (ZNY == -1)
                {
                    B2 = BAZA + 1 + KLATKA;
                    T = -21;
                }
                if (ZNY == 1)
                {
                    B2 = BAZA + 7 + KLATKA;
                    T = 2;
                }
                var ST = screens.Zone(X1, Y1 + T);
                if (ST == 0 || (ST > 100 && ST < 120 || ST > 30 && ST < 41) && A == ARM)
                {
                    Y1 += ZNY * SPEED;
                    BNR = B2;
                    KLIN--;
                }
            }

            // Local steering: if both axes were blocked (KLIN still 2) and the unit
            // still needs to move, try a perpendicular sidestep to slide around the
            // obstacle (typically another unit whose zone 1-20 blocks the probe).
            // A committed direction is kept for several ticks to avoid oscillation.
            if (KLIN == 2 && (Math.Abs(WROZX) > 4 || Math.Abs(WROZY) > 4)
                && (Math.Abs(ROZX) > 4 || Math.Abs(ROZY) > 4))
            {
                var state = unitPaths[A, I];
                if (state == null)
                {
                    state = new UnitPathState();
                    unitPaths[A, I] = state;
                }

                // Try the committed direction first if still active.
                if (state.SteerTicks > 0 && TrySteerStep(state.SteerAxis, state.SteerDir))
                {
                    state.SteerTicks--;
                }
                else
                {
                    // Pick a new direction: prefer perpendicular to primary movement axis,
                    // biased toward the waypoint. Commit for 10 ticks.
                    state.SteerTicks = 0;
                    state.SteerAxis = 0;

                    if (Math.Abs(WROZX) >= Math.Abs(WROZY))
                    {
                        // Primary is horizontal - try vertical sidesteps first.
                        var dir = WPY < Y1 ? -1 : 1;
                        if (TrySteerStep(1, dir))
                        {
                            state.SteerAxis = 1; state.SteerDir = dir; state.SteerTicks = 10;
                        }
                        else if (TrySteerStep(1, -dir))
                        {
                            state.SteerAxis = 1; state.SteerDir = -dir; state.SteerTicks = 10;
                        }
                        else
                        {
                            // Fallback: horizontal sidestep.
                            var dir2 = WPX < X1 ? -1 : 1;
                            if (TrySteerStep(2, dir2))
                            {
                                state.SteerAxis = 2; state.SteerDir = dir2; state.SteerTicks = 10;
                            }
                            else if (TrySteerStep(2, -dir2))
                            {
                                state.SteerAxis = 2; state.SteerDir = -dir2; state.SteerTicks = 10;
                            }
                        }
                    }
                    else
                    {
                        // Primary is vertical - try horizontal sidesteps first.
                        var dir = WPX < X1 ? -1 : 1;
                        if (TrySteerStep(2, dir))
                        {
                            state.SteerAxis = 2; state.SteerDir = dir; state.SteerTicks = 10;
                        }
                        else if (TrySteerStep(2, -dir))
                        {
                            state.SteerAxis = 2; state.SteerDir = -dir; state.SteerTicks = 10;
                        }
                        else
                        {
                            // Fallback: vertical sidestep.
                            var dir2 = WPY < Y1 ? -1 : 1;
                            if (TrySteerStep(1, dir2))
                            {
                                state.SteerAxis = 1; state.SteerDir = dir2; state.SteerTicks = 10;
                            }
                            else if (TrySteerStep(1, -dir2))
                            {
                                state.SteerAxis = 1; state.SteerDir = -dir2; state.SteerTicks = 10;
                            }
                        }
                    }
                }

                bool TrySteerStep(int axis, int dir)
                {
                    int ST;
                    if (axis == 1) // vertical sidestep
                    {
                        var probeOff = dir == -1 ? -21 : 2;
                        ST = screens.Zone(X1, Y1 + probeOff);
                        if (ST == 0 || (ST > 100 && ST < 120 || ST > 30 && ST < 41) && A == ARM)
                        {
                            Y1 += dir * SPEED;
                            BNR = dir == -1 ? BAZA + 1 + KLATKA : BAZA + 7 + KLATKA;
                            return true;
                        }
                    }
                    else // horizontal sidestep
                    {
                        var probeOff = dir == -1 ? -17 : 17;
                        ST = screens.Zone(X1 + probeOff, Y1);
                        if (ST == 0 || (ST > 100 && ST < 120 || ST > 30 && ST < 41) && A == ARM)
                        {
                            X1 += dir * SPEED;
                            BNR = dir == -1 ? BAZA + 4 + KLATKA : BAZA + 10 + KLATKA;
                            return true;
                        }
                    }
                    return false;
                }
            }
            else
            {
                // Unit moved normally or reached target - reset steering commitment.
                var state = unitPaths[A, I];
                if (state != null)
                {
                    state.SteerAxis = 0;
                    state.SteerTicks = 0;
                }
            }

            if (Math.Abs(ROZX) <= 4 && Math.Abs(ROZY) <= 4)
            {
                ARMIA[A, I, TTRYB] = 0;
                KLIN = 0;
            }

        SKIP:
            ARMIA[A, I, TX] = X1;
            ARMIA[A, I, TY] = Y1;

            screens.SetZone(I2, X1 - 15, Y1 - 15, X1 + 15, Y1);
            screens.Bob(I2, X1, Y1, BNR);
            if (KLIN == 2 && A == WRG)
            {
                X2 = X1 + amos.Rnd(120) - 60;
                Y2 = Y1 + amos.Rnd(100) - 50;
                if (X2 < 20)
                {
                    X2 = 20;
                }
                if (X2 > 620)
                {
                    X2 = 620;
                }
                if (Y2 < 20)
                {
                    Y2 = 20;
                }
                if (Y2 > 510)
                {
                    Y2 = 510;
                }
                ARMIA[A, I, TCELX] = X2;
                ARMIA[A, I, TCELY] = Y2;
                ARMIA[A, I, TTRYB] = 1;
            }
        }

        void RUCH()
        {
            _GET_XY(0, 0, 0);
            screens.Screen(0);
            screens.BobOff(30 + NUMER);
            OY += 8;
            if (OX > 623) OX = 623;
            if (OX < 17) OX = 17;
            if (OY > 508) OY = 508;
            if (OY < 22) OY = 22;

            var STREFA = screens.Zone(OX, OY);
            if (STREFA < 21 || STREFA > 100 && STREFA < 120 || STREFA > 30 && STREFA < 41)
            {
                ARMIA[ARM, NUMER, TCELX] = OX;
                ARMIA[ARM, NUMER, TCELY] = OY;
                ARMIA[ARM, NUMER, TTRYB] = 1;
                screens.Bob(51, OX, OY, 2 + BUBY);
                screens.BobUpdate();
                screens.WaitVbl();
            }
            while (screens.MouseKey() == LEWY) { }
        }

        void _GET_XY(int TRYB, int X1, int Y1)
        {
            if (TRYB == 1)
            {
                screens.GrWriting(2);
            }
            if (TRYB == 2)
            {
                screens.Sprite(0, screens.XMouse(), screens.YMouse(), 49);
                screens.HideOn();
                //A_S = "S: Move XM-X,YM-Y,1 ; Jump S"
                //B_S = "Anim 0,(49,4)(46,4)(41,4)(46,4)"
                //Amal 0,B_S
            }
            else
            {
                screens.ChangeMouse(CELOWNIK);
            }
            screens.Screen(0);
            do
            {
                var HY = screens.YMouse();
                OX = screens.XScreen(screens.XMouse());
                OY = screens.YScreen(screens.YMouse());
                if (TRYB == 1)
                {
                    screens.Draw(X1, Y1, OX, OY);
                    screens.WaitVbl();
                    screens.Draw(X1, Y1, OX, OY);
                }
                if (TRYB == 2)
                {
                    if (screens.MouseZone() > 0 && screens.MouseZone() < 21)
                    {
                        //Amal On 0;
                        screens.Sprite(0, screens.XMouse(), screens.YMouse(), -1);
                        screens.WaitVbl();
                    }
                    else
                    {
                        //Amal Freeze
                        screens.Sprite(0, screens.XMouse(), screens.YMouse(), 49);
                        screens.WaitVbl();
                    }
                }
                if (screens.MouseKey() == PRAWY)
                {
                    if (TRYB == 2)
                    {
                        screens.SpriteOff(0);
                        //Amal Freeze;
                    }
                    
                    SKROL(1);
                    
                    if (TRYB == 2)
                    {
                        //Amal 0,B_S;
                        screens.HideOn();
                        screens.Sprite(0, screens.XMouse(), screens.YMouse(), 49);
                    }
                }
                var A_S = screens.Inkey_S();
                var KLAW = screens.Scancode();
                if (KLAW >= KeyLeft && KLAW <= KeyDown)
                {
                    KLAWSKROL(KLAW);
                }
            }
            while (!(screens.MouseClick() == 1));

            if (TRYB == 2)
            {
                screens.AmalOff(0);
                screens.ShowOn();
            }
            if (TRYB == 1)
            {
                screens.GrWriting(0);
            }
            
            screens.SpriteOff(0);
            screens.ChangeMouse(BUBY + 6);
        }

        void WYKRESY(int A, int NR)
        {
            var SILA = ARMIA[A, NR, TSI];
            if (SILA > 80)
            {
                SILA = 80;
            }
            var SPEED = ARMIA[A, NR, TSZ];
            var SPEEDM = ARMIA[A, NR, TAMO];
            var ENERGIA = ARMIA[A, NR, TE];
            if (ENERGIA < 0)
            {
                ENERGIA = 0;
            }
            var ENERGIAM = ARMIA[A, NR, TEM];
            var MAGIA = ARMIA[A, NR, TMAG];
            var MAGIAM = ARMIA[A, NR, TMAGMA];
            screens.Ink(19, 19);
            screens.Bar(3, 3, 3 + 138, 3 + 18);

            if (A == WRG && ARMIA[WRG, NR, TGLOWA] == 0)
            {
                screens.Ink(5, 19);
                screens.Text(12, 14, "brak danych");
            }
            else
            {
                //Clip 4,4 To 4 + 73,20
                screens.Ink(2);
                screens.Box(4, 4, 4 + (ENERGIAM / 3), 6);
                screens.Ink(20);
                screens.Draw(4, 5, 4 + (ENERGIA / 3), 5);

                screens.Ink(3);
                screens.Bar(4, 8, 5 + SILA, 10);
                screens.Ink(15);
                screens.Draw(4, 9, 5 + SILA, 9);
                screens.Ink(18);
                screens.Box(4, 16, 4 + MAGIAM, 18);
                screens.Ink(13);
                screens.Draw(4, 17, 4 + MAGIA, 17);
                if (A == WRG)
                {
                    screens.Ink(4);
                    screens.Bar(4, 12, 4 + SPEED, 14);
                    screens.Ink(16);
                    screens.Draw(4, 13, 4 + SPEED, 13);
                    //Clip
                    var RA_S = "";
                    if (ARMIA[A, NR, TRASA] > 9)
                    {
                        RA_S = RASY_S[ARMIA[A, NR, TRASA]];
                    }
                    else
                    {
                        RA_S = ARMIA_S[A, NR];
                    }
                    screens.Ink(5);
                    screens.Text(90, 10, RA_S);
                    var MORALE = ARMIA[WRG, NR, TKORP] / 50;
                    if (MORALE < 0)
                    {
                        MORALE = 0;
                    }
                    if (MORALE > 4)
                    {
                        MORALE = 4;
                    }
                    screens.Text(90, 20, GUL_S[MORALE + 5]);
                }
                else
                {
                    //Clip 4,4 To 4 + 73,20
                    screens.Ink(4);
                    screens.Box(4, 12, 4 + SPEEDM, 14);
                    screens.Ink(16);
                    screens.Draw(4, 13, 4 + SPEED, 13);
                }
                //Clip
            }
            if (A == ARM)
            {
                var LB = BRON[ARMIA[ARM, NR, TLEWA], B_BOB];
                var RB = BRON[ARMIA[ARM, NR, TPRAWA], B_BOB];
                if (LB > 0)
                {
                    screens.PasteBob(108, 3, LB + BROBY);
                    screens.SetZone(20, 108, 3, 108 + 16, 3 + 16);
                }
                if (RB > 0)
                {
                    screens.PasteBob(124, 3, RB + BROBY);
                    screens.SetZone(21, 124, 3, 124 + 16, 3 + 16);
                }
                screens.GrWriting(0);
                screens.Ink(13, 20);
                screens.Text(80, 15, ARMIA_S[A, NR]);
                screens.GrWriting(1);
            }
            while (screens.MouseKey() == LEWY) { }
        }

        void _ATAK(int TYP)
        {
            _GET_XY(2, 0, 0);
            var WROG = false;
            if (TYP == 6)
            {
                WROG = false;
            }
            else
            {
                WROG = true;
            }
            screens.Screen(0);
            screens.BobOff(30 + NUMER);
            var STREFA = screens.Zone(OX, OY);
            var A = 0;
            if (STREFA > 10 && STREFA < 21)
            {
                A = WRG;
                STREFA += -10;
                //'�eby nie gadali ze zwierzakami
                if (ARMIA[A, STREFA, TRASA] < 10)
                {
                    WROG = true;
                }
            }
            else
            {
                A = ARM;
            }
            if (STREFA > 0 && STREFA < 11 && WROG)
            {
                ARMIA[ARM, NUMER, TCELX] = STREFA;
                ARMIA[ARM, NUMER, TCELY] = A;
                ARMIA[ARM, NUMER, TTRYB] = TYP;
                var X = ARMIA[A, STREFA, TX];
                var Y = ARMIA[A, STREFA, TY];
                screens.Bob(51, X, Y, 2 + BUBY);
                screens.BobUpdate();
                screens.WaitVbl();
            }
            while (screens.MouseKey() == LEWY) { }
        }

        void STRZAL()
        {
            var B1 = ARMIA[ARM, NUMER, TLEWA];
            var B2 = ARMIA[ARM, NUMER, TPRAWA];
            var BT1 = BRON[B1, B_TYP];
            var BT2 = BRON[B2, B_TYP];
            //'szybko�� lotu pocisku 
            if ((BT1 == 4 && BT2 == 5 && STRZALY[NUMER] > 0) ||
                (BT1 == 5 && BT2 == 4 && STRZALY[NUMER] > 0) ||
                (BT1 == 15 && BT2 == 16) ||
                (BT1 == 16 && BT2 == 15) ||
                (BT1 == 9 && BT2 != 12) ||
                (BT2 == 9 && BT1 != 12))
            {
                // Select target position
                _GET_XY(0, 0, 0);
                ARMIA[ARM, NUMER, TCELX] = OX;
                ARMIA[ARM, NUMER, TCELY] = OY;
                // Detect target unit for auto-aim tracking
                var zone = screens.Zone(OX, OY);
                if (zone > 10 && zone < 21)
                {
                    AutoFireTargetIdx[NUMER] = zone - 10;
                    AutoFirePrevTX[NUMER] = ARMIA[WRG, zone - 10, TX];
                    AutoFirePrevTY[NUMER] = ARMIA[WRG, zone - 10, TY];
                }
                else
                {
                    AutoFireTargetIdx[NUMER] = 0;
                }
                // Enter auto-fire mode
                ARMIA[ARM, NUMER, TTRYB] = 9;
                screens.Screen(0);
                screens.Bob(51, OX, OY + 12, 2 + BUBY);
                screens.BobUpdate();
                screens.WaitVbl();
                // Fire first projectile immediately
                FireProjectileFromUnit(ARM, NUMER, OX, OY);
            }
            else
            {
                if (BT1 != 12 && BT2 != 12)
                {
                    GADUP(3);
                }
            }
            if (BT1 == 12 || BT2 == 12)
            {
                CZARY(BT1, BT2);
            }
        }

        void CZARY(int BL, int BP)
        {
            var B1 = ARMIA[ARM, NUMER, TLEWA];
            var B2 = ARMIA[ARM, NUMER, TPRAWA];
            var CZAR_TYP = 0;
            var BR = 0;
            if (BP == 12)
            {
                CZAR_TYP = BRON[B2, B_DOSW];
                BR = B2;
            }
            if (BL == 12)
            {
                CZAR_TYP = BRON[B1, B_DOSW];
                BR = B1;
            }
            var MAG = BRON[BR, B_MAG];
            var MAG2 = ARMIA[ARM, NUMER, TMAG];
            if (MAG > MAG2)
            {
                screens.Screen(1);
                GADUP(3);
                return;
            }

            if (CZAR_TYP == 1 || CZAR_TYP == 9)
            {
                _GET_XY(0, 0, 0);
                MAG2 += -MAG;
                ARMIA[ARM, NUMER, TMAG] = MAG2;
                ARMIA[ARM, NUMER, TCELX] = OX;
                ARMIA[ARM, NUMER, TCELY] = OY;
                ARMIA[ARM, NUMER, TTRYB] = 4;
                screens.Screen(0);
                screens.Bob(51, OX, OY + 12, 2 + BUBY);
                screens.BobUpdate();
                screens.WaitVbl();
                var X1 = ARMIA[ARM, NUMER, TX];
                var Y1 = ARMIA[ARM, NUMER, TY] - 20;
                var DX_R = OX - X1;
                var DY_R = OY - Y1;
                var L_R = Math.Sqrt(DX_R * DX_R + DY_R * DY_R) + 1;
                var VX_R = DX_R / L_R;
                var VY_R = DY_R / L_R;
                VEKTOR_R[NUMER, 1] = VX_R * 6;
                VEKTOR_R[NUMER, 2] = VY_R * 6;
                if (CZAR_TYP == 1)
                {
                    VEKTOR_R[NUMER, 3] = X1;
                }
                else
                {
                    VEKTOR_R[NUMER, 3] = 0;
                }
                VEKTOR_R[NUMER, 4] = Y1;
                VEKTOR_R[NUMER, 0] = BR;
            }
            else
            {
                _GET_XY(2, 0, 0);
                var STREFA = screens.Zone(OX, OY);
                var A = 0;
                if (STREFA > 10)
                {
                    A = WRG;
                    STREFA += -10;
                }
                else
                {
                    A = ARM;
                }
                if (STREFA > 0 && STREFA < 11)
                {
                    VEKTOR_R[NUMER, 0] = BR;
                    ARMIA[ARM, NUMER, TCELX] = STREFA;
                    ARMIA[ARM, NUMER, TCELY] = A;
                    ARMIA[ARM, NUMER, TTRYB] = 5;
                    var X = ARMIA[A, STREFA, TX];
                    var Y = ARMIA[A, STREFA, TY];
                    screens.Bob(51, X, Y, 2 + BUBY);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
                else
                {
                    GADUP(3);
                }
                while (screens.MouseKey() == LEWY) { }
            }
        }

        void A_ATAK(int A, int I)
        {
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            var X1 = ARMIA[A, I, TX];
            var Y1 = ARMIA[A, I, TY];
            var BAZA = ARMIA[A, I, TBOB];
            var BNR = BAZA + 7;
            var TARGET = ARMIA[A, I, TCELX];
            var B = ARMIA[A, I, TCELY];
            var ENP = ARMIA[B, TARGET, TE];
            if (ENP <= 0)
            {
                ARMIA[A, I, TTRYB] = 0;
                return;
            }
            var X2 = ARMIA[B, TARGET, TX];
            var Y2 = ARMIA[B, TARGET, TY];
            var SPEED = ARMIA[A, I, TSZ] / 10;
            var KLATKA = ARMIA[A, I, TKLAT];
            var SPEED2 = 3 - SPEED;
            if (SPEED2 <= 0) SPEED2 = 1;
            if (SPEED <= 0) SPEED = 1;
            if (SPEED > 7) SPEED = 7;
            amos.Add(ref KLATKA, 1, 0, (SPEED2 * 4) - 1);
            ARMIA[A, I, TKLAT] = KLATKA;
            KLATKA = KLATKA / SPEED2;
            KLATKA = AN[KLATKA];
            var STREFA = screens.Zone(X1, Y1 + 1);
            var STREFA2 = screens.Zone(X1, Y1 + 30);
            var RASA = ARMIA[A, I, TRASA];
            var KLIN = 2;
            if (RASA > 9 && amos.Rnd(40) == 1)
            {
                banks.SamBank(5);
                FX(1);
                banks.SamBank(4);
            }
            if (STREFA > 30 && STREFA < 41)
            {
                var res = PLAPKA(STREFA - 30, A, I, X1, Y1);
                if (res == 1)
                {
                    return;
                }
            }
            if (STREFA2 > 20 && STREFA2 < 31)
            {
                //screens.LimitBob(I2, 0, 0, 640, 114);
            }
            else
            {
                //screens.LimitBob(I2, 0, 0, 640, 512);
            }

            var ROZX = X2 - X1;
            var ROZY = Y2 - Y1;

            if (Math.Abs(ROZX) > 33)
            {
                var ZNX = amos.Sgn(ROZX);
                var T = 0;
                if (ZNX == -1)
                {
                    BNR = BAZA + 4 + KLATKA;
                    T = -17;
                }
                if (ZNX == 1)
                {
                    BNR = BAZA + 10 + KLATKA;
                    T = 17;
                }
                var ST = screens.Zone(X1 + T, Y1);
                if (ST == 0 || ST > 30 && ST < 41 && A == ARM)
                {
                    X1 += ZNX * SPEED;
                    KLIN--;
                }
            }
            if (Math.Abs(ROZY) > 21)
            {
                var ZNY = amos.Sgn(ROZY);
                var T = 0;
                if (ZNY == -1)
                {
                    BNR = BAZA + 1 + KLATKA;
                    T = -21;
                }
                if (ZNY == 1)
                {
                    BNR = BAZA + 7 + KLATKA;
                    T = 2;
                }
                var ST = screens.Zone(X1, Y1 + T);
                if (ST == 0 || ST > 30 && ST < 41 && A == ARM)
                {
                    Y1 += ZNY * SPEED;
                    KLIN--;
                }
            }

            if (Math.Abs(ROZX) <= 33 && Math.Abs(ROZY) <= 21)
            {
                var ZNX = amos.Sgn(ROZX);
                var B2 = BAZA + 13 + amos.Rnd(2);
                if (ZNX == -1)
                {
                    BNR = BAZA + 5 + amos.Rnd(1);
                }
                else
                {
                    BNR = BAZA + 11 + amos.Rnd(1);
                    //B2 = Hrev(B2);
                }

                if (B == WRG)
                {
                    ARMIA[WRG, TARGET, TGLOWA] = 1;
                }
                if (A == WRG)
                {
                    ARMIA[WRG, I, TGLOWA] = 1;
                }
                //'auto-defence system 
                if (ARMIA[B, TARGET, TTRYB] == 0 || (B == WRG && ARMIA[B, TARGET, TTRYB] == 1))
                {
                    ARMIA[B, TARGET, TCELX] = I;
                    ARMIA[B, TARGET, TCELY] = A;
                    ARMIA[B, TARGET, TTRYB] = 2;
                }
                //'------------------- 
                var ODP = ARMIA[B, TARGET, TP];
                var SILA = ARMIA[A, I, TSI];
                var MOC = 100 - ARMIA[A, I, TDOSW];
                var MOC2 = 100 - ARMIA[B, TARGET, TDOSW];
                SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (SPEED < 1)
                {
                    SPEED = 1;
                }
                var RASA2 = ARMIA[B, TARGET, TRASA];
                if (amos.Rnd(SPEED) == 0)
                {
                    screens.Ink(10);
                    screens.Plot(X2 + amos.Rnd(20) - 10, Y2 + amos.Rnd(6) - 2);
                    BNR = B2;
                    var OPOR = ODP - amos.Rnd(((ODP * MOC2) / 100) + 1);
                    var CIOS = (SILA - amos.Rnd((SILA * MOC) / 100)) - OPOR;
                    CIOS = CIOS / 2;
                    if (CIOS <= 0)
                    {
                        CIOS = 1;
                    }
                    ENP += -CIOS;
                    var PRZELOT = 0;
                    if (RASA2 < 10)
                    {
                        PRZELOT = amos.Rnd(1);
                    }
                    if (CIOS > 13 && PRZELOT == 1)
                    {
                        ARMIA[B, TARGET, TTRYB] = 7;
                        var CELX = X2 + ROZX;
                        if (CELX < 20)
                        {
                            CELX = 20;
                        };
                        if (CELX > 620)
                        {
                            CELX = 620;
                        }
                        var CELY = Y2 + ROZY;
                        if (CELY < 20)
                        {
                            CELY = 20;
                        };
                        if (CELY > 500)
                        {
                            CELY = 500;
                        }
                        ARMIA[B, TARGET, TCELX] = CELX;
                        ARMIA[B, TARGET, TCELY] = CELY;
                    }

                    if (ENP <= 0)
                    {
                        if (CIOS > 20 && PRZELOT == 1)
                        {
                            ARMIA[B, TARGET, TTRYB] = 8;
                            ENP = 5;
                        }
                        else
                        {
                            ZABIJ(B, TARGET, 0);
                        }
                        var MUNDRY = RASY[RASA, 6];
                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                        ARMIA[A, I, TTRYB] = 0;
                        if (A == WRG)
                        {
                            ARMIA[A, I, TKORP] += amos.Rnd(20);
                        }
                    }
                    ARMIA[B, TARGET, TE] = ENP;
                    KANAL = amos.Rnd(3);
                    var SAM = 0;
                    if (RASA < 10)
                    {
                        SAM = amos.Rnd(4) + 1;
                    }
                    else
                    {
                        banks.SamBank(5);
                        SAM = 2;
                    }
                    FX(SAM);
                    banks.SamBank(4);
                }
            }
            ARMIA[A, I, TX] = X1;
            ARMIA[A, I, TY] = Y1;
            if (ARMIA[A, I, TE] > 0)
            {
                screens.Bob(I2, X1, Y1, BNR);
                screens.SetZone(I2, X1 - 15, Y1 - 15, X1 + 15, Y1);
            }
            if (KLIN > 1 && A == WRG)
            {
                X2 = X1 + amos.Rnd(120) - 60;
                Y2 = Y1 + amos.Rnd(100) - 50;
                if (X2 < 20)
                {
                    X2 = 20;
                }
                if (X2 > 620)
                {
                    X2 = 620;
                }
                if (Y2 < 20)
                {
                    Y2 = 20;
                }
                if (Y2 > 510)
                {
                    Y2 = 510;
                }
                ARMIA[A, I, TCELX] = X2;
                ARMIA[A, I, TCELY] = Y2;
                ARMIA[A, I, TTRYB] = 1;
            }
        }

        void A_STRZAL(int A, int I)
        {
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            var SILA = VEKTOR_R[I2, 0];
            var XP_R = VEKTOR_R[I2, 3];
            var YP_R = VEKTOR_R[I2, 4];
            var VX_R = VEKTOR_R[I2, 1];
            var VY_R = VEKTOR_R[I2, 2];
            XP_R = XP_R + (VX_R * 2);
            YP_R = YP_R + (VY_R * 2);
            VEKTOR_R[I2, 3] = XP_R;
            VEKTOR_R[I2, 4] = YP_R;
            var SAM = 0;
            var BB = 0;
            if (SILA < 0)
            {
                BB = PIKIETY + 21 + amos.Rnd(3);
                SILA = -SILA;
                var ODLOT = amos.Rnd(1);
                SAM = 14;
            }
            else
            {
                SAM = 11;
                var ODLOT = 0;
                BB = BSIBY + I2;
            }
            screens.Screen(0);
            screens.Bob(I2 + 30, (int)XP_R, (int)YP_R, BB);
            var STREFA = screens.Zone((int)XP_R, (int)YP_R);
            //'trafienie w go�cia
            var B = 0;
            if (STREFA > 0 && STREFA != I2 && STREFA < 21)
            {
                if (STREFA > 10)
                {
                    STREFA += -10;
                    B = WRG;
                }
                else
                {
                    B = ARM;
                }
                screens.BobOff(I2 + 30);
                ARMIA[A, I, TTRYB] = 0;
                var X1 = ARMIA[B, STREFA, TX];
                var Y1 = ARMIA[B, STREFA, TY];
                var ENP = ARMIA[B, STREFA, TE];
                var ODP = ARMIA[B, STREFA, TP];
                var RASA2 = ARMIA[B, STREFA, TRASA];
                var ODLOT = 0;
                if (RASA2 > 9)
                {
                    ODLOT = 0;
                }
                //'oszczepy l�duj� na glebie 
                var OSZ = VEKTOR_R[I2, 5];
                if (BRON[(int)OSZ, B_TYP] == 9)
                {
                    var SEK = SEKTOR(X1, Y1);
                    for (var II = 0; II <= 3; II++)
                    {
                        if (GLEBA[SEK, II] == 0)
                        {
                            GLEBA[SEK, II] = (int)OSZ;
                            II = 4;
                        }
                    }
                }
                //'----------- 
                var MOC = 100 - ARMIA[A, I, TDOSW];
                var CIOS = SILA - amos.Rnd((int)(SILA * MOC) / 100) - amos.Rnd(ODP + 2);
                if (CIOS <= 0)
                {
                    CIOS = 1;
                }

                ENP += (int)-CIOS;
                if (ODLOT == 1)
                {
                    ARMIA[B, STREFA, TTRYB] = 7;
                    var CELX = X1 + VX_R * 8;
                    if (CELX < 20)
                    {
                        CELX = 20;
                    };
                    if (CELX > 620)
                    {
                        CELX = 620;
                    }
                    var CELY = Y1 + VY_R * 8;
                    if (CELY < 20)
                    {
                        CELY = 20;
                    };
                    if (CELY > 500)
                    {
                        CELY = 500;
                    }
                    ARMIA[B, STREFA, TCELX] = (int)CELX;
                    ARMIA[B, STREFA, TCELY] = (int)CELY;
                }
                if (ENP <= 0)
                {
                    var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                    amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                    if (A == WRG)
                    {
                        ARMIA[A, I, TKORP] += amos.Rnd(20);
                    }
                    if (ODLOT == 1)
                    {
                        ARMIA[B, STREFA, TTRYB] = 8;
                        ENP = 2;
                    }
                    else
                    {
                        ZABIJ(B, STREFA, 0);
                    }
                }
                ARMIA[B, STREFA, TE] = ENP;
                FX(SAM);
            }
            //'trafienie w mur 
            if (STREFA > 20 && STREFA < 31 && KTO_ATAKUJE == A)
            {
                screens.BobOff(I2 + 30);
                FX(14);
                ARMIA[A, I, TTRYB] = 0;
                MUR[STREFA - 21] += (int)-SILA;
                if (MUR[STREFA - 21] <= 0)
                {
                    var X = (STREFA - 21) * 64;
                    screens.ResetZone(STREFA);
                    //Autoback 2;
                    screens.PasteBob(X, 111, BIBY + 12 + 2);
                    screens.WaitVbl();
                    //Autoback 1
                }
            }
            if (XP_R > 640 || XP_R < 0 || YP_R > 512 || YP_R < 0 || STREFA > 40)
            {
                screens.BobOff(I2 + 30);
                ARMIA[A, I, TTRYB] = 0;
            }
        }

        // =====================================================================
        // PROJECTILE POOL SYSTEM - multi-arrow auto-fire
        // =====================================================================

        int AllocProjectileBob()
        {
            if (FreeProjectileBobs.Count > 0)
                return FreeProjectileBobs.Dequeue();
            return NextProjectileBob++;
        }

        void FreeProjectileBob(int bobNumber)
        {
            screens.BobOff(bobNumber);
            FreeProjectileBobs.Enqueue(bobNumber);
        }

        void SpawnProjectile(int ownerArmy, int ownerIndex, double x, double y,
            double vx, double vy, double damage, int spriteRef, int weaponRef, bool isNegative)
        {
            // Enforce max projectile cap - remove oldest if full
            if (Projectiles.Count >= MAX_PROJECTILES)
            {
                var oldest = Projectiles[0];
                FreeProjectileBob(oldest.BobNumber);
                Projectiles.RemoveAt(0);
            }

            var bob = AllocProjectileBob();
            Projectiles.Add(new Projectile
            {
                X = x,
                Y = y,
                VX = vx,
                VY = vy,
                Damage = damage,
                OwnerArmy = ownerArmy,
                OwnerIndex = ownerIndex,
                BobNumber = bob,
                SpriteRef = spriteRef,
                WeaponRef = weaponRef,
                IsNegative = isNegative
            });
        }

        void UPDATE_PROJECTILES()
        {
            screens.Screen(0);
            for (var idx = Projectiles.Count - 1; idx >= 0; idx--)
            {
                var proj = Projectiles[idx];

                // Move projectile
                proj.X += proj.VX * PROJECTILE_SPEED;
                proj.Y += proj.VY * PROJECTILE_SPEED;

                // Determine sprite to display
                var BB = proj.SpriteRef;
                var SAM = 11;
                var SILA = proj.Damage;
                if (proj.IsNegative)
                {
                    BB = PIKIETY + 21 + amos.Rnd(3);
                    SILA = -SILA; // store as positive for damage calc
                    SAM = 14;
                }

                // Draw projectile
                screens.Bob(proj.BobNumber, (int)proj.X, (int)proj.Y, BB);

                // Check zone collision
                var STREFA = screens.Zone((int)proj.X, (int)proj.Y);
                var hit = false;

                // Compute the owner's zone ID to prevent self-hit
                var ownerZone = proj.OwnerArmy == WRG ? proj.OwnerIndex + 10 : proj.OwnerIndex;

                // Hit a unit (zone 1-20, not the owner)
                if (STREFA > 0 && STREFA != ownerZone && STREFA < 21)
                {
                    int B;
                    int targetIdx;
                    if (STREFA > 10)
                    {
                        targetIdx = STREFA - 10;
                        B = WRG;
                    }
                    else
                    {
                        targetIdx = STREFA;
                        B = ARM;
                    }

                    var A = proj.OwnerArmy;
                    var I = proj.OwnerIndex;

                    var X1 = ARMIA[B, targetIdx, TX];
                    var Y1 = ARMIA[B, targetIdx, TY];
                    var ENP = ARMIA[B, targetIdx, TE];
                    var ODP = ARMIA[B, targetIdx, TP];
                    var RASA2 = ARMIA[B, targetIdx, TRASA];
                    var ODLOT = 0;
                    if (RASA2 > 9)
                    {
                        ODLOT = 0;
                    }

                    // Javelins land on the ground
                    if (proj.WeaponRef > 0 && BRON[proj.WeaponRef, B_TYP] == 9)
                    {
                        var SEK = SEKTOR(X1, Y1);
                        for (var II = 0; II <= 3; II++)
                        {
                            if (GLEBA[SEK, II] == 0)
                            {
                                GLEBA[SEK, II] = proj.WeaponRef;
                                II = 4;
                            }
                        }
                    }

                    // Damage calculation
                    var MOC = 100 - ARMIA[A, I, TDOSW];
                    var CIOS = SILA - amos.Rnd((int)(SILA * MOC) / 100) - amos.Rnd(ODP + 2);
                    if (CIOS <= 0)
                    {
                        CIOS = 1;
                    }

                    ENP += (int)-CIOS;

                    // Knockback for negative-damage arrows
                    if (proj.IsNegative)
                    {
                        ODLOT = amos.Rnd(1);
                        if (RASA2 > 9) ODLOT = 0;
                    }
                    if (ODLOT == 1)
                    {
                        ARMIA[B, targetIdx, TTRYB] = 7;
                        var CELX = X1 + proj.VX * 8;
                        if (CELX < 20) CELX = 20;
                        if (CELX > 620) CELX = 620;
                        var CELY = Y1 + proj.VY * 8;
                        if (CELY < 20) CELY = 20;
                        if (CELY > 500) CELY = 500;
                        ARMIA[B, targetIdx, TCELX] = (int)CELX;
                        ARMIA[B, targetIdx, TCELY] = (int)CELY;
                    }

                    if (ENP <= 0)
                    {
                        var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                        if (A == WRG)
                        {
                            ARMIA[A, I, TKORP] += amos.Rnd(20);
                        }
                        if (ODLOT == 1)
                        {
                            ARMIA[B, targetIdx, TTRYB] = 8;
                            ENP = 2;
                        }
                        else
                        {
                            ZABIJ(B, targetIdx, 0);
                        }
                    }
                    ARMIA[B, targetIdx, TE] = ENP;
                    FX(SAM);
                    hit = true;
                }

                // Hit a wall (zone 21-30)
                if (!hit && STREFA > 20 && STREFA < 31 && KTO_ATAKUJE == proj.OwnerArmy)
                {
                    FX(14);
                    MUR[STREFA - 21] += (int)-SILA;
                    if (MUR[STREFA - 21] <= 0)
                    {
                        var X = (STREFA - 21) * 64;
                        screens.ResetZone(STREFA);
                        screens.PasteBob(X, 111, BIBY + 12 + 2);
                        screens.WaitVbl();
                    }
                    hit = true;
                }

                // Out of bounds or hit something
                if (hit || proj.X > 640 || proj.X < 0 || proj.Y > 512 || proj.Y < 0 || STREFA > 40)
                {
                    FreeProjectileBob(proj.BobNumber);
                    Projectiles.RemoveAt(idx);
                }
            }
        }

        void ClearAllProjectiles()
        {
            foreach (var proj in Projectiles)
            {
                screens.BobOff(proj.BobNumber);
                FreeProjectileBobs.Enqueue(proj.BobNumber);
            }
            Projectiles.Clear();
        }

        int ComputeFireCooldown(int distance)
        {
            // Flat 3 second cooldown (38 ticks at 80ms/tick)
            return FIRE_COOLDOWN_TICKS;
        }

        /// <summary>
        /// Computes the auto-aim target position by extrapolating the target unit's
        /// velocity and adding random spread. Returns the aim point.
        /// </summary>
        (double aimX, double aimY) ComputeAutoAim(int army, int unitIndex, int targetArmy, int targetIdx)
        {
            // Get target's current position
            var curTX = (double)ARMIA[targetArmy, targetIdx, TX];
            var curTY = (double)ARMIA[targetArmy, targetIdx, TY];

            // Get previous position for velocity estimation
            double prevTX, prevTY;
            if (army == WRG)
            {
                prevTX = AutoFirePrevTXW[unitIndex];
                prevTY = AutoFirePrevTYW[unitIndex];
            }
            else
            {
                prevTX = AutoFirePrevTX[unitIndex];
                prevTY = AutoFirePrevTY[unitIndex];
            }

            // Target velocity per tick
            var velX = curTX - prevTX;
            var velY = curTY - prevTY;

            // Store current as previous for next tick
            if (army == WRG)
            {
                AutoFirePrevTXW[unitIndex] = curTX;
                AutoFirePrevTYW[unitIndex] = curTY;
            }
            else
            {
                AutoFirePrevTX[unitIndex] = curTX;
                AutoFirePrevTY[unitIndex] = curTY;
            }

            // Estimate time-to-impact: distance / effective projectile speed per tick
            var shooterX = (double)ARMIA[army, unitIndex, TX];
            var shooterY = (double)ARMIA[army, unitIndex, TY] - 20;
            var dx = curTX - shooterX;
            var dy = curTY - shooterY;
            var dist = Math.Sqrt(dx * dx + dy * dy) + 1;
            // Effective speed per tick = normalized velocity (1.0) * PROJECTILE_SPEED
            var timeToImpact = dist / PROJECTILE_SPEED;

            // Extrapolate target position at time of impact
            var aimX = curTX + velX * timeToImpact;
            var aimY = curTY + velY * timeToImpact;

            // Add random angular spread
            var aimDX = aimX - shooterX;
            var aimDY = aimY - shooterY;
            var aimAngle = Math.Atan2(aimDY, aimDX);
            var spreadRad = (amos.Rnd((int)(AIM_SPREAD_DEGREES * 2)) - AIM_SPREAD_DEGREES) * Math.PI / 180.0;
            aimAngle += spreadRad;
            var aimDist = Math.Sqrt(aimDX * aimDX + aimDY * aimDY);
            aimX = shooterX + Math.Cos(aimAngle) * aimDist;
            aimY = shooterY + Math.Sin(aimAngle) * aimDist;

            return (aimX, aimY);
        }

        void FireProjectileFromUnit(int army, int unitIndex, int targetX, int targetY)
        {
            var B1 = ARMIA[army, unitIndex, TLEWA];
            var B2 = ARMIA[army, unitIndex, TPRAWA];
            var BT1 = BRON[B1, B_TYP];
            var BT2 = BRON[B2, B_TYP];
            var BT3 = RASY[ARMIA[army, unitIndex, TRASA], 4];

            // Check if unit has a ranged weapon
            // Player uses STRZALY[] for arrow count, enemy uses TAMO
            var ammo = army == WRG ? ARMIA[army, unitIndex, TAMO] : STRZALY[unitIndex];
            var hasRanged = (BT1 == 4 && BT2 == 5 && ammo > 0) ||
                            (BT1 == 5 && BT2 == 4 && ammo > 0) ||
                            (BT1 == 15 && BT2 == 16) ||
                            (BT1 == 16 && BT2 == 15) ||
                            (BT1 == 9 && BT2 != 12) ||
                            (BT2 == 9 && BT1 != 12);
            if (!hasRanged) return;

            var damage = (double)(BRON[B1, B_SI] + BRON[B2, B_SI]);
            var weaponRef = 0;
            var isNegative = false;

            // Consume arrow
            if (BT1 == 4 || BT2 == 4)
            {
                if (army == WRG)
                    ARMIA[army, unitIndex, TAMO]--;
                else
                    STRZALY[unitIndex]--;
            }

            // Javelin (thrown weapon, BT=9)
            if (BT1 == 9)
            {
                damage = BRON[B1, B_SI] * 3;
                PRZELICZ(3, -1);
                ARMIA[army, unitIndex, TLEWA] = 0;
                weaponRef = B1;
            }
            else if (BT2 == 9)
            {
                damage = BRON[B2, B_SI] * 3;
                PRZELICZ(4, -1);
                ARMIA[army, unitIndex, TPRAWA] = 0;
                weaponRef = B2;
            }

            // Boomerang (BT=16) - negative damage means knockback
            if (BT1 == 16)
            {
                damage = -damage;
                ARMIA[army, unitIndex, TLEWA] = 0;
                isNegative = true;
            }
            if (BT2 == 16)
            {
                damage = -damage;
                ARMIA[army, unitIndex, TPRAWA] = 0;
                isNegative = true;
            }

            // Race bonus
            if (BT3 == BT1 || BT3 == BT2)
            {
                damage += 10;
            }

            // Compute direction
            var X1 = ARMIA[army, unitIndex, TX];
            var Y1 = ARMIA[army, unitIndex, TY] - 20;
            var DX_R = (double)targetX - X1;
            var DY_R = (double)targetY - Y1;
            var L_R = Math.Sqrt(DX_R * DX_R + DY_R * DY_R) + 1;
            var VX_R = DX_R / L_R;
            var VY_R = DY_R / L_R;

            // Create arrow sprite for this projectile
            var I2 = army == WRG ? unitIndex + 10 : unitIndex;
            var spriteRef = BSIBY + I2;
            if (BT1 == 4 || BT1 == 5 || BT1 == 9 || BT2 == 9)
            {
                screens.Screen(2);
                screens.Cls(0);
                var X_R = 15d;
                var Y_R = 15d;
                for (var II = 1; II <= 20; II++)
                {
                    X_R += VX_R;
                    Y_R += VY_R;
                    screens.Ink(19);
                    screens.Plot((int)X_R, (int)Y_R + 15);
                    screens.Ink(15);
                    screens.Plot((int)X_R, (int)Y_R);
                }
                screens.GetSprite(BSIBY + I2, 0, 0, 31, 31);
                screens.WaitVbl();
                screens.HotSpot(BSIBY + I2, 11);
                screens.Screen(0);
            }

            // Spawn projectile into pool
            SpawnProjectile(army, unitIndex, X1, Y1, VX_R * 6, VY_R * 6,
                damage, spriteRef, weaponRef, isNegative);

            // Set cooldown based on distance
            var distance = (int)L_R;
            var cooldown = ComputeFireCooldown(distance);
            if (army == WRG)
                FireCooldownW[unitIndex] = cooldown;
            else
                FireCooldown[unitIndex] = cooldown;
        }

        /// <summary>
        /// AMOS "Fade speed,$0,$25,$48,$16C,$19F,$0 x11,$FFF,$0 x14" used by
        /// the fire/explosion spell flashes: a red/orange gradient on colours
        /// 0-4, a white flash on colour 16, everything else black.
        /// </summary>
        Pixel[] FADE_KOLORY_WYBUCHU()
        {
            var colors = new Pixel[32];
            for (var i = 0; i < colors.Length; i++) colors[i] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0x0);
            colors[1] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0x25);
            colors[2] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0x48);
            colors[3] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0x16C);
            colors[4] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0x19F);
            colors[16] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0xFFF);
            return colors;
        }

        /// <summary>
        /// AMOS "Fade speed,,$FFF,$FFF,...,$FFF": flashes every colour but 0
        /// to white, used by the "light"/"omniscience" spells.
        /// </summary>
        Pixel[] FADE_KOLORY_BLYSKU()
        {
            var colors = new Pixel[32];
            for (var i = 1; i < colors.Length; i++) colors[i] = AmigaNet.Amos.Screens.ScreensManager.AmosHex(0xFFF);
            return colors;
        }

        void A_CZAR(int A, int I)
        {
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            var BR = (int)VEKTOR_R[I2, 0];
            var SILA = BRON[(int)BR, B_SI];
            var CZAR_TYP = BRON[(int)BR, B_DOSW];
            var MOC = 100 - ARMIA[A, I, TDOSW];
            //'------------------------
            if (CZAR_TYP == 1)
            {
                var XP_R = VEKTOR_R[I2, 3];
                var YP_R = VEKTOR_R[I2, 4];
                var VX_R = VEKTOR_R[I2, 1];
                var VY_R = VEKTOR_R[I2, 2];
                var BB = (int)VEKTOR_R[I2, 5];
                XP_R = XP_R + (VX_R * 2);
                YP_R = YP_R + (VY_R * 2);
                VEKTOR_R[I2, 3] = XP_R;
                VEKTOR_R[I2, 4] = YP_R;
                if (BR == 42)
                {
                    amos.Add(ref BB, 1, 1, 2);
                    VEKTOR_R[I2, 5] = BB;
                }
                if (BR == 43)
                {
                    BB = 25 + amos.Rnd(2);
                }
                if (BR == 44)
                {
                    amos.Add(ref BB, 1, 10, 13);
                    VEKTOR_R[I2, 5] = BB;
                }
                screens.Bob(I2 + 30, (int)XP_R, (int)YP_R, PIKIETY + BB);
                var STREFA = screens.Zone((int)XP_R, (int)YP_R);
                if (STREFA > 0 && STREFA != I2 && STREFA < 21)
                {
                    var B = 0;
                    if (STREFA > 10)
                    {
                        STREFA += -10;
                        B = WRG;
                    }
                    else
                    {
                        B = ARM;
                    }
                    var X1 = ARMIA[B, STREFA, TX];
                    var Y1 = ARMIA[B, STREFA, TY];
                    var ENP = ARMIA[B, STREFA, TE];
                    var CIOS = SILA - amos.Rnd((SILA * MOC) / 100);
                    if (CIOS <= 0)
                    {
                        CIOS = 1;
                    }
                    ENP += -CIOS;
                    ARMIA[B, STREFA, TE] = ENP;
                    FX(12);
                    if (ENP <= 0)
                    {
                        var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                        ZABIJ(B, STREFA, 0);
                    }
                    if (BR != 44)
                    {
                        screens.BobOff(I2 + 30);
                        ARMIA[A, I, TTRYB] = 0;
                    }
                }
                if (STREFA > 20 && STREFA < 31 && KTO_ATAKUJE == A)
                {
                    screens.BobOff(I2 + 30);
                    ARMIA[A, I, TTRYB] = 0;
                    MUR[STREFA - 21] += -SILA;
                    if (MUR[STREFA - 21] <= 0)
                    {
                        var X = (STREFA - 21) * 64;
                        screens.ResetZone(STREFA);
                        //Autoback 2;
                        screens.PasteBob(X, 111, BIBY + 12 + 2);
                        screens.WaitVbl();
                        //Autoback 1
                    }
                }

                if (XP_R > 640 || XP_R < 0 || YP_R > 512 || YP_R < 0 || STREFA > 40)
                {
                    screens.BobOff(I2 + 30);
                    ARMIA[A, I, TTRYB] = 0;
                }
            }
            //'----------------- 
            if (CZAR_TYP == 2)
            {
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {

                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    var TARGET = ARMIA[A, I, TCELX];
                    var B = ARMIA[A, I, TCELY];
                    var X2 = ARMIA[B, TARGET, TX];
                    var Y2 = ARMIA[B, TARGET, TY];
                    var ENP = ARMIA[B, TARGET, TE];
                    var ODP = ARMIA[B, TARGET, TP];
                    var ENM = ARMIA[B, TARGET, TEM];
                    SILA = BRON[BR, B_SI];
                    if (BR == 45)
                    {
                        CENTER(X2, Y2, 1);
                        FX(9);
                        for (var J = 1; J <= 20; J++)
                        {
                            screens.Bob(I2 + 30, X2, Y2 + 8, PIKIETY + 7 + amos.Rnd(2));
                            screens.BobUpdate();
                            screens.WaitVbl();
                        }
                    }
                    if (BR == 46)
                    {
                        CENTER(X2, Y2, 1);
                        screens.Fade(5, FADE_KOLORY_WYBUCHU());
                        FX(8);
                        for (var J = 1; J <= 20; J++)
                        {
                            screens.Bob(I2 + 30, X2, Y2 + 8, PIKIETY + 3 + amos.Rnd(3));
                            screens.BobUpdate();
                            screens.WaitVbl();
                            if (J == 14)
                            {
                                screens.FadeTo(4, 2);
                            }
                        }
                    }
                    if (SILA == 0)
                    {
                        FX(10);
                        for (var J = 1; J <= 15; J++)
                        {
                            screens.Bob(I2 + 30, X2, Y2 + 1, PIKIETY + 14 + amos.Rnd(3));
                            screens.BobUpdate();
                            screens.WaitVbl();
                        }
                    }
                    ARMIA[B, TARGET, TSZ] += BRON[BR, B_SZ];
                    if (ARMIA[B, TARGET, TSZ] < 1)
                    {
                        ARMIA[B, TARGET, TSZ] = 1;
                    }
                    var ENERGIA = BRON[BR, B_EN];
                    var CIOS = SILA - amos.Rnd((SILA * MOC) / 100);
                    ENP += -CIOS;
                    ENP += ENERGIA;
                    if (ENP > ENM)
                    {
                        ENP = ENM;
                    }
                    if (ENP <= 0)
                    {
                        var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                        ZABIJ(B, TARGET, 0);
                    }
                    ARMIA[A, I, TTRYB] = 0;
                    ARMIA[B, TARGET, TE] = ENP;
                    screens.BobOff(I2 + 30);
                }
            }
            //'------------------
            if (CZAR_TYP == 3)
            {
                //'intuicja
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {
                    FX(13);
                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    var TARGET = ARMIA[A, I, TCELX];
                    var B = ARMIA[A, I, TCELY];
                    var X2 = ARMIA[B, TARGET, TX];
                    var Y2 = ARMIA[B, TARGET, TY];
                    for (var J = 1; J <= 20; J++)
                    {
                        screens.Bob(I2 + 30, X2, Y2 + 1, PIKIETY + 14 + amos.Rnd(3));
                        screens.BobUpdate();
                        screens.WaitVbl();
                    }
                    ARMIA[A, I, TTRYB] = 0;
                    if (B == WRG)
                    {
                        ARMIA[B, TARGET, TGLOWA] = 1;
                    }
                    screens.BobOff(I2 + 30);
                }
            }
            //'---------------------   
            if (CZAR_TYP == 5)
            {
                //'�wiat�o�� 
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {
                    screens.Fade(2, FADE_KOLORY_BLYSKU());
                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    FX(13);
                    for (var J = 1; J <= 10; J++)
                    {
                        if (ARMIA[WRG, J, TE] > 0)
                        {
                            screens.Bob(J + 10, ARMIA[WRG, J, TX], ARMIA[WRG, J, TY], ARMIA[WRG, J, TBOB] + 2);
                        }
                    }
                    screens.BobUpdate();
                    screens.WaitVbl();
                    ARMIA[A, I, TTRYB] = 0;
                    amos.Wait(30);
                    screens.FadeTo(3, 2);
                }
            }
            //'-------------   
            if (CZAR_TYP == 6)
            {
                //'wszechwiedza
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {
                    screens.Fade(2, FADE_KOLORY_BLYSKU());
                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    var B = ARMIA[A, I, TCELY];
                    FX(13);
                    if (B == WRG)
                    {
                        for (var J = 1; J <= 10; J++)
                        {
                            ARMIA[B, J, TGLOWA] = 1;
                        }
                    }
                    ARMIA[A, I, TTRYB] = 0;
                    amos.Wait(30);
                    screens.FadeTo(3, 2);
                }
            }
            //'--------------    
            if (CZAR_TYP == 7)
            {
                //'Gniew Bo�y
                if (MUZYKA)
                {
                    //Music Stop;
                }
                screens.WaitVbl();
                FX(8);
                ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                var TARGET = ARMIA[A, I, TCELX];
                var B = ARMIA[A, I, TCELY];
                var X2 = ARMIA[B, TARGET, TX];
                var Y2 = ARMIA[B, TARGET, TY];
                screens.Fade(2, FADE_KOLORY_WYBUCHU());

                CENTER(X2, Y2, 0);
                for (var J = 1; J <= 100; J++)
                {
                    var B3 = 0;
                    screens.Bob(I2 + 30, -1, -1, PIKIETY + 3 + amos.Rnd(3));
                    screens.Bob(I2 + 50, -1, -1, PIKIETY + 3 + amos.Rnd(3));
                    amos.Add(ref B3, 1, PIKIETY + 18, PIKIETY + 20);
                    screens.Bob(I2 + 40, -1, -1, B3);
                    screens.BobUpdate();
                    screens.WaitVbl();
                    var S = amos.Rnd(15);
                    if (S < 2)
                    {
                        FX(9);
                    }
                    if (S == 2)
                    {
                        FX(8);
                    }
                    if (S == 3)
                    {
                        FX(12);
                    }
                    if (S == 4)
                    {
                        FX(14);
                    }
                    if (J % 2 == 0)
                    {
                        screens.FadeTo(5, 2);
                        screens.Bob(I2 + 30, SX + amos.Rnd(320), SY + amos.Rnd(250), PIKIETY + 3 + amos.Rnd(3));
                    }

                    if (J % 2 == 1)
                    {
                        screens.Bob(I2 + 50, SX + amos.Rnd(320), SY + amos.Rnd(250), PIKIETY + 3 + amos.Rnd(3));
                        screens.Fade(5, FADE_KOLORY_WYBUCHU());
                    }
                    if (J % 3 == 0)
                    {
                        screens.Bob(I2 + 40, SX + amos.Rnd(320), SY + amos.Rnd(250), B3);
                    }
                    CENTER(X2 + amos.Rnd(10) - 5, Y2 + amos.Rnd(10) - 5, 0);
                }
                screens.BobOff(I2 + 30);
                screens.BobOff(I2 + 40);
                screens.BobOff(I2 + 50);
                for (var J = 1; J <= 10; J++)
                {
                    if (ARMIA[B, J, TE] > 0)
                    {
                        ARMIA[B, J, TE] += -amos.Rnd(SILA);
                        if (ARMIA[B, J, TE] <= 0)
                        {
                            var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                            amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                            ZABIJ(B, J, 0);
                        }
                    }
                }
                ARMIA[A, I, TTRYB] = 0;
                screens.FadeTo(2, 2);
                if (MUZYKA)
                {
                    //Music 1;
                }
            }
            //'----------------------- 
            if (CZAR_TYP == 8)
            {
                //'nawr�cenie
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {
                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    var B = ARMIA[A, I, TCELX];
                    var B2 = ARMIA[A, I, TCELY];
                    var X2 = ARMIA[B2, B, TX];
                    var Y2 = ARMIA[B2, B, TY];
                    var RASA = ARMIA[B2, B, TRASA];
                    FX(15);
                    for (var J = 1; J <= 20; J++)
                    {
                        screens.Bob(I2 + 30, X2, Y2 + 1, PIKIETY + 14 + amos.Rnd(3));
                        screens.BobUpdate();
                        screens.WaitVbl();
                    }
                    ARMIA[A, I, TTRYB] = 0;
                    if (RASA < 10 && B2 == WRG)
                    {
                        var JEST = false;
                        var L2 = 0;
                        for (var L = 1; L <= 10; L++)
                        {
                            if (ARMIA[ARM, L, TE] <= 0)
                            {
                                JEST = true;
                                L2 = L;
                                L = 10;
                            }
                        }
                        if (JEST)
                        {
                            ARMIA[ARM, L2, TRASA] = ARMIA[WRG, B, TRASA];
                            ARMIA[ARM, L2, TSI] = ARMIA[WRG, B, TSI];
                            ARMIA[ARM, L2, TSZ] = ARMIA[WRG, B, TSZ];
                            ARMIA[ARM, L2, TE] = ARMIA[WRG, B, TE];
                            ARMIA[ARM, L2, TEM] = ARMIA[WRG, B, TEM];
                            ARMIA[ARM, L2, TKLAT] = ARMIA[WRG, B, TKLAT];
                            ARMIA[ARM, L2, TMAG] = ARMIA[WRG, B, TMAG];
                            ARMIA[ARM, L2, TMAGMA] = ARMIA[WRG, B, TMAGMA];
                            ARMIA[ARM, L2, TAMO] = ARMIA[WRG, B, TSZ];
                            ARMIA[ARM, L2, TDOSW] = ARMIA[WRG, B, TDOSW];
                            ARMIA[ARM, L2, TP] = 0;
                            for (var II = TGLOWA; II <= TPLECAK + 7; II++)
                            {
                                ARMIA[ARM, L2, II] = 0;
                            }
                            ARMIA_S[ARM, L2] = ARMIA_S[WRG, B];
                            ARMIA[WRG, B, TE] = 0;
                            screens.BobOff(10 + B);
                            screens.ResetZone(10 + B);
                            screens.BobUpdate();
                            screens.WaitVbl();
                            ARMIA[WRG, B, TTRYB] = 0;
                            ARMIA[ARM, L2, TX] = ARMIA[WRG, B, TX];
                            ARMIA[ARM, L2, TY] = ARMIA[WRG, B, TY];
                            var BAZA = RASY[ARMIA[ARM, L2, TRASA], 7];
                            var X = ARMIA[ARM, L2, TX];
                            var Y = ARMIA[ARM, L2, TY];
                            ARMIA[ARM, L2, TBOB] = BAZA;
                            screens.Bob(L2, X, Y, BAZA + 1);
                            screens.BobOff(B + 10 + 30);
                            screens.SetZone(L2, X - 16, Y - 20, X + 16, Y);
                        }
                    }
                    screens.BobOff(I2 + 30);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
            }


            if (CZAR_TYP == 9)
            {
                //'wybuch
                //'      KLATKA=VEKTOR_R(I2,3)
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {

                    var X = ARMIA[A, I, TCELX];
                    var Y = ARMIA[A, I, TCELY];
                    CENTER(X, Y, 1);
                    FX(14);
                    for (var J = 0; J <= 2; J++)
                    {
                        screens.Bob(I2 + 30, X, Y, PIKIETY + 18 + J);
                        CENTER(X + amos.Rnd(10) - 5, Y + amos.Rnd(10) - 5, 0);
                        screens.BobUpdate();
                        screens.WaitVbl();
                    }
                    screens.BobOff(I2 + 30);
                    ARMIA[A, I, TTRYB] = 0;

                    var KLATKA = 0;
                    //'zliczanie odleg�o�ci od epicentrum
                    if (KLATKA == 0)
                    {
                        for (var J = 1; J <= 10; J++)
                        {
                            if (ARMIA[WRG, J, TE] > 0)
                            {
                                var X2 = ARMIA[WRG, J, TX];
                                var Y2 = ARMIA[WRG, J, TY];
                                var DX = X2 - X;
                                var DY = Y2 - Y;
                                ODLEG = (int)Math.Abs(Math.Sqrt(DX * DX + DY * DY));
                                var ODLOT = 0;
                                if (ODLEG < 60)
                                {
                                    ODLOT = 0;
                                    if (I != J)
                                    {
                                        ODLOT = amos.Rnd(1);
                                    }
                                    if (ODLOT == 1)
                                    {
                                        ARMIA[WRG, J, TTRYB] = 7;
                                        var CELX = X2 + DX;
                                        if (CELX < 20)
                                        {
                                            CELX = 20;
                                        }
                                        if (CELX > 620)
                                        {
                                            CELX = 620;
                                        }
                                        var CELY = Y2 + DY;
                                        if (CELY < 20)
                                        {
                                            CELY = 20;
                                        }
                                        if (CELY > 500)
                                        {
                                            CELY = 500;
                                        }
                                        ARMIA[WRG, J, TCELX] = CELX;
                                        ARMIA[WRG, J, TCELY] = CELY;
                                    }
                                    var ENP = ARMIA[WRG, J, TE];
                                    var CIOS = SILA - (ODLEG / 2);
                                    ENP += -CIOS;


                                    if (ENP <= 0)
                                    {
                                        var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                                        if (ODLOT == 1)
                                        {
                                            ARMIA[WRG, J, TTRYB] = 8;
                                            ENP = 2;
                                        }
                                        else
                                        {
                                            ZABIJ(WRG, J, 0);
                                        }
                                    }
                                    ARMIA[WRG, J, TE] = ENP;
                                }
                            }
                            if (ARMIA[ARM, J, TE] > 0)
                            {
                                var X2 = ARMIA[ARM, J, TX];
                                var Y2 = ARMIA[ARM, J, TY];
                                var DX = X2 - X;
                                var DY = Y2 - Y;
                                ODLEG = (int)Math.Abs(Math.Sqrt(DX * DX + DY * DY));
                                var ODLOT = 0;
                                if (ODLEG < 60)
                                {
                                    ODLOT = 0;
                                    if (I != J)
                                    {
                                        ODLOT = amos.Rnd(1);
                                    }
                                    if (ODLOT == 1)
                                    {
                                        ARMIA[ARM, J, TTRYB] = 7;
                                        var CELX = X2 + DX;
                                        if (CELX < 20)
                                        {
                                            CELX = 20;
                                        }
                                        if (CELX > 620)
                                        {
                                            CELX = 620;
                                        }
                                        var CELY = Y2 + DY;
                                        if (CELY < 20)
                                        {
                                            CELY = 20;
                                        }
                                        if (CELY > 500)
                                        {
                                            CELY = 500;
                                        }
                                        ARMIA[ARM, J, TCELX] = CELX;
                                        ARMIA[ARM, J, TCELY] = CELY;
                                    }

                                    var ENP = ARMIA[ARM, J, TE];
                                    var CIOS = SILA - (ODLEG / 2);
                                    ENP += -CIOS;
                                    if (ENP <= 0)
                                    {
                                        var MUNDRY = RASY[ARMIA[A, I, TRASA], 6];
                                        amos.Add(ref ARMIA[A, I, TDOSW], amos.Rnd(MUNDRY), ARMIA[A, I, TDOSW], 95);
                                        if (ODLOT == 1)
                                        {
                                            ARMIA[ARM, J, TTRYB] = 8;
                                            ENP = 2;
                                        }
                                        else
                                        {
                                            ZABIJ(ARM, J, 0);
                                        }
                                    }
                                    ARMIA[ARM, J, TE] = ENP;
                                }
                            }
                        }
                    }
                    //'      VEKTOR_R(I2,3)=VEKTOR_R(I2,3)+1 
                }
            }
            if (CZAR_TYP == 10)
            {
                //'uspokojenie 
                var SPEED = (100 - ARMIA[A, I, TSZ]) / 10;
                if (amos.Rnd(SPEED) == 0)
                {
                    FX(15);
                    ARMIA[A, I, TMAG] += -BRON[BR, B_MAG];
                    var TARGET = ARMIA[A, I, TCELX];
                    var B = ARMIA[A, I, TCELY];
                    var X2 = ARMIA[B, TARGET, TX];
                    var Y2 = ARMIA[B, TARGET, TY];
                    for (var J = 1; J <= 20; J++)
                    {
                        screens.Bob(I2 + 30, X2, Y2 + 1, PIKIETY + 14 + amos.Rnd(3));
                        screens.BobUpdate();
                        screens.WaitVbl();
                    }
                    ARMIA[A, I, TTRYB] = 0;
                    if (B == WRG)
                    {
                        amos.Add(ref ARMIA[B, TARGET, TKORP], -amos.Rnd(SILA) - 10 - (ARMIA[A, I, TDOSW] / 2), 0, ARMIA[B, TARGET, TKORP]);
                    }
                    screens.BobOff(I2 + 30);
                }
            }
        }

        void ZABIJ(int A, int NR, int CICHO)
        {
            var RASA = ARMIA[A, NR, TRASA];
            var SAM = 0;
            if (RASA > 9)
            {
                SAM = 3;
                banks.SamBank(5);
            }
            else
            {
                SAM = 7;
            }
            FX(SAM);
            banks.SamBank(4);
            ARMIA[A, NR, TE] = 0;
            ARMIA[A, NR, TTRYB] = 0;
            var X = ARMIA[A, NR, TX];
            var Y = ARMIA[A, NR, TY];
            var SEK = SEKTOR(X, Y);
            for (var I = 0; I <= 3; I++)
            {
                if (GLEBA[SEK, I] == 0)
                {
                    if (A == ARM)
                    {
                        GLEBA[SEK, I] = ARMIA[A, NR, TKORP + I];
                    }
                    else
                    {
                        if (RASA > 9)
                        {
                            var BR = RASY[RASA, 3 + amos.Rnd(1)];
                            GLEBA[SEK, I] = BR;
                        }
                        else
                        {
                            if (amos.Rnd(2) == 0)
                            {
                            LOSUJ:
                                var BR = amos.Rnd(MX_WEAPON);
                                if (BRON[BR, B_CENA] < (ARMIA[A, NR, TDOSW] * 30) + ARMIA[A, NR, TSI])
                                {
                                    GLEBA[SEK, I] = BR;
                                }
                                else
                                {
                                    goto LOSUJ;
                                }
                            }
                        }
                    }
                }
            }
            var BAZA = ARMIA[A, NR, TBOB];
            if (A == WRG)
            {
                NR += 10;
                for (var I = 1; I <= 10; I++)
                {
                    amos.Add(ref ARMIA[A, I, TKORP], -amos.Rnd(20), 1, ARMIA[A, I, TKORP]);
                }
            }
            screens.BobOff(NR);
            screens.BobOff(30 + NR);
            screens.BobUpdate();
            screens.WaitVbl();
            var KB = BAZA + 16;
            if (amos.Rnd(1) == 0)
            {
                //KB = Hrev(BAZA + 16);
            }
            var STREFA2 = screens.Zone(X, Y + 30);
            if (CICHO == 0 && (STREFA2 < 20 || STREFA2 > 31) && SCENERIA != 7 && PREFS[4] == 1)
            {
                //Autoback 2;
                screens.PasteBob(X - 24, Y - 20, KB);
                //Wait 2;
                //Autoback 1
            }
            screens.ResetZone(NR);
        }

        void FX(int SAM)
        {
            amos.Add(ref KANAL, 1, 1, 3);
            var KAN = 0;
            if (KANAL == 1) KAN = 0b10;
            if (KANAL == 2) KAN = 0b100;
            if (KANAL == 3) KAN = 0b1000;
            banks.PlaySample(SAM, KAN);
        }
        int PLAPKA(int NR, int A, int I, int X, int Y)
        {
            var SKOK = 0;
            var PLAPKA = PLAPKI[NR, 0];
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            //'przepa��
            if (PLAPKA == 1)
            {
                //screens.LimitBob(I2, 0, 0, 640, Y + 2);
                for (var L = Y; L <= Y + 60; L += 6)
                {
                    //screens.Bob(I2,, L,);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
                SKOK = 1;
                ZABIJ(A, I, 1);
                //Wait 20
                FX(2);
            }
            //'bagno 
            if (PLAPKA == 2)
            {
                //screens.LimitBob(I2, 0, 0, 640, Y + 2);
                for (var L = Y; L <= Y + 35; L++)
                {
                    //screens.Bob(I2,, L,;);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
                ZABIJ(A, I, 1);
                SKOK = 1;
            }
            //'lodowate jeziorko 
            if (PLAPKA == 3)
            {
                SKOK = 0;
                ARMIA[A, I, TE] += -2;
                if (ARMIA[A, I, TE] <= 0)
                {
                    SKOK = 1;
                    ZABIJ(A, I, 1);
                }
            }
            //'zapadnia
            if (PLAPKA == 4)
            {
                //Autoback 2
                screens.PasteBob(PLAPKI[NR, 1], PLAPKI[NR, 2], BIBY + 10);
                //Autoback 1
                //screens.LimitBob(I2, 0, 0, 640, PLAPKI[NR, 4]);
                for (var L = Y; L <= Y + 80; L += 6)
                {
                    //screens.Bob(I2,, L,;);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
                SKOK = 1;
                ZABIJ(A, I, 1);
                //Wait 20
                FX(2);
            }
            //'kolce 
            if (PLAPKA == 5)
            {
                SKOK = 0;
                var CIOS = amos.Rnd(10) + 5;
                FX(11);
                ARMIA[A, I, TE] += -CIOS;
                if (ARMIA[A, I, TE] <= 0)
                {
                    SKOK = 1;
                    ZABIJ(A, I, 0);
                }
                //Autoback 2
                screens.PasteBob(PLAPKI[NR, 1], PLAPKI[NR, 2], BIBY + 7);
                //Autoback 1
            }
            //'przepa�� g��boka
            if (PLAPKA == 6)
            {
                //screens.LimitBob(I2, 0, 0, 640, Y + PLAPKI[NR, 4]);
                for (var L = Y; L <= Y + 180; L += 6)
                {
                    //screens.Bob(I2,, L,;);
                    screens.BobUpdate();
                    screens.WaitVbl();
                }
                SKOK = 1;
                ZABIJ(A, I, 1);
                //Wait 20
                FX(2);
            }
            return SKOK;
        }

        void A_ROZMOWA(int A, int I)
        {
            screens.Screen(0);
            var I2 = 0;
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            var X1 = ARMIA[A, I, TX];
            var Y1 = ARMIA[A, I, TY];
            var BAZA = ARMIA[A, I, TBOB];
            var BNR = BAZA + 7;
            var TARGET = ARMIA[A, I, TCELX];
            var B = ARMIA[A, I, TCELY];
            var X2 = ARMIA[B, TARGET, TX];
            var Y2 = ARMIA[B, TARGET, TY];

            var SPEED = ARMIA[A, I, TSZ] / 10;
            var SPEED2 = 3 - SPEED;
            var KLATKA = 0;
            if (SPEED2 <= 0) SPEED2 = 1;
            if (SPEED <= 0) SPEED = 1;
            if (SPEED > 7) SPEED = 7;
            KLATKA = ARMIA[A, I, TKLAT];
            amos.Add(ref KLATKA, 1, 0, (SPEED2 * 4) - 1);
            ARMIA[A, I, TKLAT] = KLATKA;
            KLATKA = KLATKA / SPEED2;
            KLATKA = AN[KLATKA];
            var STREFA2 = screens.Zone(X1, Y1 + 30);
            var STREFA = screens.Zone(X1, Y1 + 1);
            if (STREFA > 30 && STREFA < 41)
            {
                var res = PLAPKA(STREFA - 30, A, I, X1, Y1);
                if (res == 1)
                {
                    return;
                }
            }
            if (STREFA2 > 20 && STREFA2 < 31)
            {
                //screens.LimitBob(I2, 0, 0, 640, 114);

            }
            else
            {
                //screens.LimitBob(I2,0,0, 640,512);
            }

            var ROZX = X2 - X1;
            var ROZY = Y2 - Y1;
            var T = 0;
            if (Math.Abs(ROZX) > 53)
            {
                var ZNX = amos.Sgn(ROZX);
                if (ZNX == -1)
                {
                    BNR = BAZA + 4 + KLATKA;
                    T = -17;
                }
                if (ZNX == 1)
                {
                    BNR = BAZA + 10 + KLATKA;
                    T = 17;
                }
                var ST = screens.Zone(X1 + T, Y1);
                if (ST == 0 || ST > 30 && ST < 41 && A == ARM)
                {
                    X1 += ZNX * SPEED;
                }
            }
            if (Math.Abs(ROZY) > 42)
            {
                var ZNY = amos.Sgn(ROZY);
                var B2 = 0;
                if (ZNY == -1)
                {
                    B2 = BAZA + 1 + KLATKA;
                    T = -21;
                }
                if (ZNY == 1)
                {
                    B2 = BAZA + 7 + KLATKA;
                    T = 2;
                }
                var ST = screens.Zone(X1, Y1 + T);
                if (ST == 0 || ST > 30 && ST < 41 && A == ARM)
                {
                    Y1 += ZNY * SPEED;
                    BNR = B2;
                }
            }
            if (Math.Abs(ROZX) <= 53 && Math.Abs(ROZY) <= 42)
            {
                ARMIA[A, I, TTRYB] = 0;
                GADKA(I, TARGET);
            }
            ARMIA[A, I, TX] = X1;
            ARMIA[A, I, TY] = Y1;
            screens.SetZone(I2, X1 - 15, Y1 - 15, X1 + 15, Y1);
            screens.Bob(I2, X1, Y1, BNR);
        }

        void A_LOT(int A, int I, int TRYB)
        {
            var I2 = 0;
            screens.Screen(0);
            if (A == WRG)
            {
                I2 = I + 10;
            }
            else
            {
                I2 = I;
            }
            //'wy��czam go�cia pikiete 
            screens.BobOff(I2 + 30);

            var X1 = ARMIA[A, I, TX];
            var Y1 = ARMIA[A, I, TY];
            var X2 = ARMIA[A, I, TCELX];
            var Y2 = ARMIA[A, I, TCELY];
            var BAZA = ARMIA[A, I, TBOB];
            var KLATKA = ARMIA[A, I, TKLAT];
            amos.Add(ref KLATKA, 3, 2, 11);
            ARMIA[A, I, TKLAT] = KLATKA;
            var BNR = BAZA + KLATKA;
            var ROZX = X2 - X1;
            var ROZY = Y2 - Y1;
            var STREFA = screens.Zone(X1, Y1 + 1);
            var STREFA2 = screens.Zone(X1, Y1 + 30);
            var RASA = ARMIA[A, I, TRASA];
            var SPEED = 5;
            var KLIN = 2;
            if (STREFA2 > 20 && STREFA2 < 31)
            {
                //screens.LimitBob(I2, 0, 0, 640, 114);
            }
            else
            {
                //screens.LimitBob(I2, 0, 0, 640, 512);
            }
            if (STREFA > 30 && STREFA < 41)
            {
                var res = PLAPKA(STREFA - 30, A, I, X1, Y1);
                if (res == 1)
                {
                    return;
                }
            }
            if (Math.Abs(ROZX) > 4)
            {
                var ZNX = amos.Sgn(ROZX);
                var T = 0;
                if (ZNX == -1)
                {
                    T = -17;
                }
                if (ZNX == 1)
                {
                    T = 17;
                }
                var ST = screens.Zone(X1 + T, Y1);
                if (ST == 0 || (ST > 100 && ST < 120 || ST > 30 && ST < 41) && A == ARM)
                {
                    X1 += ZNX * SPEED;
                    KLIN--;
                }
            }
            if (Math.Abs(ROZY) > 4)
            {
                var ZNY = amos.Sgn(ROZY);
                var T = 0;
                if (ZNY == -1)
                {
                    T = -21;
                }
                if (ZNY == 1)
                {
                    T = 2;
                }
                var ST = screens.Zone(X1, Y1 + T);
                if (ST == 0 || (ST > 30 && ST < 41))
                {
                    Y1 += ZNY * SPEED;
                    KLIN--;
                }
            }

            if (Math.Abs(ROZX) <= 4 && Math.Abs(ROZY) <= 4)
            {
                KLIN = 0;
                if (TRYB == 8)
                {
                    ZABIJ(A, I, 0);
                    return;
                }
                ARMIA[A, I, TTRYB] = 0;
            }
        SKIP:
            ARMIA[A, I, TX] = X1;
            ARMIA[A, I, TY] = Y1;
            screens.SetZone(I2, X1 - 15, Y1 - 15, X1 + 15, Y1);
            screens.Bob(I2, X1, Y1, BNR);
            if (KLIN == 2)
            {
                if (TRYB == 8)
                {
                    ZABIJ(A, I, 0);
                }
                ARMIA[A, I, TTRYB] = 0;
            }
        }

        void WYDAJ_ROZKAZ(int NR)
        {
            var AGRESJA = ARMIA[WRG, NR, TKORP];
            var RASA = ARMIA[WRG, NR, TRASA];
            var MAGIA = ARMIA[WRG, NR, TMAG];

            var STARAODL = 0;
            var WIDAC = false;
            var TARGET = 0;
            var CX = 0;
            var CY = 0;
            var RODZAJ = 0;

            if (AGRESJA < 50)
            {
                RODZAJ = 0;
            }
            if (AGRESJA >= 50)
            {
                RODZAJ = 1;
            }
            if (AGRESJA > 100)
            {
                RODZAJ = 2;
            }
            if (AGRESJA > 150)
            {
                RODZAJ = 3;
            }
            var X1 = ARMIA[WRG, NR, TX];
            var Y1 = ARMIA[WRG, NR, TY];
            if (RODZAJ == 1 || RODZAJ == 2 || RODZAJ == 3)
            {
                STARAODL = WIDOCZNOSC;
                WIDAC = false;
                for (var I = 1; I <= 10; I++)
                {
                    if (ARMIA[ARM, I, TE] > 0)
                    {
                        var X2 = ARMIA[ARM, I, TX];
                        var Y2 = ARMIA[ARM, I, TY];
                        ODL(X1, Y1, X2, Y2);
                        if (ODLEG < STARAODL)
                        {
                            TARGET = I;
                            CX = X2;
                            CY = Y2;
                            STARAODL = ODLEG;
                            WIDAC = true;
                        }
                    }
                }
            }
            var RAN = amos.Rnd(10);
            if (RODZAJ == 2)
            {
                if (WIDAC)
                {
                    if (STARAODL < 50)
                    {
                        WYDAJ_ROZKAZ_ATAKUJ(NR, TARGET);
                    }
                    else
                    {
                        if (STARAODL < WIDOCZNOSC - 60)
                        {
                            screens.Bob(10 + NR, X1, Y1, ARMIA[WRG, NR, TBOB] + 2);
                        }
                        if (ARMIA[WRG, NR, TAMO] > 0 || MAGIA > 10)
                        {
                            if (RAN < 5)
                            {
                                WYDAJ_ROZKAZ_STRZELAJ(NR, RASA, TARGET, ref CX, ref CY, ref X1, ref Y1);
                            }
                        }
                        else
                        {
                            if (amos.Rnd(1) == 0)
                            {
                                ARMIA[WRG, NR, TKORP] = 90;
                            }
                            else
                            {
                                ARMIA[WRG, NR, TKORP] = 155;
                            }
                        }
                    }
                }
            }

            if (RODZAJ == 3)
            {
                if (WIDAC)
                {
                    if ((ARMIA[WRG, NR, TAMO] > 0 || MAGIA > 10) && RAN < 2)
                    {
                        WYDAJ_ROZKAZ_STRZELAJ(NR, RASA, TARGET, ref CX, ref CY, ref X1, ref Y1);
                    }
                    else
                    {
                        WYDAJ_ROZKAZ_ATAKUJ(NR, TARGET);
                    }
                }
            }

            if (RODZAJ == 0)
            {
                WYDAJ_ROZKAZ_RANDOM(NR);
            }

            if (RODZAJ == 1)
            {
                if (STARAODL < 50)
                {
                    WYDAJ_ROZKAZ_ATAKUJ(NR, TARGET);
                }
                else
                {
                    WYDAJ_ROZKAZ_RANDOM(NR);
                }
            }
            return;
        }

        void WYDAJ_ROZKAZ_STRZELAJ(int NR, int RASA, int TARGET, ref int CX, ref int CY, ref int X1, ref int Y1)
        {
            var CZAR = 0;
            if (RASA > 9 && ARMIA[WRG, NR, TPLECAK] > 0)
            {
                CZAR = ARMIA[WRG, NR, TPLECAK];
            }
            else
            {
                CZAR = 42 + amos.Rnd(4);
            }
            var MAG = BRON[CZAR, B_MAG];
            if (amos.Rnd(5) == 1 && ARMIA[WRG, NR, TMAG] - MAG >= 0)
            {
                ARMIA[WRG, NR, TMAG] += -MAG;
                var CZAR_TYP = BRON[CZAR, B_DOSW];
                if (CZAR_TYP == 1)
                {
                    ARMIA[WRG, NR, TCELX] = CX;
                    CY += -10;
                    ARMIA[WRG, NR, TCELY] = CY;
                    ARMIA[WRG, NR, TTRYB] = 4;
                    Y1 += -20;
                    var DX_R = CX - X1;
                    var DY_R = CY - Y1;
                    var L_R = Math.Sqrt(DX_R * DX_R + DY_R * DY_R) + 1;
                    var VX_R = DX_R / L_R;
                    var VY_R = DY_R / L_R;
                    VEKTOR_R[NR + 10, 1] = VX_R * 6;
                    VEKTOR_R[NR + 10, 2] = VY_R * 6;
                    VEKTOR_R[NR + 10, 3] = X1;
                    VEKTOR_R[NR + 10, 4] = Y1;
                    VEKTOR_R[NR + 10, 0] = CZAR;
                }
                else
                {
                    VEKTOR_R[NR + 10, 0] = CZAR;
                    ARMIA[WRG, NR, TCELX] = TARGET;
                    ARMIA[WRG, NR, TCELY] = ARM;
                    ARMIA[WRG, NR, TTRYB] = 5;
                }
            }
            else
            {
                if (RASA < 9)
                {
                    ARMIA[WRG, NR, TAMO] += -1;
                    // Enter auto-fire mode with target
                    ARMIA[WRG, NR, TTRYB] = 9;
                    ARMIA[WRG, NR, TCELX] = CX;
                    CY += -10;
                    ARMIA[WRG, NR, TCELY] = CY;
                    // Store target unit for auto-aim tracking
                    AutoFireTargetIdxW[NR] = TARGET;
                    AutoFirePrevTXW[NR] = ARMIA[ARM, TARGET, TX];
                    AutoFirePrevTYW[NR] = ARMIA[ARM, TARGET, TY];
                    // Fire first projectile immediately
                    FireProjectileFromUnit(WRG, NR, CX, CY);
                }
            }
            screens.Screen(0);
        }

        void WYDAJ_ROZKAZ_ATAKUJ(int NR, int TARGET)
        {
            ARMIA[WRG, NR, TCELX] = TARGET;
            ARMIA[WRG, NR, TCELY] = ARM;
            ARMIA[WRG, NR, TTRYB] = 2;
        }

        void WYDAJ_ROZKAZ_RANDOM(int NR)
        {
            var X2 = amos.Rnd(600) + 20;
            var Y2 = amos.Rnd(450) + 50;
            if (screens.Zone(X2, Y2) == 0)
            {
                ARMIA[WRG, NR, TCELX] = X2;
                ARMIA[WRG, NR, TCELY] = Y2;
                ARMIA[WRG, NR, TTRYB] = 1;
            }
        }










}
}
//while (screens.MouseKey() == LEWY) { }