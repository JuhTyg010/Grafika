using System;
using System.Diagnostics;
using CommandLine;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;
using System.Text;
using Util;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace _02_Trackball;

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

  // Window.
  private static float width;
  private static float height;

  // Trackball.
  private static Trackball? tb;

  // Scene dimensions.
  private static Vector3 sceneCenter = Vector3.Zero;
  private static float sceneDiameter = 4.0f;

  // Global 3D data buffer.
  private const int MAX_INDICES = 2048;
  private const int MAX_VERTICES = 1024;
  private const int VERTEX_SIZE = 8;

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
    Objects.Add(new()
    {
      BufferId = 0,
      Type = Objects[0].Type,
      BufferOffset = Objects[0].BufferOffset,
      Indices = Objects[0].Indices
    });
    SetWindowTitle();
  }

  /// <summary>
  /// Removes the youngest object (until it is the 1st one).
  /// </summary>
  private static void DeleteObject ()
  {
    if (Objects.Count > 1)
    {
      Objects.RemoveAt(Objects.Count - 1);
      SetWindowTitle();
    }
  }

  //////////////////////////////////////////////////////
  // Application.

  private static string WindowTitle()
  {
    StringBuilder sb = new("02-Trackball");
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
    Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
    Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
    Vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
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

    // Init: cube made of triangles
    vertexBuffer.AddRange(new[]
    {
    //  x,     y,     z,     R,     G,     B,     s,    t
      -1.0f, -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,  // 0
       1.0f, -1.0f, -1.0f,  0.5f,  1.0f,  0.0f,  0.0f, 0.0f,  // 1
      -1.0f, -1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  0.0f, 0.0f,  // 2
       1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f,  // 3
      -1.0f,  1.0f, -1.0f,  1.0f,  1.0f,  0.0f,  1.0f, 1.0f,  // 4
       1.0f,  1.0f, -1.0f,  1.0f,  0.5f,  1.0f,  0.0f, 1.0f,  // 5
      -1.0f,  1.0f,  1.0f,  0.8f,  0.0f,  0.5f,  0.0f, 1.0f,  // 6
       1.0f,  1.0f,  1.0f,  0.5f,  1.0f,  0.0f,  1.0f, 1.0f,  // 7
    });
    indexBuffer.AddRange(new uint[]
    {
      // 12 triangles
      2, 3, 7, 2, 7, 6,
      6, 7, 5, 6, 5, 4,
      4, 5, 1, 4, 1, 0,
      0, 1, 3, 0, 3, 2,
      3, 1, 5, 3, 5, 7,
      0, 2, 6, 0, 6, 4,
    });

    // Create the first object (the rest will be cloned from it).
    Objects.Add(new()
    {
      BufferId = 0,
      Type = PrimitiveType.Triangles,
      BufferOffset = 0,
      Indices = 36
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

    // Put the whole scene in front of the camera.
    //viewMatrix = Matrix4X4.CreateTranslation(0.0f, 0.0f, -5.0f);

    // Projection matrix (orthographic projection).
    // 'sceneDiameter' should be set properly.
    float minSize = Math.Min(width, height);
    //projectionMatrix = Matrix4X4.CreateOrthographic(
    //  sceneDiameter * width / minSize,
    //  sceneDiameter * height / minSize,
    //  0.1f, 20.0f);

    // The tight coordinate is used for mouse scaling.
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

    // Rendering properties (set in every frame for clarity).
    Gl.Enable(GLEnum.DepthTest);
    Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
    Gl.Disable(GLEnum.CullFace);

    // Draw the scene (set of Object-s).
    VaoPointers();
    ShaderPrg.Use();

    // Shared shader uniforms.
    ShaderPrg.TrySetUniform("view", tb.View);
    ShaderPrg.TrySetUniform("projection", tb.Projection);

    // Texture.
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
    }

    // Cleanup.
    Gl.UseProgram(0);
    if (useTexture)
      Gl.BindTexture(TextureTarget.Texture2D, 0);
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
      return;
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
          Util.Util.Message($"Texture: {texture?.name}");
        else
          Util.Util.Message("Texturing off");
        SetWindowTitle();
        break;

      case Key.P:
        // Reset view.
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
          Util.Util.Message("Camera reset");
        }
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
        Util.Util.Message("T           toggle texture", true);
        Util.Util.Message("P           toggle perspective", true);
        Util.Util.Message("C           camera reset", true);
        Util.Util.Message("Left, Right rotate the object", true);
        Util.Util.Message("Home        reset the object", true);
        Util.Util.Message("F1          print help", true);
        Util.Util.Message("Esc         quit the program", true);
        Util.Util.Message("Mouse.left  Trackball rotation", true);
        Util.Util.Message("Mouse.right drag current object", true);
        Util.Util.Message("Mouse.wheel zoom in/out", true);
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
      Util.Util.MessageInvariant($"Right button down: {mouse.Position}");

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
      Util.Util.MessageInvariant($"Right button up: {mouse.Position}");

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
      Util.Util.MessageInvariant($"Mouse drag: {xy}");
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
      Util.Util.Message("Closed by double-click.", true);
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
    Util.Util.MessageInvariant($"Mouse scroll: {wheel.Y}");
  }
}
