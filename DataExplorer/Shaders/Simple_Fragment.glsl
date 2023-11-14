#version 330

in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform sampler2D blitTexture;
uniform float radius;

void main(void){
	out_Colour =  texture(blitTexture, textureCoords);

	//out_Colour.rgb =  vec3(texture(blitTexture, textureCoords).a);
}