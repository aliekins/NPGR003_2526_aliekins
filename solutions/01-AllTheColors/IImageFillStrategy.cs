using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AllTheColors.FillStrategies
{
    public interface IImageFillStrategy
    {
        void Fill (Image<Rgba32> image);
    }
}