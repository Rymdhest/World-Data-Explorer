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

int softLayers = 2;



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

	float ambientOcclusion = texture(gAlbedo, textureCoords).a;
	float rougness = texture(gMaterials, textureCoords).r;
	float emission = texture(gMaterials, textureCoords).g;

	vec3 totalAmbient = vec3(ambient*ambientOcclusion*albedo*skyColor);

	vec3 viewDir = normalize(-position);

	vec3 reflectDir = reflect(-sunDirectionViewSpace, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 256-255*(rougness));
	vec3 specular = 0.5f * spec * sunColor;  

	vec3 diffuse = max(dot(sunDirectionViewSpace, normal), 0f)*albedo*sunColor;

	vec3 lighting =(diffuse + specular)*ambientOcclusion*sunFactor + totalAmbient;
	lighting = mix(lighting, albedo , clamp(emission, 0, 1));
	lighting = applyFog(lighting, -position.z, -viewDir);

	out_Colour =  vec4(lighting, 1.0f);
	//out_Colour =  vec4(albedo, 1.0f);
	//out_Colour =  vec4(vec3(emission), 1.0f);
}