using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Core
{
    public interface IMandalaRenderer
    {
        void Render (MandalaConfig config, Image<Rgba32> image);
    }
}