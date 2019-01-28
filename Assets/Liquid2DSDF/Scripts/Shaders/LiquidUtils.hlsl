#ifndef LIQUID_UTILS_INCLUDED
#define LIQUID_UTILS_INCLUDED

float LengthVec2(float2 vec) {
	return sqrt(vec.x * vec.x + vec.y * vec.y);
}

float DistanceVec2(float2 a, float2 b) {
	return LengthVec2(a - b);
}


#endif