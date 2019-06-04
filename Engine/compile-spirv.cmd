@echo off

glslangvalidator -V -S vert -e VS --source-entrypoint main Basic-Vertex.450.glsl -o Basic-Vertex.450.spv
glslangvalidator -V -S frag -e FS --source-entrypoint main Basic-Fragment.450.glsl -o Basic-Fragment.450.spv