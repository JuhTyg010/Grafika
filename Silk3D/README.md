# Silk3D - 3D graphics support
The library [Silk.NET](https://github.com/dotnet/Silk.NET/tree/main)
([web page link](https://dotnet.github.io/Silk.NET/)) supports APIs like
[OpenGL](https://www.khronos.org/opengl/),
[DirectX 12](https://docs.microsoft.com/en-us/windows/win32/direct3d12/directx-12-programming-guide),
[Vulkan](https://www.khronos.org/vulkan/),
[OpenCL](https://www.khronos.org/opencl/) and
[OpenAL](https://openal.org/). We will be using OpenGL in NPGR003 labs.

## Directory `shared`
This directory contains source files independent of specific project.
The main purpose is your convenience.
* `BufferObject.cs` - wrapper for general server-side buffer object
* `Shader.cs` - wrapper for the shader-program (minimum: vertex shader, fragment shader)
* `Texture.cs` - wrapper for texture (can be read from a disk file using the
  `SixLabors.ImageSharp` library)
* `Trackball.cs` - interactive 3D scene rotation tool
* `VertexArrayObject.cs` - wrapper for the VAO (vertex buffer + index buffer)

## Sample projects
[01-FlatWorld](01-FlatWorld/README.md) - simple 2D object on the plane. Elementary
interaction using keyboard, matrix transformations.

[02-Trackball](02-Trackball/README.md) - 3D graphics demo using the `class Trackball`
for rotation in 3D.
