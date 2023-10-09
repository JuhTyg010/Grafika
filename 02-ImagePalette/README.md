# Task 02-ImagePalette
Your task is to design an algorithm that automatically computes a representative
color palette from an input raster image. The inspiration for this assignment
is a project [Cinema Palettes](https://www.facebook.com/cinemapalettes).

Your algorithm should analyze the input image and provide 3 to 10 characteristic
colors. If the image has a smaller number of hues (unique colors), you can fill
the rest of the palette with additional colors, for example inspired by a color
scheme from [Adobe Color](https://color.adobe.com/create/color-wheel).

* Your task is to write a **C# command-line** program which reads an input image
  and prints down the palette in RGB
* Use simple text output format: three numbers on a line, one line per color
  * for bonus points, you can create a simple output image with colored rectangles
* One command line argument defines the **input file name**, another the **desired
  number of colors**, for example
```bash
 > imagepalette -i "input.png" -c 10
```
* You may not meet the required number of colors if you have serious reasons
  (which you must explain)

## Notes
You should use `.NET 6` which is available for all platforms now.

### Image library
Using of a simple image processing library is recommended. My option would
be [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp/).

### Command-line parsing
For the parsing of command-line arguments I'd recommend
[CommandLineParser](https://www.nuget.org/packages/CommandLineParser/).

### More libraries
When you use an AI wizard, it may suggest a numerical or statistical library.
You can use it, but do it properly through the [NuGet system](https://www.nuget.org/).

# Your solution
Please place your solution in a separate [solutions](solutions/README.md)
directory in the repository. You'll find short instructions there.

# Launch date
**Monday 16 October 2023**
(Don't work on the solution before this date)

# Deadline
See the shared [point table](https://docs.google.com/spreadsheets/d/1QLukOcSRPa5exOYW1eUfQWY2WoMjo1menbjQIU7Gvs4/edit?usp=sharing).

# Credit points
It has not yet been determined.

<!--
**Basic solution: 9 points**
* all images must contain 16M colors
* image size in pixels via arguments
* output file name specified in an argument
* mode selection by command line argument
* at least three modes: `trivial`, `random`, `pattern` (pattern could be static)

**Bonus points: up to 6 more points**
* more patterns (parametrizable, visually more appealing)
  * "mandala" style (circular symmetry)
  * "ornament" could use a recursive pattern or another sort of repetition...
* more command-line arguments
-->

## Use of AI assistant
Using an AI assistant is recommended! But you have to be very critical and
test all its suggestions thoroughly. Especially test singular cases (one
pixel input image, single color image, etc.).

# Example
See the [Cinema Palettes page](https://www.facebook.com/cinemapalettes) for many examples.
Most are good enough.
