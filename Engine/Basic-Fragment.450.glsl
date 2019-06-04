#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 1, binding = 0) uniform texture2D SpriteTexture;
layout(set = 0, binding = 1) uniform sampler SpriteSampler;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;
layout (location = 0) out vec4 outputColor;

void FS()
{
    outputColor = color * texture(sampler2D(SpriteTexture, SpriteSampler), texCoord);
}
