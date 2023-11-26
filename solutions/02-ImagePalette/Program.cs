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

  [Option('o', "output", Required = true, Default = "", HelpText = "Output file name")]
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
    public Cube(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount, int count)
    {
        Colors = colors;
        ColorCount = colorCount;
        Count = count;
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
              /*List<Rgba32> uniqueColors = new List<Rgba32>();
              Dictionary<Rgba32, int> colorCount = new Dictionary<Rgba32, int>();*/
              List<Rgba32>[] uniqueColorsSectors = new List<Rgba32>[9];
              Dictionary<Rgba32, int>[] colorCountSectors = new Dictionary<Rgba32, int>[9];
                for(int i = 0; i < 9; i++) {
                    uniqueColorsSectors[i] = new List<Rgba32>();
                    colorCountSectors[i] = new Dictionary<Rgba32, int>();
                }
              for (int i = 0; i < image.Width; i++)
              {
                  for (int j = 0; j < image.Height; j++)
                  {
                      int sector = (i / (image.Width / 3 + 1)) + (j / (image.Height / 3 + 1)) * 3;

                      if (colorCountSectors[sector].ContainsKey(image[i, j]))
                      {
                          colorCountSectors[sector][image[i, j]]++;
                      } else {
                          colorCountSectors[sector].Add(image[i, j], 1);
                          uniqueColorsSectors[sector].Add(image[i, j]);
                      }
                  }
              }

              int colorCount = 0;
              foreach (var s in uniqueColorsSectors) colorCount += s.Count;

              var outColors = new List<Rgba32>();
              if (colorCount < o.Count)
              {
                  foreach (var sector in uniqueColorsSectors) {
                      outColors.AddRange(sector);
                  }

              } else {
                  //some parsing algorithm
                    List<Rgba32> tempColors = new List<Rgba32>();
                    Dictionary<Rgba32, int> tempColorCount = new Dictionary<Rgba32, int>();
                  for(int i=0; i<9; i++) {

                      List<Rgba32>tempo = MedianCutQuantization(uniqueColorsSectors[i], colorCountSectors[i], 100);
                      Dictionary<Rgba32,int> temp = new Dictionary<Rgba32, int>();
                        foreach (var c in tempo) {
                            if(temp.ContainsKey(c)) {
                                temp[c] += 1;
                            } else {
                                temp.Add(c, 1);
                            }
                        }
                        List<Rgba32> t = MedianCutQuantization(tempo, temp, o.Count);

                      foreach (var c in t) {
                          if(tempColorCount.ContainsKey(c)) {
                              tempColorCount[c] += colorCountSectors[i][c];
                          } else {
                              tempColorCount.Add(c, colorCountSectors[i][c]);
                              tempColors.Add(c);
                          }
                      }
                  }
                  outColors = MedianCutQuantization(tempColors, tempColorCount, o.Count);
              }

              for (int x = 0; x < image.Width; x++)
              {
                  for (int y = 0; y < image.Height; y++)
                  {
                      image[x, y] = FindClosestClusterColor(image[x, y], outColors);
                  }
              }
              image.Save($"{o.FileName}CLUSTERED.png");

              if (o.OutputFileName.EndsWith(".svg"))
                  {
                      // SVG output for debugging...
                      // Create an XML document to represent the SVG
                      XmlDocument svgDoc = new XmlDocument();

                      // Create the SVG root element
                      XmlElement svgRoot = svgDoc.CreateElement("svg");
                      svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
                      svgRoot.SetAttribute("width", (outColors.Count*RectWidth).ToString());
                      svgRoot.SetAttribute("height", RectHeight.ToString());
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
                      svgDoc.Save(o.OutputFileName);

                      Console.WriteLine($"SVG saved to {o.OutputFileName}");

                  }
                  else if(o.OutputFileName.EndsWith(".png"))
                  {
                      // PNG output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(outColors.Count * RectWidth, RectHeight))
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

                          outImage.Save(o.OutputFileName);
                          Console.WriteLine($"PNG saved to {o.OutputFileName}");
                      }
                  }
                  else if(o.OutputFileName.EndsWith(".jpg"))
                  {
                      // JPG output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(outColors.Count * RectWidth, RectHeight))
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

                          outImage.SaveAsJpeg(o.OutputFileName);
                          Console.WriteLine($"JPG saved to {o.OutputFileName}");
                      }
                  }
                  else if(o.OutputFileName.EndsWith(".bmp"))
                  {
                      // BMP output
                      using (Image<Rgba32> outImage = new Image<Rgba32>(outColors.Count * RectWidth, RectHeight))
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

                          outImage.SaveAsBmp(o.OutputFileName);
                          Console.WriteLine($"BMP saved to {o.OutputFileName}");
                      }
                  }
                  else
                  {
                    Console.WriteLine("Cannot handle output file without .svg, .png, .jpg, or .bmp extension");
                  }
          }
      });

  }

  public static int RectWidth { get; } = 100;
  public static int RectHeight { get; } = 100;

  static List<Rgba32> MedianCutQuantization(List<Rgba32> colors, Dictionary<Rgba32, int> colorCount, int k)
  {
      if(colors.Count <= k) return colors;

      Cube initialCube = new Cube(colors, colorCount);

      List<Cube> cubes = new List<Cube> { initialCube };


      while (cubes.Count < k)
      {
          Cube largestCube = cubes[0];
          foreach (var cube in cubes) {
              if (cube.Colors.Count > largestCube.Colors.Count) {
                  largestCube = cube;
              }
          }

          List<Cube> splitCubes = Split(largestCube);

          cubes.Remove(largestCube);
          cubes.AddRange(splitCubes);
      }

      List<Rgba32> clusteredColors = cubes.Select(cube => Average(cube.Colors)).ToList();

      return clusteredColors;
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
          new Cube(subCube2Colors, cube.ColorCount, cube.Count-subCube1Colors.Count)
      };
  }

  static Tuple<Rgba32, Rgba32> FindMostDifferentColors(List<Rgba32> colors)
  {
      Rgba32 color1 = new Rgba32(0,0,0);
      Rgba32 color2 = new Rgba32(0,0,0);

      int[] limits = new int[]{colors[0].R, colors[0].R, colors[0].G, colors[0].G, colors[0].B, colors[0].B};
      List<Rgba32>[] colorGroups = new List<Rgba32>[6];
      for(int i = 0; i < 6; i++) {
          colorGroups[i] = new List<Rgba32>();
          colorGroups[i].Add(colors[0]);
      }

      foreach (var t in colors)
      {
          for(int j = 0; j < 3; j++) {
              if(RgbGetAtr(t,j) < limits[j*2]) {
                  limits[j*2] = RgbGetAtr(t,j);
                  colorGroups[j*2].Clear();
                  colorGroups[j*2].Add(t);
              } else if(RgbGetAtr(t,j) == limits[j*2]) {
                  colorGroups[j*2].Add(t);
              }
              if(RgbGetAtr(t,j) > limits[j*2+1]) {
                  limits[j*2+1] = RgbGetAtr(t,j);
                  colorGroups[j*2+1].Clear();
                  colorGroups[j*2+1].Add(t);
              } else if(RgbGetAtr(t,j) == limits[j*2+1]) {
                  colorGroups[j*2+1].Add(t);
              }

          }
      }
      int maxDistance = 0;
      for (int i = 0; i < 3; i++) {
          foreach (var first in colorGroups[i*2]) {
              foreach (var second in colorGroups[i*2+1]) {
                  int distance = CompareColors(first, second);
                  if (distance > maxDistance) {
                        maxDistance = distance;
                        color1 = first;
                        color2 = second;
                  }
              }
          }
      }


      return new Tuple<Rgba32, Rgba32>(color1, color2);
  }
    static Rgba32 FindClosestClusterColor(Rgba32 pixel, List<Rgba32> clusterColors)
    {
        Rgba32 closestColor = clusterColors[0];
        int closestDistance = CompareColors(pixel, closestColor);

        foreach (var color in clusterColors)
        {
            int distance = CompareColors(pixel, color);
            if (distance < closestDistance)
            {
                closestColor = color;
                closestDistance = distance;
            }
        }
        return closestColor;
    }

  static int CompareColors(Rgba32 a, Rgba32 b) {
        return (a.R - b.R)*(a.R - b.R) + (a.G - b.G)*(a.G - b.G) + (a.B - b.B)*(a.B - b.B);
  }
  static Rgba32 Average(List<Rgba32> colors) {
        int r = 0;
        int g = 0;
        int b = 0;
        foreach (var color in colors)
        {
            r += color.R;
            g += color.G;
            b += color.B;
        }
        r /= colors.Count;
        g /= colors.Count;
        b /= colors.Count;
        return FindClosestClusterColor(new Rgba32((byte)r, (byte)g, (byte)b), colors);
  }
  static byte RgbGetAtr(Rgba32 color, int atr) {
      switch(atr) {
          case 0:
            return color.R;
          case 1:
            return color.G;
          case 2:
            return color.B;
          default:
            return 0;
      }
  }
}
