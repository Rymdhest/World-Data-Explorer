#version 330

in vec2 textureCoords;
layout (location = 0) out vec4 out_Colour;

uniform vec3 skyColorGround;
uniform vec3 skyColorSpace;
uniform vec3 sunColorGlare;
uniform vec3 sunColor;
uniform vec3 viewPositionWorld;
uniform vec3 sunDirectionWorldSpace;
uniform vec2 screenResolution;
uniform mat4 viewMatrix;
uniform float sunSetFactor;
uniform mat4 projectionMatrix;
void main(void){


	vec2 uv = ((textureCoords*2f)-1f);
	uv = (uv*screenResolution)/screenResolution.y;

	float viewheightFactor = clamp(1f-viewPositionWorld.y*0.001f, 0f, 1f);

	vec3 skyColor = mix(skyColorSpace,skyColorGround , viewheightFactor*sunSetFactor);

	vec3 viewDir = normalize((viewMatrix*vec4(uv, -1f, 1.0f)).xyz);

	vec3 upNormalViewSpace = normalize((vec4(0, 1f, 0.0f, 1f)).xyz);


	float sunAmount = pow(max( dot(viewDir, sunDirectionWorldSpace ), 0.0 ), 256);
	float sunAmountBigScale = pow(max( dot( viewDir, sunDirectionWorldSpace ), 0.0 ), 8)*0.45f;

	float horizon = max( 0.45f+dot( viewDir.y, -upNormalViewSpace.y ), 0.0 );
	
	skyColor *= 1f-pow(max( dot( viewDir, upNormalViewSpace), 0.0 ), 1)*0.4f; //darkness above
	skyColor += horizon*skyColor;
	skyColor = mix( skyColor, sunColor, sunAmount );
	skyColor += sunAmountBigScale*sunColorGlare;
	//skyColor = mix( skyColor, sunColorGlare*1.0f, sunAmountBigScale );

	out_Colour.rgb = skyColor;
	out_Colour.a = 1.0f;
}