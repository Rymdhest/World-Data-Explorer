#version 330

in vec4 positionViewSpace_pass;
in vec4 clipSpacePosition;
//layout (location = 0) out vec4 gDiffuse;
//layout (location = 1) out vec4 gNormal;
//layout (location = 2) out vec4 gPosition;
uniform vec2 screenResolution;
uniform float scale;
uniform vec3 color;
//layout (location = 0) out vec4 out_Color;
out vec4 out_Color;

uniform mat4 projectionMatrix;

void main() {
	/*
	gDiffuse = vec4(fragColor, 1.0);
	gNormal = vec4(normalize(fragNormal), fragMaterials.r);
	gPosition = vec4(positionViewSpace_pass, fragMaterials.g);
	*/
	float near = 0.1f;
	float far = 1000f;

	float surfaceDistance = -clipSpacePosition.w;
	surfaceDistance = 2.0 * near * far / (far + near - (2.0 * surfaceDistance - 1.0) * (far - near));

	
	vec2 lightCenterUV = (positionViewSpace_pass/positionViewSpace_pass.w).xy;
	lightCenterUV = (lightCenterUV.xy*screenResolution)/screenResolution.y;
	//lightCenterUV = positionViewSpace_pass.xy;

	vec2 uv = (clipSpacePosition/clipSpacePosition.w).xy;
	uv = (uv.xy*screenResolution)/screenResolution.y;

	float radius = surfaceDistance*5.0f*scale;

	float distance = length(uv-lightCenterUV);
	distance = smoothstep(radius, 0.0f, distance);

	vec3 col = vec3(distance)*color*2f;
	//out_Color = vec4(lightCenterUV-uv, 0f, 1.0f);
	out_Color = vec4(col, 1.0f);
}