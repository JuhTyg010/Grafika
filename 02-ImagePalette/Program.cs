using System;
using System.Xml;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _02_ImagePalette;

public class Options
{
  [Option('o', "output", Required = false, Default = "", HelpText = "Output file-name (SVG).")]
  public string FileName { get; set; } = string.Empty;
}

class Program
{
  // Input array of Rgba32 colors
  static readonly Rgba32[] Colors = new Rgba32[]
  {
    new Rgba32(255, 0, 0),    // Red
    new Rgba32(0, 255, 0),    // Green
    new Rgba32(0, 0, 255),    // Blue
    new Rgba32(255, 255, 0),  // Yellow
    new Rgba32(255, 165, 0),  // Orange
    new Rgba32(80, 80, 80)    // Dark gray
  };

  // Define the canvas size in pixels
  static readonly int ImageWidth = 600;
  static readonly int ImageHeight = 100;

  // Define the width and height of each color rectangle
  static readonly int RectWidth = ImageWidth / Colors.Length;
  static readonly int RectHeight = ImageHeight;

  static void Main (string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        if (string.IsNullOrEmpty(o.FileName))
        {
          // Simple stdout output (CSV format)
          foreach (Rgba32 c in Colors)
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
            svgRoot.SetAttribute("width", ImageWidth.ToString());
            svgRoot.SetAttribute("height", ImageHeight.ToString());
            svgDoc.AppendChild(svgRoot);

            // Create a group element to contain the color rectangles
            XmlElement group = svgDoc.CreateElement("g");
            svgRoot.AppendChild(group);

            for (int i = 0; i < Colors.Length; i++)
            {
              // Create a rectangle element for each color
              XmlElement rect = svgDoc.CreateElement("rect");
              rect.SetAttribute("x", (i * RectWidth).ToString());
              rect.SetAttribute("y", "0");
              rect.SetAttribute("width", RectWidth.ToString());
              rect.SetAttribute("height", RectHeight.ToString());
              rect.SetAttribute("fill", $"#{Colors[i].R:X2}{Colors[i].G:X2}{Colors[i].B:X2}");
              group.AppendChild(rect);
            }

            // Save the SVG document to a file
            svgDoc.Save(o.FileName);

            Console.WriteLine($"SVG saved to {o.FileName}");
          }
          else
          {
            Console.WriteLine("Cannot handle output file without .svg extension! No output was generated.");
          }
        }
      });
  }
}
