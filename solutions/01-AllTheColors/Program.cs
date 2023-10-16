using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CommandLine;
using SixLabors.ImageSharp.ColorSpaces;
using System.Numerics;

namespace _01_AllTheColors
{

  class Options
  {
    [Option] public int Width { get; set; } = 256 * 16;
    [Option] public int Height { get; set; } = 256 * 16;
    [Option(Required = true)] public string OutputFilename { get; set; }
  }

  internal class Program
  {
    static void Main (string[] args)
    {
      Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
      {
        bool isWidth = true;
        bool isIncreasing = true;
        bool greenIncreasing = false;
        int width = options.Width;
        int height = options.Height;
        int x = 0;
        int y = 0;
        int index = 0;
        Vector3 color = new Vector3(0, 0, 0);
        using (var image = new Image<Rgba32>(options.Width, options.Height))
        {
          while (width > 0 && height > 0)
          {
            for (int i = 0; i < (isWidth ? width : height); i++)
            {
              color = new Vector3(index % 256, (index / 256) % 256, (index / 256 / 256) % 256);
              Rgba32 tileColor = new Rgba32(color.X, color.Y, color.Z);
              //Console.WriteLine($"{color.X} {color.Y} {color.Z}");
              image[x, y] = tileColor;

              index++;

              if (isWidth)
              {
                x = isIncreasing ? x + 1 : x - 1;
              }
              else
              {
                y = isIncreasing ? y + 1 : y - 1;
              }
            }

            if (isWidth)
            {
              height--;
              //we are sum/sub 1 because we are adding 1 at the end of the loop
              x = isIncreasing ? x - 1 : x + 1;
              y = isIncreasing ? y + 1 : y - 1;
            }
            else
            {
              width--;
              //same as with x
              y = isIncreasing ? y - 1 : y + 1;
              x = isIncreasing ? x - 1 : x + 1;

            }

            isWidth = !isWidth;
            if (isWidth) isIncreasing = !isIncreasing;
          }


          // Save the image to a file with the specified filename
          image.Save(options.OutputFilename);

          Console.WriteLine($"Image '{options.OutputFilename}' created successfully.");
        }
      });
    }
  }
}
