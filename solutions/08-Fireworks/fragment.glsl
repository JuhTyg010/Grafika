#version 330 core

in vec3 fColor;
in vec2 fTxt;
in vec3 fNormal;
in vec4 fWorld;

uniform vec3 lightColor;
uniform vec3 lightPosition;
uniform vec3 eyePosition;
uniform float Ka;
uniform float Kd;
uniform float Ks;
uniform float shininess;
uniform bool usePhong;

uniform bool useTexture;
uniform sampler2D tex;

out vec4 FragColor;

void main()
{
  vec4 diffuseColor = useTexture ? texture(tex, fTxt) : vec4(fColor, 1.0);

  if (usePhong)
  {
    // Phong shading & interpolation.
    vec3 P = fWorld.xyz;
    vec3 N = normalize(fNormal);
    vec3 L = normalize(lightPosition - P);
    vec3 V = normalize(eyePosition - P);
    vec3 H = normalize(L + V);

    float cosb = 0.0;
    float cosa = dot(N, L);
    if (cosa > 0.0)
      cosb = pow(max(dot(N, H), 0.0), shininess);
    else
      cosa = 0.0;

    // Let's sum all Phong shading components.
    FragColor = Ka * diffuseColor +
                Kd * cosa * diffuseColor * vec4(lightColor, 0.0) +
                Ks * cosb * vec4(lightColor, 0.0);
    FragColor.w = 1.0;
  }
  else
    FragColor = diffuseColor;
}
