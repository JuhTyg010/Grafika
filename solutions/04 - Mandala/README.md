# Documentation of the task "04-Mandala"

## Author
Marek Sádovský

## Command line arguments
-i --input, Required = true, Default = ""
  -c --count, Required = true, Default = 1
  -o --output, Required = false, Default = ""
-w --width, Required = true, Default = "", Width of the image
-h --height, Required = true, Default = "", Height of the image
-s --symmetry, Required = false, Default = 6, Symmetry order - to how many parts will the circle parse
-f --figures, Required = false, Default = "all", Figures to draw (all, circles, triangles, diamonds, roundedSquares, ring)
 use initials (s for squares))and number as multiplier (s2 for squares with 2x multiplier) and f for fill
 (sf for filled squares or s2f for filled 2x multiplied)
    
-c --colors, Required = false, Default = "all", Colors to use (red, green, blue, yellow, magenta, cyan, purple) use only initials (e.i. -c rymc) for red, yellow magenta cyan
-b --background, Required = false, Default = "gradient", Background color (red, green, blue, yellow, magenta, cyan, purple, black, white) or circular gradient format: 'from'x'to'
-o --output, Required = true, Default = "", Output file name
-l --overlap, Required = false, Default = 0f, Overlap between figures in percentage(0-100) 

## Input data
there are only arguments no other input data

## Algorithm
using own implemented class MyDraw to draw objects defined by some points not using kartezian but somekind of rotational scheme.
That helps when you need to create rotated copies. By default when no special arg is given the algorith creates mandala using all figures

## Extra work / Bonuses
There are many arguments to chenge behavior. Maybe gradient background

## Use of AI
None sorry.
