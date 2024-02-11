using Silk.NET.Maths;

namespace _08_Fireworks;
using Vector3 = Vector3D<float>;
using Vector2 = Vector2D<float>;

public class Transform
{
  public Vector3 Position { get; set; }
  public Vector2 Rotation { get; set; }
  public float Scale { get; set; }
  public float Weight { get; set; }


  public Transform(Vector3 position, Vector2 rotation, float scale, float weight)
  {
    Position = position;
    Rotation = rotation;
    Scale = scale;
    Weight = weight;
  }

  public Transform copy()
  {
    Transform t = new (Position, Rotation, Scale, Weight);
    return t;
  }

}
