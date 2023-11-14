#version 330

in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform sampler2D HDRcolorTexture;
uniform float radius;


vec3 aces(vec3 x) {
  const float a = 2.51;
  const float b = 0.03;
  const float c = 2.43;
  const float d = 0.59;
  const float e = 0.14;
  return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}
vec3 reinhard(vec3 inputColor) {
	float key = 1.418f;
	return inputColor / (inputColor + vec3(1.0)) * (inputColor * key);
}
vec3 applyExposure(vec3 inputColor) {
	float exposure =0.55f;
	return vec3(1.0) - exp(-inputColor * exposure);
}

vec3 applyToneMap(vec3 inputColor) {
	return aces(inputColor);
	//return reinhard(inputColor);
}

vec3 applyGamma(vec3 inputColor) {
	float gamma = 2.2;
	return pow(inputColor, vec3(1.0 / gamma));
}

vec3 applyBrightness(vec3 inputColor) {
	float brightness = 0.9f;
	return inputColor*vec3(brightness);
}
vec3 applyContrast(vec3 inputColor) {
	float contrast = 1f;
    return mix(0.5 + (inputColor - 0.5) * contrast, inputColor, contrast);
}
vec3 applySaturation(vec3 inputColor) {
	float saturation = 1.3f;
    float luminance = dot(inputColor, vec3(0.2126, 0.7152, 0.0722));
    vec3 desaturatedColor = vec3(luminance);
    return mix(desaturatedColor, inputColor, saturation);
}
void main(void){

	vec3 color =  texture(HDRcolorTexture, textureCoords).rgb;

	
	color = applyExposure(color);
	//white balance
	color = applyContrast(color);
	color = applyBrightness(color);
	color = applySaturation(color);
	color = applyToneMap(color);
	color = applyGamma(color);
	
	out_Colour =  vec4(color, 1.0f);
}