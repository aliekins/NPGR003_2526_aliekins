using System;
using Silk.NET.OpenGL;

namespace _08_Fireworks.Rendering
{
    public sealed class ShaderProgram : IDisposable
    {
        private readonly GL _gl;
        public uint Handle { get; private set; }

        public ShaderProgram (GL gl, string vertexSource, string fragmentSource)
        {
            _gl = gl;

            uint vs = Compile(ShaderType.VertexShader, vertexSource);
            uint fs = Compile(ShaderType.FragmentShader, fragmentSource);

            Handle = _gl.CreateProgram();
            _gl.AttachShader(Handle, vs);
            _gl.AttachShader(Handle, fs);
            _gl.LinkProgram(Handle);

            _gl.GetProgram(Handle, GLEnum.LinkStatus, out int linked);
            if (linked == 0)
            {
                string log = _gl.GetProgramInfoLog(Handle);
                _gl.DeleteProgram(Handle);
                Handle = 0;
                throw new Exception("Shader link failed: " + log);
            }

            _gl.DetachShader(Handle, vs);
            _gl.DetachShader(Handle, fs);
            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);
        }

        public void Use ()
        {
            _gl.UseProgram(Handle);
        }

        public int GetUniformLocation (string name)
        {
            return _gl.GetUniformLocation(Handle, name);
        }

        private uint Compile (ShaderType type, string src)
        {
            uint s = _gl.CreateShader(type);
            _gl.ShaderSource(s, src);
            _gl.CompileShader(s);

            _gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
            if (ok == 0)
            {
                string log = _gl.GetShaderInfoLog(s);
                _gl.DeleteShader(s);
                throw new Exception($"{type} compile failed: {log}");
            }

            return s;
        }

        public void Dispose ()
        {
            if (Handle != 0)
            {
                _gl.DeleteProgram(Handle);
                Handle = 0;
            }
        }
    }
}