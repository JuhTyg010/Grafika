using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Processing;
using CommandLine;
using System.Collections.Generic;
using System;
using System.Numerics;

public class Options
{
  [Option('i', "input", Required = true, Default = "", HelpText = "input image")]
  public string Input { get; set; } = "";
  [Option('o', "output", Required = true, Default = "", HelpText = "output image")]
  public string Output { get; set; } = "out.png";

  [Option('h', "huedelta", Required = true, Default = 6, HelpText = "hue delta")]
  public float Hue { get; set; } = 6;


}

struct Region
{
  public Dictionary<Vector2, Rgba32> pixels;
  public Rgba32 averageColor;
  private Dictionary<Rgba32,int> colorScheme;
  //private Vector3 averageParams;


  public Region (Dictionary<Vector2, Rgba32> pixels, Rgba32 averageColor)
  {
    this.pixels = pixels;
    this.averageColor = averageColor;
    colorScheme = new Dictionary<Rgba32, int>();
    foreach (var pixel in pixels)
    {
      colorScheme[pixel.Value] = colorScheme.GetValueOrDefault(pixel.Value, 0) + 1;
    }
  }


  public void AddDictionary (Dictionary<Vector2, Rgba32> added)
  {
    foreach (var pixel in added)
    {
      this.pixels.Add(pixel.Key, pixel.Value);
      colorScheme[pixel.Value] = colorScheme.GetValueOrDefault(pixel.Value, 0) + 1;
    }
    RecalculateAverageColor();
  }

  public void AddPixel (Vector2 coord, Rgba32 color)
  {
    pixels.Add(coord, color);
    colorScheme[color] = colorScheme.GetValueOrDefault(color, 0) + 1;
    if (pixels.Count % 200 == 0)
      RecalculateAverageColor();  //to not to do it every time
  }

  private void RecalculateAverageColor ()
  {
    int r = 0;
    int g = 0;
    int b = 0;
    foreach (var pixel in pixels)
    {
      r += pixel.Value.R;
      g += pixel.Value.G;
      b += pixel.Value.B;
    }
    averageColor = new Rgba32((byte)(r / pixels.Count), (byte)(g / pixels.Count), (byte)(b / pixels.Count));
  }

}

class ImageRecoloringWithFaceDetection
{
  static void Main (string[] args)
  {

    Rgba32[] skinTones = new Rgba32[]
        {
            new Rgba32(255, 220, 177),
            new Rgba32(228, 185, 142),
            new Rgba32(227, 161, 115),
            new Rgba32(217, 145, 100),
            new Rgba32(187, 109, 74),
            new Rgba32(246, 225, 203),
            new Rgba32(205, 149, 116),
            new Rgba32(175, 105, 78),
            new Rgba32(196, 119, 89),
            new Rgba32(197, 136, 101),
            new Rgba32(213, 160, 131),
            new Rgba32(207, 158, 127),
            new Rgba32(181, 136, 104),
            new Rgba32(119, 56, 38),
            new Rgba32(160, 110, 89),
            new Rgba32(140, 85, 66),
            new Rgba32(113, 64, 41),
            new Rgba32(79, 40, 29),
            new Rgba32(141, 75, 49),
            new Rgba32(255, 239, 226),
            new Rgba32(180, 131, 93),
            new Rgba32(255, 41, 0),
            new Rgba32(255, 34, 0),
            new Rgba32(255, 27, 0),
            new Rgba32(255, 20, 0),
            new Rgba32(255, 13, 0),
            new Rgba32(255, 6, 0),
            new Rgba32(255, 0, 0)
        };
    Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
    {
      using (Image<Rgba32> image = Image.Load<Rgba32>(o.Input))
      {

        var regions = CreateRegions(image);
        bool[,] isSkin = new bool[image.Width, image.Height];
        foreach (var region in regions)
        {
          foreach (var tone in skinTones)
          {
            if (IsLessDistant(tone, region.averageColor, 0.12f))
            {
              foreach (var pixel in region.pixels)
              {
                isSkin[(int)pixel.Key.X, (int)pixel.Key.Y] = true;
              }
              break;
            }
          }
        }

        /*foreach (var region in regions) {
            foreach (var pixel in region.pixels)
            {
                image[(int)pixel.Key.X, (int)pixel.Key.Y] = region.averageColor;
            }
        }*/
        ProcessImage(image, o.Hue, isSkin);


        image.Save(o.Output);
      }

    });
  }

  static List<List<Vector2>> FindContours (Image<L8> gray)
  {
    List<List<Vector2>> contours = new List<List<Vector2>>();

    // Implement contour detection from the edge-detected grayscale image
    // This example assumes you have edge-detected data in 'gray'

    // Placeholder code to demonstrate the concept
    // Implement a proper contour detection algorithm based on your requirements

    // Example: Create a single contour around the entire edge
    List<Vector2> contour = new List<Vector2>();
    for (int y = 0; y < gray.Height; y++)
    {
      for (int x = 0; x < gray.Width; x++)
      {
        if (gray[x, y].PackedValue > 0) // Assuming non-zero values represent edges
        {
          contour.Add(new Vector2(x, y));
        }
      }
    }

    contours.Add(contour); // Add the single contour (replace this with actual contours)

    return contours;
  }

  static List<Region> CreateRegions (Image<Rgba32> image)
  {
    bool[,] visited = new bool[image.Width, image.Height];
    List<Region> regions = new List<Region>();
    for (int y = 0; y < image.Height; y++)
    {
      for (int x = 0; x < image.Width; x++)
      {
        if (!visited[x, y])
        {
          // Create a new region and add seed pixel to the queue
          Region region = new Region(new Dictionary<Vector2, Rgba32>(), image[x, y]);
          var queue = new System.Collections.Generic.Queue<Vector2>();
          queue.Enqueue(new Vector2(x, y));
          visited[x, y] = true;
          region.AddPixel(new Vector2(x, y), image[x, y]);
          // Process the queue
          while (queue.Count > 0)
          {

            var current = queue.Dequeue();
            int cx = (int) current.X;
            int cy = (int) current.Y;

            foreach (var neighbor in
                GetAdjacentPixels((int)current.X, (int)current.Y, image.Width, image.Height))
            {
              int nx = (int) neighbor.X;
              int ny = (int) neighbor.Y;
              if (!visited[nx, ny] && IsLessDistant(image[cx, cy], image[nx, ny], 0.05f))
              {
                queue.Enqueue(neighbor);

                region.AddPixel(neighbor, image[nx, ny]);
                visited[nx, ny] = true;
              }
            }
          }

          regions.Add(region);
        }

      }
    }

    return regions;
  }

  static bool nextToSkin (int x, int y, bool[,] isSkin)
  {
    for (int i = x - 1; i <= x + 1; i++)
    {
      for (int j = y - 1; j <= y + 1; j++)
      {
        if (i >= 0 && i < isSkin.GetLength(0) && j >= 0 && j < isSkin.GetLength(1) && isSkin[i, j])
        {
          return true;
        }
      }
    }
    return false;
  }

  static bool IsLessDistant (Rgba32 a, Rgba32 b, float dist)
  {
    var hsvA = ColorSpaceConverter.ToHsv(a);
    var hsvB = ColorSpaceConverter.ToHsv(b);
    var hueDist = Math.Min(Math.Abs(hsvA.H - hsvB.H), 360 - Math.Abs(hsvA.H - hsvB.H)) / 180;
    var satDist = Math.Abs(hsvA.S - hsvB.S) * 2; //less important
    var valDist = Math.Abs(hsvA.V - hsvB.V) * 2; //less important
    return hueDist * hueDist + satDist * satDist + valDist * valDist < dist * dist;
  }

  static IEnumerable<Vector2> GetAdjacentPixels (int x, int y, int maxW, int maxH)
  {
    if (x > 0)
    {
      yield return new Vector2(x - 1, y);
    }

    if (x < maxW - 1)
    {
      yield return new Vector2(x + 1, y);
    }

    if (y > 0)
    {
      yield return new Vector2(x, y - 1);
    }

    if (y < maxH - 1)
    {
      yield return new Vector2(x, y + 1);
    }
  }


  static void ProcessImage (Image<Rgba32> image, float hueDelta, bool[,] isSkin)
  {
    for (int y = 0; y < image.Height; y++)
    {
      for (int x = 0; x < image.Width; x++)
      {
        if (!isSkin[x, y])
        {
          if (!nextToSkin(x, y, isSkin))
          {

            var hsv = ColorSpaceConverter.ToHsv(image[x, y]);
            hsv = new Hsv((hsv.H + hueDelta) % 360, hsv.S, hsv.V);

            image[x, y] = ColorSpaceConverter.ToRgb(hsv);
          }
        }
      }
    }
  }
  static PointF VectToPointF (Vector2 from)
  {
    return new PointF(from.X, from.Y);
  }

  static PointF[] VectToPointFs (Vector2[] from)
  {
    PointF[] to = new PointF[from.Length];
    for (int i = 0; i < from.Length; i++)
    {
      to[i] = VectToPointF(from[i]);
    }
    return to;
  }
}

