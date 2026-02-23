using System;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using _08_Fireworks.DataRenderer;

namespace _08_Fireworks.Rendering
{
    public sealed class FireworksRenderer : IDisposable
    {
        private readonly GL _gl;

        private readonly uint _vao;
        private readonly uint _vbo;

        private readonly uint _tex;
        private readonly int _uTex;
        private readonly int _uUseTexture;
        private bool _useTexture = false;

        private readonly ShaderProgram _program;

        private readonly int _uModel;
        private readonly int _uView;
        private readonly int _uProjection;

        private readonly int _maxVertices;

        private readonly float[] _mModel = new float[16];
        private readonly float[] _mView = new float[16];
        private readonly float[] _mProj = new float[16];

        public FireworksRenderer (GL gl, string vertexShaderSrc, string fragmentShaderSrc, int maxParticles)
        {
            _gl = gl;
            _maxVertices = maxParticles;

            _program = new ShaderProgram(_gl, vertexShaderSrc, fragmentShaderSrc);

            _uTex = _program.GetUniformLocation("tex");
            _uUseTexture = _program.GetUniformLocation("useTexture");
            _tex = _gl.GenTexture();

            _uModel = _program.GetUniformLocation("model");
            _uView = _program.GetUniformLocation("view");
            _uProjection = _program.GetUniformLocation("projection");

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            int floatsCapacity = _maxVertices * VertexLayout.FloatCountPerVertex;
            var init = new float[floatsCapacity];
            _gl.BufferData<float>(BufferTargetARB.ArrayBuffer, init.AsSpan(), BufferUsageARB.DynamicDraw);

            ConfigureVertexAttributes();

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);

            _gl.Enable(GLEnum.ProgramPointSize);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _gl.Enable(EnableCap.DepthTest);
        }

        private void ConfigureVertexAttributes ()
        {
            int strideBytes = VertexLayout.FloatCountPerVertex * sizeof(float);

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(
                0, 3, VertexAttribPointerType.Float, false,
                (uint)strideBytes,
                (nint)(VertexLayout.PosOffset * sizeof(float))
            );

            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(
                1, 3, VertexAttribPointerType.Float, false,
                (uint)strideBytes,
                (nint)(VertexLayout.ColorOffset * sizeof(float))
            );

            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(
                2, 3, VertexAttribPointerType.Float, false,
                (uint)strideBytes,
                (nint)(VertexLayout.NormalOffset * sizeof(float))
            );

            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(
                3, 2, VertexAttribPointerType.Float, false,
                (uint)strideBytes,
                (nint)(VertexLayout.TxtOffset * sizeof(float))
            );

            _gl.EnableVertexAttribArray(4);
            _gl.VertexAttribPointer(
                4, 1, VertexAttribPointerType.Float, false,
                (uint)strideBytes,
                (nint)(VertexLayout.SizeOffset * sizeof(float))
            );
        }

        public void Render (float[] packedVertices, int vertexCount, Matrix4X4<float> model, Matrix4X4<float> view, Matrix4X4<float> projection)
        {
            if (vertexCount <= 0)
            {
                return;
            }

            if (vertexCount > _maxVertices)
            {
                vertexCount = _maxVertices;
            }

            _program.Use();

            if (_uUseTexture >= 0)
            {
                _gl.Uniform1(_uUseTexture, _useTexture ? 1 : 0);
            }

            if (_useTexture)
            {
                _gl.ActiveTexture(TextureUnit.Texture0);
                _gl.BindTexture(TextureTarget.Texture2D, _tex);
                _gl.Uniform1(_uTex, 0);
            }

            FillMatrixArray(model, _mModel);
            FillMatrixArray(view, _mView);
            FillMatrixArray(projection, _mProj);

            _gl.UniformMatrix4(_uModel, 1, false, _mModel);
            _gl.UniformMatrix4(_uView, 1, false, _mView);
            _gl.UniformMatrix4(_uProjection, 1, false, _mProj);

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            int floatsToUpload = vertexCount * VertexLayout.FloatCountPerVertex;
            _gl.BufferSubData<float>(BufferTargetARB.ArrayBuffer, 0, packedVertices.AsSpan(0, floatsToUpload));

            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _tex);
            _gl.Uniform1(_uTex, 0);

            _gl.DrawArrays(PrimitiveType.Points, 0, (uint)vertexCount);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);
        }

        private static void FillMatrixArray (Matrix4X4<float> m, float[] dst16)
        {
            // writes row major:
            dst16[0] = m.M11;
            dst16[1] = m.M12;
            dst16[2] = m.M13;
            dst16[3] = m.M14;
            dst16[4] = m.M21;
            dst16[5] = m.M22;
            dst16[6] = m.M23;
            dst16[7] = m.M24;
            dst16[8] = m.M31;
            dst16[9] = m.M32;
            dst16[10] = m.M33;
            dst16[11] = m.M34;
            dst16[12] = m.M41;
            dst16[13] = m.M42;
            dst16[14] = m.M43;
            dst16[15] = m.M44;
        }

        public void Dispose ()
        {
            _program.Dispose();
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteVertexArray(_vao);
        }

        public unsafe void SetTextureRgba8 (int width, int height, byte[] rgba)
        {
            _gl.BindTexture(TextureTarget.Texture2D, _tex);

            fixed (byte* p = rgba)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
            }

            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _gl.BindTexture(TextureTarget.Texture2D, 0);
            _useTexture = true;
        }


        public void DisableTexture ()
        {
            _useTexture = false;
        }
    }
}