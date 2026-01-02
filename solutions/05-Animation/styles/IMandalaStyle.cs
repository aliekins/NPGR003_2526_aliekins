using _05Animation.Core;

namespace _05Animation.Styles
{
    public interface IMandalaStyle : IMandalaRenderer
    {
        MandalaStyleKind Kind { get; }
    }
}