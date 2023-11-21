#version 330

in vec2 fragUV;
in vec3 fragNormal;
in vec3 fragMaterials;
in vec3 positionViewSpace_pass;
in mat3 TBN;
layout (location = 0) out vec4 gAlbedo;
layout (location = 1) out vec4 gNormal;
layout (location = 2) out vec4 gPosition;
layout (location = 3) out vec4 gMaterials;

uniform int countryHighlights[255];
uniform sampler2D albedoTexture;
uniform sampler2D topographyTexture;
uniform sampler2D countryDataTexture;
uniform mat4 normalModelViewMatrix;
uniform mat4 viewMatrix;
uniform vec2 heightmapSize;

vec3 calcNormal() {
	float uvPixelX = 1/ heightmapSize.x;
	float uvPixelY = 1/ heightmapSize.y;

	float center = texture(topographyTexture, fragUV).r;
	float left = texture(topographyTexture, fragUV-vec2(-uvPixelX, 0)).r;
	float right = texture(topographyTexture, fragUV+vec2(-uvPixelX, 0)).r;
	float up = texture(topographyTexture, fragUV+vec2(0, uvPixelY)).r;
	float down = texture(topographyTexture, fragUV+vec2(0, -uvPixelY)).r;

    vec3 normal =normalize( vec3(2.0*(right-left), 2.0*(down-up), -1.0));
    return normal;
}

void drawLatitudeLines() {
	float sections = 18f;
	float latitudeLine = mod(fragUV.y+1f/sections/2f, 1f/sections);
	vec3 lineColor = vec3(0.0, 0.0, 0.0);
	float lineSize = 0.02f;
	float distance = abs(latitudeLine*sections-0.5f);
	gAlbedo.rgb = mix(gAlbedo.rgb,lineColor, smoothstep(0, 1, 1-distance/lineSize));

}

void drawLongitudeLines() {
	float sections = 36f;
	float longitudeLine = mod(fragUV.x+1f/sections/2f, 1f/sections);
	vec3 lineColor = vec3(0.0, 0.0, 0.0);
	float lineSize = 0.01f;
	float distance = abs(longitudeLine*sections-0.5f);
	gAlbedo.rgb = mix(gAlbedo.rgb,lineColor, smoothstep(0, 1, 1-distance/lineSize));

}

void drawBorders() {
	float strength = texture(countryDataTexture, fragUV).g;
	vec3 borderColor = vec3(1.0, 0.2, 0.1)*strength;
	gAlbedo.rgb = mix(gAlbedo.rgb,borderColor, strength);

}
void drawCountryHighligts() {
	int id =int( texture(countryDataTexture, fragUV).r*255.0);
	if (countryHighlights[id] == 1) {

	vec3 hightlightColour = vec3(1.0, 0.0, 0.0);
	gAlbedo.rgb = mix(gAlbedo.rgb, hightlightColour, 0.5);
	}
}

void main() {
	vec3 albedo =texture(albedoTexture, fragUV).rgb;
	//albedo = vec3(1.0f);
	gAlbedo = vec4(albedo, 1.0);

	//drawLatitudeLines();
	//drawLongitudeLines();
	drawBorders();
	drawCountryHighligts();

	vec3 normal_tangent = calcNormal();
	//normal_tangent.x *= -1f;
	//normal_tangent.y *= -1f;
	gNormal.xyz = normalize(normal_tangent*transpose(TBN));
	//gNormal.x *= -1; // WTF HACK??????
	//gNormal.y *= -1; // WTF HACK??????
	//gNormal.xyz = fragNormal;
	gPosition = vec4(positionViewSpace_pass, 0.0f);
	gMaterials = vec4(fragMaterials, 0.0f);
}