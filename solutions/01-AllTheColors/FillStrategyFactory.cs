using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using AllTheColors.Validator;
using AllTheColors.Utils;

namespace AllTheColors.FillStrategies
{
    public static class FillStrategyFactory
    {
        public static IImageFillStrategy? Create (string mode, int? seed)
        {
            if (mode == "trivial")
            {
                return new TrivialFillStrategy();
            }
            if (mode == "random")
            {
                return new RandomFillStrategy(seed);
            }
            if (mode == "pattern")
            {
                return new PatternFillStrategy();
            }

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

            Random rng;
            if (_seed.HasValue)
            {
                rng = new Random(_seed.Value);
            }
            else
            {
                rng = new Random();
            }

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
        public void Fill (Image<Rgba32> image)
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
                    {
                        if (colorIndex < ImageRequestValidator.AllColorsCount)
                        {
                            lastColor = ColorIndexer.ColorFromIndex(colorIndex);
                            colorIndex++;
                        }
                        image[x, y] = lastColor;
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
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
    }
}