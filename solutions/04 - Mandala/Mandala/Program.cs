using System;
using System.ComponentModel;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using static Mandala.MyDraw;
using CommandLine;

public class Options
{
    [Option('w', "width", Required = true, Default = "", HelpText = "Width of the image")]
    public int Width { get; set; } = 800;
    [Option('h', "height", Required = true, Default = "", HelpText = "Height of the image")]
    public int Height { get; set; } = 800;

    [Option('s', "symmetry", Required = false, Default = 6, HelpText = "Symmetry order")]
    public int Symmetry { get; set; } = 6;

    [Option('f', "figures", Required = false, Default = "all", HelpText =
        "Figures to draw (all, circles, triangles, diamonds, roundedSquares, ring) use initials " +
        "(s for squares))and number as multiplier (s2 for squares with 2x multiplier) and f for fill (sf for filled squares or s2f for filled 2x multiplied)")]
    public string Figures { get; set; } = "all";

    [Option('c', "colors", Required = false, Default = "all", HelpText =
        "Colors to use (red, green, blue, yellow, magenta, cyan, purple) use initials")]
    public string Colors { get; set; } = "all";

    [Option('b', "background", Required = false, Default = "gradient", HelpText =
        "Background color (red, green, blue, yellow, magenta, cyan, purple, black, white) or circular gradient format: 'from'x'to'")]
    public string Background { get; set; } = "gradient";

    [Option('o', "output", Required = true, Default = "", HelpText = "Output file name")]
    public string FileName { get; set; } = "Mandala.png";

    [Option('l',"overlap", Required = false, Default = 0f, HelpText = "Overlap between figures in percentage")]
    public float Overlap { get; set; } = 0;
}



class Program
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
    struct Figure
    {
        public Figures Type { get; set; }
        public float Start;
        public float End { get; private set; }
        public int Symmetry { get; set; }
        public Rgba32 Color { get; set; }
        public bool Fill { get; set; }

        public Figure(Figures type, float start, float end, int symmetry, Rgba32 color, bool fill)
        {
            Type = type;
            Start = start;
            End = end;
            Symmetry = symmetry;
            Color = color;
            Fill = fill;
        }
        public  void SetRadius(float start, float end)
        {
            Start = start;
            End = end;
        }

    }

    static void Main(string[] args)
    {
        // Set the size of the image
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
        {
            // Create a blank image with a white background
            using (Image<Rgba32> image = new Image<Rgba32>(o.Width, o.Height, new Rgba32(255, 255, 255)))
            {
                Vector2 center = new Vector2((float)o.Width / 2, (float)o.Height / 2);
                // Create a random number generator
                Random rand = new Random();

                // Draw the background
                if (o.Background == "gradient") {



                    List<Rgba32> cols = new List<Rgba32> {Red, Green, Blue, Yellow, Magenta, Cyan, Purple, White, Black};
                    DrawBackground(image, cols[rand.Next(cols.Count)], cols[rand.Next(cols.Count)]);
                }



                else
                if (o.Background.Contains("x")) {
                    DrawBackground(image, GetColor(o.Background.Split('x')[0]), GetColor(o.Background.Split('x')[1]));
                }
                else {
                    DrawBackground(image, GetColor(o.Background));
                }

                float maxRadius = (float)Math.Min(o.Width, o.Height) / 2;

                // Setups the figures to draw


                List<Rgba32> colors = new List<Rgba32>();
                if (o.Colors == "all" || o.Colors == ""){
                    List<Rgba32> colorsList = new List<Rgba32> {Red, Green, Blue, Yellow, Magenta, Cyan, Purple};
                    int lenght = colorsList.Count;
                    for(int i = 0; i < lenght; i++)
                    {
                        int index = rand.Next(colorsList.Count);
                        colors.Add(colorsList[index]);
                        colorsList.Remove(colorsList[index]);
                    }
                }

                else {
                    foreach (char c in o.Colors)
                    {
                        switch (c) {
                            case 'r': colors.Add(Red); break;
                            case 'g': colors.Add(Green); break;
                            case 'b': colors.Add(Blue); break;
                            case 'y': colors.Add(Yellow); break;
                            case 'm': colors.Add(Magenta); break;
                            case 'c': colors.Add(Cyan); break;
                            case 'p': colors.Add(Purple); break;
                            default: Console.WriteLine("Invalid color ignored"); break;
                        }
                    }

                    if (colors.Count == 0)
                    {
                        colors.Add(Red);
                        colors.Add(Green);
                        colors.Add(Blue);
                        colors.Add(Yellow);
                        colors.Add(Magenta);
                        colors.Add(Cyan);
                        colors.Add(Purple);
                    }
                }

                List<Figure> figures = new List<Figure>();
                if (o.Figures == "all" || o.Figures == "")
                {
                    List<Figures> types = new List<Figures> {Figures.Circles, Figures.Triangles, Figures.Diamonds, Figures.RoundedSquares, Figures.Ring};
                    int lenght = types.Count;
                    for(int i = 0; i < lenght; i++)
                    {
                        int index = rand.Next(types.Count);
                        bool fill = rand.Next(2) == 1;
                        figures.Add(new Figure(types[index], 0, 80, o.Symmetry, colors[i % colors.Count], fill));
                        types.Remove(types[index]);
                    }
                }
                else
                {
                    int colorIndex = 0;
                    bool hasFigure = false;
                    bool hasMultiplier = false;
                    Figures type = Figures.Circles;
                    int symmetry = o.Symmetry;
                    bool fill = false;
                    for (int i = 0; i < o.Figures.Length; i++)
                    {
                        char c = o.Figures[i];
                        if (hasFigure && hasMultiplier)
                        {
                            if (c == 'f')
                                fill = true;
                            else {
                                fill = false;
                                i--;
                            }
                            hasFigure = false;
                            hasMultiplier = false;
                            figures.Add(new Figure(type, 0, 80, symmetry, colors[(colorIndex++)%colors.Count], fill));
                            symmetry = o.Symmetry;
                        }
                        else if (hasFigure)
                        {
                            try
                            {
                                symmetry = int.Parse(c.ToString()) * o.Symmetry;
                                hasMultiplier = true;
                            }
                            catch (Exception e)
                            {
                                //not a number
                                if (c == 'f')
                                    fill = true;
                                else
                                {
                                    fill = false;
                                    i--;
                                }
                                hasFigure = false;
                                hasMultiplier = false;
                                figures.Add(new Figure(type, 0, 80, symmetry, colors[(colorIndex++)%colors.Count], fill));
                                symmetry = o.Symmetry;
                            }
                        }
                        else {
                            switch (c) {
                                case 'c':
                                    type = Figures.Circles;
                                    hasFigure = true;
                                    break;
                                case 't':
                                    type = Figures.Triangles;
                                    hasFigure = true;
                                    break;
                                case 'd':
                                    type = Figures.Diamonds;
                                    hasFigure = true;
                                    break;
                                case 's':
                                    type = Figures.RoundedSquares;
                                    hasFigure = true;
                                    break;
                                case 'r':
                                    type = Figures.Ring;
                                    hasFigure = true;
                                    break;
                                default: Console.WriteLine("Invalid figure type ignored"); break;
                            }
                            if(i+1 == o.Figures.Length)
                                figures.Add(new Figure(type, 0, 80, symmetry, colors[(colorIndex++)%colors.Count], false));
                        }
                    }
                }


                CalculateRadius radius = new CalculateRadius(maxRadius, figures.Count, o.Overlap);
                for (int i = 0; i < figures.Count; i++)
                {
                    Tuple<float, float> r = radius.Next();

                    //cause struct in array are immutable
                    Figure f = figures[i];
                    f.SetRadius(r.Item1, r.Item2);
                    figures[i] = f;
                }

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
                            DrawRing(image, f.Start, f.End, o.Symmetry * 20, f.Color, center, f.Fill);
                            break;
                        default:
                            Console.WriteLine("Invalid figure type ignored");
                            break;
                    }
                }

                //DrawCrescents(image, 320, o.Symmetry , 100, Red, center, .1f, 0.9f, true);

                image.Save(o.FileName);
            }
        });
    }

    static Rgba32 GetColor(string color)
    {
        switch (color) {
            case "red": return Red;
            case "green": return Green;
            case "blue": return Blue;
            case "yellow": return Yellow;
            case "magenta": return Magenta;
            case "cyan": return Cyan;
            case "purple": return Purple;
            case "black": return Black;
            case "white": return White;
            default: return White;
        }
    }
}

class CalculateRadius
{
    float maxRadius;
    int count;
    float overlap;
    float average;
    float maxDelta;
    float minDelta;
    float innerRadius;
    float outerRadius;
    float lastThickness;
    private Random rand;

    public CalculateRadius(float maxRadius, int count, float overlap)
    {
        this.count = count;
        this.maxRadius = maxRadius;
        this.overlap = overlap;
        average = maxRadius / count;
        //limit the delta to 20% of the average
        maxDelta = average * 1.2f;
        minDelta = average * 0.8f;
        innerRadius = 0;
        outerRadius = 0;
        lastThickness = 0;
        rand = new Random();
    }
    public Tuple<float, float> Next()
    {
        innerRadius = outerRadius - (lastThickness * (overlap/100));
        float thikness = (float)rand.Next((int)(minDelta*1000),(int)(maxDelta*1000)) / 1000;
        lastThickness = thikness;
        outerRadius = Math.Min(innerRadius + thikness, maxRadius);
        average = ((average * count) - thikness) / (count - 1);
        count--;
        maxDelta = average * 1.2f;
        minDelta = average * 0.8f;
        if (minDelta > maxDelta)
        {
            (minDelta, maxDelta) = (maxDelta, minDelta);
        }

        return new Tuple<float, float>(innerRadius, outerRadius);
    }
}
