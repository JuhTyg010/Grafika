# Documentation of the task "08-Firework"

## Author
Marek Sádovský

## Command line arguments
command-line arguments
-w, -h - initial window size in pixels
-p - maximum number of particles in the system (see details later)
-r - particle generator rate (number of generated particles in one second, could be changed later using the keys Up and Down)
-t - optional texture file (default is :check: = checkerboard)


## Input data
G - add launcher (max 10)
H - remove launchr (min 0)
function keys (f1 to f10) - manual fire first to 10-th launcher
W/S - zoom in/out
A/D - camera move left/right
Q/E - camera move down/up


## Algorithm
I have Object (class) Particle and some child Objects. I apply gravity and physics on them. Using the simulation to simulate it in time.
simulate control how much of particles we have it generates new ones and so on. Buffer calls simulation to get data and simulation asks 
every particle to get specific data of the paritcle.


## Extra work / bonuses
there is posibily that rocket explodes to more rockets wich will explode later on. Also there is trail of the rockets and every explousion 
slowely wanishes.

## Use of AI
None sorry.
