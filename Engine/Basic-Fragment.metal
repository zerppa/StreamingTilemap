#include <metal_stdlib>
using namespace metal;

struct PS_INPUT
{
    float4 pos [[ position ]];
    float4 col;
    float2 uv;
};

fragment float4 FS(
    PS_INPUT input [[ stage_in ]],
    texture2d<float> SpriteTexture [[ texture(0) ]],
    sampler SpriteSampler [[ sampler(0) ]])
{
    float4 out_col = input.col * SpriteTexture.sample(SpriteSampler, input.uv);
    return out_col;
}