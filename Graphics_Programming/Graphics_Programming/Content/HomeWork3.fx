#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External Properties
float4x4 World, View, Projection;

float3 LightPosition, CameraPosition;
float Time;

Texture2D DayTex;
sampler2D DayTextureSampler = sampler_state
{
    Texture = <DayTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D NightTex;
sampler2D NightTextureSampler = sampler_state
{
    Texture = <NightTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D CloudsTex;
sampler2D CloudsTextureSampler = sampler_state
{
    Texture = <CloudsTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU = WRAP;
};

Texture2D MoonTex;
sampler2D MoonTextureSampler = sampler_state
{
    Texture = <MoonTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D MarsTex;
sampler2D MarsTextureSampler = sampler_state
{
    Texture = <MarsTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

TextureCube SkyTex;
samplerCUBE SkyTextureSampler = sampler_state
{
    Texture = <SkyTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};


// Getting out vertex data from vertex shader to pixel shader
struct VertexShaderOutput {
    float4 position     : SV_POSITION;
    float4 color        : COLOR0;
    float2 uv           : TEXCOORD0;
    float3 worldPos     : TEXCOORD1;
    float3 worldNormal  : TEXCOORD2;
};

// Vertex shader, receives values directly from semantic channels
VertexShaderOutput MainVS(float4 position : POSITION, float4 color : COLOR0, float2 uv : TEXCOORD, float3 normal : NORMAL)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.position = mul(mul(mul(position, World), View), Projection);
    output.color = color;
    output.uv = uv;
    output.worldPos = mul(position, World).xyz;
    output.worldNormal = mul(normal, World).xyz;

    return output;
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Textures
    float4 texColor = tex2D(DayTextureSampler, input.uv);
    float4 nightColor = tex2D(NightTextureSampler, input.uv);
    float4 cloudsColor = tex2D(CloudsTextureSampler, input.uv + half2(Time * 0.0045f, 0));

    // calculate vector for light
    float3 viewDirection = normalize(input.worldPos - CameraPosition);
    float3 lightDirection = normalize(input.worldPos - LightPosition);

    // calculate specular
    float3 refl = normalize(-reflect(lightDirection, input.worldNormal));
    float spec = pow(max(dot(refl, normalize(viewDirection)), 0.0), 8);

    // calc light
    float light = max(dot(input.worldNormal, -lightDirection), 0.0);
    float3 skyColor = float3(.529f, 0.808f, 0.992f);
    float3 fresnel = pow(dot(input.worldNormal, viewDirection) * .5 + .5, 3) * 8 * light * skyColor;
    float3 diffuse = lerp(nightColor.rgb, texColor.rgb, light) + cloudsColor.rgb * light;
    return float4((max(light, 0.4) + spec) * diffuse.rgb + fresnel, 1);
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 SkyPS(VertexShaderOutput input) : COLOR
{
    float3 viewDirection = normalize(input.worldPos - CameraPosition);
    float3 skyColor = texCUBE(SkyTextureSampler, viewDirection).rgb;

    return float4(pow(skyColor,2),1);
}

float4 MoonPS(VertexShaderOutput input) : COLOR
{
    // Textures
    float4 texColor = tex2D(MoonTextureSampler, input.uv);
    float3 lightDirection = normalize(input.worldPos - LightPosition);
    // calc light
    float light = min(max(dot(input.worldNormal, -lightDirection), 0.0)*64, 1.0);
    return float4((max(light, 0.4)) * texColor.rgb, 1);
}

float4 MarsPS(VertexShaderOutput input) : COLOR
{
    // Textures
    float4 texColor = tex2D(MarsTextureSampler, input.uv);
    float3 lightDirection = normalize(input.worldPos - LightPosition);
    // calc light
    float light = min(max(dot(input.worldNormal, -lightDirection), 0.0) * 64, 1.0);
    return float4((max(light, 0.4)) * texColor.rgb, 1);
}

technique Earth
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique Moon
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MoonPS();
    }
};

technique Mars
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MarsPS();
    }

};
technique Sky
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL SkyPS();
    }
};