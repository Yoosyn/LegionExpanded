using AmigaNet.Types.Graphics;

namespace AmigaNet.Amos
{
    public interface IGameEngine
    {
        //ImageData LoadIff(string fileName);

        //ImageData Load(string fileName);

        void LoadTrack(string fileName);

        bool IsTrackLoop { get; set; }

        void PlayTrack();

        void StopTrack();

        /// <summary>
        /// SAM PLAY: fire-and-forget playback of a raw mono signed 8-bit PCM
        /// sample, independent of the currently playing tracker module.
        /// </summary>
        void PlaySample(sbyte[] pcm, int frequencyHz);

        void HideCursor();
        
        void ShowCursor();

        void ChangeMouseCursor(ImageData cursorImage);

        void WaitVbl();

        int GetKeyPressed();
        
        String GetInkey();

        int GetScancode();

        void ClearKey();

        int GetMousePosX();

        int GetMousePosY();

        int GetMouseKey();

        int GetMouseClick();
    }
}
