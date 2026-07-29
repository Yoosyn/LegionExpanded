using AmigaNet.Types.Graphics;

namespace AmigaNet.Amos.Screens
{
    public class Screen : IGraphicElement
    {
        public const int DEFAULT_X = 128;
        public const int DEFAULT_Y = 50;
        public const int DEFAULT_WIDTH = 320;
        public const int DEFAULT_HEIGHT = 200;

        public Screen() { }

        public Pixel[] Data { get; internal set; } = Array.Empty<Pixel>();

        public IGraphicElement CurrentShape { get; internal set; }

        public List<Bob> Bobs { get; internal set; } = new List<Bob>();

        public List<Block> Blocks { get; internal set; } = new List<Block>();

        public Zone[] Zones { get; internal set; }

        /// <summary>
        /// Bumped whenever a static-terrain zone (id &gt;= 21) on this screen is
        /// added, moved or cleared. See <see cref="ScreensManager.ZoneEpoch"/>.
        /// </summary>
        public int ZoneEpoch { get; internal set; }

        public Pixel[] Palette { get; internal set; }

        public int Number { get; internal set; }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public int Colors { get; internal set; }

        public PixelMode PixelMode { get; internal set; }

        public int CurrentGraphicWritingMode { get; internal set; } = 0;

        public int CurrentFontNr { get; internal set; } = 1;

        public int X { get; internal set; }

        public int Y { get; internal set; }

        public int OffsetX { get; internal set; }

        public int OffsetY { get; internal set; }

        public int DisplayWidth { get; internal set; }

        public int DisplayHeight { get; internal set; }

        public bool IsVisible { get; internal set; } = true;

        public bool IsModified { get; set; } = false;

        /// <summary>Non-null while a FADE started on this screen is still in progress.</summary>
        public FadeState ActiveFade { get; internal set; }

    }
}
