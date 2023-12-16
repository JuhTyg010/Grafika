using System;
using System.Diagnostics;
using CommandLine;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Util;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace _07_Terrain;

using Vector3 = Vector3D<float>;
using Matrix4 = Matrix4X4<float>;

public class Options
{
  [Option('w', "width", Required = false, Default = 800, HelpText = "Window width in pixels.")]
  public int WindowWidth { get; set; } = 800;

  [Option('h', "height", Required = false, Default = 600, HelpText = "Window height in pixels.")]
  public int WindowHeight { get; set; } = 600;

  [Option('t', "texture", Required = false, Default = ":check:", HelpText = "User-defined texture.")]
  public string TextureFile { get; set; } = ":check:";
}

/// <summary>
/// Single object = sequence of primitives of the same type (point, line, triangle).
/// </summary>
class Object
{
  public Object() => Reset();

  /// <summary>
  /// Buffer id. Not used here as we have only one shared buffer.
  /// </summary>
  public uint BufferId { get; set; }

  public PrimitiveType Type { get; set; } = PrimitiveType.Triangles;

  /// <summary>
  /// Start of the object in the index buffer (in indices).
  /// </summary>
  public int BufferOffset { get; set; }

  /// <summary>
  /// Number of indices (should be multiple of two for lines, multiple of three for triangles, multiple of four for quads).
  /// </summary>
  public int Indices { get; set; }

  /// <summary>
  /// World space coordinates of the object's center.
  /// </summary>
  public Vector3 Center { get; private set; }

  /// <summary>
  /// Object-to-world (local-to-world) transformation.
  /// </summary>
  public Matrix4 ModelTransform { get; private set; }

  public void Reset()
  {
    Center = Vector3.Zero;
    ModelTransform = Matrix4.Identity;
  }

  public void Translate(Vector3 t)
  {
    Center += t;
    Matrix4 translation = Matrix4X4.CreateTranslation(t);
    ModelTransform *= translation;
  }

  /// <summary>
  /// Rotate the object around its center.
  /// </summary>
  /// <param name="angle">Angle in radians.</param>
  public void Rotate(float angle)
  {
    Matrix4 rotation = Matrix4X4.CreateFromYawPitchRoll(angle, -0.414141414f * angle, 0.3333333f * angle);
    ModelTransform *= Matrix4X4.CreateTranslation(-Center) * rotation * Matrix4X4.CreateTranslation(Center);
  }
}

internal class Program
{
  private static IWindow? window;
  private static GL? Gl;

  // VB locking (too lousy?)
  private static object renderLock = new();

  // Window.
  private static float width;
  private static float height;

  // Trackball.
  private static Trackball? tb;

  // FPS counter.
  private static FPS fps = new();

  // Scene dimensions.
  private static Vector3 sceneCenter = Vector3.Zero;
  private static float sceneDiameter = 4.0f;

  // Global 3D data buffer.
  private const int MAX_INDICES = 4096;
  private const int MAX_VERTICES = 4096;
  private const int VERTEX_SIZE = 11;

  private static List<uint> indexBuffer = new(MAX_INDICES);
  private static List<float> vertexBuffer = new(MAX_VERTICES * VERTEX_SIZE);

  private static BufferObject<float>? Vbo;
  private static BufferObject<uint>? Ebo;
  private static VertexArrayObject<float, uint>? Vao;

  // Texture.
  private static Util.Texture? texture;
  private static bool useTexture = false;
  private static string textureFile = ":check:";
  private const int TEX_SIZE = 128;

  // Lighting.
  private static bool usePhong = false;

  // Shader program.
  private static ShaderProgram? ShaderPrg;

  // 2D objects - referring to the shared buffer.
  private static List<Object> Objects = new();

  private static Object LastObject => Objects[^1];

  /// <summary>
  /// Adds a new object (pristine copy of the 1st one).
  /// </summary>
  private static void NewObject()
  {
    lock (renderLock)
    {
      Objects.Add(new()
      {
        BufferId = 0,
        Type = Objects[0].Type,
        BufferOffset = Objects[0].BufferOffset,
        Indices = Objects[0].Indices
      });
    }
    SetWindowTitle();
  }

  /// <summary>
  /// Removes the youngest object (until it is the 1st one).
  /// </summary>
  private static void DeleteObject ()
  {
    if (Objects.Count > 1)
      lock (renderLock)
      {
        Objects.RemoveAt(Objects.Count - 1);
        SetWindowTitle();
      }
  }

  //////////////////////////////////////////////////////
  // Application.

  private static string WindowTitle()
  {
    StringBuilder sb = new("07-Terrain");

    sb.Append(string.Format(CultureInfo.InvariantCulture, ", fps={0:f1}", fps.Fps));
    if (window != null &&
        window.VSync)
      sb.Append(" [VSync]");

    double pps = fps.Pps;
    if (pps > 0.0)
      if (pps < 5.0e5)
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}k", pps * 1.0e-3));
      else
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}m", pps * 1.0e-6));

    if (tb != null)
    {
      sb.Append(tb.UsePerspective ? ", perspective" : ", orthographic");
      sb.Append(string.Format(CultureInfo.InvariantCulture, ", zoom={0:f2}", tb.Zoom));
    }

    if (useTexture &&
        texture != null &&
        texture.IsValid())
      sb.Append($", txt={texture.name}");
    else
      sb.Append(", no texture");

    if (usePhong)
      sb.Append(", Phong shading");

    return sb.ToString();
  }

  private static void SetWindowTitle()
  {
    if (window != null)
      window.Title = WindowTitle();
  }

  private static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(o.WindowWidth, o.WindowHeight);
        options.Title = WindowTitle();
        options.VSync = true;

        window = Window.Create(options);
        width  = o.WindowWidth;
        height = o.WindowHeight;

        window.Load    += OnLoad;
        window.Render  += OnRender;
        window.Closing += OnClose;
        window.Resize  += OnResize;

        textureFile = o.TextureFile;

        window.Run();
      });
  }

  private static void VaoPointers()
  {
    Debug.Assert(Vao != null);
    Vao.Bind();
    Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 0);
    Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 3);
    Vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 6);
    Vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, VERTEX_SIZE, 9);
  }

  private static void OnLoad()
  {
    Debug.Assert(window != null);

    // Initialize all the inputs (keyboard + mouse).
    IInputContext input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
      input.Keyboards[i].KeyUp   += KeyUp;
    }
    for (int i = 0; i < input.Mice.Count; i++)
    {
      input.Mice[i].MouseDown   += MouseDown;
      input.Mice[i].MouseUp     += MouseUp;
      input.Mice[i].MouseMove   += MouseMove;
      input.Mice[i].DoubleClick += MouseDoubleClick;
      input.Mice[i].Scroll      += MouseScroll;
    }

    // OpenGL global reference (shortcut).
    Gl = GL.GetApi(window);

    //------------------------------------------------------
    // Render data.

    // How to allocate 2000 vertices (all floats will be 0.0f)
    //vertexBuffer.AddRange(new float[2000 * VERTEX_SIZE]);

    // Init: triangle mesh with 9 vertices and 8 triangles
    vertexBuffer.AddRange(new[]
    {
    //  x,     y,     z,     R,     G,     B,     Nx,    Ny,    Nz,   s,    t
      -1.0f,  0.0f,  1.0f,  1.0f,  1.0f,  1.0f, -0.3f,  1.0f,  0.6f, 0.0f, 0.0f,  // 0
       0.0f,  0.1f,  1.0f,  0.5f,  0.8f,  1.0f,  0.0f,  1.0f,  0.6f, 0.5f, 0.0f,  // 1
       1.0f,  0.0f,  1.0f,  0.0f,  0.6f,  1.0f,  0.3f,  1.0f,  0.6f, 1.0f, 0.0f,  // 2
      -1.0f,  0.1f,  0.0f,  0.0f,  0.2f,  0.2f, -0.6f,  1.0f,  0.0f, 0.0f, 0.5f,  // 3
       0.0f,  0.3f,  0.0f,  0.2f,  0.4f,  0.4f,  0.0f,  1.0f, -0.1f, 0.5f, 0.5f,  // 4
       1.0f,  0.2f,  0.0f,  0.9f,  0.6f,  0.6f,  0.6f,  1.0f,  0.0f, 1.0f, 0.5f,  // 5
      -1.0f,  0.2f, -1.0f,  1.0f,  0.0f,  0.2f, -0.4f,  1.0f, -0.5f, 0.0f, 1.0f,  // 6
       0.0f,  0.3f, -1.0f,  0.7f,  0.5f,  0.6f,  0.0f,  1.0f, -0.5f, 0.5f, 1.0f,  // 7
       1.0f,  0.2f, -1.0f,  0.4f,  1.0f,  0.0f,  0.4f,  1.0f, -0.5f, 1.0f, 1.0f,  // 8

    });
    indexBuffer.AddRange(new uint[]
    {
      // 8 triangles
      0, 1, 4, 0, 4, 3,
      1, 2, 5, 1, 5, 4,
      3, 4, 7, 3, 7, 6,
      4, 5, 8, 4, 8, 7,
    });

    // Create the first object (the rest will be cloned from it).
    lock (renderLock)
    {
      Objects.Add(new()
      {
        BufferId = 0,
        Type = PrimitiveType.Triangles,
        BufferOffset = 0,
        Indices = 24
      });

      // Vertex Array Object = Vertex buffer + Index buffer.
      Ebo = new BufferObject<uint>(Gl, indexBuffer.ToArray(), BufferTargetARB.ElementArrayBuffer);
      Vbo = new BufferObject<float>(Gl, vertexBuffer.ToArray(), BufferTargetARB.ArrayBuffer);
      Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);
      VaoPointers();

      // Initialize the shaders.
      ShaderPrg = new ShaderProgram(Gl, "shader.vert", "shader.frag");

      // Initialize the texture.
      if (textureFile.StartsWith(":"))
      {
        // Generated texture.
        texture = new(TEX_SIZE, TEX_SIZE, textureFile);
        texture.GenerateTexture(Gl);
      }
      else
      {
        texture = new(textureFile, textureFile);
        texture.OpenglTextureFromFile(Gl);
      }

      // Trackball.
      tb = new(sceneCenter, sceneDiameter);
    }

    // Main window.
    SetWindowTitle();
    SetupViewport();
  }

  /// <summary>
  /// Mouse horizontal scaling coefficient.
  /// One unit/pixel of mouse movement corresponds to this distance in world space.
  /// </summary>
  private static float mouseCx =  0.001f;

  /// <summary>
  /// Mouse vertical scaling coefficient.
  /// Vertical scaling is just negative value of horizontal one.
  /// </summary>
  private static float mouseCy = -0.001f;

  /// <summary>
  /// Does all necessary steps after window setup/resize.
  /// Assumes valid values in 'width' and 'height'.
  /// </summary>
  private static void SetupViewport()
  {
    // OpenGL viewport.
    Gl?.Viewport(0, 0, (uint)width, (uint)height);

    tb?.ViewportChange((int)width, (int)height, 0.05f, 20.0f);

    // The tight coordinate is used for mouse scaling.
    float minSize = Math.Min(width, height);
    mouseCx = sceneDiameter / minSize;
    // Vertical mouse scaling is just negative...
    mouseCy = -mouseCx;
  }

  /// <summary>
  /// Called after window resize.
  /// </summary>
  /// <param name="newSize">New window size in pixels.</param>
  private static void OnResize(Vector2D<int> newSize)
  {
    width  = newSize[0];
    height = newSize[1];
    SetupViewport();
  }

  /// <summary>
  /// Called every time the content of the window should be redrawn.
  /// </summary>
  /// <param name="obj"></param>
  private static unsafe void OnRender(double obj)
  {
    Debug.Assert(Gl != null);
    Debug.Assert(ShaderPrg != null);
    Debug.Assert(tb != null);

    Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

    lock (renderLock)
    {
      // Rendering properties (set in every frame for clarity).
      Gl.Enable(GLEnum.DepthTest);
      Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
      Gl.Disable(GLEnum.CullFace);

      // Draw the scene (set of Object-s).
      VaoPointers();
      ShaderPrg.Use();

      // Shared shader uniforms - matrices.
      ShaderPrg.TrySetUniform("view", tb.View);
      ShaderPrg.TrySetUniform("projection", tb.Projection);

      // Shared shader uniforms - Phong shading.
      ShaderPrg.TrySetUniform("lightColor", 1.0f, 1.0f, 1.0f);
      ShaderPrg.TrySetUniform("lightPosition", -8.0f, 8.0f, 8.0f);
      ShaderPrg.TrySetUniform("eyePosition", tb.Eye);
      ShaderPrg.TrySetUniform("Ka", 0.1f);
      ShaderPrg.TrySetUniform("Kd", 0.7f);
      ShaderPrg.TrySetUniform("Ks", 0.3f);
      ShaderPrg.TrySetUniform("shininess", 60.0f);
      ShaderPrg.TrySetUniform("usePhong", usePhong);

      // Shared shader uniforms - Texture.
      if (texture == null || !texture.IsValid())
        useTexture = false;
      ShaderPrg.TrySetUniform("useTexture", useTexture);
      ShaderPrg.TrySetUniform("tex", 0);
      if (useTexture)
        texture?.Bind(Gl);

      // Draw the objects.
      foreach (var o in Objects)
      {
        // Object-specific uniforms.
        ShaderPrg.TrySetUniform("model", o.ModelTransform);

        // Draw the batch.
        Gl.DrawElements(o.Type, (uint)o.Indices, DrawElementsType.UnsignedInt, (void*)(o.BufferOffset * sizeof(float)));

        // Update Pps.
        fps.AddPrimitives(o.Indices / 3);
      }
    }

    // Cleanup.
    Gl.UseProgram(0);
    if (useTexture)
      Gl.BindTexture(TextureTarget.Texture2D, 0);

    // FPS.
    if (fps.AddFrames())
      SetWindowTitle();
  }

  /// <summary>
  /// Demo function - shows how to update one vertex of the triangle mesh.
  /// </summary>
  private static unsafe void UpdateVertex(float dy)
  {
    lock (renderLock)
    {
      // I'm going to change the y coordinate (offset = 1) of the middle vertex (index = 4)...
      vertexBuffer[4 * VERTEX_SIZE + 1] += dy;

      // ...but I'm changing the whole vertex data (11 floats)
      var span = CollectionsMarshal.AsSpan(vertexBuffer);

      Vbo?.UpdateData(span, 4 * VERTEX_SIZE, VERTEX_SIZE);
      // This is not the nicest solution, I'd better use float[] and pass it directly to UpdateData()...
    }
  }

  /// <summary>
  /// Handler for window close event.
  /// </summary>
  private static void OnClose()
  {
    Vao?.Dispose();
    ShaderPrg?.Dispose();

    // Remember to dispose the textures.
    texture?.Dispose();
  }

  /// <summary>
  /// Shift counter (0 = no shift pressed).
  /// </summary>
  private static int shiftDown = 0;

  /// <summary>
  /// Ctrl counter (0 = no ctrl pressed).
  /// </summary>
  private static int ctrlDown = 0;

  /// <summary>
  /// Handler function for keyboard key up.
  /// </summary>
  /// <param name="arg1">Keyboard object.</param>
  /// <param name="arg2">Key identification.</param>
  /// <param name="arg3">Key scancode.</param>
  private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyDown(arg1, arg2, arg3))
    {
      SetWindowTitle();
      //return;
    }

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown++;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown++;
        break;

      case Key.Home:
        // Reset object transformation.
        if (Objects.Count > 0)
        {
          LastObject.Reset();
        }
        break;

      case Key.T:
        // Toggle texture.
        useTexture = !useTexture;
        if (useTexture)
          Ut.Message($"Texture: {texture?.name}");
        else
          Ut.Message("Texturing off");
        SetWindowTitle();
        break;

      case Key.I:
        // Toggle Phong shading.
        usePhong = !usePhong;
         Ut.Message("Phong shading: " + (usePhong ? "on" : "off"));
        SetWindowTitle();
        break;

      case Key.P:
        // Perspective <-> orthographic.
        if (tb != null)
        {
          tb.UsePerspective = !tb.UsePerspective;
          SetWindowTitle();
        }
        break;

      case Key.C:
        // Reset view.
        if (tb != null)
        {
          tb.Reset();
          Ut.Message("Camera reset");
        }
        break;

      case Key.V:
        // Toggle VSync.
        if (window != null)
        {
          window.VSync = !window.VSync;
          if (window.VSync)
          {
            Ut.Message("VSync on");
            fps.Reset();
          }
          else
            Ut.Message("VSync off");
        }
        break;

      case Key.Up:
        // Move the middle vertex a little bit higher.
        UpdateVertex(0.1f);
        break;

      case Key.Down:
        // Move the middle vertex a little bit lower.
        UpdateVertex(-0.1f);
        break;

      case Key.Left:
        if (Objects.Count > 0)
        {
          LastObject.Rotate(-0.1f);
        }
        break;

      case Key.Right:
        if (Objects.Count > 0)
        {
          LastObject.Rotate(0.1f);
        }
        break;

      case Key.F1:
        // Help.
        Ut.Message("T           toggle texture", true);
        Ut.Message("I           toggle Phong shading", true);
        Ut.Message("P           toggle perspective", true);
        Ut.Message("V           toggle VSync", true);
        Ut.Message("C           camera reset", true);
        Ut.Message("Up, Down    update the vertex", true);
        Ut.Message("Left, Right rotate the object", true);
        Ut.Message("Home        reset the object", true);
        Ut.Message("F1          print help", true);
        Ut.Message("Esc         quit the program", true);
        Ut.Message("Mouse.left  Trackball rotation", true);
        Ut.Message("Mouse.right drag current object", true);
        Ut.Message("Mouse.wheel zoom in/out", true);
        break;

      case Key.Escape:
        // Close the application.
        window?.Close();
        break;
    }
  }

  /// <summary>
  /// Handler function for keyboard key up.
  /// </summary>
  /// <param name="arg1">Keyboard object.</param>
  /// <param name="arg2">Key identification.</param>
  /// <param name="arg3">Key scancode.</param>
  private static void KeyUp(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyUp(arg1, arg2, arg3))
      return;

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown--;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown--;
        break;
    }
  }

  /// <summary>
  /// Mouse dragging - current X coordinate in pixels.
  /// </summary>
  private static float currentX = 0.0f;

  /// <summary>
  /// Mouse dragging - current Y coordinate in pixels.
  /// </summary>
  private static float currentY = 0.0f;

  /// <summary>
  /// True if dragging mode is active.
  /// </summary>
  private static bool dragging = false;

  /// <summary>
  /// Handler function for mouse button down.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  private static void MouseDown(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseDown(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button down: {mouse.Position}");

      // Start dragging.
      dragging = true;
      currentX = mouse.Position.X;
      currentY = mouse.Position.Y;
    }
  }

  /// <summary>
  /// Handler function for mouse button up.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  private static void MouseUp(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseUp(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button up: {mouse.Position}");

      // Stop dragging.
      dragging = false;
    }
  }

  /// <summary>
  /// Handler function for mouse move.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="xy">New mouse position in pixels.</param>
  private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
  {
    if (tb != null)
      tb.MouseMove(mouse, xy);

    if (mouse.IsButtonPressed(MouseButton.Right))
    {
      Ut.MessageInvariant($"Mouse drag: {xy}");
    }

    // Object dragging.
    if (dragging)
    {
      float newX = mouse.Position.X;
      float newY = mouse.Position.Y;

      if (newX != currentX || newY != currentY)
      {
        if (Objects.Count > 0)
        {
          LastObject.Translate(new((newX - currentX) * mouseCx, (newY - currentY) * mouseCy, 0.0f));
        }

        currentX = newX;
        currentY = newY;
      }
    }
  }

  /// <summary>
  /// Handler function for mouse button double click.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  /// <param name="xy">Double click position in pixels.</param>
  private static void MouseDoubleClick(IMouse mouse, MouseButton btn, System.Numerics.Vector2 xy)
  {
    if (btn == MouseButton.Right)
    {
      Ut.Message("Closed by double-click.", true);
      window?.Close();
    }
  }

  /// <summary>
  /// Handler function for mouse wheel rotation.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Mouse wheel object (Y coordinate is used here).</param>
  private static void MouseScroll(IMouse mouse, ScrollWheel wheel)
  {
    if (tb != null)
    {
      tb.MouseWheel(mouse, wheel);
      SetWindowTitle();
    }

    // wheel.Y is -1 or 1
    Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
  }
}
