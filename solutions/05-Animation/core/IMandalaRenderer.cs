using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Core
{
    public interface IMandalaRenderer
    {
        void Render (MandalaConfig config, Image<Rgba32> image, float time);
    }
}