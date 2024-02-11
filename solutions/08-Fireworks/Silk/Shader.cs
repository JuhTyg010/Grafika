using System;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Util;

using Matrix4 = Matrix4X4<float>;
using Vector2 = Vector2D<float>;
using Vector3 = Vector3D<float>;
using Vector4 = Vector4D<float>;

public class ShaderProgram : IDisposable
{
  private uint _handle;
  private GL _gl;

  public ShaderProgram(GL gl, string vertexPath, string fragmentPath)
  {
    _gl = gl;

    uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
    uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
    _handle = _gl.CreateProgram();
    _gl.AttachShader(_handle, vertex);
    _gl.AttachShader(_handle, fragment);
    _gl.LinkProgram(_handle);
    _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
    if (status == 0)
    {
      throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
    }

    _gl.DetachShader(_handle, vertex);
    _gl.DetachShader(_handle, fragment);
    _gl.DeleteShader(vertex);
    _gl.DeleteShader(fragment);
  }

  public void Use()
  {
    _gl.UseProgram(_handle);
  }

  public bool TrySetUniform(string name, int value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform1(location, value);
    return true;
  }

  public bool TrySetUniform(string name, float value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform1(location, value);
    return true;
  }

  public bool TrySetUniform(string name, bool value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform1(location, value ? 1 : 0);
    return true;
  }

  public unsafe bool TrySetUniform(string name, Matrix4 value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.UniformMatrix4(location, 1, false, (float*)&value);
    return true;
  }

  public unsafe bool TrySetUniform (string name, Vector2 value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform2(location, value.X, value.Y);
    return true;
  }

  public unsafe bool TrySetUniform (string name, float x, float y)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform2(location, x, y);
    return true;
  }

  public unsafe bool TrySetUniform (string name, Vector3 value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform3(location, value.X, value.Y, value.Z);
    return true;
  }

  public unsafe bool TrySetUniform (string name, float x, float y, float z)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform3(location, x, y, z);
    return true;
  }

  public unsafe bool TrySetUniform (string name, Vector4 value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    return true;
  }

  public unsafe bool TrySetUniform (string name, float x, float y, float z, float w)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform4(location, x, y, z, w);
    return true;
  }

  public unsafe bool TrySetUniform(string name, float[] value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform1(location, value.AsSpan());
    return true;
  }

  public unsafe bool TrySetUniform(string name, Vector3[] value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    float[] tmp = new float[value.Length * 3];
    fixed (float* pinned = tmp)
    {
      float* ptr = pinned;
      foreach (var v in value)
      {
        *ptr++ = v.X;
        *ptr++ = v.Y;
        *ptr++ = v.Z;
      }
    }

    _gl.Uniform3(location, tmp.AsSpan());
    return true;
  }

  public unsafe bool TrySetUniform(string name, Vector4[] value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    float[] tmp = new float[value.Length * 4];
    fixed (float* pinned = tmp)
    {
      float* ptr = pinned;
      foreach (var v in value)
      {
        *ptr++ = v.X;
        *ptr++ = v.Y;
        *ptr++ = v.Z;
        *ptr++ = v.W;
      }
    }

    _gl.Uniform4(location, tmp.AsSpan());
    return true;
  }

  public unsafe bool TrySetUniform4(string name, float[] value)
  {
    int location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
      return false;

    _gl.Uniform4(location, value.AsSpan());
    return true;
  }

  public void Dispose()
  {
    _gl.DeleteProgram(_handle);
  }

  private uint LoadShader(ShaderType type, string path)
  {
    string src = File.ReadAllText(path);
    uint handle = _gl.CreateShader(type);
    _gl.ShaderSource(handle, src);
    _gl.CompileShader(handle);
    string infoLog = _gl.GetShaderInfoLog(handle);
    if (!string.IsNullOrWhiteSpace(infoLog))
    {
      throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
    }

    return handle;
  }
}
