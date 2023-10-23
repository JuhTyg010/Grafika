using System.Diagnostics;

namespace _03_SFC;

// Callback function for the Pass() functions.
using PixelAction = Action<int, int>;

/// <summary>
/// API for the pixel orders.
/// </summary>
public interface IPixelOrder
{
  /// <summary>
  /// Passes all pixels from the rectangle [0,0]-[width-1,height-1]
  /// </summary>
  /// <param name="width">Rectangle width.</param>
  /// <param name="height">Rectangle height.</param>
  void Pass(int width, int height);
}

/// <summary>
/// Support class - inherit your own methods from it.
/// </summary>
public abstract class DefaultPixelOrder : IPixelOrder
{
  /// <summary>
  /// Function called for every pixel.
  /// </summary>
  protected PixelAction Callback;

  /// <summary>
  /// Constructors will be used for callback function definitions.
  /// </summary>
  /// <param name="callback">Callback function.</param>
  protected DefaultPixelOrder(PixelAction callback) => Callback = callback;

  /// <summary>
  /// Passes the given rectangle, calls the Callback() function for every pixel.
  /// The Callback() function should be called exactly width * height times.
  /// </summary>
  /// <param name="width">Rectangle width.</param>
  /// <param name="height">Rectangle height.</param>
  public abstract void Pass(int width, int height);
}

/// <summary>
/// Pilot pixel ordering: ScanLine order.
/// You could create your own methods in a similar way.
/// </summary>
public class ScanLine : DefaultPixelOrder
{
  public ScanLine(PixelAction callback) : base(callback)
  {}

  /// <summary>
  /// Passes the given rectangle, calls the Callback() function for every pixel.
  /// The Callback() function should be called exactly width * height times.
  /// </summary>
  /// <param name="width">Rectangle width.</param>
  /// <param name="height">Rectangle height.</param>
  public override void Pass(int width, int height)
  {
    Debug.Assert(Callback != null);

    for (int y = 0; y < height; y++)
      for (int x = 0; x < width; x++)
        Callback(x, y);
  }
}
