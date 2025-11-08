using SixLabors.ImageSharp.PixelFormats;

namespace AllTheColors.Utils
{
    public static class ColorIndexer
    {
        public static Rgba32 ColorFromIndex (int index)
        {
            byte r = (byte)((index >> 16) & 0xFF);
            byte g = (byte)((index >> 8) & 0xFF);
            byte b = (byte)(index & 0xFF);
            return new Rgba32(r, g, b);
        }
    }
}