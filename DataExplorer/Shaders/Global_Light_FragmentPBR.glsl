#version 330


in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedo;
uniform sampler2D gMaterials;

uniform int numberOfCascades;


uniform vec3 sunDirectionViewSpace;
uniform vec3 sunColor;
uniform vec3 skyColor;
uniform vec3 sunScatterColor;
uniform vec3 fogColor;
uniform float ambient;
uniform float fogDensity;

uniform mat4 sunSpaceMatrices[8];
uniform sampler2DShadow shadowMaps[8];
uniform vec2 shadowMapResolutions[8];
uniform float cascadeProjectionSizes[8];

uniform vec2 resolution;

int softLayers = 1;

const float PI = 3.14159265359;
float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);


float calcShadow(vec3 positionViewSpace) {
	
	int cascadeToUse = -1;
	for (int i = 0 ; i< numberOfCascades ; i++) {
		if (length(positionViewSpace)*2f < cascadeProjectionSizes[i]) {
			cascadeToUse = i;
			break;
		}
	}
	if (cascadeToUse == -1) return 0f;
	//vec2 shadowPixelSize = 1f/shadowMapResolution;
	vec2 pixelSize = 1f/resolution;
	vec4 positionSunSpace = (vec4(positionViewSpace, 1.0))*sunSpaceMatrices[cascadeToUse];
	positionSunSpace = positionSunSpace * vec4(0.5) + vec4(0.5);

	float shadowFactor = 0f;
	float totalWeight =0;
	for (int x = -softLayers ; x <= softLayers ; x++) {
		for (int y = -softLayers ; y <= softLayers ; y++) {
			shadowFactor += 1f-texture(shadowMaps[cascadeToUse], positionSunSpace.xyz+vec3(x*pixelSize.x, y*pixelSize.y, 0));
			totalWeight += 1f;
		}
	}
	
	shadowFactor /= totalWeight;
	//out_Colour =  vec4(vec3(clamp((-shadowDepth+positionSunSpace.z)*8f, 0f, 1f)), 1.0f);
	return shadowFactor;
}

vec3 applyFog( in vec3  baseColor, in float depth, in vec3  viewDirection)
{
    float fogAmount = 1.0 - exp( -depth*fogDensity );
    float sunAmount = max( dot( viewDirection, sunDirectionViewSpace ), 0.0 );
    vec3  sunScatteredFogColor  = mix(fogColor, sunScatterColor, pow(sunAmount,2.0));
    return mix( baseColor, sunScatteredFogColor, fogAmount );
}

void main(void){
	vec3 position = texture(gPosition, textureCoords).xyz;
	vec3 normal = texture(gNormal, textureCoords).xyz;
	vec3 albedo = texture(gAlbedo, textureCoords).rgb;
	
	float sunFactor = 1f-calcShadow(position);
	//sunFactor = 1.0f;

	float ambientOcclusion = texture(gAlbedo, textureCoords).a;
	float roughness = clamp(texture(gMaterials, textureCoords).r, 0.05f, 1f);
	float emission = texture(gMaterials, textureCoords).g;
	float metallic = texture(gMaterials, textureCoords).b;

	vec3 totalAmbient = vec3(ambient*ambientOcclusion*albedo*skyColor);

	vec3 viewDir = normalize(-position);

	vec3 F0 = vec3(0.04); 
	vec3 Lo = vec3(0.0);
    F0 = mix(F0, albedo, metallic);
	vec3 N = normalize(normal);
    vec3 V = viewDir;

    // calculate per-light radiance
    vec3 L = normalize(sunDirectionViewSpace);
    vec3 H = normalize(V + L);
    vec3 radiance     = sunColor;        
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, roughness);        
    float G   = GeometrySmith(N, V, L, roughness);    
	
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  
    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;  
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(N, L), 0.0);                
    Lo += (kD * albedo / PI + specular) * radiance * NdotL; 

	//vec3 ambient = vec3(0.03) * albedo * ambientOcclusion;
    vec3 color = totalAmbient + Lo*sunFactor;

	color = mix(color, albedo , clamp(emission, 0, 1));

	color = applyFog(color, -position.z, -viewDir);
	//color = color / (color + vec3(1.0));
    //color = pow(color, vec3(1.0/2.2));  
	//lighting = applyFog(lighting, -position.z, -viewDir);
	out_Colour = vec4(color, 1.0);
	//out_Colour = vec4(normalize(normal), 1.0);
	//out_Colour =  vec4(lighting, 1.0f);
	//out_Colour =  vec4(positionSunSpace.xyz, 1.0f);
	//out_Colour =  vec4(vec3(emission), 1.0f);

	
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}  