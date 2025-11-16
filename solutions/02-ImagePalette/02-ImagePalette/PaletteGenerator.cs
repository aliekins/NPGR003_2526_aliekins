using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImagePalette
{
    public static class PaletteGenerator
    {
        public static IReadOnlyList<Rgba32> GeneratePalette (Image<Rgba32> image, int requestedColorCount)
        {
            int maxDepth = 8;
            int maxColors = requestedColorCount * 4;

            if (maxColors < 32)
            {
                maxColors = 32;
            }
            else if (maxColors > 4096)
            {
                maxColors = 4096;
            }

            var quantizer = new OctreeQuantizer(maxDepth, maxColors);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> rowSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < rowSpan.Length; x++)
                    {
                        quantizer.AddColor(rowSpan[x]);
                    }
                }
            });

            var pixelCounts = new List<int>();
            List<Rgba32> rawPalette = quantizer.GetPalette(pixelCounts);

            if (rawPalette.Count == 0)
            {
                return Array.Empty<Rgba32>();
            }

            List<Rgba32> finalPalette;

            if (rawPalette.Count <= requestedColorCount)
            {
                finalPalette = new List<Rgba32>(rawPalette);
            }
            else if (requestedColorCount == 1)
            {
                finalPalette = new List<Rgba32>(1)
                {
                    ComputeWeightedAverage(rawPalette, pixelCounts)
                };
            }
            else
            {
                finalPalette = SelectRepresentativeColors(rawPalette, pixelCounts, requestedColorCount);
            }

            var ordered = OrderPalette(finalPalette);
            return ordered;

        }
        private static Rgba32 ComputeWeightedAverage (List<Rgba32> colors, List<int> counts)
        {
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            long total = 0;

            for (int i = 0; i < colors.Count && i < counts.Count; i++)
            {
                int c = counts[i];
                sumR += (long)colors[i].R * c;
                sumG += (long)colors[i].G * c;
                sumB += (long)colors[i].B * c;
                total += c;
            }

            if (total <= 0)
            {
                return colors[0];
            }

            byte r = (byte)(sumR / total);
            byte g = (byte)(sumG / total);
            byte b = (byte)(sumB / total);
            return new Rgba32(r, g, b);
        }

        private static List<Rgba32> SelectRepresentativeColors (List<Rgba32> colors, List<int> counts, int targetCount)
        {
            int n = colors.Count;
            var result = new List<Rgba32>(targetCount);

            if (targetCount >= n)
            {
                result.AddRange(colors);
                return result;
            }

            var selected = new List<int>(targetCount);
            var isSelected = new bool[n];
            var minDist2 = new double[n];

            int seedIndex = 0;
            for (int i = 1; i < n && i < counts.Count; i++)
            {
                if (counts[i] > counts[seedIndex])
                {
                    seedIndex = i;
                }
            }

            selected.Add(seedIndex);
            isSelected[seedIndex] = true;

            for (int i = 0; i < n; i++)
            {
                minDist2[i] = ColorDistanceSquared(colors[i], colors[seedIndex]);
            }

            while (selected.Count < targetCount)
            {
                int bestIndex = -1;
                double bestDistance = -1.0;

                for (int i = 0; i < n; i++)
                {
                    if (isSelected[i])
                    {
                        continue;
                    }

                    double d = minDist2[i];
                    if (d > bestDistance)
                    {
                        bestDistance = d;
                        bestIndex = i;
                    }
                }

                if (bestIndex == -1)
                {
                    break;
                }

                selected.Add(bestIndex);
                isSelected[bestIndex] = true;

                Rgba32 newlySelected = colors[bestIndex];
                for (int i = 0; i < n; i++)
                {
                    if (isSelected[i])
                    {
                        continue;
                    }

                    double d = ColorDistanceSquared(colors[i], newlySelected);
                    if (d < minDist2[i])
                    {
                        minDist2[i] = d;
                    }
                }
            }

            foreach (int index in selected)
            {
                result.Add(colors[index]);
            }

            return result;
        }

        private static double ColorDistanceSquared (Rgba32 c1, Rgba32 c2)
        {
            int dr = (int)c1.R - c2.R;
            int dg = (int)c1.G - c2.G;
            int db = (int)c1.B - c2.B;
            return (double)dr * dr + (double)dg * dg + (double)db * db;
        }

        private static List<Rgba32> OrderPalette (List<Rgba32> colors)
        {
            int n = colors.Count;
            if (n <= 2)
            {
                return new List<Rgba32>(colors);
            }

            var result = new List<Rgba32>(n);
            var used = new bool[n];

            int startIndex = 0;
            double bestLuma = Luma(colors[0]);

            for (int i = 1; i < n; i++)
            {
                double l = Luma(colors[i]);
                if (l < bestLuma)
                {
                    bestLuma = l;
                    startIndex = i;
                }
            }

            int current = startIndex;
            used[current] = true;
            result.Add(colors[current]);

            for (int k = 1; k < n; k++)
            {
                int bestIndex = -1;
                double bestDist = double.MaxValue;

                for (int i = 0; i < n; i++)
                {
                    if (used[i])
                    {
                        continue;
                    }

                    double d = ColorDistanceSquared(colors[current], colors[i]);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestIndex = i;
                    }
                }

                if (bestIndex == -1)
                {
                    break;
                }

                current = bestIndex;
                used[current] = true;
                result.Add(colors[current]);
            }

            return result;
        }

        private static double Luma (Rgba32 c)
        {
            return 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
        }

    }
}