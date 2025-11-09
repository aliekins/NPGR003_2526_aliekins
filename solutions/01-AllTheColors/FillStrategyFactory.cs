using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using AllTheColors.Validator;
using AllTheColors.Utils;

namespace AllTheColors.FillStrategies
{
    public static class FillStrategyFactory
    {
        public static IImageFillStrategy? Create (Options o)
        {
            string mode = o.Mode ?? "trivial";
            mode = mode.ToLowerInvariant();

            if (mode == "trivial")
                return new TrivialFillStrategy();

            if (mode == "random")
                return new RandomFillStrategy(o.Seed);

            if (mode == "pattern")
                return new PatternFillStrategy(o.PatternStyle, o.PatternBlockSize);

            if (mode == "mandala")
            {
                int cx = (o.MandalaCenterX < 0) ? (o.Width / 2) : o.MandalaCenterX;
                int cy = (o.MandalaCenterY < 0) ? (o.Height / 2) : o.MandalaCenterY;
                return new MandalaFillStrategy(o.MandalaArms, cx, cy);
            }

            if (mode == "ornament")
                return new OrnamentFillStrategy(o.OrnamentDepth, o.OrnamentMinSize);

            return null;
        }
    }

    public class TrivialFillStrategy : IImageFillStrategy
    {
        public void Fill (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (colorIndex < ImageRequestValidator.AllColorsCount)
                    {
                        lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                        colorIndex++;
                    }

                    image[x, y] = lastColor;
                }
            }
        }
    }
    public class RandomFillStrategy : IImageFillStrategy
    {
        private readonly int? _seed;

        public RandomFillStrategy (int? seed)
        {
            _seed = seed;
        }

        public void Fill (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] order = new int[ImageRequestValidator.AllColorsCount];
            for (int i = 0; i < ImageRequestValidator.AllColorsCount; i++)
            {
                order[i] = i;
            }

            Random rng = _seed.HasValue ? new Random(_seed.Value) : new Random();

            // shuffle
            for (int i = ImageRequestValidator.AllColorsCount - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = order[i];
                order[i] = order[j];
                order[j] = tmp;
            }

            int index = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (index < ImageRequestValidator.AllColorsCount)
                    {
                        lastColor = ColorIndexer.ColorFromIndex(order[index]);
                        index++;
                    }

                    image[x, y] = lastColor;
                }
            }
        }
    }

    public class PatternFillStrategy : IImageFillStrategy
    {
        private readonly string _style;
        private readonly int _blockSize;

        public PatternFillStrategy (string style, int blockSize)
        {
            _style = (style ?? "snake").ToLowerInvariant();
            _blockSize = Math.Max(4, blockSize);
        }

        public void Fill (Image<Rgba32> image)
        {
            switch (_style)
            {
                case "diagonal":
                    FillDiagonal(image);
                    break;
                case "spiral":
                    FillSpiral(image);
                    break;
                case "blocks":
                    FillBlocks(image, _blockSize);
                    break;
                default:
                    FillSnake(image);
                    break;
            }
        }

        // original serpentine 
        private void FillSnake (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                if ((y % 2) == 0)
                {
                    for (int x = 0; x < width; x++)
                        WriteNext(image, x, y, ref colorIndex, ref lastColor);
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                        WriteNext(image, x, y, ref colorIndex, ref lastColor);
                }
            }
        }

        // diagonal stripes
        private void FillDiagonal (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;
            int maxK = (width - 1) + (height - 1);

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            for (int k = 0; k <= maxK; k++)
            {
                int xStart = Math.Max(0, k - (height - 1));
                int xEnd = Math.Min(width - 1, k);

                // zigzag per diagonal
                bool forward = (k % 2 == 0);

                if (forward)
                {
                    for (int x = xStart; x <= xEnd; x++)
                    {
                        int y = k - x;
                        WriteNext(image, x, y, ref colorIndex, ref lastColor);
                    }
                }
                else
                {
                    for (int x = xEnd; x >= xStart; x--)
                    {
                        int y = k - x;
                        WriteNext(image, x, y, ref colorIndex, ref lastColor);
                    }
                }
            }
        }

        // spiral from outer border inward
        private void FillSpiral (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            int left = 0;
            int right = width - 1;
            int top = 0;
            int bottom = height - 1;

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            while (left <= right && top <= bottom)
            {
                // top
                for (int x = left; x <= right; x++)
                    WriteNext(image, x, top, ref colorIndex, ref lastColor);
                top++;

                // right
                for (int y = top; y <= bottom; y++)
                    WriteNext(image, right, y, ref colorIndex, ref lastColor);
                right--;

                if (top > bottom || left > right)
                    break;

                // bottom
                for (int x = right; x >= left; x--)
                    WriteNext(image, x, bottom, ref colorIndex, ref lastColor);
                bottom--;

                // left
                for (int y = bottom; y >= top; y--)
                    WriteNext(image, left, y, ref colorIndex, ref lastColor);
                left++;
            }
        }

        // tiled blocks
        private void FillBlocks (Image<Rgba32> image, int blockSize)
        {
            int width = image.Width;
            int height = image.Height;

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            for (int by = 0; by < height; by += blockSize)
            {
                for (int bx = 0; bx < width; bx += blockSize)
                {
                    int bw = Math.Min(blockSize, width - bx);
                    int bh = Math.Min(blockSize, height - by);

                    bool serp = (((bx / blockSize) + (by / blockSize)) % 2 == 0);

                    if (serp)
                    {
                        for (int yy = 0; yy < bh; yy++)
                        {
                            if ((yy % 2) == 0)
                            {
                                for (int xx = 0; xx < bw; xx++)
                                    WriteNext(image, bx + xx, by + yy, ref colorIndex, ref lastColor);
                            }
                            else
                            {
                                for (int xx = bw - 1; xx >= 0; xx--)
                                    WriteNext(image, bx + xx, by + yy, ref colorIndex, ref lastColor);
                            }
                        }
                    }
                    else
                    {
                        for (int yy = bh - 1; yy >= 0; yy--)
                        {
                            for (int xx = 0; xx < bw; xx++)
                                WriteNext(image, bx + xx, by + yy, ref colorIndex, ref lastColor);
                        }
                    }
                }
            }
        }

        private void WriteNext (Image<Rgba32> image, int x, int y, ref int colorIndex, ref Rgba32 lastColor)
        {
            if (colorIndex < ImageRequestValidator.AllColorsCount)
            {
                lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                colorIndex++;
            }

            image[x, y] = lastColor;
        }
    }


    public class MandalaFillStrategy : IImageFillStrategy
    {
        private readonly int _arms;
        private readonly int _cx;
        private readonly int _cy;

        public MandalaFillStrategy (int arms, int cx, int cy)
        {
            _arms = Math.Max(1, arms);
            _cx = cx;
            _cy = cy;
        }

        public void Fill (Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            bool[,] filled = new bool[width, height];

            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            double maxRadius = Math.Sqrt(width * width + height * height);

            double full = 2.0 * Math.PI;
            double sectorAngle = full / _arms;

            for (int r = 0; r <= (int)maxRadius; r++)
            {
                int steps = Math.Max(_arms * 8, (int)(r * 0.35) + 1);

                for (int s = 0; s < steps; s++)
                {
                    double angle = full * s / steps;

                    int arm = (int)(angle / sectorAngle);
                    double local = angle - arm * sectorAngle;

                    double normalized = (local / sectorAngle) - 0.5;
                    double bend = normalized * 0.12;
                    double finalAngle = angle - bend;

                    int x = _cx + (int)(r * Math.Cos(finalAngle));
                    int y = _cy + (int)(r * Math.Sin(finalAngle));

                    StampPixel(image, filled, x, y, ref colorIndex, ref lastColor);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!filled[x, y])
                    {
                        if (colorIndex < ImageRequestValidator.AllColorsCount)
                        {
                            lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                            colorIndex++;
                        }
                        image[x, y] = lastColor;
                        filled[x, y] = true;
                    }
                }
            }
        }

        private void StampPixel (
            Image<Rgba32> image,
            bool[,] filled,
            int x,
            int y,
            ref int colorIndex,
            ref Rgba32 lastColor)
        {
            int width = image.Width;
            int height = image.Height;

            for (int dy = 0; dy < 2; dy++)
            {
                for (int dx = 0; dx < 2; dx++)
                {
                    int xx = x + dx;
                    int yy = y + dy;

                    if (xx >= 0 && xx < width && yy >= 0 && yy < height && !filled[xx, yy])
                    {
                        if (colorIndex < ImageRequestValidator.AllColorsCount)
                        {
                            lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                            colorIndex++;
                        }
                        image[xx, yy] = lastColor;
                        filled[xx, yy] = true;
                    }
                }
            }
        }
    }

    public class OrnamentFillStrategy : IImageFillStrategy
    {
        private readonly int _depth;
        private readonly int _minSize;

        public OrnamentFillStrategy (int depth, int minSize)
        {
            _depth = Math.Max(1, depth);
            _minSize = Math.Max(8, minSize);
        }

        public void Fill (Image<Rgba32> image)
        {
            int colorIndex = 0;
            Rgba32 lastColor = new Rgba32(0, 0, 0);

            FillRegion(image, 0, 0, image.Width, image.Height, _depth, ref colorIndex, ref lastColor);
        }

        private void FillRegion (Image<Rgba32> image, int x, int y, int w, int h, int depth, ref int colorIndex, ref Rgba32 lastColor)
        {
            DrawBorder(image, x, y, w, h, ref colorIndex, ref lastColor);

            if (depth <= 1 || w <= _minSize || h <= _minSize)
            {
                for (int yy = y + 1; yy < y + h - 1; yy++)
                {
                    for (int xx = x + 1; xx < x + w - 1; xx++)
                    {
                        if (colorIndex < ImageRequestValidator.AllColorsCount)
                        {
                            lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                            colorIndex++;
                        }
                        image[xx, yy] = lastColor;
                    }
                }
                return;
            }

            int w2 = w / 2;
            int h2 = h / 2;

            FillRegion(image, x + 1, y + 1, w2 - 1, h2 - 1, depth - 1, ref colorIndex, ref lastColor);
            FillRegion(image, x + w2, y + 1, w - w2 - 1, h2 - 1, depth - 1, ref colorIndex, ref lastColor);
            FillRegion(image, x + 1, y + h2, w2 - 1, h - h2 - 1, depth - 1, ref colorIndex, ref lastColor);
            FillRegion(image, x + w2, y + h2, w - w2 - 1, h - h2 - 1, depth - 1, ref colorIndex, ref lastColor);
        }

        private void DrawBorder (Image<Rgba32> image, int x, int y, int w, int h, ref int colorIndex, ref Rgba32 lastColor)
        {
            int x2 = x + w - 1;
            int y2 = y + h - 1;

            for (int xx = x; xx <= x2; xx++)
            {
                if (colorIndex < ImageRequestValidator.AllColorsCount)
                {
                    lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                    colorIndex++;
                }
                image[xx, y] = lastColor;

                if (y2 != y)
                {
                    if (colorIndex < ImageRequestValidator.AllColorsCount)
                    {
                        lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                        colorIndex++;
                    }
                    image[xx, y2] = lastColor;
                }
            }

            for (int yy = y + 1; yy <= y2 - 1; yy++)
            {
                if (colorIndex < ImageRequestValidator.AllColorsCount)
                {
                    lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                    colorIndex++;
                }
                image[x, yy] = lastColor;

                if (x2 != x)
                {
                    if (colorIndex < ImageRequestValidator.AllColorsCount)
                    {
                        lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                        colorIndex++;
                    }
                    image[x2, yy] = lastColor;
                }
            }
        }
    }
}