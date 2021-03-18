#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float _power; // 1.0
float _radius; // 1.25
float Time;


texture2D _MainTex;
sampler2D _MainTexSampler = sampler_state {
	Texture = <_MainTex>;
};

float4 InvertPS(float2 uv : VPOS) : COLOR
{
	uv = (uv + 0.5) * float2(1.0 / 1920.0, 1.0 / 1080.0);
	float4 color = tex2D(_MainTexSampler, uv);
	return 1 - color;
}

technique Invert
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL InvertPS();
	}
};


float4 ChromaticAberrationPS(float2 uv : VPOS) : COLOR
{
	uv = (uv + 0.5) * float2(1.0 / 1920.0, 1.0 / 1080.0);
	
	float strenght = 5;
	float3 rgbOffset = 1 + float3(0.01, 0.005, 0) * strenght;

	float dist = distance(uv, float2(0.5, 0.5));
	float2 dir = uv - float2(0.5, 0.5);

	rgbOffset = normalize(rgbOffset * dist);

	float2 uvR = float2(0.5, 0.5) + rgbOffset.r * dir;
	float2 uvG = float2(0.5, 0.5) + rgbOffset.g * dir;
	float2 uvB = float2(0.5, 0.5) + rgbOffset.b * dir;

	float4 colorR = tex2D(_MainTexSampler, uvR);
	float4 colorG = tex2D(_MainTexSampler, uvG);
	float4 colorB = tex2D(_MainTexSampler, uvB);
	//dosomething 

	return float4(colorR.r, colorG.g, colorB.b, 1);
}

technique ChromaticAberration
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL ChromaticAberrationPS();
	}
};


float4 mainPS(float2 texCoord:TEXCOORD0) : COLOR0
{
	float power = 1;
	float radius = clamp((1 + sin(Time)), 0,1.5f);
	float4 color = tex2D(_MainTexSampler, texCoord);
	float2 dist = (texCoord - 0.5f) * radius;
	dist.x = 1 - dot(dist, dist) * power;
	color.rgb *= dist.x;

	return color;
}

technique Vignette
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL mainPS();
	}
}

float4 PixelShaderPS(float2 Tex: TEXCOORD0) : COLOR
{
	 Tex.x += sin(Time + Tex.x * 10) * 0.01f;
	 Tex.y += cos(Time + Tex.y * 10) * 0.01f;

	 float4 Color = tex2D(_MainTexSampler, Tex);
	 return Color;
}


technique PostProcess
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderPS();
	}
}