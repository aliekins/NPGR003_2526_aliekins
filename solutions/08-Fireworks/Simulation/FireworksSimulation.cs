using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Effects;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Physics;
using _08_Fireworks.Spawning;
using _08_Fireworks.Launchers;

namespace _08_Fireworks.Simulation
{
    public sealed class FireworksSimulation
    {
        private readonly FireworksConfig _cfg;
        private readonly RuntimeControls _runtime;

        private readonly SimClock _clock;

        private readonly ParticlePool _pool;
        private readonly IRandSource _rng;

        // Spawning helpers
        private readonly ParticleFactory _factory;
        private readonly BurstSpawner _spawner;

        // Effects
        private readonly ExplosionSystem _explosions;
        private readonly CrackleSystem _crackle;

        // Launchers + input
        private LauncherController? _launchers;
        private LauncherInput _input;

        // Debug / UI stats
        public int ExplosionsLastFrame { get; private set; }
        public int CrackleEventsLastFrame { get; private set; }
        public int MicroSparksLastFrame { get; private set; }

        public FireworksSimulation (FireworksConfig cfg, RuntimeControls runtime, int seed)
        {
            _cfg = cfg;
            _runtime = runtime;

            _cfg.Validate();

            _clock = new SimClock();

            _pool = new ParticlePool(_cfg.MaxParticles);
            _rng = new RandSource(seed);

            _factory = new ParticleFactory(_cfg);
            _spawner = new BurstSpawner(_cfg, _factory);

            _explosions = new ExplosionSystem();
            _crackle = new CrackleSystem(_cfg);

            _launchers = null;
            _input = new LauncherInput(false, false, new Silk.NET.Maths.Vector3D<double>(0.0, 1.0, 0.0));
        }

        public ParticlePool Pool => _pool;
        public int AliveCount => _pool.AliveCount;
        public System.ReadOnlySpan<int> AliveIds => _pool.AliveIds;

        public void SetLaunchers (LauncherController controller)
        {
            _launchers = controller;
        }

        public void SetInput (in LauncherInput input)
        {
            _input = input;
        }

        public void Clear ()
        {
            _pool.Clear();
            _clock.Reset();

            ExplosionsLastFrame = 0;
            CrackleEventsLastFrame = 0;
            MicroSparksLastFrame = 0;
        }

        public void SimulateTo (double timeSeconds)
        {
            if (_runtime.FreezeSimulation)
            {
                _clock.StepTo(timeSeconds, _cfg.MaxDt);
                ExplosionsLastFrame = 0;
                CrackleEventsLastFrame = 0;
                MicroSparksLastFrame = 0;
                return;
            }

            double dtTotal = _clock.StepTo(timeSeconds, _cfg.MaxDt);
            if (dtTotal <= 0.0)
            {
                ExplosionsLastFrame = 0;
                CrackleEventsLastFrame = 0;
                MicroSparksLastFrame = 0;
                return;
            }

            dtTotal *= _runtime.GlobalTimeScale;

            int explosions = 0;
            int crackles = 0;
            int microSparks = 0;

            double remaining = dtTotal;

            double hMax = _cfg.MaxDt;
            if (hMax <= 0.0)
            {
                hMax = 1.0 / 60.0;
            }

            while (remaining > 0.0)
            {
                double h = remaining;
                if (h > hMax)
                {
                    h = hMax;
                }

                if (!_runtime.PauseSpawning && _launchers != null)
                {
                    _launchers.Step(h, in _input, _pool, _factory, _cfg, _runtime, _rng);
                }

                StepPhysicsAndAge(h);

                _explosions.Step(h, _pool, _spawner, _rng);
                explosions += _explosions.ExplosionsThisFrame;

                _crackle.Step(h, _pool, _spawner, _rng, _runtime.CrackleEnabled);
                crackles += _crackle.CrackleEventsThisFrame;
                microSparks += _crackle.MicroSparksSpawnedThisFrame;

                RetireExpiredParticles();

                remaining -= h;

                if (remaining < 1e-12)
                {
                    break;
                }
            }

            ExplosionsLastFrame = explosions;
            CrackleEventsLastFrame = crackles;
            MicroSparksLastFrame = microSparks;
        }

        private void StepPhysicsAndAge (double dt)
        {
            var alive = _pool.AliveIds;
            for (int i = 0; i < alive.Length; i++)
            {
                int id = alive[i];
                ref var p = ref _pool.Get(id);

                p.Age += dt;

                float t = p.NormalizedAge;
                p.Color = ColorUtil.ColorOverLife(p.BaseColor, p.ColorDrift, t, p.StrobePhase);
                // p.Color = new Silk.NET.Maths.Vector3D<float>(1.0f - t, t, 0.0f);

                PhysicsIntegrator.Step(
                    ref p.Position,
                    ref p.Velocity,
                    p.Mass,
                    _cfg.Gravity,
                    p.DragLinear,
                    p.DragQuadratic,
                    dt);
            }
        }

        private void RetireExpiredParticles ()
        {
            var alive = _pool.AliveIds;
            for (int i = alive.Length - 1; i >= 0; i--)
            {
                int id = alive[i];
                ref var p = ref _pool.Get(id);

                if (!p.IsAlive)
                {
                    _pool.Free(id);
                }
            }
        }
    }
}