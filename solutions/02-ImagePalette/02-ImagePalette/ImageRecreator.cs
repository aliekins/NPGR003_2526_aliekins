using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImagePalette
{
    public static class ImageRecreator
    {
        public static void SaveRecreatedImage (string fileName, Image<Rgba32> original, IReadOnlyList<Rgba32> palette)
        {
            if (palette == null || palette.Count == 0)
            {
                Console.Error.WriteLine("ERROR: Palette is empty. Cannot recreate image.");
            }

            using Image<Rgba32> output = new Image<Rgba32>(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Rgba32 src = original[x, y];
                    Rgba32 nearest = FindNearestPaletteColor(src, palette!);
                    output[x, y] = nearest;
                }
            }

            output.Save(fileName);
        }

        private static Rgba32 FindNearestPaletteColor (Rgba32 color, IReadOnlyList<Rgba32> palette)
        {
            int bestIndex = 0;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                Rgba32 p = palette[i];

                int dr = (int)color.R - p.R;
                int dg = (int)color.G - p.G;
                int db = (int)color.B - p.B;

                int dist = dr * dr + dg * dg + db * db;
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestIndex = i;
                }
            }

            return palette[bestIndex];
        }
    }
}
