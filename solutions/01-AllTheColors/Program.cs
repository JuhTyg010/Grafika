using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CommandLine;
using System.Numerics;

namespace _01_AllTheColors
{
  enum Mode {Trivial, Random, Pattern}

  class Options
  {
    [Option('w', "width", Required = false, Default = 4096, HelpText = "Image width in pixels.")]
    public int Width { get; set; }

    [Option('h', "height", Required = false, Default = 4096, HelpText = "Image height in pixels.")]
    public int Height { get; set; }
    

    [Option('o', "output", Required = false, Default = "output.png", HelpText = "Output file-name.")]
    public string FileName { get; set; } = "output.png";

    [Option('m', "mode", Required = false, Default = Mode.Trivial, HelpText = "Visual mode. Trivial, Random, Pattern")]
    public Mode mode {get; set;}
    
    [Option('s', "seed", Required = false, Default = 0, HelpText = "Seed for random mode.")]
    public int seed {get; set;}
  }

  internal class Program
  {
    

    static void Main (string[] args)
    {
      
      Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
      {
        if (options.Width*options.Height < 256*256*256)
        {
          Exception e = new Exception("Image size is too small. It should be at least 2^24 pixels.");
          throw e;
        }
        switch(options.mode)
        {
          case Mode.Trivial:
            TrivialMode(options.Width, options.Height, options.FileName);
            break;
          case Mode.Random:
            RandomMode(options.Width, options.Height, options.FileName, options.seed);
            break;
          case Mode.Pattern:
            PatternMode(options.Width, options.Height, options.FileName, options.seed);
            break;
        }
      });
    }
    static void TrivialMode(int width, int height, string output)
    {
      uint index = 0;
      byte[] rgb = new byte[3]; 
      
      
      using (var image = new Image<Rgba32>(width, height))
      {
        for(int i =0; i < width; i++)
        {
          for(int j = 0; j < height; j++)
          {
            
            rgb = new byte[] {(byte)index , (byte)(index>>8), (byte)(index>>16)};
            Rgba32 color = new Rgba32(rgb[0], rgb[1], rgb[2]);
            
            image[i, j] = color;
            index++;
          }
        }

        // Save the image to a file with the specified filename
        image.Save(output);
        Console.WriteLine($"Image '{output}' created successfully.");
      }
    }

    static void RandomMode(int width, int height, string output, int seed)
    {
      Random rand = new Random(seed);
      bool[,] used = new bool[width, height];
      uint index = 0;
      byte[] rgb = new byte[3]; 
      
      int x = rand.Next(width);
            
      int y = rand.Next(height);
      
      
      using (var image = new Image<Rgba32>(width, height))
      {
        for(int i =0; i < width; i++)
        {
          for(int j = 0; j < height; j++)
          {
            
            rgb = new byte[] {(byte)index , (byte)(index>>8), (byte)(index>>16)};
            Rgba32 color = new Rgba32(rgb[0], rgb[1], rgb[2]);

            while (used[x, y])
            {
              x = rand.Next(width);
              y = rand.Next(height);
            }
            image[x, y] = color;
            used[x, y] = true;
            index++;
          }
        }

        // Save the image to a file with the specified filename
        image.Save(output);
        Console.WriteLine($"Image '{output}' created successfully.");
      }
    }
  
    static void PatternMode(int width, int height, string output, int seed)
    {
      using (var image = new Image<Rgba32>(width, height))
      {
        Random rand = new Random(seed);
        bool[,] used = new bool[width, height];
        int posCount = 0;

        for (int r = 0; r < 256; r++)
        {
          for (int g = 0; g < 256; g++)
          {
            for (int b = 0; b < 256; b++)
            {
              var color = new Rgba32((byte)r, (byte)g, (byte)b);

              // Calculate the position for the current color
              int x = (int)((width/2)+ (width/2-1)*Math.Cos(r*2*Math.PI/256) * Math.Cos((g+r)*2*Math.PI/256) * Math.Cos((r+b)*2*Math.PI/256));
              int y = (int)((height/2)+ (height/2-1)*Math.Cos(r*2*Math.PI/256) * Math.Sin((g+r)*2*Math.PI/256) * Math.Sin((b+g)*2*Math.PI/256));
              
              if (used[x, y])
              {
                Vector2 pos = CaclulateClose(used, width, height, x, y, rand);
                image[(int)pos.X, (int)pos.Y] = color;
                used[(int)pos.X, (int)pos.Y] = true;
              }
              else
              {
                image[x, y] = color;
                used[x, y] = true;
              }

              posCount++;
            }
          }
        }
        for(int i =0; i < width; i++)
        {
          for(int j = 0; j < height; j++)
          {
            if (!used[i, j])
            {
              image[i, j] = new Rgba32(128, 128, 128);
            }
          }
        }
        
        image.Save(output);
        Console.WriteLine($"Image '{output}' created successfully.");
        
      }
    }
    
    static Vector2 CaclulateClose(bool[,] used, int width, int height, int x, int y, Random rand)
    {
      
      while (used[x, y])
      {
        x = rand.Next(width);
        y = rand.Next(height);
      }
      return new Vector2(x, y);
    }
    
    
  }
}
