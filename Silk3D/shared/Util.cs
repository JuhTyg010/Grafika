using System.Text;
// ReSharper disable once InconsistentNaming

namespace Util;

/// <summary>
/// Assorted utilities.
/// </summary>
public class Ut
{
  /// <summary>
  /// How many characters of the previous message should be overwritten.
  /// </summary>
  private static int messageLength = 0;

  public static void Message(string msg, bool permanent = false)
  {
    StringBuilder sb = new(msg);

    // Finalizing.
    int newLen = sb.Length;
    if (newLen < messageLength)
      sb.Append(new string(' ', messageLength - newLen));
    Console.Write('\r' + sb.ToString());
    if (permanent)
    {
      Console.WriteLine();
      messageLength = 0;
    }
    else
      messageLength = newLen;
  }

  public static void MessageInvariant(FormattableString msg, bool permanent = false)
  {
    Message(FormattableString.Invariant(msg), permanent);
  }
}

public class FPS
{
  private int frameCounter = 0;

  private long primitiveCounter = 0L;

  private double lastFpsTime = 0.0;

  private double currentFps = 0.0;

  private double currentPps = 0.0;

  private object _lock = new();

  private static long ticks0 = DateTime.Now.Ticks;

  public static long NowInTicks => DateTime.Now.Ticks - ticks0;

  public static double NowInSeconds => (DateTime.Now.Ticks - ticks0) * 1.0e-7;

  public double Fps
  {
    get
    {
      lock (_lock)
      {
        return currentFps;
      }
    }
  }

  public double Pps
  {
    get
    {
      lock (_lock)
      {
        return currentPps;
      }
    }
  }

  /// <summary>
  /// Recomputes FPS value as well, should be called after AddPrimitives()
  /// </summary>
  /// <param name="frames">How many frames passed since the last call of AddFrames()</param>
  /// <returns>True if FPS/PPS changed.</returns>
  public bool AddFrames(int frames = 1)
  {
    lock(_lock)
    {
      frameCounter += frames;
      double now = NowInSeconds;

      if (now - lastFpsTime < 0.5)
        return false;

      currentFps = 0.2 * currentFps + 0.8 * (frameCounter     / (now - lastFpsTime));
      currentPps = 0.2 * currentPps + 0.8 * (primitiveCounter / (now - lastFpsTime));
      lastFpsTime = now;
      frameCounter = 0;
      primitiveCounter = 0L;
      return true;
    }
  }

  /// <summary>
  /// Updates the internal state only, for FPS recomputation you should call AddFrames().
  /// </summary>
  /// <param name="primitives">How many primitives were processed.</param>
  public void AddPrimitives(long primitives)
  {
    lock(_lock)
    {
      primitiveCounter += primitives;
    }
  }

  public void Reset()
  {
    lock (_lock)
    {
      currentFps =
      currentPps = 0.0;
      frameCounter = 0;
      primitiveCounter = 0L;
    }
  }
}
