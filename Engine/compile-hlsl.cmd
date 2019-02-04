@echo off

fxc /E VS /T vs_5_0 Basic-Vertex.hlsl /Fo Basic-Vertex.hlsl.bytes
fxc /E FS /T ps_5_0 Basic-Fragment.hlsl /Fo Basic-Fragment.hlsl.bytes