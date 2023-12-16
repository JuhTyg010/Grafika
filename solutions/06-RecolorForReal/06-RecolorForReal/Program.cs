using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using CommandLine;
using System;
using SixLabors.ImageSharp.Processing;
using DlibDotNet;
using Microsoft.VisualBasic;

public class Options
{
    [Option('i', "input", Required = true, Default = "", HelpText = "input image")]
    public string Input { get; set; } = "";
    [Option('o', "output", Required = true, Default = "", HelpText = "output image")]
    public string Output { get; set; } = "out.png";

    [Option('h', "huedelta", Required = true, Default = 6, HelpText = "hue delta")]
    public float Hue { get; set; } = 6;

    
}


class ImageRecoloringWithFaceDetection
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: ImageRecoloringWithFaceDetection -i <input> -o <output> -h <Hue-delta>");
            return;
        }


        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => {
            using (Image<Rgba32> image = Image.Load<Rgba32>(o.Input))
            { 
                using (var faceDetector = Dlib.GetFrontalFaceDetector())
                {
                    // Detect faces
                    var faceLocations = faceDetector.Operator(Dlib.LoadImage<RgbPixel>(o.Input));
                    ProcessImage(image, o.Hue, faceLocations);

                    image.Save(o.Output);
                }
            }
        });
    }
static void ProcessImage(Image<Rgba32> image, float hueDelta, DlibDotNet.Rectangle[] faceLocations)
{
    for (int y = 0; y < image.Height; y++) {
        for (int x = 0; x < image.Width; x++) {
            if (IsWithinFaceRegion(x, y, faceLocations)) {
                continue;
            }
            var pixel = image[x, y];
            var hsv = ColorSpaceConverter.ToHsv(pixel);
            hsv = new Hsv(hsv.H + hueDelta, hsv.S, hsv.V);
            pixel = ColorSpaceConverter.ToRgb(hsv);
            image[x, y] = pixel;
        }
    }
}

static bool IsWithinFaceRegion(int x, int y, DlibDotNet.Rectangle[] faceLocations) {
    foreach (var faceLocation in faceLocations) {
        if (faceLocation.Left <= x && x <= faceLocation.Right && faceLocation.Top <= y && y <= faceLocation.Bottom) {
            return true;
        }
    }

    return false;
}
}
