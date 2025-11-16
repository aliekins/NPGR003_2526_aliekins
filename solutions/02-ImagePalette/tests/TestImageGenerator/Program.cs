using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class TestImageGenerator
{
    static void Main ()
    {
        GenerateRed("red.png");
        GenerateRedGreen("red_green.png");
        GenerateRgbStripes("rgb_stripes.png");
        GenerateGradient("gradient.png");
        Console.WriteLine("Test images generated.");
    }

    static void GenerateRed (string fileName)
    {
        using Image<Rgba32> img = new Image<Rgba32>(200, 100);
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgba32(255, 0, 0);
                }
            }
        });
        img.Save(fileName);
    }

    static void GenerateRedGreen (string fileName)
    {
        int width = 200;
        int height = 100;
        using Image<Rgba32> img = new Image<Rgba32>(width, height);
        int half = width / 2;

        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    if (x < half)
                    {
                        row[x] = new Rgba32(255, 0, 0);
                    }
                    else
                    {
                        row[x] = new Rgba32(0, 255, 0);
                    }
                }
            }
        });

        img.Save(fileName);
    }

    static void GenerateRgbStripes (string fileName)
    {
        int width = 300;
        int height = 100;
        using Image<Rgba32> img = new Image<Rgba32>(width, height);
        int stripeWidth = width / 3;

        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    if (x < stripeWidth)
                        row[x] = new Rgba32(255, 0, 0);
                    else if (x < 2 * stripeWidth)
                        row[x] = new Rgba32(0, 255, 0);
                    else
                        row[x] = new Rgba32(0, 0, 255);
                }
            }
        });

        img.Save(fileName);
    }

    static void GenerateGradient (string fileName)
    {
        int width = 256;
        int height = 50;
        using Image<Rgba32> img = new Image<Rgba32>(width, height);

        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    byte v = (byte)x; // 0..255 across the width
                    row[x] = new Rgba32(v, v, v);
                }
            }
        });

        img.Save(fileName);
    }
}
