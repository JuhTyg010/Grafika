# `02-Trackball`
3D cube is examined using mouse in the "trackball" mode.

## Keyboard
**T**
Toggle texture.

**P**
Switch between perspective & orthographic camera.

**C**
Camera reset.

**Left, Right**
Rotate the object.

**F1**
Help is printed to the console window.

**Esc**
Quits the application.

## Mouse
**Left button**
Rotating of the object in front of the viewer using **Trackball**.

**Wheel**
Zoom in/out.

**Right button**
Drag the object.

# Notes
* matrix transformations
  - concatenation of three matrices (uniforms) in the vertex shader:
    - **model transform** places an object into the **world space**
    - **view transform** takes care of scene visibility
    - **projection transform** - 2D "orthographics" projection is used
* set of OpenGL shaders
  - **vertex shader** - performs "model-view-transform"
  - **fragment shader** - assigns color interpolated from vertices, or texture
* **window resize** handling
  - the defined scene region (diameter = 3) is always displayed
