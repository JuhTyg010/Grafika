using System;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// ReSharper disable AssignNullToNotNullAttribute

namespace Util;

using Vector3 = Vector3D<float>;

public class Texture : IDisposable
{
  // OpenGL binding.
  protected uint _handle = uint.MaxValue;

  private static GL? _gl;

  // Descriptive parameters.

  /// <summary>
  /// Full path to the image file.
  /// </summary>
  public string fileName = "";

  /// <summary>
  /// Key of the image.
  /// </summary>
  public string name = "";

  /// <summary>
  /// image description for debugging.
  /// </summary>
  public string descr;

  public int Width { get; protected set; }

  public int Height { get; protected set; }

  public bool IsValid()
  {
    return !string.IsNullOrEmpty(name) &&
           _handle != uint.MaxValue;
  }

  // Constructors.
  public Texture(int width, int height, string nam = "")
  {
    name = nam;
    Width = width;
    Height = height;
    descr = "Will ba generated";
  }

  public Texture(string filename = "", string nam = "")
  {
    name = nam;
    fileName = filename;
    Width = 0;
    Height = 0;
    descr = fileName;
  }

  public unsafe Texture(GL gl, Span<byte> data, int width, int height)
  {
    // Saving the GL instance.
    _gl = gl;

    // Generating the OpenGL handle;
    _handle = gl.GenTexture();
    Bind(gl);

    Width = width;
    Height = height;

    // We want the ability to create a texture using data generated from code as well.
    fixed (void* d = &data[0])
    {
      // Setting the data of a texture.
      gl.TexImage2D(
        TextureTarget.Texture2D,
        0,
        (int)InternalFormat.Rgba,
        (uint)width, (uint)height, 0,
        PixelFormat.Rgba,
        PixelType.UnsignedByte,
        d);
    }

    descr = "bin";
    SetTexParameters(gl);
  }

  public unsafe void OpenglTextureFromFile(GL gl)
  {
    // Saving the GL instance.
    _gl = gl;

    if (string.IsNullOrEmpty(fileName))
    {
      // Generated texture.
      GenerateTexture(gl);
    }
    else
    {
      // Texture read from a disk file.
      if (_handle != uint.MaxValue)
        gl.DeleteTexture(_handle);

      _handle = gl.GenTexture();
      Bind(gl);

      Width = Height = 0;

      if (fileName.ToLower().EndsWith(".png") ||
          fileName.ToLower().EndsWith(".jpg") ||
          fileName.ToLower().EndsWith(".bmp"))
      {
        // Loading a LDR image using ImageSharp.
        using (var img = Image.Load<Rgba32>(fileName))
        {
          Width = img.Width;
          Height = img.Height;

          // Reserve enough memory from the gpu for the whole image
          gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, null);

          img.ProcessPixelRows(accessor =>
          {
            // ImageSharp 2 does not store images in contiguous memory by default, so we must send the image row by row
            for (int y = 0; y < accessor.Height; y++)
            {
              fixed (void* data = accessor.GetRowSpan(y))
              {
                // Loading the actual image.
                gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba,
                  PixelType.UnsignedByte, data);
              }
            }
          });
        }

        descr = "ldr";
      }

      SetTexParameters(gl);
    }
  }

  public unsafe void GenerateTexture(GL gl)
  {
    // Saving the GL instance.
    _gl = gl;

    if (_handle != uint.MaxValue)
      gl.DeleteTexture(_handle);

    _handle = gl.GenTexture();
    Bind(gl);

    string nam = name;
    name = $"{name}[{Width}x{Height}]";

    // Generated texture data.
    if (nam.StartsWith(":check:"))
    {
      // Checkerboard texture.
      const int TEX_CHECKER_SIZE = 16;
      Vector3 colWhite = new( 0.85f, 0.75f, 0.30f );
      Vector3 colBlack = new( 0.15f, 0.15f, 0.60f );
      Vector3 colShade = new( 0.15f, 0.15f, 0.15f );

      _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
      Vector3[] data = new Vector3[Width * Height];
      for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
          int i = y * Width + x;
          bool odd = ((x / TEX_CHECKER_SIZE + y / TEX_CHECKER_SIZE) & 1) > 0;
          data[i] = odd ? colBlack : colWhite;

          // Add some fancy shading on the edges.
          if ((x % TEX_CHECKER_SIZE) == 0 ||
              (y % TEX_CHECKER_SIZE) == 0)
            data[i] += colShade;
          if (((x + 1) % TEX_CHECKER_SIZE) == 0 ||
              ((y + 1) % TEX_CHECKER_SIZE) == 0)
            data[i] -= colShade;
        }

      // Setting the data of a texture.
      fixed (Vector3* d = data)
      {
        gl.TexImage2D(
          TextureTarget.Texture2D, 0,
          (int)InternalFormat.Rgb,
          (uint)Width, (uint)Height, 0,
          PixelFormat.Rgb,
          PixelType.Float,
          d);
      }
    }
    else
    {
      // Sinc(radius^2).
      float[] p = new float[Width * Height * 3];
      float widHalf = Width * 0.5f;
      float heiHalf = Height * 0.5f;
      const float scale = 0.08f;
      const float amplitude = 1.0f;

      fixed (float* d = p)
      {
        // Compute HDR image data.
        float* ptr = d;
        for (int y = 0; y < Height; y++)
        {
          float ay = scale * (y - heiHalf);
          for (int x = 0; x < Width; x++)
          {
            float ax = scale * (x - widHalf);
            float radius2 = ay * ay + ax * ax + 1.0E-6f;
            float value = amplitude * (float)Math.Sin(radius2) / radius2;
            *ptr++ = Math.Abs(value * ax);
            *ptr++ = Math.Abs(value * ay);
            *ptr++ = Math.Abs(value);
          }
        }

        // Setting the data of a texture.
        gl.TexImage2D(
          TextureTarget.Texture2D, 0,
          (int)InternalFormat.Rgb16f,
          (uint)Width, (uint)Height, 0,
          PixelFormat.Rgb,
          PixelType.Float,
          d);
      }
    }

    SetTexParameters(gl);

    descr = "gen";
  }

  private void SetTexParameters(GL gl)
  {
    // Setting some texture parameters so the texture behaves as expected.
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
    //gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
    //gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
    // Generating MipMaps.
    //gl.GenerateMipmap(TextureTarget.Texture2D);
  }

  public void Bind(GL gl, TextureUnit textureSlot = TextureUnit.Texture0)
  {
    // When we bind a texture we can choose which texture-slot we can bind it to.
    gl.ActiveTexture(textureSlot);
    gl.BindTexture(TextureTarget.Texture2D, _handle);
  }

  public void Dispose()
  {
    // In order to dispose we need to delete the OpenGL handle for the texure.
    _gl?.DeleteTexture(_handle);
    _handle = uint.MaxValue;
  }
}
