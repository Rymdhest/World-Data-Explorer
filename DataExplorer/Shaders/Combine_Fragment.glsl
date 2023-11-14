#version 330

in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main(void){

	vec3 color0 = texture(texture0, textureCoords).rgb;
	vec3 color1 = texture(texture1, textureCoords).rgb;
	out_Colour.rgb = color0+color1;
	out_Colour.a = 1.0f;
}