#version 330
layout (location = 0) out vec4 out_Colour;

in vec2 textureCoords;

uniform int numBorderPoints;
uniform vec2 borderPoints[13000];
uniform int countryID;

bool pointIsOnLeftSideOfLine(vec2 p, vec2 a, vec2 b) {
	return (b.x - a.x) * (p.y-a.y) - (p.x-a.x) * (b.y-a.y) > 0;
}

bool pointInPolygon(vec2 p) {
	int windingNumber = 0;
	for (int i = 0 ; i<numBorderPoints-1 ; i++) {
	
		vec2 a = borderPoints[i];
		vec2 b = borderPoints[i+1];
		
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
	vec2 p = (vec2(textureCoords.x, textureCoords.y)-0.5)*2.0;

	if (pointInPolygon(p)) {
		out_Colour.r = 1.0;
	}
	out_Colour.r = 1.0;

}