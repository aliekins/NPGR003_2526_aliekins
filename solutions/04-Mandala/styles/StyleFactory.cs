using System;
using _04Mandala.Core;

namespace _04Mandala.Styles
{
    public static class StyleFactory
    {
        public static IMandalaStyle Create (MandalaStyleKind kind)
        {
            return kind switch
            {
                MandalaStyleKind.Geometric => new GeometricStyle(),
                MandalaStyleKind.Sand => new SandStyle(),
                MandalaStyleKind.Hindu => new HinduStyle(),
                MandalaStyleKind.Celtic => new CelticStyle(),
                MandalaStyleKind.Lotus => new LotusStyle(),
                MandalaStyleKind.Chakra => new ChakraStyle(),
                MandalaStyleKind.Tantric => new TantricStyle(),
                MandalaStyleKind.Buddha => new BuddhaStyle(),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}