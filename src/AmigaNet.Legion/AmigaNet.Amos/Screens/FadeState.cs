using AmigaNet.Types.Graphics;

namespace AmigaNet.Amos.Screens
{
    /// <summary>
    /// Tracks an in-progress AMOS FADE on a screen's palette. Advanced one
    /// step every <see cref="Speed"/> VBLs (see ScreensManager.AdvanceFades),
    /// mirroring AMOS's non-blocking, VBL-driven FADE command.
    /// </summary>
    public class FadeState
    {
        /// <summary>Palette snapshot captured when the fade started.</summary>
        public Pixel[] From;

        /// <summary>Palette the fade is moving towards. A null entry means "leave this index unchanged".</summary>
        public Pixel[] To;

        /// <summary>Number of VBLs between each step (AMOS FADE's speed argument).</summary>
        public int Speed;

        /// <summary>Total number of steps to reach the target (approximation of AMOS's per-nibble stepping).</summary>
        public int TotalSteps;

        public int CurrentStep;

        public int TicksSinceStep;
    }
}
