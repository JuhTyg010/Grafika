#version 330 core

in vec3 fColor;
in vec2 fTxt;

uniform bool useTexture;
uniform sampler2D tex;

out vec4 FragColor;

void main()
{
  if (useTexture)
    FragColor = texture(tex, fTxt);
  else
    FragColor = vec4(fColor, 1.0);
}
