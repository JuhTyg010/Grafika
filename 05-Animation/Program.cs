using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace _05_Animation;

public class Options
{
  [Option('w', "width", Required = true, HelpText = "Image width in pixels.")]
  public int Width { get; set; }

  [Option('h', "height", Required = true, HelpText = "Image height in pixels.")]
  public int Height { get; set; }

  [Option('p', "fps", Required = false, Default = 30.0f, HelpText = "Frames per second.")]
  public float FPS { get; set; } = 30.0f;

  [Option('f', "frames", Required = false, Default = 60, HelpText = "Number of output frames.")]
  public int Frames { get; set; } = 60;

  [Option('o', "output", Required = false, Default = "anim/out{0:0000}.png", HelpText = "Output file-name mask.")]
  public string FileMask { get; set; } = "anim/out{0:0000}.png";
}

internal class Program
{
  // Constants for tile colors
  private static Rgba32 BackgroundColor = new(0xFF, 0xFF, 0xFF); // White background
  private static Rgba32 DrawColor       = new(0x10, 0x20, 0x80); // Blue lines

  static void Main (string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
       .WithParsed<Options>(o =>
       {
         int frames = Math.Max(10, o.Frames);  // at least 10 frames
         PointF center = new(0.5f * o.Width, 0.5f * o.Height);
         float radius = Math.Min(center.X, center.Y) * 0.9f;
         float velocity = (float)(Math.PI * 0.4);   // in "radians per second"

         for (int frame = 0; frame < frames; frame++ )
         {
           // Create a new image with the specified dimensions
           using (var image = new Image<Rgba32>(o.Width, o.Height))
           {
             float time = frame / o.FPS;
             float angle = time * velocity;
             PointF target = center + radius * new PointF((float)Math.Sin(angle), -(float)Math.Cos(angle));

             image.Mutate(i => i.Clear(BackgroundColor).DrawLine(DrawColor, 3.0f, center, target));

             // Save the frame to a file with the synthetic filename
             string fileName = string.Format(o.FileMask, frame);
             image.Save(fileName);

             Console.WriteLine($"Frame '{fileName}' created successfully.");
           }
         }
       });
  }
}
