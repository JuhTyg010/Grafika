using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _01_AllTheColors;

internal class Program
{
  // Constants for image dimensions and tile size
  private const int Width = 256*16;
  private const int Height = 256*16;
  
  // Constant for the output filename
  private const string OutputFilename = "checkerboard.png";
  
  private static byte red = 0;
  private static byte green = 0;
  private static byte blue = 0;

  static void Main (string[] args)
  {
    // Create a new image with the specified dimensions
    bool isWidth = true;
    bool isIncreasing = true;
    bool greenIncreasing = false;
    int width = Width;
    int height = Height;
    int x = 0;
    int y = 0;
    using (var image = new Image<Rgba32>(Width, Height))
    {
      while (width > 0 && height > 0)
      {
        for (int i = 0; i < (isWidth ? width : height); i++)
        {
          
          Rgba32 tileColor = new Rgba32((byte)red, (byte)green, (byte)blue);
          Console.WriteLine($"{red} {green} {blue}");
          image[x, y] = tileColor;

          #region changeColor

          red++;
          if (red == 0)
          {
            green++;
            greenIncreasing = true;
          }

          if (greenIncreasing && green == 0)
          {
            blue++;
            greenIncreasing = false;
          }

          #endregion

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
        if(isWidth) isIncreasing = !isIncreasing;
      }
      

      // Save the image to a file with the specified filename
      image.Save(OutputFilename);

      Console.WriteLine($"Image '{OutputFilename}' created successfully.");
    }
  }
}
