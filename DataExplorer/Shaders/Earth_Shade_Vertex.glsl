#version 330

layout(location=0) in vec3 position;
layout(location=1) in vec2 uvs;
layout(location=2) in vec3 materials;
layout(location=3) in vec3 normal;
layout(location=4) in vec3 tangent;

out vec2 fragUV;
out vec3 positionViewSpace_pass;
out vec3 fragMaterials;
out vec3 fragNormal;
out mat3 TBN;
uniform mat4 modelViewMatrix;
uniform mat4 modelViewProjectionMatrix;
uniform mat4 normalModelViewMatrix;
uniform sampler2D topographyTexture;

void main() {
	gl_Position =  vec4(position, 1.0)*modelViewProjectionMatrix;
	positionViewSpace_pass =  (vec4(position, 1.0)*modelViewMatrix).xyz;
	fragUV = uvs;
	fragMaterials = materials;
	fragNormal = (vec4(normal, 1.0f)*normalModelViewMatrix).xyz;

	vec3 N = normalize((vec4(-normal, 0.0f)*modelViewMatrix).xyz);
	vec3 T = normalize((vec4(tangent, 0.0f)*modelViewMatrix).xyz);
	vec3 B = normalize(cross(N, T));
	TBN = mat3(T, B, N);
}