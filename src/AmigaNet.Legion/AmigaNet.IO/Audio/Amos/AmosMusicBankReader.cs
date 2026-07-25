using AmigaNet.Types.Audio;
using System.Text;

namespace AmigaNet.IO.Audio.Amos
{
    public class AmosMusicBankReader
    {
        public AmosMusicData Read(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            return Read(data);
        }

        public AmosMusicData Read(byte[] data)
        {
            var reader = new BytesReader(data);

            reader.Seek(20);

            var instrOff = reader.Read32();
            var musicsOff = reader.Read32();
            var patternsOff = reader.Read32();
            reader.Read32();

            var instruments = ReadInstruments(reader, 20 + instrOff);
            var (tempo, v0p, v1p, v2p, v3p) = ReadMusics(reader, 20 + musicsOff);
            var patterns = ReadPatterns(reader, 20 + patternsOff);

            return new AmosMusicData
            {
                Instruments = instruments,
                Tempo = tempo,
                Voice0Patterns = v0p,
                Voice1Patterns = v1p,
                Voice2Patterns = v2p,
                Voice3Patterns = v3p,
                Patterns = patterns
            };
        }

        private AmosInstrument[] ReadInstruments(BytesReader reader, int sectionOff)
        {
            reader.Seek(sectionOff);
            var count = (ushort)reader.Read16();
            var result = new AmosInstrument[count];

            for (var i = 0; i < count; i++)
            {
                var descPos = sectionOff + 2 + i * 32;

                reader.Seek(descPos);
                var attackOff = reader.Read32();
                var loopOff = reader.Read32();
                var attackLenWords = (ushort)reader.Read16();
                var loopLenWords = (ushort)reader.Read16();
                var volume = (ushort)reader.Read16();
                var totalLenWords = (ushort)reader.Read16();
                var nameBytes = reader.Read(16);
                var name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');

                var attackLen = attackLenWords * 2;
                var loopLen = loopLenWords * 2;

                reader.Seek(sectionOff + attackOff);
                var attackSamples = ReadPcm(reader, attackLen);

                reader.Seek(sectionOff + loopOff);
                var loopSamples = ReadPcm(reader, loopLen);

                result[i] = new AmosInstrument
                {
                    Name = name,
                    AttackSamples = attackSamples,
                    LoopSamples = loopSamples,
                    Volume = volume,
                    HasLoop = loopLen > 0 && loopOff != attackOff
                };
            }

            return result;
        }

        private (int tempo, int[] v0, int[] v1, int[] v2, int[] v3) ReadMusics(
            BytesReader reader, int sectionOff)
        {
            reader.Seek(sectionOff);
            var numMusics = (ushort)reader.Read16();

            var musicOffsets = new int[numMusics];
            for (var i = 0; i < numMusics; i++)
                musicOffsets[i] = reader.Read32();

            if (numMusics == 0)
                return (60, Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>());

            var musicPos = sectionOff + musicOffsets[0];

            reader.Seek(musicPos);
            var tempo = (ushort)reader.Read16();

            var v0Off = (ushort)reader.Read16();
            var v1Off = (ushort)reader.Read16();
            var v2Off = (ushort)reader.Read16();
            var v3Off = (ushort)reader.Read16();
            reader.Read16();

            var v0 = v0Off > 0 ? ReadPatternList(reader, musicPos + v0Off) : Array.Empty<int>();
            var v1 = v1Off > 0 ? ReadPatternList(reader, musicPos + v1Off) : Array.Empty<int>();
            var v2 = v2Off > 0 ? ReadPatternList(reader, musicPos + v2Off) : Array.Empty<int>();
            var v3 = v3Off > 0 ? ReadPatternList(reader, musicPos + v3Off) : Array.Empty<int>();

            return (tempo, v0, v1, v2, v3);
        }

        private int[] ReadPatternList(BytesReader reader, int offset)
        {
            reader.Seek(offset);
            var list = new List<int>();
            while (true)
            {
                var val = (ushort)reader.Read16();
                if (val == 0xFFFF || val == 0xFFFE)
                    break;
                list.Add(val);
            }
            return list.ToArray();
        }

        private AmosPattern[] ReadPatterns(BytesReader reader, int sectionOff)
        {
            reader.Seek(sectionOff);
            var numPatterns = (ushort)reader.Read16();
            var result = new AmosPattern[numPatterns];

            var patOffsets = new int[numPatterns * 4];
            for (var i = 0; i < numPatterns; i++)
            {
                for (var v = 0; v < 4; v++)
                    patOffsets[i * 4 + v] = (ushort)reader.Read16();
            }

            for (var i = 0; i < numPatterns; i++)
            {
                result[i] = new AmosPattern
                {
                    Voice0 = patOffsets[i * 4 + 0] > 0 ? ReadNoteList(reader, sectionOff + patOffsets[i * 4 + 0]) : Array.Empty<AmosNoteEvent>(),
                    Voice1 = patOffsets[i * 4 + 1] > 0 ? ReadNoteList(reader, sectionOff + patOffsets[i * 4 + 1]) : Array.Empty<AmosNoteEvent>(),
                    Voice2 = patOffsets[i * 4 + 2] > 0 ? ReadNoteList(reader, sectionOff + patOffsets[i * 4 + 2]) : Array.Empty<AmosNoteEvent>(),
                    Voice3 = patOffsets[i * 4 + 3] > 0 ? ReadNoteList(reader, sectionOff + patOffsets[i * 4 + 3]) : Array.Empty<AmosNoteEvent>(),
                };
            }

            return result;
        }

        private AmosNoteEvent[] ReadNoteList(BytesReader reader, int offset)
        {
            reader.Seek(offset);
            var list = new List<AmosNoteEvent>();
            while (true)
            {
                var word = (ushort)reader.Read16();

                if ((word & 0x8000) != 0)
                {
                    var label = (word >> 8) & 0x7F;
                    var param = word & 0xFF;

                    if (label == 0)
                        break;

                    list.Add(new AmosNoteEvent
                    {
                        Label = label,
                        Param = param
                    });
                }
                else
                {
                    var period = word & 0x3FFF;
                    list.Add(new AmosNoteEvent
                    {
                        IsNote = true,
                        Period = period
                    });
                }
            }
            return list.ToArray();
        }

        private sbyte[] ReadPcm(BytesReader reader, int length)
        {
            if (length <= 0) return Array.Empty<sbyte>();
            var raw = reader.Read(length);
            var pcm = new sbyte[raw.Length];
            Buffer.BlockCopy(raw, 0, pcm, 0, raw.Length);
            return pcm;
        }
    }
}
