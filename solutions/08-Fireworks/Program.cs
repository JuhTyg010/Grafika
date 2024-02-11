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

namespace _08_Fireworks;

using Vector3 = Vector3D<float>;
using Matrix4 = Matrix4X4<float>;

public class Options
{
  [Option('w', "width", Required = false, Default = 800, HelpText = "Window width in pixels.")]
  public int WindowWidth { get; set; } = 800;

  [Option('h', "height", Required = false, Default = 600, HelpText = "Window height in pixels.")]
  public int WindowHeight { get; set; } = 600;

  [Option('p', "particles", Required = false, Default = 10000, HelpText = "Maximum number of particles.")]
  public int Particles { get; set; } = 10000;

  [Option('r', "rate", Required = false, Default = 1000.0, HelpText = "Particle generation rate per second.")]
  public double ParticleRate { get; set; } = 1000.0;

  [Option('t', "texture", Required = false, Default = ":check:", HelpText = "User-defined texture.")]
  public string TextureFile { get; set; } = ":check:";
}



abstract class Particle
{
  public Vector3 Position { get; protected set;}
  public Vector3 Color { get; protected set;}
  public float Size { get; protected set;}
  public float timeScale;
  public Vector3 Velocity { get; protected set;}
  public double GravityConst = 9.81;

  public double Weight { get; protected set; }
  public double Friction { get; protected set; }
  public double Age { get; set; }
  public  double SimulatedTime { get; protected set; }
  abstract public bool SimulateTo(double time);
  abstract public void FillBuffer(float[] buffer, ref int i);

  protected double Magnitude(Vector3 v) {
    return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
  }

}

class FlameParticle : Particle
{
  private double decay;
  public FlameParticle(double now, Vector3 position, Vector3 color, float size, Vector3 velocity, double age, double weight = 1.0, double friction = 0.0)
  {
    Position = position;
    Color = color;
    Size = size;
    Velocity = velocity;
    Age = age;
    SimulatedTime = now;
    this.Weight = weight;
    this.Friction = friction;
    decay = 0;
  }

  public override bool SimulateTo (double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;
    //dont care about the age the velocity chenges over time and the color and size when is smaller than constant remove,
    //also if the magnitude of the velocity is smaller than 0.5 and also if size is smaller than 0.1
    Age -= dt;
    if (Age <= 0.0) return false;
    if ( Size < .2 || Magnitude(Color) < 0.1) return false;



    Vector3 gravitation = new Vector3(0, (float)-(GravityConst * dt * Weight), 0);
    Vector3 friction = -Velocity / (float)(Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y + Velocity.Z * Velocity.Z)) * (float)(Friction * dt);
    Velocity += gravitation;
    Velocity += friction;
    Position += Velocity * (float)dt;

    // Change particle color.
    //TODO chenge color and size based on speed

    Color *= (float)Math.Pow(0.5, dt);
    decay += dt/4;
    Size -= (float)decay;

    return true;
  }

  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = Position.X;
    buffer[i++] = Position.Y;
    buffer[i++] = Position.Z;

    // offset  3: Color
    buffer[i++] = Color.X;
    buffer[i++] = Color.Y;
    buffer[i++] = Color.Z;

    // offset  6: Normal
    buffer[i++] = 0.0f;
    buffer[i++] = 1.0f;
    buffer[i++] = 0.0f;

    // offset  9: Txt coordinates
    buffer[i++] = 0.5f;
    buffer[i++] = 0.5f;

    // offset 11: Point size
    buffer[i++] = Size;
  }
}

class RocketParticle : Particle
{

  /// <summary>
  /// Create a new particle.
  /// </summary>
  public RocketParticle (double now, Vector3 position, Vector3 color, float size, Vector3 velocity, double age, double weight = 1.0, double friction = 0.0)
  {
    Position = position;
    Color = color;
    Size = size;
    Velocity = velocity;
    Age = age;
    SimulatedTime = now;
    this.Weight = weight;
    this.Friction = friction;
  }

  /// <summary>
  /// Simulate one step in time.
  /// </summary>
  /// <param name="time">Target time to simulate to (in seconds).</param>
  /// <returns></returns>
  public override bool SimulateTo (double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;

    Age -= dt;
    if (Age <= 0.0)
      return false;

    Vector3 gravitation = new Vector3(0, (float)-(GravityConst * dt * Weight), 0);
    Vector3 friction = -Velocity / (float)(Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y + Velocity.Z * Velocity.Z)) * (float)(Friction * dt);
    Velocity += gravitation;
    Velocity += friction;

    Position += Velocity * (float)dt;

    return true;
  }
  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = Position.X;
    buffer[i++] = Position.Y;
    buffer[i++] = Position.Z;

    // offset  3: Color
    buffer[i++] = Color.X;
    buffer[i++] = Color.Y;
    buffer[i++] = Color.Z;

    // offset  6: Normal
    buffer[i++] = 0.0f;
    buffer[i++] = 1.0f;
    buffer[i++] = 0.0f;

    // offset  9: Txt coordinates
    buffer[i++] = 0.5f;
    buffer[i++] = 0.5f;

    // offset 11: Point size
    buffer[i++] = Size;
  }
}

class Launcher : Particle
{
  private double delta;

  public Launcher(double now, Vector3 position, Vector3 color, float size, Vector3 velocity, double age)
  {
    Position = position;
    Color = color;
    Size = size;
    Velocity = velocity;
    Age = age;
    SimulatedTime = now;
    delta = age;
  }

  public override bool SimulateTo (double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;

    Age -= dt;
    if (Age <= 0.0)
    {
      Age = delta;
      return false;
    }

    return true;
  }

  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = Position.X;
    buffer[i++] = Position.Y;
    buffer[i++] = Position.Z;

    // offset  3: Color
    buffer[i++] = Color.X;
    buffer[i++] = Color.Y;
    buffer[i++] = Color.Z;

    // offset  6: Normal
    buffer[i++] = 0.0f;
    buffer[i++] = 1.0f;
    buffer[i++] = 0.0f;

    // offset  9: Txt coordinates
    buffer[i++] = 0.5f;
    buffer[i++] = 0.5f;

    // offset 11: Point size
    buffer[i++] = Size;
  }
}

public class Simulation
{
  private List<Particle> particles = new();

  private double AirFriction = .5;
  public int Particles => particles.Count;

  public float TimeScale { get; private set; }
  public int MaxParticles { get; private set; }

  private double SimulatedTime;
  public double ParticleRate { get; set; }

  public Simulation (double now, double particleRate, int maxParticles, int initParticles, float timeScale = 1.0f)
  {
    SimulatedTime = now;
    ParticleRate = particleRate;
    MaxParticles = maxParticles;
    TimeScale = timeScale;
    GenerateLauncher(initParticles);
  }


  private void GenerateExplode(int number, Vector3 position, Vector3 color, float size, double velocity, double age)
  {
    Random rnd = new();
    Vector3 direction;
    float theta;
    float phi;
    if (number <= 0)
      return;

    for (int i = 0; i < number; i++)
    {
      theta = (float)(rnd.NextDouble() * 2 * Math.PI);
      phi = (float)(rnd.NextDouble() * Math.PI);
      direction = new Vector3((float)(Math.Sin(phi) * Math.Cos(theta)), (float)Math.Cos(phi), (float)(Math.Sin(phi) * Math.Sin(theta)));
      Particle p = new FlameParticle(SimulatedTime, position, color, size,
        direction * (float)velocity, age, .05, AirFriction);
      p.timeScale = TimeScale;
      particles.Add(p);
    }
  }
  private void GenerateLauncher(int number)
  {
    Random rnd = new();
    if (number <= 0)
      return;

    while (number-- > 0)
    {
      Console.WriteLine("Generating");
      // Generate one new particle.
      Particle p = new Launcher(SimulatedTime, new Vector3((float)rnd.NextDouble(), -1, (float)rnd.NextDouble()),
        new Vector3(1, 0, 0), 10, new Vector3(0, 0, 0), rnd.Next(1, 5));
      p.timeScale = TimeScale;
      particles.Add(p);
    }
  }

  public void SimulateTo(double time)
  {
    Random rnd = new();
    if(time <= SimulatedTime)
      return;

    List<int> toRemove = new();
    for(int i = 0; i < particles.Count; i++)
    {
      if (!particles[i].SimulateTo(time))
        toRemove.Add(i);
    }
    SimulatedTime = time;
    for(int i = toRemove.Count - 1; i >= 0; i--)
    {
      if(particles[toRemove[i]] is Launcher)
      {
        Console.WriteLine("Shoot");
        //TODO: velocity should be something pointed up but in range angle
        Vector3 velocity = new Vector3(0 + (float)(Math.Min(rnd.NextDouble(), 0.5) * rnd.Next(-1, 1)),
          1 + (float)rnd.NextDouble(),
          0 + (float)(Math.Min(rnd.NextDouble(), 0.5) * rnd.Next(-1, 1)));
        Particle p = new RocketParticle(time, particles[toRemove[i]].Position, new Vector3((float)rnd.NextDouble(),(float)rnd.NextDouble(),(float)rnd.NextDouble()),
          particles[toRemove[i]].Size/2, velocity, 1, .1, AirFriction);
        p.timeScale = TimeScale;
        particles.Add(p);
      }
      else if(particles[toRemove[i]] is RocketParticle)
      {
        Console.WriteLine("Explode");
        Particle p = particles[toRemove[i]];
        GenerateExplode(400, p.Position, p.Color, p.Size, Math.Max(0.5, rnd.NextDouble()), 10);
        particles.RemoveAt(toRemove[i]);
      }
      else
      {
        particles.RemoveAt(toRemove[i]);
      }
    }
    // 3. Generate new ones if there is space.
    //int toGenerate = Math.Min(MaxParticles - particles.Count, (int)(dt * ParticleRate));
  }

  public int FillBuffer (float[] buffer)
  {
    int i = 0;
    foreach (var p in particles)
      p.FillBuffer(buffer, ref i);

    return particles.Count;
  }

  public void Reset()
  {
    particles.Clear();
    GenerateLauncher(5);
  }

  public void ChangeTimeScale(float timeScale)
  {
    foreach (var p in particles)
      p.timeScale = timeScale;
    TimeScale = timeScale;
  }
}

internal class Program
{
  // System objects.
  private static IWindow? window;
  private static GL? Gl;

  // VB locking (too lousy?)
  private static object renderLock = new();

  // Window size.
  private static float width;
  private static float height;

  // Trackball.
  private static Trackball? tb;

  // FPS counter.
  private static FPS fps = new();

  // Scene dimensions.
  private static Vector3 sceneCenter = Vector3.Zero;
  private static float sceneDiameter = 1.5f;

  // Global 3D data buffer.
  private const int MAX_VERTICES = 65536;
  private const int VERTEX_SIZE = 12;     // x, y, z, R, G, B, Nx, Ny, Nz, s, t, size

  /// <summary>
  /// Current dynamic vertex buffer in .NET memory.
  /// Better idea is to store the buffer on GPU and update it every frame.
  /// </summary>
  private static float[] vertexBuffer = new float[MAX_VERTICES * VERTEX_SIZE];

  /// <summary>
  /// Current number of vertices to draw.
  /// </summary>
  private static int vertices;

  public static int maxParticles;
  public static double particleRate = 1000.0;

  private static BufferObject<float>? Vbo;
  private static VertexArrayObject<float>? Vao;

  // Texture.
  private static Util.Texture? texture;
  private static bool useTexture = false;
  private static string textureFile = ":check:";
  private const int TEX_SIZE = 128;

  // Lighting.
  private static bool usePhong = false;

  // Shader program.
  private static ShaderProgram? ShaderPrg;

  private static double nowSeconds = FPS.NowInSeconds;
  private static double timeMultiplier = 1.0;

  // Particle simulation system.
  private static Simulation? sim;


  private static string WindowTitle()
  {
    StringBuilder sb = new("08-Fireworks");

    if (sim != null)
    {
      sb.Append(string.Format(CultureInfo.InvariantCulture, " [{0} of {1}], rate={2:f0}", sim.Particles, sim.MaxParticles, sim.ParticleRate));
    }

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
        maxParticles = Math.Min(MAX_VERTICES, o.Particles);
        particleRate = o.ParticleRate;

        window.Run();
      });
  }

  private static void VaoPointers()
  {
    Debug.Assert(Vao != null);
    Vao.Bind();
    Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  0);
    Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  3);
    Vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  6);
    Vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, VERTEX_SIZE,  9);
    Vao.VertexAttributePointer(4, 1, VertexAttribPointerType.Float, VERTEX_SIZE, 11);
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

    Gl = GL.GetApi(window);

    lock (renderLock)
    {
      // Initialize the simulation object and fill the VB.
      sim = new Simulation(nowSeconds, particleRate, maxParticles,  5);
      vertices = sim.FillBuffer(vertexBuffer);

      // Vertex Array Object = Vertex buffer + Index buffer.
      Vbo = new BufferObject<float>(Gl, vertexBuffer, BufferTargetARB.ArrayBuffer);
      Vao = new VertexArrayObject<float>(Gl, Vbo);
      VaoPointers();

      // Initialize the shaders.
      ShaderPrg = new ShaderProgram(Gl, "vertex.glsl", "fragment.glsl");

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

  //scaling for mouse movement
  private static float mouseCx =  0.001f;

  private static float mouseCy = -0.001f;

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

  private static void OnResize(Vector2D<int> newSize)
  {
    width  = newSize[0];
    height = newSize[1];
    SetupViewport();
  }

  private static unsafe void OnRender(double obj)
  {
    Debug.Assert(Gl != null);
    Debug.Assert(ShaderPrg != null);
    Debug.Assert(tb != null);

    Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

    lock (renderLock)
    {
      // Simulation the particle system.
      nowSeconds = FPS.NowInSeconds;
      if (sim != null)
      {
        sim.SimulateTo(nowSeconds);
        vertices = sim.FillBuffer(vertexBuffer);
      }

      // Rendering properties (set in every frame for clarity).
      Gl.Enable(GLEnum.DepthTest);
      Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
      Gl.Disable(GLEnum.CullFace);
      Gl.Enable(GLEnum.VertexProgramPointSize);

      // Draw the scene (set of Object-s).
      VaoPointers();
      ShaderPrg.Use();

      // Shared shader uniforms - matrices.
      ShaderPrg.TrySetUniform("view", tb.View);
      ShaderPrg.TrySetUniform("projection", tb.Projection);
      ShaderPrg.TrySetUniform("model", Matrix4.Identity);

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

      // Draw the particle system.
      vertices = (sim != null) ? sim.FillBuffer(vertexBuffer) : 0;

      if (Vbo != null &&
          vertices > 0)
      {
        Vbo.UpdateData(vertexBuffer, 0, vertices * VERTEX_SIZE);

        // Draw the batch of points.
        Gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)vertices);

        // Update Pps.
        fps.AddPrimitives(vertices);
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
  /// Handler for window close event.
  /// </summary>
  private static void OnClose()
  {
    Vao?.Dispose();
    ShaderPrg?.Dispose();

    // Remember to dispose the textures.
    texture?.Dispose();
  }

  private static int shiftDown = 0;

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
      case Key.KeypadAdd:
        timeMultiplier *= 2;
        sim.ChangeTimeScale((float)timeMultiplier);
        break;
      case Key.KeypadSubtract:
        timeMultiplier /= 2;
        sim.ChangeTimeScale((float)timeMultiplier);
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

      case Key.R:
        // Reset the simulator.
        if (sim != null)
        {
          sim.Reset();
          Ut.Message("Simulator reset");
        }
        break;

      case Key.Up:
        // Increase particle generation rate.
        if (sim != null)
        {
          sim.ParticleRate *= 1.1;
          SetWindowTitle();
        }
        break;

      case Key.Down:
        // Decrease particle generation rate.
        if (sim != null)
        {
          sim.ParticleRate /= 1.1;
          SetWindowTitle();
        }
        break;

      case Key.F1:
        // Help.
        Ut.Message("T           toggle texture", true);
        Ut.Message("I           toggle Phong shading", true);
        Ut.Message("P           toggle perspective", true);
        Ut.Message("V           toggle VSync", true);
        Ut.Message("C           camera reset", true);
        Ut.Message("R           reset the simulation", true);
        Ut.Message("Up, Down    change particle generation rate", true);
        Ut.Message("F1          print help", true);
        Ut.Message("Esc         quit the program", true);
        Ut.Message("Mouse.left  Trackball rotation", true);
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
  //dragging variables
  private static float currentX = 0.0f;

  private static float currentY = 0.0f;

  private static bool dragging = false;

  private static void MouseDown(IMouse mouse, MouseButton btn)
  {
    if (tb != null) tb.MouseDown(mouse, btn);

    if (btn == MouseButton.Right) {
      Ut.MessageInvariant($"Right button down: {mouse.Position}");

      dragging = true;
      currentX = mouse.Position.X;
      currentY = mouse.Position.Y;
    }
  }

  private static void MouseUp(IMouse mouse, MouseButton btn)
  {
    if (tb != null) tb.MouseUp(mouse, btn);

    if (btn == MouseButton.Right) {
      Ut.MessageInvariant($"Right button up: {mouse.Position}");
      dragging = false;
    }
  }

  private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
  {
    if (tb != null)
      tb.MouseMove(mouse, xy);

    if (mouse.IsButtonPressed(MouseButton.Right)) {
      Ut.MessageInvariant($"Mouse drag: {xy}");
    }

    if (dragging) {
      float newX = mouse.Position.X;
      float newY = mouse.Position.Y;

      if (newX != currentX || newY != currentY) {
        currentX = newX;
        currentY = newY;
      }
    }
  }

  private static void MouseDoubleClick(IMouse mouse, MouseButton btn, System.Numerics.Vector2 xy)
  {
    if (btn == MouseButton.Right) {
      Ut.Message("Closed by double-click.", true);
      window?.Close();
    }
  }

  private static void MouseScroll(IMouse mouse, ScrollWheel wheel)
  {
    if (tb != null) {
      tb.MouseWheel(mouse, wheel);
      SetWindowTitle();
    }

    // wheel.Y is -1 or 1
    Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
  }
}
