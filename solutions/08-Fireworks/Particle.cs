using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;


namespace _08_Fireworks;
using Vector3 = Vector3D<float>;
using Vector2 = Vector2D<float>;

abstract class Particle
{
  public Transform transform { get; set; }
  public Vector3 Color { get; protected set;}

  public float timeScale;
  public Vector3 Velocity { get; protected set;}

  public double TimeToLive { get; set; }
  public  double SimulatedTime { get; protected set; }

  public Particle()
  {
    transform = new Transform(new Vector3(0, 0, 0), new Vector2(0, 0), 1, 1);
    Color = new Vector3(1, 1, 1);
    Velocity = new Vector3(0, 0, 0);
    TimeToLive = 0;
  }
  public Particle (double now, Transform transform, Vector3 color, Vector3 velocity, double timeToLive, Func<string> onDeath = null)
  {
    SimulatedTime = now;
    this.transform = transform;
    Color = color;
    Velocity = velocity;
    TimeToLive = timeToLive;
  }
  abstract public bool SimulateTo(double time, double gravity, double friction);
  abstract public void FillBuffer(float[] buffer, ref int i);

  protected double Magnitude(Vector3 v) {
    return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
  }

  protected Vector3 Normalize (Vector3 v)
  {
    return v / (float) Magnitude(v);
  }

  protected Vector3 ApplyGravity (double gravity, double dt) {
    return new Vector3(0, (float)-(gravity * transform.Weight * dt), 0);
  }

  protected Vector3 ApplyFriction (double friction, Vector3 velocity, double dt)
  {
    return -Normalize(velocity) * (float)(friction * dt);
  }

  protected Vector3 CalculateVelocity (double dt, double gravity, double friction)
  {

    Velocity += ApplyGravity(gravity, dt);
    Velocity += ApplyFriction(friction, Velocity, dt);
    return Velocity * (float)dt;
  }
  protected Vector3 Rebase(Vector3 v, Vector2 rotation)
  {
    return new Vector3(
      v.X * MathF.Cos(rotation.X) * MathF.Sin(rotation.Y),
      v.Y * MathF.Cos(rotation.Y),
      v.Z * MathF.Sin(rotation.X) * MathF.Sin(rotation.Y)
    );
  }

  public void AddForce(Vector3 force)
  {
    Velocity += Rebase(force, transform.Rotation);
  }
}

class FlameParticle : Particle
{
  private double wholeTime;
  public FlameParticle(double now, Transform transform, Vector3 color, Vector3 velocity, double timeToLive)
  {
    this.transform = transform;
    Color = color;
    Velocity = Rebase(velocity, transform.Rotation);
    TimeToLive = timeToLive;
    wholeTime = 1;
    SimulatedTime = now;
  }

  public override bool SimulateTo (double time, double gravity, double friction)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;
    //dont care about the age the velocity changes over time and the color and size when is smaller than constant remove,
    //also if the magnitude of the velocity is smaller than 0.5 and also if size is smaller than 0.1
    TimeToLive -= dt;
    if (TimeToLive <= 0.0) return false;
    if ( transform.Scale < .7 || Magnitude(Color) < 0.2) return false;

    transform.Position += CalculateVelocity(dt, gravity, friction);

    // Change particle color.
    //TODO chenge color and size based on speed

    Color *= .98f;
    transform.Scale -=  (float) (wholeTime * dt);
    wholeTime += .2;


    return true;
  }

  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = transform.Position.X;
    buffer[i++] = transform.Position.Y;
    buffer[i++] = transform.Position.Z;

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
    buffer[i++] = transform.Scale;
  }
}

class RocketParticle : Particle
{

  /// <summary>
  /// Create a new particle.
  /// </summary>
  public RocketParticle (double now, Transform transform, Vector3 color, Vector3 velocity, double timeToLive)
  {
    Color = color;
    this.transform = transform;
    Velocity = Rebase(velocity, transform.Rotation);
    TimeToLive = timeToLive;
    SimulatedTime = now;
  }

  public override bool SimulateTo (double time, double gravity, double friction)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;

    TimeToLive -= dt;
    if (TimeToLive <= 0.0)
      return false;

    transform.Position += CalculateVelocity(dt, gravity, friction);
    return true;
  }
  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = transform.Position.X;
    buffer[i++] = transform.Position.Y;
    buffer[i++] = transform.Position.Z;

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
    buffer[i++] = transform.Scale;
  }
}

class Launcher : Particle
{
  private double delta;

  public Launcher(double now, Transform transform, Vector3 color, Vector3 velocity, double timeToLive)
  {
    this.transform = transform;
    Color = color;
    Velocity = velocity;
    TimeToLive = timeToLive;
    SimulatedTime = now;
    delta = timeToLive;
  }

  public override bool SimulateTo (double time, double gravity, double friction)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    dt *= timeScale;
    SimulatedTime = time;

    TimeToLive -= dt;
    if (TimeToLive <= 0.0)
    {
      TimeToLive = delta;
      return false;
    }

    return true;
  }

  public override void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = transform.Position.X;
    buffer[i++] = transform.Position.Y;
    buffer[i++] = transform.Position.Z;

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
    buffer[i++] = transform.Scale;
  }
}
