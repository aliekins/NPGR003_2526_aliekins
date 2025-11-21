using System;
using System.Collections.Generic;
#nullable enable

namespace _03_SFC
{
    internal static class CurveRegistry
    {
        private static readonly List<ICurve> Curves = new List<ICurve>
        {
            new HilbertCurve(),
            new MortonCurve(),
            new DragonCurve(),
            new LevyCurve(),
            new BarnsleyFernCurve(),
            new PeanoCurve(),
            new SierpinskiCurve(),
            new GosperFlowsnakeCurve(),
            new NewtonFractalCurve()
        };

        public static ICurve Resolve(string curveSpec)
        {
            if (string.IsNullOrWhiteSpace(curveSpec))
                throw new ArgumentException("Curve type must not be empty.");

            if (int.TryParse(curveSpec, out int idx))
            {
                if (idx < 1 || idx > Curves.Count)
                    throw new ArgumentException($"Unknown curve index {idx}.");
                return Curves[idx - 1];
            }

            string key = curveSpec.Trim().ToLowerInvariant();

            foreach (var c in Curves)
            {
                if (c.Name.ToLowerInvariant() == key)
                    return c;
            }

            if (key == "z" || key == "zorder" || key == "z-order")
            {
                var morton = Curves.Find(c => c.Name == "morton");
                if (morton != null) return morton;
            }

            if (key == "fern" || key == "barnsley")
            {
                var fern = Curves.Find(c => c.Name == "barnsley-fern");
                if (fern != null) return fern;
            }

            if (key == "gosper" || key == "flowsnake")
            {
                var gosper = Curves.Find(c => c.Name == "gosper");
                if (gosper != null) return gosper;
            }

            throw new ArgumentException($"Unknown curve type '{curveSpec}'.");
        }

        public static void PrintAvailable()
        {
            for (int i = 0; i < Curves.Count; i++)
            {
                var c = Curves[i];
                Console.Error.WriteLine(
                  $"{i + 1}: {c.Name,-18}  ({c.Description})" +
                  (c.IsImplemented ? "" : " [NOT IMPLEMENTED]"));
            }
        }
    }
}