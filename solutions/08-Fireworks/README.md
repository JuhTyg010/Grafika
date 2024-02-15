# Documentation of the task "08-Firework"

## Author
Marek Sádovský

## Command line arguments
-w, -h - initial window size in pixels

-p - maximum number of particles in the system (see details later)

-i - initial launcher caunt (accept range 1-10)

-t - optional texture file (default is :check: = checkerboard)

-c - by default false if set to true you get more colors from rockets


## Input data
G - add launcher (max 10)

H - remove launchr (min 0)

function keys (f1 to f10) - manual fire first to 10-th launcher

W/S - zoom in/out

A/D - camera move left/right

Q/E - camera move down/up

R - reset the simulation

C - camera reset

T - toggle texture

I - toggle phong shading

P - toggle perspective

V - toggle Vsync

ESC - quits the program

Mouse.left - Trackball rotation

Mouse.wheel - zoom in/out

## Algorithm
I have Object (class) Particle and some child Objects. I apply gravity and physics on them. Using the simulation to simulate it in time.
simulate control how much of particles we have it generates new ones and so on. Buffer calls simulation to get data and simulation asks 
every particle to get specific data of the paritcle. Automatic particle count adjustment for better performence.


## Extra work / bonuses
There is posibily that rocket explodes to more rockets wich will explode later on. In other words multiple stage explousion. Also there is trail of the rockets and every explousion 
slowely wanishes. You can also interactively fire from launchers generate new ones or remove old. Color of particles change over time and geting smaller and disappier like in real life.
colors are selected the basic ones but can be changed by argument it will add more colors.

## Use of AI
None sorry.
