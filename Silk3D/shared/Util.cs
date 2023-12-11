using System.Text;
// ReSharper disable once InconsistentNaming

namespace Util;

/// <summary>
/// Assorted utilities.
/// </summary>
public class Util
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
