#version 330
layout (location = 0) out vec4 out_Colour;

in vec2 textureCoords;

uniform int numBorderPoints;
uniform samplerBuffer borderPoints;
uniform int countryID;

bool pointIsOnLeftSideOfLine(vec2 p, vec2 a, vec2 b) {
	return (b.x - a.x) * (p.y-a.y) - (p.x-a.x) * (b.y-a.y) > 0;
}

bool pointInPolygon(vec2 p) {
	int windingNumber = 0;
	for (int i = 0 ; i<numBorderPoints-1 ; i++) {
	
		//vec2 a =vec2( texelFetch(borderPoints, i*2).r, texelFetch(borderPoints, i*2+1).r);
		//vec2 b =vec2( texelFetch(borderPoints, (i+1)*2).r, texelFetch(borderPoints, (i+1)*2+1).r);
		
		vec2 a =texelFetch(borderPoints, i).rg;
		vec2 b =texelFetch(borderPoints, i+1).rg;
		
		if (a.y <= p.y) {
			if (b.y > p.y && pointIsOnLeftSideOfLine(p, a, b)) {
				windingNumber++;
			}
		}
		
		else if (b.y <= p.y && !pointIsOnLeftSideOfLine(p, a, b)) {
			windingNumber--;
		}
		
	}
	return windingNumber != 0;
}

void main(void){
	//out_Colour.r = 1f;
	vec2 p = (vec2(textureCoords.x, textureCoords.y));

	if (pointInPolygon(p)) {
		out_Colour.r = countryID/255.0;
	} else {
		discard;
	}

}