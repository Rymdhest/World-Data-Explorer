#version 330


in vec3 positionViewSpace;
in vec3 basePositionWorldSpace;
in vec3 baseNormalWorldSpace;
in vec3 fragNormalFront;
in vec3 fragNormalBack;
in vec3 grassColor;
in float tipFactor;
layout (location = 0) out vec4 gDiffuse;
layout (location = 1) out vec4 gNormal;
layout (location = 2) out vec4 gPosition;

void main() {
	
	vec3 normal = fragNormalFront;
	if (!gl_FrontFacing) {
		normal = fragNormalBack;
	}
	if (baseNormalWorldSpace.y < 0.6f) {
		discard;
	}
	if (basePositionWorldSpace.y < 3.6f) {
		discard;
	}
		if (basePositionWorldSpace.y > 96.6f) {
		discard;
	}
	float bloom = 0.1f;
	float specularity = 0.2f;
	vec3 color = mix(grassColor,grassColor*0.4f, smoothstep(1f, .0f, tipFactor));
	gDiffuse = vec4(color, 1.0);
	gNormal = vec4(normalize(normal), specularity);
	gPosition = vec4(positionViewSpace, bloom);
}