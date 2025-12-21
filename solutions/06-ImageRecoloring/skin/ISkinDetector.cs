using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;

namespace _06ImageRecoloring.Skin
{
    public interface ISkinDetector
    {
        double SkinProbability (Rgba32 px, HsvColor hsv);
    }
}