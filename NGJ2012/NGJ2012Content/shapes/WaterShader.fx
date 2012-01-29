uniform const texture BasicTexture;

uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (BasicTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};

uniform const float4x4	World		: register(vs, c0);	// 0 - 3
uniform const float4x4	View		: register(vs, c4);	// 4 - 7
uniform const float4x4	Projection	: register(vs, c8);	// 8 - 11

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 pos_ws = mul(input.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	output.Position = pos_ps;
	output.Color = input.Color;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if(input.Color.r > 0.93) {
		return float4(0.88,0.88,1,1);
	} else {
		float darkness = lerp(0.5, 1, input.Color.r);
		return float4(0.05*darkness,0.05*darkness,0.8*darkness,0.5);
	}
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
