using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;


namespace _08_Fireworks;
using Vector3 = Vector3D<float>;


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
