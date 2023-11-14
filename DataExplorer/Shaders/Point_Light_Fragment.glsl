#version 330

layout (location = 0) out vec4 out_Colour;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedo;
uniform sampler2D gMaterials;

uniform vec3 lightPositionViewSpace;
uniform vec3 lightColor;
uniform vec3 attenuation;
uniform float gScreenSizeX;
uniform float gScreenSizeY;


void main(void){
	vec2 textureCoords = gl_FragCoord.xy / vec2(gScreenSizeX, gScreenSizeY);
	vec3 position = texture(gPosition, textureCoords).xyz;
	vec3 normal = texture(gNormal, textureCoords).xyz;
	vec3 albedo = texture(gAlbedo, textureCoords).rgb;
	float specularStrength = texture(gNormal, textureCoords).a;

	vec3 viewDir = normalize(-position);
	vec3 lightDir = normalize(lightPositionViewSpace - position);

	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0f), 32);
	vec3 specular = specularStrength * spec * lightColor;  
	float lighting = max(dot(lightDir, normal), 0.0f);
	
	float distance = length(lightPositionViewSpace - position.xyz);
	float attenuationFactor = 1.0 / (attenuation.x + attenuation.y * distance+ attenuation.z * distance * distance);
	lighting *= attenuationFactor;
	specular *= attenuationFactor;

	vec3 color = albedo*lighting*lightColor+specular;

	out_Colour =  vec4(color, 1.0f);
}