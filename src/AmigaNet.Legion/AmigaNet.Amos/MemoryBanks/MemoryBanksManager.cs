using AmigaNet.IO.Audio.Amos;
using AmigaNet.IO.Graphics.Amos;
using AmigaNet.Types.Audio;
using AmigaNet.Types.Graphics;

namespace AmigaNet.Amos.MemoryBanks
{
    public class MemoryBanksManager
    {
        private readonly IGameEngine gameEngine;

        public MemoryBanksManager(IGameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
        }

        private readonly MemoryBank<ImageData> bank1 = new MemoryBank<ImageData>();
        //private MemoryBank<ImageData> bank2;
        //private MemoryBank<MusicData> bank3;
        private readonly MemoryBank<AudioSampleData> bank4 = new MemoryBank<AudioSampleData>();
        private readonly MemoryBank<AudioSampleData> bank5 = new MemoryBank<AudioSampleData>();

        /// <summary>SAM BANK: which of bank4/bank5 "Sam Play" currently reads from.</summary>
        private int currentSampleBank = 4;

        public MemoryBank<ImageData> Bank1 => bank1;

        public Pixel[] BobPalette { get; private set; }

        public void Load(String fileName)
        {
            Load(fileName, 1);
        }

        public void Load(String fileName, int bankNumber)
        {
            if (bankNumber == 1)
            {
                var sprites = new SpriteBanksReader().Read(fileName);
                bank1.Data.AddRange(sprites.Images.Where(s => s.Width > 0));
                BobPalette = sprites.Palette;
            }
            else if (bankNumber == 4 || bankNumber == 5)
            {
                var bank = bankNumber == 4 ? bank4 : bank5;
                bank.Data.Clear();
                bank.Data.AddRange(new SampleBanksReader().Read(fileName));
            }
        }

        public void TrackLoad(String fileName, int bankNumber)
        {
            gameEngine.LoadTrack(fileName);
        }

        /// <summary>
        /// SAM BANK n
        /// instruction: select which loaded sample bank subsequent "Sam Play"
        /// calls read from.
        /// </summary>
        public void SamBank(int bankNumber)
        {
            currentSampleBank = bankNumber;
        }

        /// <summary>
        /// SAM PLAY channel,sampleNumber
        /// instruction: play sample "sampleNumber" (1-based, per AMOS
        /// convention) from the currently selected sample bank. The channel
        /// argument is an AMOS hardware-channel bitmask in the original -
        /// not meaningful here since playback goes through MonoGame's own
        /// mixer instead of emulated Paula channels, so it's accepted but
        /// unused.
        /// </summary>
        public void PlaySample(int sampleNumber, int channel = 0)
        {
            var bank = currentSampleBank == 5 ? bank5 : bank4;
            var index = sampleNumber - 1;
            if (index < 0 || index >= bank.Data.Count) return;

            var sample = bank.Data[index];
            gameEngine.PlaySample(sample.Pcm, sample.FrequencyHz);
        }

        /// <summary>
        /// ERASE
        /// instruction: clear a single memory bank
        /// Erase bank number
        /// </summary>
        public void Erase(int bankNumber)
        {
            switch (bankNumber)
            {
                case 1: bank1.Data.Clear(); break;
                case 4: bank4.Data.Clear(); break;
                case 5: bank5.Data.Clear(); break;
                    //TODO: handle bank 2 (pictures) / bank 3 (music) if needed
            }
        }

        /// <summary>
        /// ERASE ALL
        /// instruction: clear all current memory banks
        /// Erase All
        /// </summary>
        public void EraseAll()
        {
            //TODO: bank 2 (pictures) / bank 3 (music) if needed
            bank1.Data.Clear();
            bank4.Data.Clear();
            bank5.Data.Clear();
        }

        public void Set(int bankNumber, int imageNumber, ImageData imageData)
        {
            if (bankNumber == 1)
            {
                if (Bank1.Data.Count < imageNumber)
                {
                    var diff = imageNumber - Bank1.Data.Count;
                    for (var i =0; i<diff; i++)
                    {
                        Bank1.Data.Add(null!);
                    }
                }
                Bank1.Data[imageNumber - 1] = imageData;
            }
        }

    }
}
