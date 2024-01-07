using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using static _05_Animation.DrawMandala;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace _05_Animation;

public class Options
{
  [Option('w', "width", Required = true, HelpText = "Image width in pixels.")]
  public int Width { get; set; } = 400;

  [Option('h', "height", Required = true, HelpText = "Image height in pixels.")]
  public int Height { get; set; } = 400;

  [Option('p', "fps", Required = false, Default = 30.0f, HelpText = "Frames per second.")]
  public float Fps { get; set; } = 30.0f;

  [Option('f', "frames", Required = false, Default = 60, HelpText = "Number of output frames.")]
  public int Frames { get; set; } = 60;

  [Option('o', "output", Required = false, Default = "anim/out{0:0000}.png", HelpText = "Output file-name mask.")]
  public string FileMask { get; set; } = "anim/out{0:0000}.png";
}

internal class Program
{
  const int Precision = 50;
  private static readonly Rgba32 Red = new Rgba32(255, 0, 0);
  private static readonly Rgba32 Green = new Rgba32(0, 255, 0);
  private static readonly Rgba32 Blue = new Rgba32(0, 0, 255);
  private static readonly Rgba32 Yellow = new Rgba32(255, 255, 0);
  private static readonly Rgba32 Magenta = new Rgba32(255, 0, 255);
  private static readonly Rgba32 Cyan = new Rgba32(0, 255, 255);
  private static readonly Rgba32 Gray = new Rgba32(128, 128, 128);
  private static readonly Rgba32 Purple = new Rgba32(128, 0, 128);
  private static readonly Rgba32 Black = new Rgba32(0, 0, 0);
  private static readonly Rgba32 White = new Rgba32(255, 255, 255);
  enum Figures { Circles, Triangles, Diamonds, RoundedSquares, Ring }
  struct Figure {
    public Figures Type { get; set; }
    public float Start { get; private set; }
    public float End { get; private set; }
    public int Symmetry { get; set; }
    public Rgba32 Color { get; set; }
    public bool Fill { get; set; }

    public Figure(Figures type, int start, int end, int symmetry, Rgba32 color, bool fill) {
      Type = type;
      Start = start;
      End = end;
      Symmetry = symmetry;
      Color = color;
      Fill = fill;
    }
    public  void SetRadius(float start, float end) {
      Start = start;
      End = end;
    }
  }




  static void Main (string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
       .WithParsed<Options>(o =>
       {
         int frames = Math.Max(10, o.Frames);  // at least 10 frames

         //PointF center = new(0.5f * o.Width, 0.5f * o.Height);
         //float radius = Math.Min(center.X, center.Y) * 0.9f;

         Random rand = new Random();
         Vector2 center = new Vector2((float)o.Width / 2, (float)o.Height / 2);
         float maxRadius = (float)Math.Min(o.Width, o.Height) / 2;
         int symmetry = rand.Next(6, 16);

         #region Generate backgroud
         List<Rgba32> cols = new List<Rgba32> {Red, Green, Blue, Yellow, Magenta, Cyan, Purple, White, Black};
         (Rgba32, Rgba32) backgroudColors = (cols[rand.Next(cols.Count)], cols[rand.Next(cols.Count)]);
         #endregion

         List<Rgba32> colors = new List<Rgba32>();
         List<Rgba32> colorsList = new List<Rgba32> {Red, Green, Blue, Yellow, Magenta, Cyan, Purple};
         int lenght = colorsList.Count;
         for(int i = 0; i < lenght; i++) {
           int index = rand.Next(colorsList.Count);
           colors.Add(colorsList[index]);
           colorsList.Remove(colorsList[index]);
         }

         List<Figure> figures = new List<Figure>();
         List<Figures> types = new List<Figures> {Figures.Circles, Figures.Triangles, Figures.Diamonds, Figures.RoundedSquares, Figures.Ring};
         //using int from previous for-cycle cause no need for it
         lenght = types.Count;
         for(int i = 0; i < lenght; i++) {
           int index = rand.Next(types.Count);
           bool fill = rand.Next(2) == 1;
           figures.Add(new Figure(types[index], 0, 80, symmetry, colors[i % colors.Count], fill));
           types.Remove(types[index]);
         }

         //float speed = maxRadius / (figures.Count * o.FPS);
         float speed = maxRadius / (o.Fps);
         CalculateRadius radius = new CalculateRadius(maxRadius, figures.Count);
         for (int i = 0; i < figures.Count; i++) {

           Tuple<float, float> r = radius.Next();
           Figure f = figures[i];
           f.SetRadius(r.Item1, r.Item2);
           figures[i] = f;
         }
         types = new List<Figures> {Figures.Circles, Figures.Triangles, Figures.Diamonds, Figures.RoundedSquares, Figures.Ring};
         figures.Insert(0,new Figure(types[rand.Next(types.Count)], 0, 1, symmetry, colors[rand.Next(colors.Count)], rand.Next(2) == 1));

         Image<Rgba32> backImage = new Image<Rgba32>(o.Width, o.Height);
         DrawBackground(backImage, backgroudColors.Item1,backgroudColors.Item2);

         for (int j = 0; j < 5; j++) {
           for (int i = 0; i < figures.Count; i++) {
             if(i == figures.Count - 1)  figures[i] = SetRadius(figures[i].Start + speed, figures[i].End, figures[i]);
             else if (i == 0) figures[i] = SetRadius(figures[i].Start, Math.Min(maxRadius, figures[i].End + speed), figures[i]);
             else figures[i] = SetRadius(figures[i].Start + speed, Math.Min(maxRadius, figures[i].End + speed), figures[i]);
           }

           if (figures.Last().Start >= figures.Last().End) {
             Figure temp = figures.Last();
             float tempRad = figures.Last().End - figures.Last().Start;
             temp.SetRadius(tempRad,tempRad);
             figures.Insert(0,temp);
             figures.RemoveAt(figures.Count - 1); //last
           }
         }

         int frameHalf = (frames >> 1) - 1 ;


         // Generate the frames
         for (int frame = 0; frame < (frames >> 1); frame++ )
         {
           // Create a new image with the specified dimensions
           using (var image = backImage.Clone())
           {

             foreach (Figure f in figures)
             {
               switch (f.Type)
               {
                 case Figures.Circles:
                   DrawCircles(image, f.Start, f.End, f.Symmetry, f.Color, center, f.Fill);
                   break;
                 case Figures.Triangles:
                   DrawTriangles(image, f.Start, f.End, f.Symmetry, f.Color, center, 0, 1f, 0.5f, f.Fill);
                   break;
                 case Figures.Diamonds:
                   DrawDiamonds(image, f.Start, f.End, (f.Start + f.End ) / 2, f.Symmetry, f.Color, center, 0.2f, 0.8f, f.Fill);
                   break;
                 case Figures.RoundedSquares:
                   DrawRoundedSquares(image, f.Start, f.End, f.Symmetry, f.Color, center, 0.1f, 0.9f, f.Fill);
                   break;
                 case Figures.Ring:
                   DrawRing(image, f.Start, f.End, symmetry * 20, f.Color, center, f.Fill);
                   break;
                 default:
                   Console.WriteLine("Invalid figure type ignored");
                   break;
               }
             }
             // Save the frame to a file with the synthetic filename
             string fileName = string.Format(o.FileMask, frame);
             string fileName2 = string.Format(o.FileMask, frames - (frame + 1));
             image.Save(fileName);
             image.Save(fileName2);

             Console.WriteLine($"Frames '{fileName}' and '{fileName2}' created successfully.");


             Out(CustomFunction((float)frame / frameHalf) * speed, figures, maxRadius);
           }
         }
         if (frames % 2 == 1)
         {
           using (var image = backImage.Clone())
           {

             foreach (Figure f in figures)
             {
               switch (f.Type)
               {
                 case Figures.Circles:
                   DrawCircles(image, f.Start, f.End, f.Symmetry, f.Color, center, f.Fill);
                   break;
                 case Figures.Triangles:
                   DrawTriangles(image, f.Start, f.End, f.Symmetry, f.Color, center, 0, 1f, 0.5f, f.Fill);
                   break;
                 case Figures.Diamonds:
                   DrawDiamonds(image, f.Start, f.End, (f.Start + f.End) / 2, f.Symmetry, f.Color, center, 0.2f, 0.8f,
                     f.Fill);
                   break;
                 case Figures.RoundedSquares:
                   DrawRoundedSquares(image, f.Start, f.End, f.Symmetry, f.Color, center, 0.1f, 0.9f, f.Fill);
                   break;
                 case Figures.Ring:
                   DrawRing(image, f.Start, f.End, symmetry * 20, f.Color, center, f.Fill);
                   break;
                 default:
                   Console.WriteLine("Invalid figure type ignored");
                   break;
               }
             }

             // Save the frame to a file with the synthetic filename
             string fileName = string.Format(o.FileMask, (frames >> 1));
             image.Save(fileName);

             Console.WriteLine($"Frame '{fileName}' created successfully.");
           }
         }
         if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
           Console.WriteLine("OS Windows meh... Creating video...");
           Console.WriteLine("Not tested on Windows, please report any issues!!!");
           Process.Start("cmd.exe", "/C ffmpeg -r " + o.Fps + " -i " + fileMaskStr(o.FileMask) + " -f avi  -vcodec  msmpeg4v2 -y " + fileFromMask(o.FileMask, ".avi"));
         }
         else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
           Console.WriteLine("Linux, now we're talking! Creating video...");
           Process a = new Process();
           a.StartInfo.FileName = "ffmpeg";
           a.StartInfo.Arguments = "-framerate " + o.Fps + " -i " + fileMaskStr(o.FileMask) + " -f avi  -vcodec  png -y " + fileFromMask(o.FileMask, ".avi");
           a.Start();
           a.WaitForExit();

           Console.WriteLine("Video created successfully. Saved as " + fileFromMask(o.FileMask, ".avi"));
         }
         else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
           Console.WriteLine("No support for rich guys :( try better OS ;)");
         }
         else {
           Console.WriteLine("Unknown OS");
         }
       });
  }

  //used in vector of structs cause its immutable, I hate it

  static string fileFromMask(string mask, string format)
  {
    string result = "";

    for(int i = 0; i < mask.Length; i++) {
      if(mask[i] == '{') {
        break;
      }
      result += mask[i];
    }
    return result + format;
  }

  static string fileMaskStr(string mask) {
    // TODO get mask e.g. out{0:0000}.png -> out%04d.png to return
    // anim/out{0:00000}.png -> anim/out%05d.png

    string result = "";

    for(int i = 0; i < mask.Length; i++) {
      if(mask[i] == '{') {
        result += "%";
        i++;
        while(mask[i] != ':') {
          i++;
        }
        int temp = -1;
        while(mask[i] != '}') {
          i++;
          temp++;
        }
        result += "0" + temp.ToString();
        result += "d";
      }
      else {
        result += mask[i];
      }
    }
    return result;
  }
  static Figure SetRadius(float a, float b, Figure f) {
    f.SetRadius(a,b);
    return f;
  }


  static List<Figure> Out(float speed, List<Figure> input, float maxRadius)
  {
    List<Figure> figures = input;
    for (int i = 0; i < figures.Count; i++) {
      if(i == figures.Count - 1)  figures[i] = SetRadius(figures[i].Start + speed, figures[i].End, figures[i]);
      else if (i == 0) figures[i] = SetRadius(figures[i].Start, Math.Min(maxRadius, figures[i].End + speed), figures[i]);
      else figures[i] = SetRadius(figures[i].Start + speed, Math.Min(maxRadius, figures[i].End + speed), figures[i]);
    }

    if (figures.Last().Start >= figures.Last().End) {
      Figure temp = figures.Last();
      float tempRad = temp.End - temp.Start;
      temp.SetRadius(tempRad,tempRad);
      figures.Insert(0,temp);
      figures.RemoveAt(figures.Count - 1); //last
    }
    return figures;
  }


// is good but only in range 0-1 than it goes wild
  static float CustomFunction(float x)
  {
    x -= 0.5f;
    float xx = x * x;
    return 2 * ((8 * xx - 4) * xx + 0.5f);
  }

  static float EasyInOutCubic(float x) {
    return x < 0.5f ? 4 * x * x * x : 1 - (float)Math.Pow(-2 * x + 2, 3) / 2;
  }
  static float DerivativeEasyInOutCubic(float x) {
    return x < 0.5f ? 12 * x * x : 3 * (float)Math.Pow(-2 * x + 2, 2) / 2;
  }
}



class CalculateRadius
{
  float maxRadius;
  int count;
  float average;
  float maxDelta;
  float minDelta;
  float innerRadius;
  float outerRadius;
  private Random rand;

  public CalculateRadius(float maxRadius, int count)
  {
    this.count = count;
    this.maxRadius = maxRadius;
    average = maxRadius / count;
    //limit the delta to 20% of the average
    maxDelta = average * 1.2f;
    minDelta = average * 0.8f;
    innerRadius = 0;
    outerRadius = 0;
    rand = new Random();
  }
  public Tuple<float, float> Next()
  {
    innerRadius = outerRadius;
    float thikness = (float)rand.Next((int)(minDelta*1000),(int)(maxDelta*1000)) / 1000;
    outerRadius = Math.Min(innerRadius + thikness, maxRadius);
    average = ((average * count) - thikness) / (count - 1);
    count--;
    maxDelta = average * 1.2f;
    minDelta = average * 0.8f;
    if (minDelta > maxDelta) {
      (minDelta, maxDelta) = (maxDelta, minDelta);
    }

    return new Tuple<float, float>(innerRadius, outerRadius);
  }
}
