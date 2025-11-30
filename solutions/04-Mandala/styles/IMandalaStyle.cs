using _04Mandala.Core;

namespace _04Mandala.Styles
{
    public interface IMandalaStyle : IMandalaRenderer
    {
        MandalaStyleKind Kind { get; }
    }
}