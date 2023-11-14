#version 330

layout(location=0) in vec3 position;

out vec3 positionViewSpace;
out vec3 fragNormalFront;
out vec3 fragNormalBack;
out float tipFactor;
out vec3 baseNormalWorldSpace;
out vec3 basePositionWorldSpace;
out vec3 grassColor;
uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 modelViewProjectionMatrix;
uniform mat4 normalModelViewMatrix;
uniform float patchSizeWorld;
uniform float bladesPerRow;
uniform float bladeHeight;
uniform sampler2D normalHeightMap;
uniform vec3 normal;
uniform float time;
#define PI 3.1415926538

vec3 color = vec3(0.36f,  0.51f,  0.23f);


float hash11(float t) {
    float x = fract(sin(t*1.35)*144563.5);
    return x;
}
vec2 hash12(float t) {
    float x = fract(sin(t*346.65)*4361.5);
    float y = fract(sin(t*5435.1*x)*645.3);
    return vec2(x,y);
}
vec2 hash22(vec2 t) {
    float x = fract(sin(t.x*876.65*t.y)*234.5);
    float y = fract(sin(t.y*5435.1*x)*645.3);
    
    return vec2(x,y);
}
float hash21(vec2 t) {
    float x = fract(sin(t.y*5435.1*t.x)*5656.0);
    
    return x;
}
vec3 hash13(float t) {
    float x = hash11(t);
    float y = fract(sin(113.1*x)*65652.0);
    float z = fract(sin(1755.1*y)*512336.0);
    
    return vec3(x, y, z);
}
mat3 rotXMatrix(float a) {
	return mat3(
	1, 0f, 0,
	0f, cos(a), -sin(a),
	0f,sin(a),cos(a));
}
mat3 rotYMatrix(float a) {
	return mat3(
	cos(a), 0f, sin(a),
	0f, 1f, 0f,
	-sin(a),0f,cos(a));
}
mat3 rotZMatrix(float a) {
	return mat3(
	cos(a), -sin(a), 0,
	sin(a), cos(a), 0,
	0f,0,1f);
}
void main() {
	vec3 pos = position;
	pos.y *=  1f+(hash11(gl_InstanceID+3)*2f-1)*0.4f;
	tipFactor = position.y/bladeHeight;
	float bendX = (hash11(gl_InstanceID+542)-0.5f)+sin(time+gl_InstanceID)*0.1f;
	float bendZ = (hash11(gl_InstanceID+123)-0.5f)*0.5f;
	grassColor = (0.75+hash13(gl_InstanceID+123)*0.5f)*color;
	mat3 localRotMatrix = rotZMatrix(tipFactor*bendZ)*rotXMatrix(tipFactor*bendX)*rotYMatrix(hash11(gl_InstanceID)*PI);
	//localRotMatrix = rotYMatrix(hash11(gl_InstanceID)*PI);
	pos = pos*localRotMatrix;
	float spacing = (patchSizeWorld)/(bladesPerRow);

	vec2 localOffset = hash12(gl_InstanceID)*spacing;
	vec3 localPosition = vec3(localOffset.x, 0, localOffset.y);

	vec3 offset = vec3((floor(gl_InstanceID/(bladesPerRow))), 0, mod(float(gl_InstanceID),bladesPerRow))*spacing;
	localPosition += offset;


	vec4 textureData = texture(normalHeightMap, (vec2(offset.x*(0.9925)+1, offset.z*(0.9925)+1))/patchSizeWorld);
	localPosition.y = textureData.a;

	pos += localPosition;
	baseNormalWorldSpace = textureData.xyz;


	vec3 norm = (normal)*localRotMatrix;


	gl_Position =  vec4(pos, 1.0)*modelViewProjectionMatrix;
	basePositionWorldSpace =  (vec4(localPosition, 1.0)*modelMatrix).xyz;
	positionViewSpace =  (vec4(pos, 1.0)*modelViewMatrix).xyz;

	fragNormalFront = (vec4(norm, 1.0f)*normalModelViewMatrix).xyz;
	fragNormalBack = (vec4(normalize(vec3(-norm.x, norm.y, -norm.z)), 1.0f)*normalModelViewMatrix).xyz;
}