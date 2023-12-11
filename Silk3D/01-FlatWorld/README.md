# `01-FlatWorld`
You can place 2D objects (triangles) on the plane. Elementary
interaction using keyboard & mouse, matrix transformations.

The last object (= current object) can be moved and rotated using mouse.

## Keyboard
**+**
Adds a new object.

**-**
Deletes the current object (unless it is the only one).

**Home**
Resets transformation of the current object.

**Esc**
Quits the application.

**F1**
Help is printed to the console window.

## Mouse
**Left button**
Dragging (translation) of the current object.

**Wheel**
Rotation of the current object.

# Notes
* matrix transformations
  - concatenation of three matrices (uniforms) in the vertex shader:
    - **model transform** places an object into the **world space**
    - **view transform** takes care of scene visibility
    - **projection transform** - 2D "orthographics" projection is used
* minimal set of OpenGL shaders
  - **vertex shader** - performs "model-view-transform"
  - **fragment shader** - assigns color interpolated from vertices, no textures are used
* **window resize** handling
  - the defined scene region (diameter = 4) is always displayed
