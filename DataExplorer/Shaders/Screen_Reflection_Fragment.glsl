#version 330

layout (location = 0) out vec4 out_Colour;

in vec2 textureCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gMaterials;
uniform sampler2D shadedColor;

uniform vec3 skyColor;
uniform mat4 projectionMatrix;

uniform float rayStep =8.5f;
uniform int iterationCount = 20;
uniform int binaryIterationCount = 30;
uniform float distanceBias = 0.001f;
uniform bool enableSSR = true;
uniform bool isBinarySearchEnabled = true;
uniform bool debugDraw = false;
uniform float stepExponent = 1.25f;

float random (vec2 uv) {
	return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123)-0.5f; //simple random function
}

vec2 generateProjectedPosition(vec3 pos){
	vec4 samplePosition =  vec4(pos, 1.f) * projectionMatrix;
	samplePosition.xy = (samplePosition.xy / samplePosition.w) * 0.5 + 0.5;
	return samplePosition.xy;
}

vec3 SSR(vec3 position, vec3 reflection) {
	vec3 step = rayStep * reflection;
	vec3 marchingPosition = position + step;
	float delta;
	float depthFromScreen;
	vec2 screenPosition;
	int i = 0;

	for (; i < iterationCount; i++) {
		screenPosition = generateProjectedPosition(marchingPosition);
		depthFromScreen = -(texture(gPosition, screenPosition).z);
		delta = abs(marchingPosition.z) - depthFromScreen;
		if (abs(delta) < distanceBias) {
			vec3 color = vec3(1);
			if(debugDraw)
				color = vec3( 0.5+ sign(delta)/2,0.3,0.5- sign(delta)/2);
			return texture(shadedColor, screenPosition).xyz * color;
		}
		if (delta > 0) {
			break;
		}
		marchingPosition += step;
		step *= stepExponent;
    }
	if(isBinarySearchEnabled){
		for(; i < binaryIterationCount; i++){
			
			step *= 0.5;
			marchingPosition = marchingPosition - step * sign(delta);
			
			screenPosition = generateProjectedPosition(marchingPosition);
			depthFromScreen = -(texture(gPosition, screenPosition).z);
			delta = -(marchingPosition.z) - depthFromScreen;
			
			if (abs(delta) < distanceBias) {
                vec3 color = vec3(1);
                if(debugDraw)
                    color = vec3( 0.5+ sign(delta)/2,0.3,0.5- sign(delta)/2);
				return texture(shadedColor, screenPosition).xyz * color;
			}
		}
	}
	
    return vec3(0.0);
}

void main(){
	vec3 position = texture(gPosition, textureCoords).xyz;
	vec3 normal =  texture(gNormal, textureCoords).xyz;
	float rougness = 0.2f * texture(gMaterials, textureCoords).r;
	normal.x += random(textureCoords+1)*rougness;
	normal.y += random(textureCoords+2)*rougness;
	normal.z += random(textureCoords+3)*rougness;
	float metallic = 1.0f;
	if (!enableSSR || metallic < 0.0001) {
		out_Colour = texture(shadedColor, textureCoords);
	} else {
		vec3 reflectionDirection = normalize(reflect(position, normalize(normal)));
		out_Colour = vec4(SSR(position, normalize(reflectionDirection)), 1.f);
		if (out_Colour.xyz == vec3(0.f)){
			out_Colour = texture(shadedColor, textureCoords);
			out_Colour = vec4(skyColor, 1);
		}
	}
}