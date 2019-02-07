@echo off

glslangvalidator -V -S vert Basic-Vertex.450.glsl -o Basic-Vertex.450.spv
glslangvalidator -V -S frag Basic-Fragment.450.glsl -o Basic-Fragment.450.spv