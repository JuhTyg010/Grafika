#version 330 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vColor;
layout (location = 2) in vec3 vNormal;
layout (location = 3) in vec2 vTxt;
layout (location = 4) in float vSize;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 fColor;
out vec2 fTxt;
out vec3 fNormal;
out vec4 fWorld;

void main()
{
    // World-space coordinates.
    fWorld = model * vec4(vPos, 1.0);

    // View- and then the projection-transform.
    gl_Position = projection * view * fWorld;

    // Point size.
    gl_PointSize = vSize;

    // Normal in the world space (not entirely correct).
    fNormal = normalize(vec3(model * vec4(vNormal, 0.0)));

    // Correctly divided out amongst the fragments.
    fColor = vColor;
    fTxt = vTxt;
}
