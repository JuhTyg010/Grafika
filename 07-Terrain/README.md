# Task 07-Terrain
Your task is to implement fractal terrain generation using a "subdivision" algorithm. 3D scene will
be displayed in an interactive environment using
[OpenGL library](https://www.opengl.org/) bound to the .NET by the
[Silk.NET library](https://github.com/dotnet/Silk.NET).

# Ideas
You can find many pages dedicated to random terrain generation, for example
[this one on vterrain.org](http://vterrain.org/Elevation/Artificial/) or
[Fractal landscape on Wikipedia](https://en.wikipedia.org/wiki/Fractal_landscape).
One slightly outdated page -
[Terrific: Fast Terrain Rendering](https://www.cosc.brocku.ca/Offerings/4P98/gallery/projects/Winter2008/bg05he/)

I'd recommend simple and efficient
[Diamond-square algorithm](https://en.wikipedia.org/wiki/Diamond-square_algorithm), but
you can use any algorithm which is capable of creating terrains gradually, by increasing
the subdivision (recursion) depth.

## Commands
Use **keyboard commands** to change the terrain triangle mesh. You should implement at least
these commands:
* **Subdivide** the mesh - create more detailed mesh. You should keep the shared vertices
  and insert new ones according to the defined Hausdorff coefficient (relative amplitude of the
  random displacement of the middle points).
* **Updivide** the mesh - return back to the coarser mesh, so number of vertices and triangles will
  disappear. Executing the Updivide+Subdivide command pair will cause the original terrain to
  change slightly.
* Change the **Hausdorff coefficient** - random subdivision of an edge should use the random value
  based on original edge size and this coefficient.
* You should compute **normal vectors** (at the vertices) automatically, use the `I` key to toggle the
  shading mode to see results.
* Include all the **new keyboard commands** (hotkeys) in the `F1` help list.

## Rendering
Use one large vertex buffer (`VB`) for storing all vertices of the mesh.
Index buffer[s] (`IB`) should be used to define triangles of the mesh (three indices per triangle).

You have two options:
1. either you will re-upload the whole `VB` and `IB` every time subdivision/updivision
is required,
2. or you come up with a smarter solution (e.g. you can allocate full-sized `VB` at the beginning,
using only sparse vertices from it, according to `IB` specific to every subdivision level). `VB` will
be updated but index buffers can be prepared in advance and remain constant.

**Vertex shader** is almost the "classical" one - doing "model-view-transform" for vertex
coordinates, passing the rest of quantities unchanged. The only extension is passing the original
*world-space coordinates* for shading computations in the fragment shader.

**Fragment shader** in the pilot solution is able to compute optional "Phong shading".
You will need to provide valid **normal vectors** before turning on the shading!
You are supposed to update the shader if you're going to implement more advanced
terrain visualization (texture[s], etc).

Pilot solution shows how to **update vertex buffer data** - see the
`UpdateVertex()` function. Locking was introduced because the **UI** (keyboard) and
**rendering** (`OnRender()`) run in parallel.

# Silk.NET framework
It is easy to use the [Silk.NET](https://github.com/dotnet/Silk.NET) in your C#
program, you just install the [Silk.NET NuGet package](https://www.nuget.org/packages/Silk.NET/).

You can view our sample projects in the
[Silk3D directory](../Silk3D/README.md) of our repository.

# Your solution
Please place your solution in a separate [solutions](solutions/README.md)
directory in the repository. You'll find short instructions there.

# Launch date
**Monday 4 December 2023**
(Don't work on the solution before this date)

# Deadline
See the shared [point table](https://docs.google.com/spreadsheets/d/1QLukOcSRPa5exOYW1eUfQWY2WoMjo1menbjQIU7Gvs4/edit?usp=sharing).

# Credit points
**Basic solution: 10 points**
* Variable **Hausdorff dimension** coefficient (random relative amplitude)
* Variable level of recursion (interactive subdivide/updivide)
* Normal vectors for basic appearance
* Interactive terrain exploration (e.g. using the [Trackball class](../Silk3D/shared/Trackball.cs))

**Bonus points: up to 16+ more points**
* Vertex color according to elevation (2)
* Reasonable terrain texturing (2)
* Initialization from height-map texture (4)
* Infinite terrain concept ... "periodic terrain" (4)
* Hovercraft mode, interactive or scripted (3 to 8)

## Use of AI assistant
It is possible to use an AI assistant, but you have to be critical and
test all its suggestions thoroughly.

# Images
![Diamond-square algorithm](diamond-square-diagram.png)
Diagram of the diamond-square subdivision.

![Screenshot](terrain-screenshot.jpg)
Example of terrain visualization using vertex colors and simple shading.