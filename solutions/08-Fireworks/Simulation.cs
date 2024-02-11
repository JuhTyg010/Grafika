using Silk.NET.Maths;


namespace _08_Fireworks;

using Vector3 = Vector3D<float>;
using Vector2 = Vector2D<float>;

public class Simulation
{
  private List<Particle> particles = new();

  private readonly double AirFriction = .5;
  private readonly double Gravity = 9.81;
  public int Particles => particles.Count;

  public float TimeScale { get; private set; }
  public int MaxParticles { get; private set; }

  private double SimulatedTime;
  private int LaunchCount;
  public double ParticleRate { get; set; }

  public Simulation (double now, double particleRate, int maxParticles, int initParticles, float timeScale = 1.0f)
  {
    SimulatedTime = now;
    ParticleRate = particleRate;
    MaxParticles = maxParticles;
    TimeScale = timeScale;
    LaunchCount = 0;
    GenerateLaunchers(initParticles);
  }


  private void GenerateExplode(int number,Transform transform, Vector3 color, Vector3 velocity, double age)
  {
    float scale = transform.Scale/2;
    Random rnd = new();
    float theta;
    float phi;
    if (number <= 0)
      return;

    for (int i = 0; i < number; i++)
    {
      theta = (float)(rnd.NextDouble() * 2 * Math.PI);
      phi = (float)(rnd.NextDouble() * Math.PI);
      transform.Rotation = new Vector2(theta, phi);
      //transform.Scale = scale;
      transform.Weight = .05f;
      Particle p = new FlameParticle(SimulatedTime, transform.copy(), color,  velocity, age);
      p.timeScale = TimeScale;
      particles.Add(p);
    }
  }
  private void GenerateLaunchers(int number)
  {
    if (number <= 0)
      return;
    while (number-- > 0)
    {
      // Generate one new particle.
      GenerateLauncher();
    }
  }

  public void GenerateLauncher()
  {
    if (LaunchCount > 10) return;
    LaunchCount++;
    Random rnd = new();
    Transform transform = new (new Vector3((float)rnd.NextDouble(), -1, (float)rnd.NextDouble()), new Vector2(1, 0), 10, 0);
    Particle p = new Launcher(SimulatedTime, transform, new Vector3(1, 0, 0), new Vector3(0, 0, 0), rnd.Next(1,5));
    p.timeScale = TimeScale;
    particles.Add(p);
  }

  private int FindLauncher (int n = 0)
  {
    for (int i = 0; i < particles.Count; i++)
    {
      if (particles[i] is Launcher)
      {
        if (n <= 0) return i;
        n--;
      }
    }

    return -1;
  }

  public void RemoveLauncher() {
    int i = FindLauncher();
    if (i != -1)
    {
      particles.RemoveAt(i);
      LaunchCount--;
    }
  }

  public void FireLauncher (int n)
  {
    int i = FindLauncher(n);
    if (i != -1) particles[i].TimeToLive = 0;
  }

  public void SimulateTo(double time)
  {
    Random rnd = new();
    if(time <= SimulatedTime)
      return;

    List<int> toRemove = new();
    List<Particle> toAdd = new();
    for(int i = 0; i < particles.Count; i++)
    {
      if (!particles[i].SimulateTo(time, Gravity, AirFriction))
        toRemove.Add(i);
      else if (particles[i] is RocketParticle)
      {
        particles[i].AddForce(new Vector3(0, .16f, 0) * TimeScale);
        toAdd.Add(particles[i]);
      }
    }
    SimulatedTime = time;
    for(int i = toRemove.Count - 1; i >= 0; i--)
    {
      if(particles[toRemove[i]] is Launcher)
      {
        Vector3 velocity = new Vector3(1, 1, 1) * 1.7f;
        double theta = rnd.NextDouble() * 2 * Math.PI;
        double phi = rnd.NextDouble() * Math.PI / 8;
        Transform transform = particles[toRemove[i]].transform.copy();
        transform.Weight = 1f;
        transform.Scale = 4;
        transform.Rotation = new Vector2((float)theta, (float)phi);
        Particle p = new RocketParticle(time, transform,
          new Vector3((float)rnd.NextDouble(),(float)rnd.NextDouble(),(float)rnd.NextDouble()), velocity, 1.5);
        p.timeScale = TimeScale;
        particles.Add(p);
      }
      else if(particles[toRemove[i]] is RocketParticle)
      {
        Particle p = particles[toRemove[i]];
        GenerateExplode(400, p.transform, p.Color, new Vector3(1,1,1) * (float)Math.Max(0.5, rnd.NextDouble()), 10);
        particles.RemoveAt(toRemove[i]);
      }
      else
      {
        particles.RemoveAt(toRemove[i]);
      }
    }
    // 3. Generate new ones if there is space.
    foreach (var particle in toAdd)
    {
      GenerateExplode(1, particle.transform.copy(), particle.Color, new Vector3(1,1,1)*.2f, .1);
    }

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
    LaunchCount = 0;  
    GenerateLaunchers(5);
  }

  public void ChangeTimeScale(float timeScale)
  {
    foreach (var p in particles)
      p.timeScale = timeScale;
    TimeScale = timeScale;
  }
}
