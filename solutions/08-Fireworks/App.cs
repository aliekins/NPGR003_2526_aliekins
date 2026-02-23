using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Util;
using _08_Fireworks.Config;
using _08_Fireworks.DataRenderer;
using _08_Fireworks.Launchers;
using _08_Fireworks.Rendering;
using _08_Fireworks.Simulation;
using StbImageSharp;

namespace _08_Fireworks.App;

using Matrix4 = Matrix4X4<float>;
using Vector3 = Vector3D<float>;

public readonly struct StartupOptions
{
    public readonly int MaxParticles;
    public readonly double RocketRate;
    public readonly string TextureFile;

    public StartupOptions (int maxParticles, double rocketRate, string textureFile)
    {
        MaxParticles = maxParticles;
        RocketRate = rocketRate;
        TextureFile = textureFile;
    }
}

public sealed class FireworksApp
{
    private readonly IWindow _window;
    private readonly StartupOptions _startup;

    private GL? _gl;
    private Trackball? _tb;
    private readonly FPS _fps = new();

    private FireworksConfig? _cfg;
    private RuntimeControls? _runtime;

    private FireworksSimulation? _sim;
    private VertexPacker? _packer;
    private FireworksRenderer? _renderer;
    private LauncherController? _launchers;
    private UI.InputController? _input;

    private const int VertexSize = 12;
    private float[] _vertexBuffer = Array.Empty<float>();
    private int _vertices;
    private bool _dbgFireHeld;
    private bool _dbgFirePressed;
    private double _nowSeconds;

    public FireworksApp (IWindow window, StartupOptions startup)
    {
        _window = window;
        _startup = startup;
    }

    public void OnLoad ()
    {
        _gl = GL.GetApi(_window);

        var inputCtx = _window.CreateInput();
        _window.FramebufferResize += fb => SetupViewport(fb.X, fb.Y);
        AttachTrackballInput(inputCtx);

        _runtime = new RuntimeControls
        {
            RocketRatePerSecond = _startup.RocketRate,
            StrobeEnabled = true,
            CrackleEnabled = true,
            GlobalTimeScale = 1.0
        };

        _input = new UI.InputController(_runtime);
        _input.Attach(inputCtx);

        _cfg = Presets.Default();
        _cfg.MaxParticles = System.Math.Max(1, _startup.MaxParticles);
        _cfg.Validate();

        _sim = new FireworksSimulation(_cfg, _runtime, seed: 12345);

        _launchers = new LauncherController();
        _launchers.Add(new PointLauncher(new Vector3D<double>(0, -1, 0)));

        _packer = new VertexPacker(_cfg, _runtime);
        _vertexBuffer = new float[_cfg.MaxParticles * VertexSize];

        string vs = File.ReadAllText("vertex.glsl");
        string fs = File.ReadAllText("fragment.glsl");
        _renderer = new FireworksRenderer(_gl, vs, fs, _cfg.MaxParticles);

        TryLoadAndSetParticleTexture(_startup.TextureFile);

        _tb = new Trackball(Vector3.Zero, 1.5f);

        var fb = _window.FramebufferSize;
        SetupViewport(fb.X, fb.Y);

        _window.Title = WindowTitle();
    }

    public void OnResize (Vector2D<int> size)
    {
        var fb = _window.FramebufferSize;
        SetupViewport(fb.X, fb.Y);
    }

    public void OnRender (double _)
    {
        Debug.Assert(_gl != null);
        Debug.Assert(_tb != null);
        Debug.Assert(_sim != null);
        Debug.Assert(_packer != null);
        Debug.Assert(_renderer != null);
        Debug.Assert(_input != null);

        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _nowSeconds = FPS.NowInSeconds;

        var li = _input.ConsumeLauncherInput();

        if (_input.ConsumeResetPressed())
        {
            _sim.Clear();
        }

        if (_input.ConsumeQuitPressed())
        {
            _window.Close();
            return;
        }

        _dbgFireHeld = li.FireHeld;
        _dbgFirePressed = li.FirePressed;
        _sim.SetInput(li);
        _sim.SetLaunchers(_launchers!);
        _sim.SimulateTo(_nowSeconds);

        _vertices = _packer.Fill(_vertexBuffer, _sim.Pool, _nowSeconds);

        _renderer.Render(
            _vertexBuffer,
            _vertices,
            Matrix4.Identity,
            _tb.View,
            _tb.Projection);

        _fps.AddPrimitives(_vertices);
        if (_fps.AddFrames())
            _window.Title = WindowTitle();
    }

    public void OnClose ()
    {
        _input?.Detach();
        _renderer?.Dispose();
    }

    private void SetupViewport (int w, int h)
    {
        _gl?.Viewport(0, 0, (uint)w, (uint)h);
        _tb?.ViewportChange(w, h, 0.05f, 20.0f);
    }

    private string WindowTitle ()
    {
        if (_cfg == null || _runtime == null || _sim == null)
            return "08-Fireworks";

        return string.Format(
            CultureInfo.InvariantCulture,
            "08-Fireworks alive={0}/{1} packed={2} rockets/s={3:f1} fire={4}/{5} strobe={6} crackle={7} fps={8:f1}",
            _sim.AliveCount,
            _cfg.MaxParticles,
            _vertices,
            _runtime.RocketRatePerSecond,
            _dbgFireHeld ? 1 : 0,
            _dbgFirePressed ? 1 : 0,
            _runtime.StrobeEnabled ? "on" : "off",
            _runtime.CrackleEnabled ? "on" : "off",
            _fps.Fps
        );
    }

    private void AttachTrackballInput (IInputContext input)
    {
        foreach (var k in input.Keyboards)
        {
            k.KeyDown += (kb, key, sc) => _tb?.KeyDown(kb, key, sc);
            k.KeyUp += (kb, key, sc) => _tb?.KeyUp(kb, key, sc);
        }

        foreach (var m in input.Mice)
        {
            m.MouseDown += (mouse, btn) => _tb?.MouseDown(mouse, btn);
            m.MouseUp += (mouse, btn) => _tb?.MouseUp(mouse, btn);
            m.MouseMove += (mouse, xy) => _tb?.MouseMove(mouse, xy);
            m.Scroll += (mouse, wheel) => _tb?.MouseWheel(mouse, wheel);
        }
    }

    private void TryLoadAndSetParticleTexture (string textureFile)
    {
        Debug.Assert(_renderer != null);

        if (string.IsNullOrEmpty(textureFile) || textureFile == ":check:")
        {
            _renderer.DisableTexture();
            return;
        }

        try
        {
            var bytes = File.ReadAllBytes(textureFile);
            using var ms = new MemoryStream(bytes);

            var img = StbImageSharp.ImageResult.FromStream(
            ms,
            StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            _renderer.SetTextureRgba8(img.Width, img.Height, img.Data);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Texture load failed '{textureFile}': {e.Message}");
            var fallback = MakeRadialGlowRgba8(64, 64);
            _renderer.SetTextureRgba8(64, 64, fallback);
        }
    }

    private static byte[] MakeRadialGlowRgba8 (int w, int h)
    {
        var rgba = new byte[w * h * 4];

        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;
        float invR = 1.0f / System.MathF.Min(cx, cy);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) * invR;
                float dy = (y - cy) * invR;
                float r = System.MathF.Sqrt(dx * dx + dy * dy);

                float a = 1.0f - r;
                if (a < 0f)
                {
                    a = 0f;
                }

                a *= a;

                int idx = (y * w + x) * 4;
                rgba[idx + 0] = 255; // R
                rgba[idx + 1] = 255; // G
                rgba[idx + 2] = 255; // B
                rgba[idx + 3] = (byte)(a * 255.0f); // A
            }
        }

        return rgba;
    }
}