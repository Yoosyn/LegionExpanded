using System;

namespace AmigaNet.Types.Audio
{
    /// <summary>
    /// A single sample from an AMOS "Samples"/"HSamples" memory bank: mono,
    /// signed 8-bit PCM (two's complement), as used by AMOS's "Sam Play".
    /// </summary>
    public class AudioSampleData
    {
        public string Name { get; set; } = "";

        public int FrequencyHz { get; set; }

        public sbyte[] Pcm { get; set; } = Array.Empty<sbyte>();
    }
}
