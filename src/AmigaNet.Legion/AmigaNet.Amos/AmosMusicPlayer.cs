using AmigaNet.Types.Audio;

namespace AmigaNet.Amos
{
    public class AmosMusicPlayer
    {
        private AmosMusicData music;
        private int sampleRate = 48000;
        private int channels = 2;
        private bool loop = true;
        private bool playing;

        private const double PaulaClock = 3546895.0;
        private const double BasePeriod = 428.0;
        private double ticksPerSecond = 50.0;

        private struct VoiceState
        {
            public int CurrentPatternIdx;
            public int CurrentPatternListIdx;
            public int[] PatternList;
            public int NoteListPos;
            public AmosNoteEvent[] NoteList;
            public int CurrentInstrument;
            public int CurrentVolume;
            public int DelayTicks;
            public sbyte[] CurrentSample;
            public sbyte[] LoopSample;
            public int SamplePos;
            public double SampleStep;
            public int AttackLen;
            public bool InAttack;
            public int InstrumentVolume;
            public int JumpTarget;
            public bool HasJump;
        }

        private VoiceState[] voices;

        public AmosMusicPlayer(AmosMusicData music)
        {
            this.music = music;
            voices = new VoiceState[4];
            InitVoices();
        }

        public bool IsLoop { get => loop; set => loop = value; }
        public bool IsPlaying => playing;

        private void InitVoices()
        {
            for (var v = 0; v < 4; v++)
            {
                voices[v] = new VoiceState();
            }
            ResetVoices();
        }

        private void ResetVoices()
        {
            var patLists = new[] { music.Voice0Patterns, music.Voice1Patterns,
                                   music.Voice2Patterns, music.Voice3Patterns };
            for (var v = 0; v < 4; v++)
            {
                voices[v].PatternList = patLists[v];
                voices[v].CurrentPatternListIdx = 0;
                voices[v].CurrentPatternIdx = -1;
                voices[v].NoteListPos = 0;
                voices[v].NoteList = Array.Empty<AmosNoteEvent>();
                voices[v].CurrentInstrument = 0;
                voices[v].CurrentVolume = 64;
                voices[v].DelayTicks = 0;
                voices[v].CurrentSample = Array.Empty<sbyte>();
                voices[v].LoopSample = Array.Empty<sbyte>();
                voices[v].SamplePos = 0;
                voices[v].SampleStep = 1.0;
                voices[v].AttackLen = 0;
                voices[v].InAttack = false;
                voices[v].InstrumentVolume = 64;
                voices[v].HasJump = false;
                voices[v].JumpTarget = -1;

                if (patLists[v].Length > 0)
                    AdvanceToPattern(v);
            }
        }

        private void AdvanceToPattern(int v)
        {
            var vs = voices[v];
            if (vs.PatternList.Length == 0) return;

            var patIdx = vs.PatternList[vs.CurrentPatternListIdx];
            if (patIdx < 0 || patIdx >= music.Patterns.Length)
            {
                vs.CurrentPatternIdx = -1;
                vs.NoteList = Array.Empty<AmosNoteEvent>();
                return;
            }

            vs.CurrentPatternIdx = patIdx;
            var pattern = music.Patterns[patIdx];
            vs.NoteList = v switch
            {
                0 => pattern.Voice0,
                1 => pattern.Voice1,
                2 => pattern.Voice2,
                _ => pattern.Voice3
            };
            vs.NoteListPos = 0;
        }

        public void Start()
        {
            playing = true;
            ResetVoices();
        }

        public void Stop()
        {
            playing = false;
        }

        private int tickCounter = 0;
        private int samplesPerTick;

        public int Render(byte[] buffer, int offset, int byteCount)
        {
            if (!playing || music == null)
            {
                Array.Clear(buffer, offset, byteCount);
                return byteCount;
            }

            samplesPerTick = (int)(sampleRate / ticksPerSecond);

            var frameSize = channels * 2;
            var framesWanted = byteCount / frameSize;
            var outPos = offset;
            var remaining = framesWanted;

            while (remaining > 0)
            {
                var chunk = Math.Min(remaining, samplesPerTick - tickCounter);
                if (chunk > 0)
                {
                    RenderFrames(buffer, outPos, chunk);
                    outPos += chunk * frameSize;
                    remaining -= chunk;
                    tickCounter += chunk;
                }

                if (tickCounter >= samplesPerTick)
                {
                    tickCounter = 0;
                    ProcessTick();
                }
            }

            return byteCount;
        }

        private void RenderFrames(byte[] buffer, int offset, int frameCount)
        {
            for (var i = 0; i < frameCount; i++)
            {
                var mixed = 0f;

                for (var v = 0; v < 4; v++)
                {
                    var vs = voices[v];
                    if (vs.CurrentSample.Length == 0) continue;

                    var sample = vs.CurrentSample;
                    double pos = vs.SamplePos;
                    var step = vs.SampleStep;
                    var vol = vs.CurrentVolume * vs.InstrumentVolume / 127f;

                    if (pos < sample.Length && pos >= 0)
                    {
                        var sampleIdx = (int)pos;
                        if (sampleIdx < sample.Length)
                        {
                            mixed += sample[sampleIdx] / 128f * vol;
                        }
                    }

                    pos += step;
                    vs.SamplePos = (int)pos;

                    if (vs.InAttack && pos >= vs.AttackLen)
                    {
                        if (vs.LoopSample.Length > 0)
                        {
                            vs.CurrentSample = vs.LoopSample;
                            vs.SamplePos = 0;
                            vs.InAttack = false;
                        }
                        else
                        {
                            vs.CurrentSample = Array.Empty<sbyte>();
                        }
                    }
                    else if (!vs.InAttack && pos >= vs.CurrentSample.Length)
                    {
                        if (loop && vs.LoopSample.Length > 0)
                        {
                            vs.SamplePos = 0;
                        }
                        else
                        {
                            vs.CurrentSample = Array.Empty<sbyte>();
                        }
                    }
                }

                mixed = Math.Clamp(mixed, -1f, 1f);
                var val = (short)(mixed * 32767);
                var idx = offset + i * 4;
                buffer[idx] = (byte)(val & 0xFF);
                buffer[idx + 1] = (byte)((val >> 8) & 0xFF);
                buffer[idx + 2] = (byte)(val & 0xFF);
                buffer[idx + 3] = (byte)((val >> 8) & 0xFF);
            }
        }

        private void ProcessTick()
        {
            for (var v = 0; v < 4; v++)
            {
                var vs = voices[v];
                if (vs.PatternList.Length == 0) continue;

                if (vs.DelayTicks > 0)
                {
                    vs.DelayTicks--;
                    continue;
                }

                var events = vs.NoteList;
                if (events.Length == 0) continue;

                var processed = false;
                while (!processed && vs.NoteListPos < events.Length)
                {
                    var evt = events[vs.NoteListPos];
                    vs.NoteListPos++;

                    if (evt.IsNote)
                    {
                        PlayNote(v, evt.Period);
                        processed = true;
                    }
                    else
                    {
                        switch (evt.Label)
                        {
                            case 3:
                                vs.CurrentVolume = evt.Param;
                                break;
                            case 5:
                                break;
                            case 8:
                                ticksPerSecond = 50.0 + evt.Param;
                                break;
                            case 9:
                                vs.CurrentInstrument = evt.Param;
                                break;
                            case 14:
                                break;
                            case 15:
                                break;
                            case 16:
                                vs.DelayTicks = evt.Param;
                                processed = true;
                                break;
                            case 17:
                                vs.HasJump = true;
                                vs.JumpTarget = evt.Param;
                                break;
                        }
                    }
                }

                if (vs.NoteListPos >= events.Length)
                {
                    var nextIdx = vs.CurrentPatternListIdx + 1;

                    if (vs.HasJump && vs.JumpTarget >= 0)
                    {
                        nextIdx = vs.JumpTarget;
                        vs.HasJump = false;
                    }

                    if (nextIdx < vs.PatternList.Length)
                    {
                        vs.CurrentPatternListIdx = nextIdx;
                        AdvanceToPattern(v);
                    }
                    else if (loop)
                    {
                        vs.CurrentPatternListIdx = 0;
                        AdvanceToPattern(v);
                    }
                    else
                    {
                        vs.PatternList = Array.Empty<int>();
                    }
                }
            }
        }

        private void PlayNote(int voiceIdx, int period)
        {
            var vs = voices[voiceIdx];
            if (period <= 0 || vs.CurrentInstrument >= music.Instruments.Length) return;

            var instr = music.Instruments[vs.CurrentInstrument];
            vs.InstrumentVolume = instr.Volume;

            if (instr.AttackSamples.Length > 0)
            {
                vs.CurrentSample = instr.AttackSamples;
                vs.AttackLen = instr.AttackSamples.Length;
                vs.InAttack = true;
                vs.LoopSample = instr.HasLoop ? instr.LoopSamples : instr.AttackSamples;
            }
            else if (instr.LoopSamples.Length > 0)
            {
                vs.CurrentSample = instr.LoopSamples;
                vs.AttackLen = 0;
                vs.InAttack = false;
                vs.LoopSample = instr.LoopSamples;
            }
            else
            {
                vs.CurrentSample = Array.Empty<sbyte>();
                return;
            }

            vs.SamplePos = 0;
            var freq = PaulaClock / period;
            var baseFreq = PaulaClock / BasePeriod;
            vs.SampleStep = freq / baseFreq;
        }
    }
}
