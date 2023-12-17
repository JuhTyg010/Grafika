# Documentation of the task "04-Mandala"

## Author
Marek Sádovský

## Command line arguments
-i --input, Required = true, input image

-h --huedelta, Required = true, value of chenge 

-o --output, Required = true, output file (image)

## Input data
only input is image

## Algorithm
I render so called regions based on the colors. then there is list of colors which are offen used as skin colors. Then it choose regions
Where is high probability to region being skin and ignore pixels in those regions. 

## Extra work / Bonuses
There is algorithm which should find all sections which are skin and if not he still identify it as one part of image.

## Use of AI
Tried to use for getting some constants But never used them. 