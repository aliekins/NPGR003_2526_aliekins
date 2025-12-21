using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;
using _06ImageRecoloring.Skin;
using _06ImageRecoloring.Transforms;

namespace _06ImageRecoloring.Pipeline
{
    public sealed class ImageProcessor
    {
        private readonly ISkinDetector _skinDetector;
        private readonly IMaskedRecolorTransform _transform;

        public ImageProcessor (ISkinDetector skinDetector, IMaskedRecolorTransform transform)
        {
            _skinDetector = skinDetector ?? throw new ArgumentNullException(nameof(skinDetector));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public ProcessingResult Process (string inputPath, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return new ProcessingResult(false, "Input path is missing.", 1);
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return new ProcessingResult(false, "Output path is missing.", 1);
            }

            if (!File.Exists(inputPath))
            {
                return new ProcessingResult(false, $"Input file not found: {inputPath}", 2);
            }

            try
            {
                EnsureOutputDirectory(outputPath);

                using Image<Rgba32> img = Image.Load<Rgba32>(inputPath);
                SkinMaskBuilder maskBuilder = new SkinMaskBuilder(_skinDetector);
                double[,] pSkin = maskBuilder.Build(img);

                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            Rgba32 px = row[x];
                            HsvColor hsv = ColorConverter.ToHsv(px);
                            row[x] = _transform.Apply(px, hsv, pSkin[y, x]);
                        }
                    }
                });

                img.Save(outputPath);
                return new ProcessingResult(true, $"Saved: {outputPath}", 0);
            }
            catch (Exception ex)
            {
                return new ProcessingResult(false, $"Failed to process image: {ex.Message}", 3);
            }
        }

        private static void EnsureOutputDirectory (string outputPath)
        {
            string full = Path.GetFullPath(outputPath);
            string? dir = Path.GetDirectoryName(full);

            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}