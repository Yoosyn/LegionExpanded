using System.Collections.Generic;

namespace AmigaNet.Legion
{
    public class Projectile
    {
        public double X, Y;         // current position
        public double VX, VY;       // velocity per tick
        public double Damage;       // hit power (SILA)
        public int OwnerArmy;       // ARM or WRG
        public int OwnerIndex;      // unit index 1-10
        public int BobNumber;       // assigned Bob slot
        public int SpriteRef;       // BSIBY + offset or PIKIETY variant
        public int WeaponRef;       // weapon index (for javelins landing on ground)
        public bool IsNegative;     // SILA < 0 (knockback arrows)
    }

    public partial class Legion
    {
        public string KAT_S;

        public int LEWY = 1;
        public int PRAWY = 2;

        public int[,,] ARMIA = new int[41, 11, 31];
        public int[,] WOJNA = new int[6, 6];
        public int[,] GRACZE = new int[5, 4];

        public string[,] ARMIA_S = NewStringArray(41, 11);
        public string[] IMIONA_S = NewStringArray(5);

        public int[] AN = new int[4] { 0, 1, 0, 2 };

        public double[,] VEKTOR_R = new double[21, 6];

        // Projectile pool for multi-arrow auto-fire system
        public List<Projectile> Projectiles = new List<Projectile>();
        public int[] FireCooldown = new int[11];   // per-unit cooldown timer (player)
        public int[] FireCooldownW = new int[11];  // per-unit cooldown timer (enemy)
        public Queue<int> FreeProjectileBobs = new Queue<int>();
        public int NextProjectileBob = 60;
        public const int MAX_PROJECTILES = 30;
        public const int PROJECTILE_SPEED = 4;     // was 2 in original
        public const int FIRE_COOLDOWN_TICKS = 38; // 3 seconds at 80ms/tick
        public const double AIM_SPREAD_DEGREES = 7.0; // random aim spread ±degrees

        // Auto-aim target tracking
        public int[] AutoFireTargetIdx = new int[11];   // target unit index for player auto-fire (0=none)
        public int[] AutoFireTargetIdxW = new int[11];  // target unit index for enemy auto-fire (0=none)
        public double[] AutoFirePrevTX = new double[11];  // previous target X (player)
        public double[] AutoFirePrevTY = new double[11];  // previous target Y (player)
        public double[] AutoFirePrevTXW = new double[11]; // previous target X (enemy)
        public double[] AutoFirePrevTYW = new double[11]; // previous target Y (enemy)

        public int[] PREFS = new int[11];

        public int[,,] MIASTA = new int[51, 21, 7];
        public string[] MIASTA_S = NewStringArray(51);
        public int[] MUR = new int[11];
        public int[,] SKLEP = new int[21, 22];
        public int[] STRZALY = new int[11];

        public int TEM = 0, TX = 1, TY = 2, TSI = 3, TSZ = 4, TCELX = 5, TCELY = 6, TTRYB = 7, TE = 8, TP = 9,
            TBOB = 10, TKLAT = 11, TAMO = 12, TLEWA = 16, TPRAWA = 17, TNOGI = 15, TGLOWA = 13,
            TPLECAK = 18, TKORP = 14, TMAG = 26, TDOSW = 27, TRASA = 28, TWAGA = 29, TMAGMA = 30;

        public int M_X = 1, M_Y = 2, M_LUDZIE = 3, M_PODATEK = 4, M_CZYJE = 5, M_MORALE = 6, M_MUR = 0;

        public int OX, OY, NUMER, ARM = 0, WRG = 40, SX, SY, MSX, MSY, FONT, FONR, LOK, DZIEN, FON1, FON2;
        public int FONTSZ = 5;

        public int WPI, IMIONA, ODLEG, WIDOCZNOSC, BUBY, BIBY, BSIBY, PIKIETY, POTWORY;
        public string WPI_S;
        public double WPI_R;

        public int SCENERIA, LAST_GAD, KANAL, POWER, REZULTAT, GOBY, KONIEC_INTRA;
        public bool MUZYKA;
        public int CENTER_V = 100;

        public int[,] RASY = new int[21, 8];
        public string[] RASY_S = NewStringArray(21);

        public int[,] BRON = new int[121, 12];
        public string[] BRON_S = NewStringArray(121);
        public string[] BRON2_S = NewStringArray(26);

        public int[,] GLEBA = new int[111, 5];
        public int[,] PLAPKI = new int[11, 5];

        public int[,] BUDYNKI = new int[13, 7];
        public string[] BUDYNKI_S = NewStringArray(13);
        public string[] GUL_S = NewStringArray(11);

        public string[,] ROZMOWA_S = NewStringArray(6, 6);
        public string[] ROZMOWA2_S = NewStringArray(51);
        public string[,] PRZYGODY_S = NewStringArray(21, 11);
        public string[] IM_PRZYGODY_S = NewStringArray(4);
        public int[,] PRZYGODY = new int[4, 11];

        public int TRWA_PRZYGODA;

        public int P_TYP = 0, P_X = 1, P_Y = 2, P_TERMIN = 3, P_KIERUNEK = 4, P_LEVEL = 5, P_CENA = 6, P_NAGRODA = 7, P_BRON = 8, P_TEREN = 9, P_STAREX = 10;

        public int BROBY = 15;
        public int B_SI = 1, B_PAN = 2, B_SZ = 3, B_EN = 4, B_TYP = 5, B_WAGA = 6;
        public int B_PLACE = 7, B_DOSW = 8, B_MAG = 9, B_CENA = 10, B_BOB = 11;
        public int OKX, OKY, SPX, SPY, WYNIK_AKCJI, TESTING, CELOWNIK;
        public bool REAL_KONIEC;
        public bool KONIEC_AKCJI;
        public int KTO_ATAKUJE = -1;
        public int _MODULO, SUPERVISOR, MX_WEAPON;
        public bool GAME_OVER;

        // AMOS "Dim" defaults every slot of a string array to "" - C# arrays
        // default to null. Most of these arrays get fully populated by a
        // WCZYTAJ_* data loader at startup (safe either way), but some are
        // only partially filled during play (e.g. MIASTA_S / IM_PRZYGODY_S,
        // only as many slots as there are active cities/adventures) - an
        // unfilled null slot crashes anything that calls .Length on it
        // (confirmed: _SAVE_GAME did exactly this on MIASTA_S). Pre-filling
        // with "" here matches AMOS semantics everywhere at once instead of
        // guarding every individual read/write site.
        private static string[] NewStringArray(int length)
        {
            var array = new string[length];
            Array.Fill(array, "");
            return array;
        }

        private static string[,] NewStringArray(int length0, int length1)
        {
            var array = new string[length0, length1];
            for (var i = 0; i < length0; i++)
            {
                for (var j = 0; j < length1; j++)
                {
                    array[i, j] = "";
                }
            }
            return array;
        }

    }
}



