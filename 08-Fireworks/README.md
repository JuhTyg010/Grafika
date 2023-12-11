# Task 08-Fireworks
Your task is to implement a **simulation of fireworks** using a 3D **particle system**.
Individual particles (flying rockets, sparks, flares) should obey the **laws of physics**
(gravity and air resistance), but you do not have to worry about their mutual
interactions or collisions.

The entire 3D scene (fireworks) must be rendered interactively using the
[OpenGL library](https://www.opengl.org/) bound to the .NET through the
[Silk.NET library](https://github.com/dotnet/Silk.NET).

# Ideas & Details
* **Gravity** is a constant force pointing downwards (g=9.81 m/s^2)
* **Drag vector** is pointing against the current velocity - [Wikipedia page](https://en.wikipedia.org/wiki/Drag_(physics))
  ([more simple Czech version](https://cs.wikipedia.org/wiki/Odpor_prost%C5%99ed%C3%AD)).
  You can experiment with coefficients but basic **laminar (linear)** +
  **turbulent (quadratic)** formula will suffice
* The concept **"launchers + particles"** is recommended
* **Launchers** are points or areas where new particles are created
  - Launchers don't need to be visible (but you can render them for bonus points)
* **Particles** have obvious physical/appearance attributes plus simulation-logic
  related quantities
  - current **position** (`Vector3`)
  - current **velocity** (`Vector3`)
  - current **RGB color** (`Vector3`)
  - **age** in seconds (`float`) to simulate aging and demise
  - **inner state** of the particle (anything you need for proper simulation)
  - **mass** could be constant (`float`)
  - **drag coefficients** could be constant (`float[]`)
* Particles **need not** interact with each other
* **Discrete time simulation** approximates an actual continuous spacetime by using a short
  period of time (**delta-t**, `dT` in seconds) to simplify the simulation
  - `dT` could be the time between two successive rendered frames
    (i.e. 1/60 of the second or so) 
  - We assume that during the simulation step (`dT`) the values of some
    quantities **remain constant** (although this is not actually true). Velocity
    vector (~drag vector)...
  -	This is called **"Euler method"** for solving differential equations
    [see the Wikipedia page](https://en.wikipedia.org/wiki/Euler_method).
    Higher order methods (like
    [Runge-Kutta](https://en.wikipedia.org/wiki/Runge%E2%80%93Kutta_methods)) are not necessary.
  - Just a reminder how basic mechanic simulation works:
    1. determine all the **forces** active during the simulation step/interval (gravity, drag)
    2. compute the **total acceleration** (`Vector3`) from the forces
    3. apply the acceleration to the current position and velocity - compute the
       **new position and velocity**
       (you can use a little bit smarter formula for that, using the idea of
       [uniform acceleration](https://en.wikipedia.org/wiki/Acceleration))
    4. that's it for the **mechanics**, now you should update color, age, inner state...
       accordingly
* You should manage the reasonable **number of current particles** in the
  system by modulating the creation rate in launchers and/or changing the
  particle life span.
* Launchers should be active **during every simulation step** (`dT`) to add new
  particles into the system (trying to dilute the addition of new particles).
  This will make the simulation steps less obvious.

## Rendering
Use of point primitives (`GL_POINTS` in plain OpenGL, `PrimitiveType.Points` in Silk.NET)
is recommended. You can use point color and size to get better visualization.

It makes no sense to use **Index buffers** (`Gl.DrawElements()`) to draw points,
you should call `Gl.DrawArrays()` instead.

You should keep the coordinates of all the simulated particles in one large
**vertex buffer** (`VB`). Don't complicate you implementation too much - there
could be a secondary array `List<struct Particle> Particles`(usig the same
indices as the `VB`) to describe the set of particles. `VB` will be used
for rendering, `Particles` for everything else. **Don't read** from GPU's copy
of vertex buffer, just update it using `Gl.BufferSubData()` calls (the process could
be summarized as **"Simulate - UpdateVB - Render"**).
The only complication arises when a particle **retires**. You should either reuse
its index immediately or come up with some mechanism to tell the vertex
shader to avoid that vertex...?

No special **fragment shader** is required, you can only change it in case of
advanced appearance improvements.

# Sil.NET framework
It is easy to use the [Silk.NET](https://github.com/dotnet/Silk.NET) in your C#
program, you just install the [Silk.NET NuGet package](https://www.nuget.org/packages/Silk.NET/).

You can view our sample projects in the
[Silk3D directory](../Silk3D/README.md) of our repository.

# Your solution
Please place your solution in a separate [solutions](solutions/README.md)
directory in the repository. You'll find short instructions there.

# Launch date
**Monday 11 December 2023**
(Don't work on the solution before this date)

# Deadline
See the shared [point table](https://docs.google.com/spreadsheets/d/1QLukOcSRPa5exOYW1eUfQWY2WoMjo1menbjQIU7Gvs4/edit?usp=sharing).

# Credit points
**Basic solution: 10 points**
* Working interactive fireworks simulation
  - The particles must move according to the **laws of physics** (so that no disturbing
    deviations are visible at first glance)
  - The particles must have a **lifetime limit** so that the simulation
    system is not overloaded (the simulation must be able to run for several minutes)
* The entire 3D scene is interactively viewed by the user using the [Trackball system](../Silk3D/shared/Trackball.cs)

**Bonus points: up to 14+ more points**
* Animation of launch ramps
* Multiple rocket/particle types
* Multi-stage explosions
* Color/point-size changes during life of a particle/rocket...
* Visualization of rocket trajectories
* Interactive fireworks control (mouse, keyboard). Launcher fire trigger
* Advanced shading effects, etc.

## Use of AI assistant
It is possible to use an AI assistant, but you have to be critical and
test all its suggestions thoroughly.

# Images
![KiLi](fireworks-KiLi.png)
