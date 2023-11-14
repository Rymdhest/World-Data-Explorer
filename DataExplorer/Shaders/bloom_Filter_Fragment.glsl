#version 330

in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform sampler2D shadedInput;
uniform sampler2D gMaterials;
float bloomDampener = 0.5;
void main(void){

	vec3 diffuse = texture(shadedInput, textureCoords).rgb;
	float bloom = texture(gMaterials, textureCoords).y;

	float luminance = dot(diffuse, vec3(0.2126, 0.7152, 0.0722));
	out_Colour.rgb = diffuse+diffuse*bloom;
	out_Colour.a = 1.0f;
}