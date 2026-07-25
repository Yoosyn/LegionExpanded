using AmigaNet.Types.Audio;
using System.Text;

namespace AmigaNet.IO.Audio.Amos
{
    /// <summary>
    /// Reads AMOS "Samples"/"HSamples" memory bank files (the format produced
    /// by AMOS's "Sample Bank Maker" / saved via "Save Bank n" for a sample
    /// bank, and loaded in-game via "Load A$,n").
    ///
    /// Layout (all multi-byte integers big-endian), reverse-engineered and
    /// verified against original/legion/dane/sound and
    /// original/legion/dane/potwory/szkielet.snd:
    ///   0    4   "AmBk" magic
    ///   4    2   bank number
    ///   6    2   memory type (0 = chip)
    ///   8    4   bank length, high bit(s) set = chip memory flag
    ///   12   8   bank name, space-padded ("Samples ", "HSamples")
    ///   20   2   sample count N
    ///   22   4*N offset table, big-endian, EACH OFFSET IS RELATIVE TO FILE
    ///            OFFSET 20 (the position of the count field just above)
    ///   ...  per sample, at header(20) + offset[i]:
    ///          8   sample name, space-padded
    ///          2   frequency in Hz
    ///          4   sample length (unreliable per AMOS format docs - the
    ///              true length is derived from consecutive offset-table
    ///              entries instead, which is what this reader does)
    ///          N   signed 8-bit PCM data, running up to the next sample's
    ///              offset (or end of file for the last sample)
    /// </summary>
    public class SampleBanksReader
    {
        private const string AmBkMagic = "AmBk";
        private const int HeaderSize = 20;
        private const int SampleSubHeaderSize = 14; // 8 name + 2 frequency + 4 length

        public string Name => "AMOS Sample Bank File";

        public List<AudioSampleData> Read(string fileName)
        {
            var samples = new List<AudioSampleData>();
            var bytes = File.ReadAllBytes(fileName);
            var reader = new BytesReader(bytes);

            var magic = Encoding.ASCII.GetString(reader.Read(4));
            if (magic != AmBkMagic)
            {
                return samples;
            }

            reader.Seek(HeaderSize);
            var count = reader.Read16();
            if (count <= 0)
            {
                return samples;
            }

            var offsets = new int[count];
            for (var i = 0; i < count; i++)
            {
                offsets[i] = reader.Read32();
            }

            for (var i = 0; i < count; i++)
            {
                var dataStart = HeaderSize + offsets[i];
                var dataEnd = (i + 1 < count) ? HeaderSize + offsets[i + 1] : bytes.Length;

                reader.Seek(dataStart);
                var name = reader.ReadText(8).Trim(' ', '\0');
                var frequency = reader.Read16();
                reader.Read(4); // length field - unreliable, using offset-table-derived length instead

                var pcmLength = Math.Max(0, dataEnd - dataStart - SampleSubHeaderSize);
                var pcmBytes = reader.Read(pcmLength);
                var pcm = new sbyte[pcmBytes.Length];
                Buffer.BlockCopy(pcmBytes, 0, pcm, 0, pcmBytes.Length);

                samples.Add(new AudioSampleData
                {
                    Name = name,
                    FrequencyHz = frequency,
                    Pcm = pcm
                });
            }

            return samples;
        }
    }
}
