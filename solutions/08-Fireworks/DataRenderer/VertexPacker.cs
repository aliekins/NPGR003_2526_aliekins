using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Effects;
using _08_Fireworks.Particles;
using Silk.NET.Maths;

namespace _08_Fireworks.DataRenderer
{
    public sealed class VertexPacker
    {
        private readonly FireworksConfig _cfg;
        private readonly RuntimeControls _runtime;

        public VertexPacker (FireworksConfig cfg, RuntimeControls runtime)
        {
            _cfg = cfg;
            _runtime = runtime;
        }

        // Fills buffer with packed vertices
        // Returns number of vertices written
        public int Fill (float[] buffer, ParticlePool pool, double timeSeconds)
        {
            var alive = pool.AliveIds;
            int vertexCount = alive.Length;

            int required = vertexCount * VertexLayout.FloatCountPerVertex;
            if (buffer.Length < required)
            {
                vertexCount = buffer.Length / VertexLayout.FloatCountPerVertex;
            }

            int j = 0; // float index into buffer

            for (int i = 0; i < vertexCount; i++)
            {
                int id = alive[i];
                ref var p = ref pool.Get(id);

                // position (double - float)
                buffer[j + 0] = (float)p.Position.X;
                buffer[j + 1] = (float)p.Position.Y;
                buffer[j + 2] = (float)p.Position.Z;

                // base + fade + strobe
                var c = ComputeRenderColor(p, timeSeconds);

                buffer[j + 3] = c.X;
                buffer[j + 4] = c.Y;
                buffer[j + 5] = c.Z;

                // normal (dummy)
                buffer[j + 6] = 0.0f;
                buffer[j + 7] = 0.0f;
                buffer[j + 8] = 1.0f;

                // texture coords (dummy)
                buffer[j + 9] = 0.0f;
                buffer[j + 10] = 0.0f;

                // point size (age curve + optional strobe pulse)
                buffer[j + 11] = ComputeRenderSize(p, timeSeconds);

                j += VertexLayout.FloatCountPerVertex;
            }

            return vertexCount;
        }

        private Vector3D<float> ComputeRenderColor (in Particle p, double timeSeconds)
        {
            var color = p.Color;

            // fade over lifetime
            if (_cfg.Render.FadeToBlack)
            {
                float t = p.NormalizedAge;

                // simple fade curve
                float fade = 1.0f - t;
                if (fade < 0f)
                {
                    fade = 0f;
                }

                color = ColorUtil.Scale(color, fade);
            }

            // Strobe
            if (_runtime.StrobeEnabled && p.Kind == ParticleKind.Spark)
            {
                float b = StrobeModel.Evaluate(timeSeconds, p.StrobeHz, p.StrobePhase, _cfg.Spark.StrobeDuty, _cfg.Spark.StrobeHard);
                color = ColorUtil.Scale(color, b);
            }

            return color;
        }

        private float ComputeRenderSize (in Particle p, double timeSeconds)
        {
            float size = p.Size;

            // shrink over life
            if (_cfg.Render.ShrinkOverLife)
            {
                float t = p.NormalizedAge;
                // to 30%
                float s = 1.0f - 0.7f * t;
                if (s < 0.0f)
                {
                    s = 0.0f;
                }
                size *= s;
            }

            // pulse size a bit
            if (_runtime.StrobeEnabled && p.Kind == ParticleKind.Spark && p.StrobeHz > 1e-6f)
            {
                float b = StrobeModel.Evaluate(timeSeconds, p.StrobeHz, p.StrobePhase, _cfg.Spark.StrobeDuty, _cfg.Spark.StrobeHard);
                size *= (0.8f + 0.4f * b);
            }

            // Clamp
            if (size < _cfg.Render.MinPointSize)
            {
                size = _cfg.Render.MinPointSize;
            }
            if (size > _cfg.Render.MaxPointSize)
            {
                size = _cfg.Render.MaxPointSize;
            }

            return size;
        }
    }
}