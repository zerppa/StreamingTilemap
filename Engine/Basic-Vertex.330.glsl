#version 330 core

layout(std140) uniform OrthographicProjection
{
    mat4 projection_matrix;
};

in vec2 in_position;
in vec2 in_texCoord;
in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
	texCoord = in_texCoord;
}
