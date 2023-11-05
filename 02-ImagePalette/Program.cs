using System.Xml;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace _02_ImagePalette;

public class Options
{
  [Option('i', "input", Required = true, Default = "", HelpText = "Text to get palette for")]
  public string FileName { get; set; } = string.Empty;
  [Option('c', "count", Required = true, Default = 1, HelpText = "Number of colors to get")]
  public int Count { get; set; } = 1;

  [Option('o', "output", Required = false, Default = "", HelpText = "Output file name")]
  public string OutputFileName { get; set; } = "";
}

class Cube
{
    public List<Rgba32> Colors { get; }
    
    public Dictionary<Rgba32, int> ColorCount { get; }
    
    public int Count { get; }
    
    public Cube(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount)
    {
        Colors = colors;
        ColorCount = colorCount;
        Count = 0;
        foreach (var color in Colors)
        {
            Count += ColorCount[color];
        }
    }
}


class Program
{
  static void Main (string[] args)
  {
      DateTime startTime = DateTime.Now;
      Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
      {
          using (Image<Rgba32> image = Image.Load<Rgba32>(o.FileName))
          {
              List<Rgba32> allColors = new List<Rgba32>();
              List<Rgba32> uniqueColors = new List<Rgba32>();
              Dictionary<Rgba32, int> colorCount = new Dictionary<Rgba32, int>();
              for (int i = 0; i < image.Width; i++)
              {
                  for (int j = 0; j < image.Height; j++)
                  {
                      allColors.Add(image[i, j]);
                      if (colorCount.ContainsKey(image[i, j]))
                      {
                          colorCount[image[i, j]]++;
                      }
                      else
                      {
                          colorCount.Add(image[i, j], 1);
                          uniqueColors.Add(image[i, j]);
                      }
                  }
              }

              var outColors = new List<Rgba32>();
              if (uniqueColors.Count < o.Count)
              {
                  outColors = uniqueColors;
              }
              else
              {
                  //outImage = new Image<Rgba32>(o.Count * 200, 200);

                  if (uniqueColors.Count > 100000)
                  {
                      var temp = KMeansClustering(uniqueColors, colorCount, o.Count + 5, 114);
                      var temp2 = KMeansClustering(uniqueColors, colorCount, o.Count + 2, 225);
                      List<Rgba32> temp3 = KMeansClustering(uniqueColors, colorCount, o.Count + 7, 100 + o.Count);
                      var temp4 = KMeansClustering(uniqueColors, colorCount, o.Count * 2,
                          42 + 24 + 9 + 17 + 8 + 4 + 2 + 1);
                      temp.AddRange(temp2);
                      temp.AddRange(temp3);
                      temp.AddRange(temp4);
                      outColors = KMeansClustering(temp, colorCount, o.Count, 100);
                  }
                  else
                  {
                      outColors = MedianCutQuantization(uniqueColors, colorCount, o.Count);
                  }



              }

              if (string.IsNullOrEmpty(o.FileName))
              {
                  // Simple stdout output (CSV format)
                  foreach (Rgba32 c in outColors)
                  {
                      Console.WriteLine($"{c.R},{c.G},{c.B}");
                  }
              }
              else
              {
                  if (o.FileName.EndsWith(".svg"))
                  {
                      // SVG output for debugging...
                      // Create an XML document to represent the SVG
                      XmlDocument svgDoc = new XmlDocument();

                      // Create the SVG root element
                      XmlElement svgRoot = svgDoc.CreateElement("svg");
                      svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
                      svgRoot.SetAttribute("width", image.Width.ToString());
                      svgRoot.SetAttribute("height", image.Height.ToString());
                      svgDoc.AppendChild(svgRoot);

                      // Create a group element to contain the color rectangles
                      XmlElement group = svgDoc.CreateElement("g");
                      svgRoot.AppendChild(group);

                      for (int i = 0; i < outColors.Count; i++)
                      {
                          // Create a rectangle element for each color
                          XmlElement rect = svgDoc.CreateElement("rect");
                          rect.SetAttribute("x", (i * RectWidth).ToString());
                          rect.SetAttribute("y", "0");
                          rect.SetAttribute("width", RectWidth.ToString());
                          rect.SetAttribute("height", RectHeight.ToString());
                          rect.SetAttribute("fill", $"#{outColors[i].R:X2}{outColors[i].G:X2}{outColors[i].B:X2}");
                          group.AppendChild(rect);
                      }

                      // Save the SVG document to a file
                      svgDoc.Save(o.FileName);

                      Console.WriteLine($"SVG saved to {o.FileName}");
                      
                  }
                  else if(o.FileName.EndsWith(".png"))
                  {
                      // PNG output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(o.Count * RectWidth, RectHeight))
                      {
                          for (int i = 0; i < outColors.Count; i++)
                          {
                              for (int j = 0; j < RectWidth; j++)
                              {
                                  for (int k = 0; k < RectHeight; k++)
                                  {
                                      outImage[i * RectWidth + j, k] = outColors[i];
                                  }
                              }
                          }

                          outImage.Save(o.FileName);
                          Console.WriteLine($"PNG saved to {o.FileName}");
                      }
                  }
                  else if(o.FileName.EndsWith(".jpg"))
                  {
                      // JPG output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(o.Count * RectWidth, RectHeight))
                      {
                          for (int i = 0; i < outColors.Count; i++)
                          {
                              for (int j = 0; j < RectWidth; j++)
                              {
                                  for (int k = 0; k < RectHeight; k++)
                                  {
                                      outImage[i * RectWidth + j, k] = outColors[i];
                                  }
                              }
                          }

                          outImage.SaveAsJpeg(o.FileName);
                          Console.WriteLine($"JPG saved to {o.FileName}");
                      }
                  }
                  else if(o.FileName.EndsWith(".bmp"))
                  {
                      // BMP output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(o.Count * RectWidth, RectHeight))
                      {
                          for (int i = 0; i < outColors.Count; i++)
                          {
                              for (int j = 0; j < RectWidth; j++)
                              {
                                  for (int k = 0; k < RectHeight; k++)
                                  {
                                      outImage[i * RectWidth + j, k] = outColors[i];
                                  }
                              }
                          }

                          outImage.SaveAsBmp(o.FileName);
                          Console.WriteLine($"BMP saved to {o.FileName}");
                      }
                  }

                  else
                  {
                      Console.WriteLine("Cannot handle output file without .svg, .png, .jpg, or .bmp extension");
                  }
              }
          }
      });
    
  }

  public static int RectWidth { get; } = 100;
  public static int RectHeight { get; } = 100;

  static List<Rgba32> MedianCutQuantization(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount, int k)
  {
      Cube initialCube = new Cube(colors, colorCount);

      List<Cube> cubes = new List<Cube> { initialCube };
      
      while (cubes.Count < k)
      {
          Cube largestCube = cubes.OrderByDescending(cube =>
          {
              int x = 0;
              foreach (var color in cube.Colors)
              {
                  x += cube.ColorCount[color];
              }
              return x;
          }).First();

          List<Cube> splitCubes = Split(largestCube);

          cubes.Remove(largestCube);
          cubes.AddRange(splitCubes);
      }

      List<Rgba32> clusteredColors = cubes.Select(cube => Average(cube.Colors, cube.ColorCount)).ToList();

      return clusteredColors;
  }

  static List<Rgba32> KMeansClustering(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount, int k, int iter)
  {

      //List<Rgba32> clusterCenters = TopKList(colorCount, k);
        
      List<Rgba32> clusterCenters = InitializeRandomClusterCenters(colors, k, k * iter);
        

        List<List<Rgba32>> clusters = new List<List<Rgba32>>();

        for (int iteration = 0; iteration < iter; iteration++) 
        {
            clusters = AssignColorsToClusters(colors, clusterCenters);

            // Update cluster centers
            List<Rgba32> newCenters = CalculateClusterCenters(clusters, colorCount);

            // Check for convergence
            bool converged = true;
            for (int i = 0; i < k; i++)
            {
                if (CompareColors(clusterCenters[i], newCenters[i]) > 0)
                {
                    converged = false;
                    break;
                }
            }
            if (converged) {
                break;
            }

            clusterCenters = newCenters;
        }

        return clusterCenters;
    }
  
  static List<Cube> Split(Cube cube)
  {
      Tuple<Rgba32,Rgba32> diff =FindMostDifferentColors(cube.Colors);
      List<Rgba32> subCube1Colors = new List<Rgba32>();
      List<Rgba32> subCube2Colors = new List<Rgba32>();
      foreach (var color in cube.Colors)
      {
        if (CompareColors(color, diff.Item1) < CompareColors(color, diff.Item2)) {
            subCube1Colors.Add(color);
        }
        else {
            subCube2Colors.Add(color);
        }
      }  

      return new List<Cube>(){
          new Cube(subCube1Colors, cube.ColorCount),
          new Cube(subCube2Colors, cube.ColorCount)
      };
  }

  static Tuple<Rgba32, Rgba32> FindMostDifferentColors(List<Rgba32> colors)
  {
      Rgba32 color1 = new Rgba32(0,0,0);
      Rgba32 color2 = new Rgba32(0,0,0);;
      double maxDistance = 0;

      for (int i = 0; i < colors.Count; i++)
      {
          for (int j = i + 1; j < colors.Count; j++)
          {
              Rgba32 c1 = colors[i];
              Rgba32 c2 = colors[j];
              double distance = CompareColors(c1, c2);

              if (distance > maxDistance)
              {
                  maxDistance = distance;
                  color1 = c1;
                  color2 = c2;
              }
          }
      }

      return new Tuple<Rgba32, Rgba32>(color1, color2);
  }
    static List<Rgba32> InitializeRandomClusterCenters(List<Rgba32> colors, int k, int seed = 0)
    {
        Random rand = new Random(seed);
        
        List<Rgba32> clusterCenters = new List<Rgba32>();

        for (int i = 0; i < k; i++)
        {
            int randomIndex = rand.Next(colors.Count);
            clusterCenters.Add(colors[randomIndex]);
        }

        return clusterCenters;
    }

    static List<List<Rgba32>> AssignColorsToClusters(List<Rgba32> colors, List<Rgba32> clusterCenters)
    {
        List<List<Rgba32>> clusters = new List<List<Rgba32>>();
        for (int i = 0; i < clusterCenters.Count; i++)
        {
            clusters.Add(new List<Rgba32>());
        }

        foreach (Rgba32 color in colors)
        {
            int closestClusterIndex = FindClosestClusterIndex(color, clusterCenters);
            clusters[closestClusterIndex].Add(color);
        }

        return clusters;
    }

    static List<Rgba32> CalculateClusterCenters(List<List<Rgba32>> clusters, Dictionary<Rgba32, int> colorCount)
    {
        List<Rgba32> newCenters = new List<Rgba32>();

        foreach (var cluster in clusters)
        {
            if (cluster.Count == 0)
            {
                newCenters.Add(new Rgba32(0,0,0));
            }
            else
            {
              Rgba32 average = Average(cluster, colorCount);
              newCenters.Add(FindClosestClusterColor(average, cluster));
            }
        }

        return newCenters;
    }

    static int FindClosestClusterIndex(Rgba32 color, List<Rgba32> clusterCenters)
    {
        int closestIndex = 0;
        double closestDistance = CompareColors(color, clusterCenters[0]);

        for (int i = 1; i < clusterCenters.Count; i++)
        {
            double distance = CompareColors(color, clusterCenters[i]);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }

    

    static Rgba32 FindClosestClusterColor(Rgba32 pixel, List<Rgba32> clusterColors)
    {
        Rgba32 closestColor = clusterColors[0];
        double closestDistance = CompareColors(pixel, closestColor);

        foreach (var color in clusterColors)
        {
            double distance = CompareColors(pixel, color);
            if (distance < closestDistance)
            {
                closestColor = color;
                closestDistance = distance;
            }
        }

        return closestColor;
    }

  static double CompareColors(Rgba32 a, Rgba32 b)
  {
    var aHsv = SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter.ToHsv(a);
    var bHsv = SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter.ToHsv(b);
    var dh = Math.Min(Math.Abs(aHsv.H - bHsv.H), 360 - Math.Abs(aHsv.H - bHsv.H))/180;
    var ds = Math.Abs(aHsv.S - bHsv.S);
    var dv = Math.Abs(aHsv.V - bHsv.V)/255;
    return Math.Sqrt(dh*dh+ds*ds+dv*dv);
  }
  static Rgba32 Average(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount)
  {
    var hue = 0f;
    var saturation = 0f;
    var value = 0f;
    int total = 0;
    foreach (var color in colors)
    {
      var hsv = SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter.ToHsv(color);
      hue += hsv.H * colorCount[color];
      saturation += hsv.S * colorCount[color];
      value += hsv.V * colorCount[color];
      total += colorCount[color];
    }
    hue /= total;
    saturation /= total;
    value /= total;
    return SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter.ToRgb(new Hsv(hue, saturation, value));
  }
}