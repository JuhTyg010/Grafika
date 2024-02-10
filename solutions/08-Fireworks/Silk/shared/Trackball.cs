using System;
using Silk.NET.Input;
using Silk.NET.Maths;
// ReSharper disable InconsistentNaming
// ReSharper disable VirtualMemberCallInConstructor

namespace Util;

using Vector3 = Vector3D<float>;
using Vector3d = Vector3D<double>;
using Matrix4 = Matrix4X4<float>;

/// <summary>
/// Camera for realtime (OpenGL) applications, animation, etc.
/// </summary>
public interface IDynamicCamera
{
  /// <summary>
  /// Center of the scene in world coordinates (rotation center if applicable).
  /// </summary>
  Vector3 Center { get; set; }

  /// <summary>
  /// Scene diameter in world coordinates (for default zoom).
  /// </summary>
  float Diameter { get; set; }

  /// <summary>
  /// Zoom factor (multiplication).
  /// </summary>
  float Zoom { get; set; }

  /// <summary>
  /// Zoom factor lower bound (if applicable).
  /// </summary>
  float MinZoom { get; set; }

  /// <summary>
  /// Zoom factor upper bound (if applicable).
  /// </summary>
  float MaxZoom { get; set; }

  /// <summary>
  /// Camera time (in seconds).
  /// </summary>
  double Time { get; set; }

  /// <summary>
  /// Camera time lower bound (in seconds).
  /// </summary>
  double MinTime { get; set; }

  /// <summary>
  /// Camera time upper bound (in seconds).
  /// </summary>
  double MaxTime { get; set; }

  /// <summary>
  /// Update the camera instance.
  /// </summary>
  /// <param name="param">Text param from the UI.</param>
  /// <param name="cameraFile">Optional camera definition file.</param>
  void Update (string param, string cameraFile);

  /// <summary>
  /// Resets the camera (whatever it means).
  /// </summary>
  void Reset ();

  /// <summary>
  /// Called every time a viewport is changed.
  /// It is possible to ignore some arguments in case of scripted camera.
  /// </summary>
  /// <param name="width">Viewport width in pixels.</param>
  /// <param name="height">Viewport height in pixels.</param>
  /// <param name="near">Near frustum distance if applicable.</param>
  /// <param name="far">Far frustum distance if applicable.</param>
  void ViewportChange (int width, int height, float near = 0.01f, float far = 1000.0f);

  /// <summary>
  /// Gets a current view transformation matrix.
  /// </summary>
  Matrix4 View { get; }

  /// <summary>
  /// Gets inverted view transformation matrix.
  /// </summary>
  Matrix4 ViewInv { get; }

  /// <summary>
  /// Perspective / orthographic projection?
  /// </summary>
  bool UsePerspective { get; set; }

  /// <summary>
  /// Gets a current projection matrix.
  /// </summary>
  Matrix4 Projection { get; }

  /// <summary>
  /// Gets a current eye/camera position in world coordinates.
  /// </summary>
  Vector3 Eye { get; }

  /// <summary>
  /// Vertical field-of-view angle in radians.
  /// </summary>
  float Fov { get; set; }

  /// <summary>
  /// Handle keyboard-key down.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool KeyDown (IKeyboard keyboard, Key key, int arg3);

  /// <summary>
  /// Handle keyboard-key up.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool KeyUp (IKeyboard keyboard, Key key, int arg3);

  /// <summary>
  /// Handles mouse-button down.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool MouseDown (IMouse mouse, MouseButton button);

  /// <summary>
  /// Handles mouse-button up.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool MouseUp (IMouse mouse, MouseButton button);

  /// <summary>
  /// Handles mouse move.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool MouseMove (IMouse mouse, System.Numerics.Vector2 position);

  /// <summary>
  /// Handles mouse-wheel change.
  /// </summary>
  /// <returns>True if handled.</returns>
  bool MouseWheel (IMouse mouse, ScrollWheel scrollWheel);
}

/// <summary>
/// Basic IDynamicCamera implementation with reasonable properties/functions.
/// It can save your effort if you derive your own class from it.
/// </summary>
public class DefaultDynamicCamera : IDynamicCamera
{
  /// <summary>
  /// Center of the scene in world coordinates (rotation center if applicable).
  /// </summary>
  public virtual Vector3 Center { get; set; }

  /// <summary>
  /// Scene diameter in world coordinates (for default zoom).
  /// </summary>
  public virtual float Diameter { get; set; } = 5.0f;

  /// <summary>
  /// Zoom factor (multiplication).
  /// </summary>
  public virtual float Zoom { get; set; } = 1.0f;

  /// <summary>
  /// Zoom factor lower bound (if applicable).
  /// </summary>
  public virtual float MinZoom { get; set; }

  /// <summary>
  /// Zoom factor upper bound (if applicable).
  /// </summary>
  public virtual float MaxZoom { get; set; }

  /// <summary>
  /// Camera time (in seconds).
  /// </summary>
  public virtual double Time { get; set; } = 0.0;

  /// <summary>
  /// Camera time lower bound (in seconds).
  /// </summary>
  public virtual double MinTime { get; set; } = 0.0;

  /// <summary>
  /// Camera time upper bound (in seconds).
  /// </summary>
  public virtual double MaxTime { get; set; } = 1.0;

  /// <summary>
  /// Update the camera instance.
  /// </summary>
  /// <param name="param">Text param from the UI.</param>
  /// <param name="cameraFile">Optional camera definition file.</param>
  public virtual void Update (string param, string cameraFile)
  {
  }

  /// <summary>
  /// Resets the camera (whatever it means).
  /// </summary>
  public virtual void Reset ()
  {
    Time = MinTime;
  }

  /// <summary>
  /// Called every time a viewport is changed.
  /// It is possible to ignore some arguments in case of scripted camera.
  /// </summary>
  /// <param name="width">Viewport width in pixels.</param>
  /// <param name="height">Viewport height in pixels.</param>
  /// <param name="near">Near frustum distance if applicable.</param>
  /// <param name="far">Far frustum distance if applicable.</param>
  public virtual void ViewportChange (int width, int height, float near = 0.01f, float far = 1000.0f)
  {}

  /// <summary>
  /// Gets a current view transformation matrix.
  /// </summary>
  public virtual Matrix4 View { get; }

  /// <summary>
  /// Gets a current view transformation matrix.
  /// </summary>
  public virtual Matrix4 ViewInv { get; }

  /// <summary>
  /// Perspective / orthographic projection?
  /// </summary>
  public virtual bool UsePerspective { get; set; } = true;

  /// <summary>
  /// Gets a current projection matrix.
  /// </summary>
  public virtual Matrix4 Projection { get; }

  /// <summary>
  /// Gets a current eye/camera position in world coordinates.
  /// </summary>
  public virtual Vector3 Eye => new (ViewInv.M41, ViewInv.M42, ViewInv.M43);

  /// <summary>
  /// Vertical field-of-view angle in radians.
  /// </summary>
  public virtual float Fov { get; set; } = 1.0f;

  /// <summary>
  /// Handle keyboard-key down.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool KeyDown (IKeyboard keyboard, Key key, int arg3)
  {
    return false;
  }

  /// <summary>
  /// Handle keyboard-key up.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool KeyUp (IKeyboard keyboard, Key key, int arg3)
  {
    return false;
  }

  /// <summary>
  /// Handles mouse-button down.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool MouseDown (IMouse mouse, MouseButton button)
  {
    return false;
  }

  /// <summary>
  /// Handles mouse-button up.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool MouseUp (IMouse mouse, MouseButton button)
  {
    return false;
  }

  /// <summary>
  /// Handles mouse move.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool MouseMove (IMouse mouse, System.Numerics.Vector2 position)
  {
    return false;
  }

  /// <summary>
  /// Handles mouse-wheel change.
  /// </summary>
  /// <returns>True if handled.</returns>
  public virtual bool MouseWheel (IMouse mouse, ScrollWheel wheel)
  {
    return false;
  }
}

/// <summary>
/// Trackball interactive 3D scene navigation
/// Original code: Matyas Brenner
/// </summary>
public class Trackball : DefaultDynamicCamera
{
  class Ellipse
  {
    private float   a, b, c;
    private Vector3 center;

    // Sphere constructor
    public Ellipse (float r, Vector3 center) : this(r, r, r, center)
    {}

    // Ellipse constructor
    public Ellipse (float a, float b, float c, Vector3 center)
    {
      this.a = a;
      this.b = b;
      this.c = c;
      this.center = center;
    }

    // "polar coordinates" method
    public Vector3 IntersectionI (float x, float y)
    {
      Vector3d o = new(0, 0, -c);
      Vector3d m = new(x - center.X, y - center.Y, c);
      Vector3d v = o - m;
      v = Vector3D.Normalize(v);
      double A = v.X * v.X * b * b * c * c + v.Y * v.Y * a * a * c * c + v.Z * v.Z * a * a * b * b;
      double B = 2 * (v.X * b * b * c * c + v.Y * a * a * c * c + v.Z * a * a * b * b);
      double C = v.X * v.X * b * b * c * c + v.Y * v.Y * a * a * c * c + v.Z * a * a * b * b - a * a * b * b * c * c;
      double D = Math.Sqrt(B * B - 4 * A * C);
      double t = (-B - D) / (2 * A);
      double X = m.X + t * v.X;
      double Y = m.Y + t * v.Y;
      double Z = m.Z + t * v.Z;
      return new Vector3((float)X, -(float)Y, (float)Z);
    }

    // "parallel rays" method
    public Vector3? Intersection (float x, float y, bool restricted)
    {
      x -= center.X;
      y -= center.Y;

      if ((x < -a) || (x > a) || (y < -b) || (y > b))
      {
        float x1 = (float)Math.Sqrt(a * a * b * b * y * y / (b * b * y * y + x * x));
        float x2 = -x1;
        float y1 = y * x1 / -x;
        float y2 = y * x2 / -x;
        if (Math.Abs(x - x1) < Math.Abs(x - x2))
          return new Vector3(x1, y1, 0);
        else
          return new Vector3(x2, y2, 0);
      }

      float z = (1.0f - x * x / (a * a) - y * y / (b * b)) * c * c;
      if (z < 0)
        return null;
      z = (float)Math.Sqrt(z);
      return new Vector3(x, -y, z);
    }
  }

  private readonly Vector3 absoluteUp = Vector3.UnitY;

  /// <summary>
  /// Camera RIGHT vector - not rotated (parallel to world axes)
  /// </summary>
  public Vector3 Right => Vector3D.Normalize(Vector3D.Cross(absoluteUp, Direction));

  /// <summary>
  /// Camera UP vector - not rotated (parallel to world axes)
  /// </summary>
  public Vector3 Up => Vector3D.Normalize(Vector3D.Cross(Direction, Right));

  /// <summary>
  /// Camera direction vector
  /// </summary>
  public Vector3 Direction => Vector3D.Normalize(Center - Eye);

  /// <summary>
  /// Which mouse button is used for trackball movement?
  /// </summary>
  public MouseButton Button { get; set; }

  public Trackball (Vector3 cent, float diam = 5.0f)
  {
    Center = cent;
    Diameter = diam;
    MinZoom = 0.05f;
    MaxZoom = 100.0f;
    Zoom = 1.0f;
    UsePerspective = true;
    Button = MouseButton.Left;
  }

  private Matrix4 prevRotation = Matrix4.Identity;
  private Matrix4 rotation     = Matrix4.Identity;

  private Ellipse? ellipse;
  private Vector3? a, b;

  private Matrix4 perspectiveProjection;
  private Matrix4 orthographicProjection;

  public override Matrix4 Projection => UsePerspective ? perspectiveProjection : orthographicProjection;

  /// <summary>
  /// Called every time a viewport is changed.
  /// </summary>
  public override void ViewportChange (int width, int height, float near = 0.01f, float far = 1000.0f)
  {
    // 1. set projection matrix
    // 1a. perspective
    if (float.IsPositiveInfinity(far))
    {
      float viewAngleVertical = (float)(Fov * 180 / Math.PI);
      float f = (float)(1.0 / Math.Tan(viewAngleVertical / 2.0));
      float aspect = width / (float)height;

      //perspectiveProjection = new Matrix4(focalLength, 0, 0, 0, 0, focalLength / ratio, 0, 0, 0, 0, -1, -2 * near, 0, 0, -1, 0);
      perspectiveProjection = new Matrix4(
        f / aspect, 0.0f,  0.0f,        0.0f,
        0.0f,       0.0f,  0.0f,        0.0f,
        0.0f,       0.0f, -1.0f,       -1.0f,
        0.0f,       0.0f, -2.0f * near, 0.0f);
    }
    else
    {
      perspectiveProjection = Matrix4X4.CreatePerspectiveFieldOfView(Fov, width / (float)height, near, far);
    }

    // 2b. orthographic
    float minSize = Math.Min(width, height);
    orthographicProjection = Matrix4X4.CreateOrthographic(
      width / minSize,    // the rest of the scaling is done in View (Zoom / Diameter)
      height / minSize,
      near, far);
    setEllipse(width, height);
  }

  public override Matrix4 View =>
    Matrix4X4.CreateTranslation(-Center) *
    Matrix4X4.CreateScale(Zoom / Diameter) *
    prevRotation *
    rotation *
    Matrix4X4.CreateTranslation(0.0f, 0.0f, -1.5f);

  public override Matrix4 ViewInv
  {
    get
    {
      Matrix4 rot = prevRotation * rotation;
      rot = Matrix4X4.Transpose(rot);

      return Matrix4X4.CreateTranslation(0.0f, 0.0f, 1.5f) *
             rot *
             Matrix4X4.CreateScale(Diameter / Zoom) *
             Matrix4X4.CreateTranslation(Center);
    }
  }

  /// <summary>
  /// Resets the camera - it will look in the negative Z direction. Y axis will point upwards.
  /// </summary>
  public override void Reset ()
  {
    base.Reset();
    Zoom = 1.0f;
    rotation = Matrix4.Identity;
    prevRotation = Matrix4.Identity;
  }

  /// <summary>
  /// Resets the camera by a caller-provided rotational matrix.
  /// </summary>
  /// <param name="rot">Matrix of rotation from view direction to the negative Z axis (must be matrix of rotation).</param>
  public void Reset (Matrix4 rot)
  {
    base.Reset();
    Zoom = 1.0f;
    rotation = Matrix4.Identity;
    prevRotation = rot;
  }

  /// <summary>
  /// Resets the camera to look at the required direction. Y axis will point upwards.
  /// </summary>
  /// <param name="dir">Required view direction.</param>
  public void Reset (Vector3 dir)
  {
    dir = Vector3D.Normalize(dir);
    if (dir.Length < 1.0e-6f)
    {
      Reset();
      return;
    }

    // 1. rotation around vertical (Y) axis
    float len = MathF.Sqrt(dir.X * dir.X + dir.Z * dir.Z);
    Matrix4 roty = Matrix4.Identity;
    if (len > 1.0e-6f)
    {
      roty.M11 = roty.M33   = -dir.Z / len;
      roty.M13 = -(roty.M31 = dir.X / len);
    }

    // 2. rotation around X axis (to look in the negative Z direction)
    float len2 = MathF.Sqrt(len * len + dir.Y * dir.Y);
    Matrix4 rotx = Matrix4.Identity;
    if (len2 > 1.0e-6f)
    {
      rotx.M22 = rotx.M33   = len / len2;
      rotx.M23 = -(rotx.M32 = dir.Y / len2);
    }

    Reset(roty * rotx);
  }

  private void setEllipse (int width, int height)
  {
    width  /= 2;
    height /= 2;

    ellipse = new Ellipse(Math.Min(width, height), new Vector3(width, height, 0));
  }

  /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
  private static float CalculateAngle (in Vector3 first, in Vector3 second)
  {
    float temp = Vector3D.Dot(first, second);
    return MathF.Acos(Math.Clamp(temp / (first.Length * second.Length), -1.0f, 1.0f));
  }

  private Matrix4 calculateRotation (Vector3? a, Vector3? b, bool sensitive)
  {
    if (!a.HasValue || !b.HasValue)
      return rotation;

    if (a.Value == b.Value)
      return Matrix4.Identity;

    Vector3 axis = Vector3D.Normalize(Vector3D.Cross(a.Value, b.Value));
    float angle = CalculateAngle(a.Value, b.Value);
    if (sensitive)
      angle *= 0.4f;

    return Matrix4X4.CreateFromAxisAngle(axis, angle);
  }

  //--- GUI interaction ---

  protected System.Numerics.Vector2 current = System.Numerics.Vector2.Zero;
  protected MouseButton currentButton = MouseButton.Unknown;

  /// <summary>
  /// Handles mouse-button down.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool MouseDown (IMouse mouse, MouseButton button)
  {
    if (ellipse == null || button != Button)
      return false;

    currentButton = button;
    a = ellipse.IntersectionI(current.X, current.Y);
    return true;
  }

  /// <summary>
  /// Handles mouse-button up.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool MouseUp (IMouse mouse, MouseButton button)
  {
    if (button != Button)
      return false;

    currentButton = MouseButton.Unknown;
    prevRotation *= rotation;
    rotation = Matrix4.Identity;
    a = null;
    b = null;
    return true;
  }

  /// <summary>
  /// Handles mouse move.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool MouseMove (IMouse mouse, System.Numerics.Vector2 position)
  {
    current = position;

    if (ellipse == null || currentButton != Button)
      return false;

    b = ellipse.IntersectionI(current.X, current.Y);
    rotation = calculateRotation(a, b, false);
    return true;
  }

  /// <summary>
  /// Handles mouse-wheel change.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool MouseWheel (IMouse mouse, ScrollWheel wheel)
  {
    float dZoom = -wheel.Y;     // -wheel.Y / 120.0f;
    Zoom *= (float)Math.Pow(1.04, dZoom);

    // Zoom bounds:
    Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);
    return true;
  }

  /// <summary>
  /// Handle keyboard-key down.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool KeyDown (IKeyboard keyboard, Key key, int arg3)
  {
    switch (key)
    {
      case Key.W:
        MoveCenter(movementDirection.Forward);
        return true;

      case Key.S:
        MoveCenter(movementDirection.Backwards);
        return true;

      case Key.A:
        MoveCenter(movementDirection.Left);
        return true;

      case Key.D:
        MoveCenter(movementDirection.Right);
        return true;

      case Key.E:
        MoveCenter(movementDirection.Up);
        return true;

      case Key.Q:
        MoveCenter(movementDirection.Down);
        return true;

      case Key.R:
        MoveCenter(movementDirection.Reset);
        return true;

      default:
        return false;
    }
  }

  /// <summary>
  /// Handle keyboard-key up.
  /// </summary>
  /// <returns>True if handled.</returns>
  public override bool KeyUp (IKeyboard keyboard, Key key, int arg3)
  {
    switch (key)
    {
      case Key.O:
        return true;

      default:
        return false;
    }
  }

  private const float moveFactor = 0.5f;

  /// <summary>
  /// Moves Center by moveChange in specified direction (absolute in world coordinates)
  /// Movement is relative to current camera direction (Eye - Center)
  /// Direction.Reset sets Center to origin (0, 0, 0)
  /// </summary>
  /// <param name="movementDirection">Direction to move Center to</param>
  private void MoveCenter (movementDirection movementDirection)
  {
    Vector3 movement = Vector3.Zero;

    switch (movementDirection)
    {
      case movementDirection.Left:
        movement += Right * moveFactor;
        break;

      case movementDirection.Right:
        movement -= Right * moveFactor;
        break;

      case movementDirection.Forward:
        movement += Direction * moveFactor;
        break;

      case movementDirection.Backwards:
        movement -= Direction * moveFactor;
        break;

      case movementDirection.Up:
        movement += Up * moveFactor;
        break;

      case movementDirection.Down:
        movement -= Up * moveFactor;
        break;

      case movementDirection.Reset:
        Center = Vector3.Zero;
        break;
    }

    Center += movement;
  }

  private const float moveChange = 0.4f;

  /// <summary>
  /// Moves Center by moveChange in specified direction (absolute in world coordinates)
  /// Movement is alligned with absolute world coodinates and current position of camera does not matter
  /// Direction.Reset sets Center to origin (0, 0, 0)
  /// </summary>
  /// <param name="movementDirection">Direction to move Center to</param>
  private void MoveCenterByAxes (movementDirection movementDirection)
  {
    Vector3 movement = Vector3.Zero;

    switch (movementDirection)
    {
      case movementDirection.Left:
        movement.X = -moveChange;
        break;

      case movementDirection.Right:
        movement.X = moveChange;
        break;

      case movementDirection.Forward:
        movement.Z = -moveChange;
        break;

      case movementDirection.Backwards:
        movement.Z = moveChange;
        break;

      case movementDirection.Up:
        movement.Y = moveChange;
        break;

      case movementDirection.Down:
        movement.Y = -moveChange;
        break;

      case movementDirection.Reset:
        Center = Vector3.Zero;
        break;
    }

    Center += movement;
  }

  private enum movementDirection
  {
    Left,
    Right,
    Forward,
    Backwards,
    Up,
    Down,
    Reset
  };
}
