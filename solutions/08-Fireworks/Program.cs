using CommandLine;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace _08_Fireworks;

public class Options
{
    [Option('w', "width", Required = false, Default = 800)]
    public int WindowWidth { get; set; } = 800;

    [Option('h', "height", Required = false, Default = 600)]
    public int WindowHeight { get; set; } = 600;

    [Option('p', "particles", Required = false, Default = 10000)]
    public int Particles { get; set; } = 10000;

    [Option('r', "rate", Required = false, Default = 2.0)]
    public double ParticleRate { get; set; } = 2.0;

    [Option('t', "texture", Required = false, Default = ":check:")]
    public string TextureFile { get; set; } = ":check:";
}

internal static class Program
{
    private static IWindow? _window;
    private static App.FireworksApp? _app;

    private static void Main (string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                var opts = WindowOptions.Default;
                opts.Size = new Vector2D<int>(o.WindowWidth, o.WindowHeight);
                opts.Title = "08-Fireworks";
                opts.PreferredDepthBufferBits = 24;
                opts.VSync = true;

                _window = Window.Create(opts);

                _app = new App.FireworksApp(
                    _window,
                    new App.StartupOptions(
                        o.Particles,
                        o.ParticleRate,
                        o.TextureFile)
                    );

                _window.Load += _app.OnLoad;
                _window.Render += _app.OnRender;
                _window.Resize += _app.OnResize;
                _window.Closing += _app.OnClose;

                _window.Run();
            }
        );
    }
}