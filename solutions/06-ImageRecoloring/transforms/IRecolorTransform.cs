using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;

namespace _06ImageRecoloring.Transforms
{
    public interface IRecolorTransform
    {
        Rgba32 Apply (Rgba32 original, HsvColor hsv);
    }
}