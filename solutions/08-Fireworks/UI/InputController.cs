using _08_Fireworks.Config;
using _08_Fireworks.Launchers;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace _08_Fireworks.UI
{
    public sealed class InputController
    {
        private readonly RuntimeControls _runtime;

        private IKeyboard? _kbd;
        private IMouse? _mouse;

        private bool _fireHeld;
        private bool _firePressedEdge;
        private bool _resetPressedEdge;
        private bool _quitPressedEdge;

        // optional aim direction (straight up)
        private Vector3D<double> _aimDir = new Vector3D<double>(0.0, 1.0, 0.0);

        public InputController (RuntimeControls runtime)
        {
            _runtime = runtime;
        }

        public void Attach (IInputContext input)
        {
            if (input.Keyboards.Count > 0)
            {
                _kbd = input.Keyboards[0];
                _kbd.KeyDown += OnKeyDown;
                _kbd.KeyUp += OnKeyUp;
            }

            if (input.Mice.Count > 0)
            {
                _mouse = input.Mice[0];
                _mouse.MouseDown += OnMouseDown;
                _mouse.MouseUp += OnMouseUp;
            }
        }

        public void Detach ()
        {
            if (_kbd != null)
            {
                _kbd.KeyDown -= OnKeyDown;
                _kbd.KeyUp -= OnKeyUp;
                _kbd = null;
            }

            if (_mouse != null)
            {
                _mouse.MouseDown -= OnMouseDown;
                _mouse.MouseUp -= OnMouseUp;
                _mouse = null;
            }
        }

        public LauncherInput ConsumeLauncherInput ()
        {
            var li = new LauncherInput(fireHeld: _fireHeld, firePressed: _firePressedEdge, aimDirection: _aimDir);

            _firePressedEdge = false;

            return li;
        }

        private void OnKeyDown (IKeyboard keyboard, Key key, int _)
        {
            switch (key)
            {
                case Key.Space:
                    if (!_fireHeld)
                    {
                        _firePressedEdge = true;
                    }
                    _fireHeld = true;
                    break;

                case Key.Up:
                    _runtime.RocketRatePerSecond = System.Math.Min(_runtime.RocketRatePerSecond + 0.5, 50.0);
                    break;

                case Key.Down:
                    _runtime.RocketRatePerSecond = System.Math.Max(_runtime.RocketRatePerSecond - 0.5, 0.0);
                    break;

                case Key.S:
                    _runtime.StrobeEnabled = !_runtime.StrobeEnabled;
                    break;

                case Key.C:
                    _runtime.CrackleEnabled = !_runtime.CrackleEnabled;
                    break;

                case Key.P:
                    _runtime.PauseSpawning = !_runtime.PauseSpawning;
                    break;

                case Key.F:
                    _runtime.FreezeSimulation = !_runtime.FreezeSimulation;
                    break;

                case Key.R:
                    _resetPressedEdge = true;
                    break;

                case Key.Escape:
                    _quitPressedEdge = true;
                    break;
            }
        }

        private void OnKeyUp (IKeyboard keyboard, Key key, int _)
        {
            switch (key)
            {
                case Key.Space:
                    _fireHeld = false;
                    break;
            }
        }

        private void OnMouseDown (IMouse mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                if (!_fireHeld)
                {
                    _firePressedEdge = true;
                }
                _fireHeld = true;
            }
        }

        private void OnMouseUp (IMouse mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                _fireHeld = false;
            }
        }

        public bool ConsumeResetPressed ()
        {
            bool v = _resetPressedEdge;
            _resetPressedEdge = false;
            return v;
        }

        public bool ConsumeQuitPressed ()
        {
            bool v = _quitPressedEdge;
            _quitPressedEdge = false;
            return v;
        }
    }
}