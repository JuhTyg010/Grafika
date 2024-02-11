using Silk.NET.Maths;


namespace _08_Fireworks;

using Vector3 = Vector3D<float>;

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
      else if (particles[i] is RocketParticle)
      {
        //TODO animate trajectory
        double theta = rnd.NextDouble() * 2 * Math.PI;
        double phi = rnd.NextDouble() * Math.PI / 8;
        Vector3 blur = new Vector3((float)(Math.Sin(phi) * Math.Cos(theta)), (float)Math.Cos(phi), (float)(Math.Sin(phi) * Math.Sin(theta)));
        blur /= 2;
        Particle p = new FlameParticle(time, particles[i].Position, particles[i].Color, particles[i].Size/2,
          particles[i].Velocity - blur, rnd.NextDouble(), particles[i].Weight*2, AirFriction);
        p.timeScale = TimeScale;
        particles.Add(p);
      }
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
