namespace AmigaNet.Types.Audio
{
    public class AmosInstrument
    {
        public string Name { get; set; } = "";
        public sbyte[] AttackSamples { get; set; } = System.Array.Empty<sbyte>();
        public sbyte[] LoopSamples { get; set; } = System.Array.Empty<sbyte>();
        public int Volume { get; set; }
        public bool HasLoop { get; set; }
    }

    public class AmosNoteEvent
    {
        public bool IsEnd;
        public bool IsNote;
        public int Period;
        public int Label;
        public int Param;
    }

    public class AmosPattern
    {
        public AmosNoteEvent[] Voice0 { get; set; } = System.Array.Empty<AmosNoteEvent>();
        public AmosNoteEvent[] Voice1 { get; set; } = System.Array.Empty<AmosNoteEvent>();
        public AmosNoteEvent[] Voice2 { get; set; } = System.Array.Empty<AmosNoteEvent>();
        public AmosNoteEvent[] Voice3 { get; set; } = System.Array.Empty<AmosNoteEvent>();
    }

    public class AmosMusicData
    {
        public AmosInstrument[] Instruments { get; set; } = System.Array.Empty<AmosInstrument>();
        public int Tempo { get; set; } = 60;
        public int[] Voice0Patterns { get; set; } = System.Array.Empty<int>();
        public int[] Voice1Patterns { get; set; } = System.Array.Empty<int>();
        public int[] Voice2Patterns { get; set; } = System.Array.Empty<int>();
        public int[] Voice3Patterns { get; set; } = System.Array.Empty<int>();
        public AmosPattern[] Patterns { get; set; } = System.Array.Empty<AmosPattern>();
    }
}
