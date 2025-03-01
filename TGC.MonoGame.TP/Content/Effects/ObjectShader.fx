// Shader para los objetos en las escenas
// Blinn-Phong + Normal Map + Shadow Map

#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float4x4 World;
float4x4 InverseTransposeWorld;
float4x4 LightViewProjection;

static const float modulatedEpsilon = 0.004;
static const float maxEpsilon = 0.002;

float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 lightPosition;
float3 eyePosition; // Camera position
float2 Tiling;

float3 hitPosition;
float hitRadius;

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
    MIPFILTER = LINEAR;
};

texture WrapTexture;
sampler2D wrapSampler = sampler_state
{
    Texture = (WrapTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
    MIPFILTER = LINEAR;
};


texture NormalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture shadowMap;
sampler2D shadowMapSampler = sampler_state
{
    Texture = <shadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct DepthPassVertexShaderInput
{
    float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ScreenSpacePosition : TEXCOORD1;
};

struct ShadowedVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct ShadowedVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldSpacePosition : TEXCOORD1;
    float4 LightSpacePosition : TEXCOORD2;
    float4 Normal : TEXCOORD3;
};

DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
    DepthPassVertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.ScreenSpacePosition = mul(input.Position, WorldViewProjection);
    return output;
}

float4 DepthPS(in DepthPassVertexShaderOutput input) : COLOR
{
    float depth = input.ScreenSpacePosition.z / input.ScreenSpacePosition.w;
    return float4(depth, depth, depth, 1.0);
}

float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

ShadowedVertexShaderOutput MainVS(in ShadowedVertexShaderInput input)
{
    ShadowedVertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates * Tiling;
    output.WorldSpacePosition = mul(input.Position, World);
    output.LightSpacePosition = mul(output.WorldSpacePosition, LightViewProjection);
    output.Normal = mul(float4(input.Normal, 1), InverseTransposeWorld);
    return output;
}

ShadowedVertexShaderOutput TankVS(in ShadowedVertexShaderInput input)
{
    ShadowedVertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates * Tiling;
    output.WorldSpacePosition = mul(input.Position, World);
    
    float distanceToHitPosition = distance(output.WorldSpacePosition.xyz, hitPosition);
    if (distanceToHitPosition < hitRadius)
    {
        float3 direction = normalize(distanceToHitPosition - output.WorldSpacePosition.xyz);
        float displacement = hitRadius - distanceToHitPosition;
        output.WorldSpacePosition.xyz += (direction * displacement);
    }
    
    output.LightSpacePosition = mul(output.WorldSpacePosition, LightViewProjection);
    output.Normal = mul(float4(input.Normal, 1), InverseTransposeWorld);
    return output;
}

float4 DrawNormalMapPS(ShadowedVertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldSpacePosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    float3 normal = getNormalFromMap(input.TextureCoordinates, input.WorldSpacePosition.xyz, normalize(input.Normal.xyz));

	// Get the texture texel
    float4 texelColor = tex2D(wrapSampler, input.TextureCoordinates);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(normal, halfVector);
    float3 specularLight = KSpecular * specularColor * pow(saturate(NdotH), shininess);
    
    // ambient + diffuse + specular
    float4 baseColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    
    
    // Shadows
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
    
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(normal, lightDirection)), maxEpsilon);
    float shadowMapDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates).r + inclinationBias;
	
	// Compare the shadowmap with the REAL depth of this fragment
	// in light space
    float notInShadow = step(lightSpacePosition.z, shadowMapDepth);
        
    float4 finalColor = baseColor * (0.5 + 0.5 * notInShadow);
    return finalColor;

}

float4 DrawObjectPS(ShadowedVertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldSpacePosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);

	// Get the texture texel
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
    
    // Final calculation
    float4 baseColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
     
    // Shadows
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
    
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(input.Normal.xyz, lightDirection)), maxEpsilon);
    float shadowMapDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates).r + inclinationBias;
	
	// Compare the shadowmap with the REAL depth of this fragment
	// in light space
    float notInShadow = step(lightSpacePosition.z, shadowMapDepth);
        
    float4 finalColor = baseColor * (0.5 + 0.5 * notInShadow);
    return finalColor;

}

technique DepthPass
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DepthVS();
        PixelShader = compile PS_SHADERMODEL DepthPS();
    }
};

technique DrawNormalMap
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DrawNormalMapPS();
    }
};

technique DrawObject
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DrawObjectPS();
    }
};

technique DrawTank
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL TankVS();
        PixelShader = compile PS_SHADERMODEL DrawObjectPS();
    }
};