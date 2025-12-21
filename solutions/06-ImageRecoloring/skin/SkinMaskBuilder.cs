using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;

namespace _06ImageRecoloring.Skin
{
    public sealed class SkinMaskBuilder
    {
        private readonly ISkinDetector _detector;

        private const double T_HIGH = 0.55;
        private const double T_LOW  = 0.22;
        private const double MIN_COMPONENT_FRACTION = 0.0008;

        public SkinMaskBuilder (ISkinDetector detector)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public double[,] Build (Image<Rgba32> img)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            int w = img.Width;
            int h = img.Height;

            double[,] raw = ComputeRaw(img);
            double[,] smooth = GaussianBlur(raw, w, h, radius: 3, sigma: 1.2);

            bool[,] high = Threshold(smooth, w, h, T_HIGH);
            bool[,] low  = Threshold(smooth, w, h, T_LOW);
            bool[,] hyst = HysteresisGrow(high, low, w, h);

            bool[,] closed = Close(hyst, w, h, radius: 1);
            bool[,] cleaned = RemoveSmallComponents(closed, w, h, minFraction: MIN_COMPONENT_FRACTION);

            bool[,] filled = FillHoles(cleaned, w, h);

            double[,] soft = SoftFromBinary(filled, w, h);
            double[,] finalMask = GaussianBlur(soft, w, h, radius: 2, sigma: 1.0);

            return finalMask;
        }

        private double[,] ComputeRaw (Image<Rgba32> img)
        {
            int w = img.Width;
            int h = img.Height;
            double[,] p = new double[h, w];

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        Rgba32 px = row[x];
                        HsvColor hsv = ColorConverter.ToHsv(px);
                        p[y, x] = _detector.SkinProbability(px, hsv);
                    }
                }
            });

            return p;
        }

        private static bool[,] Threshold (double[,] p, int w, int h, double t)
        {
            bool[,] b = new bool[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    b[y, x] = p[y, x] >= t;
                }
            }

            return b;
        }

        private static bool[,] HysteresisGrow (bool[,] seedsHigh, bool[,] allowLow, int w, int h)
        {
            bool[,] outMask = new bool[h, w];
            Queue<int> q = new Queue<int>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (seedsHigh[y, x])
                    {
                        outMask[y, x] = true;
                        q.Enqueue(y * w + x);
                    }
                }
            }

            while (q.Count > 0)
            {
                int idx = q.Dequeue();
                int cy = idx / w;
                int cx = idx - cy * w;

                for (int dy = -1; dy <= 1; dy++)
                {
                    int ny = cy + dy;
                    if (ny < 0 || ny >= h)
                        continue;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = cx + dx;
                        if (nx < 0 || nx >= w)
                            continue;
                        if (dx == 0 && dy == 0)
                            continue;

                        if (!outMask[ny, nx] && allowLow[ny, nx])
                        {
                            outMask[ny, nx] = true;
                            q.Enqueue(ny * w + nx);
                        }
                    }
                }
            }

            return outMask;
        }

        private static bool[,] FillHoles (bool[,] src, int w, int h)
        {
            bool[,] inv = new bool[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    inv[y, x] = !src[y, x];
                }
            }

            bool[,] bg = new bool[h, w];
            Queue<int> q = new Queue<int>();

            void EnqueueIf (int x, int y)
            {
                if (inv[y, x] && !bg[y, x])
                {
                    bg[y, x] = true;
                    q.Enqueue(y * w + x);
                }
            }

            for (int x = 0; x < w; x++)
            {
                EnqueueIf(x, 0);
                EnqueueIf(x, h - 1);
            }
            for (int y = 0; y < h; y++)
            {
                EnqueueIf(0, y);
                EnqueueIf(w - 1, y);
            }

            while (q.Count > 0)
            {
                int idx = q.Dequeue();
                int cy = idx / w;
                int cx = idx - cy * w;

                Try(cx + 1, cy);
                Try(cx - 1, cy);
                Try(cx, cy + 1);
                Try(cx, cy - 1);

                void Try (int nx, int ny)
                {
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        return;
                    if (!inv[ny, nx] || bg[ny, nx])
                        return;
                    bg[ny, nx] = true;
                    q.Enqueue(ny * w + nx);
                }
            }

            bool[,] filled = new bool[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isHole = inv[y, x] && !bg[y, x];
                    filled[y, x] = src[y, x] || isHole;
                }
            }

            return filled;
        }

        private static bool[,] RemoveSmallComponents (bool[,] src, int w, int h, double minFraction)
        {
            int minArea = (int)Math.Max(1, (w * h) * minFraction);

            bool[,] visited = new bool[h, w];
            bool[,] dst = new bool[h, w];

            Queue<int> q = new Queue<int>();
            List<int> component = new List<int>(4096);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!src[y, x] || visited[y, x])
                        continue;

                    component.Clear();
                    visited[y, x] = true;
                    q.Enqueue(y * w + x);

                    while (q.Count > 0)
                    {
                        int idx = q.Dequeue();
                        component.Add(idx);

                        int cy = idx / w;
                        int cx = idx - cy * w;

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ny = cy + dy;
                            if (ny < 0 || ny >= h)
                                continue;

                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = cx + dx;
                                if (nx < 0 || nx >= w)
                                    continue;
                                if (dx == 0 && dy == 0)
                                    continue;

                                if (src[ny, nx] && !visited[ny, nx])
                                {
                                    visited[ny, nx] = true;
                                    q.Enqueue(ny * w + nx);
                                }
                            }
                        }
                    }

                    if (component.Count >= minArea)
                    {
                        for (int i = 0; i < component.Count; i++)
                        {
                            int idx = component[i];
                            int cy = idx / w;
                            int cx = idx - cy * w;
                            dst[cy, cx] = true;
                        }
                    }
                }
            }

            return dst;
        }

        private static bool[,] Close (bool[,] src, int w, int h, int radius)
        {
            bool[,] dil = Dilate(src, w, h, radius);
            bool[,] ero = Erode(dil, w, h, radius);
            return ero;
        }

        private static bool[,] Dilate (bool[,] src, int w, int h, int radius)
        {
            bool[,] dst = new bool[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool any = false;

                    for (int dy = -radius; dy <= radius && !any; dy++)
                    {
                        int yy = y + dy;
                        if (yy < 0 || yy >= h)
                            continue;

                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int xx = x + dx;
                            if (xx < 0 || xx >= w)
                                continue;

                            if (src[yy, xx])
                            {
                                any = true;
                                break;
                            }
                        }
                    }

                    dst[y, x] = any;
                }
            }

            return dst;
        }

        private static bool[,] Erode (bool[,] src, int w, int h, int radius)
        {
            bool[,] dst = new bool[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool all = true;

                    for (int dy = -radius; dy <= radius && all; dy++)
                    {
                        int yy = y + dy;
                        if (yy < 0 || yy >= h)
                        {
                            all = false;
                            break;
                        }

                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int xx = x + dx;
                            if (xx < 0 || xx >= w)
                            {
                                all = false;
                                break;
                            }

                            if (!src[yy, xx])
                            {
                                all = false;
                                break;
                            }
                        }
                    }

                    dst[y, x] = all;
                }
            }

            return dst;
        }

        private static double[,] SoftFromBinary (bool[,] b, int w, int h)
        {
            double[,] p = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    p[y, x] = b[y, x] ? 1.0 : 0.0;
                }
            }
            return p;
        }

        private static double[,] GaussianBlur (double[,] src, int w, int h, int radius, double sigma)
        {
            double[] k = GaussianKernel(radius, sigma);

            double[,] tmp = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double sum = 0.0;
                    double wsum = 0.0;

                    for (int i = -radius; i <= radius; i++)
                    {
                        int xx = x + i;
                        if (xx < 0)
                            xx = 0;
                        if (xx >= w)
                            xx = w - 1;

                        double kw = k[i + radius];
                        sum += src[y, xx] * kw;
                        wsum += kw;
                    }

                    tmp[y, x] = sum / wsum;
                }
            }

            double[,] dst = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double sum = 0.0;
                    double wsum = 0.0;

                    for (int i = -radius; i <= radius; i++)
                    {
                        int yy = y + i;
                        if (yy < 0)
                            yy = 0;
                        if (yy >= h)
                            yy = h - 1;

                        double kw = k[i + radius];
                        sum += tmp[yy, x] * kw;
                        wsum += kw;
                    }

                    dst[y, x] = sum / wsum;
                }
            }

            return dst;
        }

        private static double[] GaussianKernel (int radius, double sigma)
        {
            int n = radius * 2 + 1;
            double[] k = new double[n];
            double s2 = 2.0 * sigma * sigma;

            double sum = 0.0;
            for (int i = -radius; i <= radius; i++)
            {
                double v = Math.Exp(-(i * i) / s2);
                k[i + radius] = v;
                sum += v;
            }

            for (int i = 0; i < n; i++)
            {
                k[i] /= sum;
            }

            return k;
        }
    }
}