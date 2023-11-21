#version 330
layout (location = 0) out vec4 out_Colour;

in vec2 textureCoords;

uniform int numBorderPoints;
uniform samplerBuffer borderPoints;
uniform int countryID;

float sdSegment( in vec2 p, in vec2 a, in vec2 b )
{
    vec2 pa = p-a, ba = b-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}
float getDist(vec2 p) {
	float dist = 99999.0;
	for (int i = 0 ; i<numBorderPoints-1 ; i++) {
		vec2 a =texelFetch(borderPoints, i).rg;
		vec2 b =texelFetch(borderPoints, i+1).rg;

		dist = min(sdSegment(p, a, b), dist);
	}
	return dist;
}
void main(void){
	vec2 p = (vec2(textureCoords.x, textureCoords.y));

	float dist = getDist(p);
	
    float dampener = 10000.0;

    float glow = 1.0/(dist*dampener)-0.1; 
	glow = clamp(glow, 0.0, 1.0);
	if (glow > 0.3) out_Colour.g = glow; // UGLY HACK FOR NOW
	else discard;
	//out_Colour.g = max(out_Colour.g, glow);


}