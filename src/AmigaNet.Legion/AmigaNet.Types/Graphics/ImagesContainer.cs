using System;

namespace AmigaNet.Types.Graphics
{
    public class ImagesContainer
    {
        public List<ImageData> Images { get; set; } = new List<ImageData>();

        public Pixel[] Palette { get; set; } = Array.Empty<Pixel>();
    }
}
