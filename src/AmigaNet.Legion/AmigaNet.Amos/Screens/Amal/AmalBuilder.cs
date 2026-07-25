namespace AmigaNet.Amos.Screens.Amal
{
    public class AmalBuilder
    {
        private List<AmalInstruction> instructions = new List<AmalInstruction>();

        public AmalBuilder Move(int horizontal, int vertical, int step)
        {
            instructions.Add(new AmalMove { Horizontal = horizontal, Vertical = vertical, Step = step });
            return this;
        }

        public AmalBuilder Jump(String label)
        {
            instructions.Add(new AmalJump { Label = label });
            return this;
        }

        public AmalBuilder Label(String name)
        {
            instructions.Add(new AmalLabel { Name = name });
            return this;
        }

        /// <summary>
        /// AMOS AMAL "Anim times,(image,delay)(image,delay)...": cycles
        /// through a list of images, holding each for "delay" VBLs. A times
        /// value of 0 repeats the sequence indefinitely (until AMAL OFF),
        /// matching how the intro's sword animation is used.
        /// </summary>
        public AmalBuilder Anim(int times, params (int Image, int Delay)[] frames)
        {
            var anim = new AmalAnim { Times = times };
            foreach (var frame in frames)
            {
                anim.Images.Add(new AmalAnimImageDelay { Image = frame.Image, Delay = frame.Delay });
            }
            instructions.Add(anim);
            return this;
        }

        public List<AmalInstruction> Compile()
        {
            return instructions;
        }
    }
}
