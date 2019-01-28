#ifndef LIQUID_2D_DEF_INCLUDED
#define LIQUID_2D_DEF_INCLUDED

// boundTex tag def
#define BOUND_TEX_IS_BOUNDARY 0
#define BOUND_TEX_IS_KNOWN 1
#define BOUND_TEX_QUEUE_COMPLETED 2

#define FSM_SWEEPING_QUEUE0_COMPLETED 3
#define FSM_SWEEPING_QUEUE1_COMPLETED 4
#define FSM_SWEEPING_QUEUE2_COMPLETED 5
#define FSM_SWEEPING_QUEUE3_COMPLETED 6

#define SWEEPING_DIR_RIGHT_DOWN	0	// used by left up corner
#define SWEEPING_DIR_LEFT_UP	1	// used by right down corner
#define SWEEPING_DIR_RIGHT_UP	2	// used by left down corner
#define SWEEPING_DIR_LEFT_DOWN	3	// used by right up corner

#define INFINITY 1000000.0
#define EPS 0.0001

bool isInf(float val) {
	return abs(val) > 0.1 * INFINITY;
}

uint GetBoundary2DMask(uint tagBit) {
	return (1 << tagBit);
}

void SetTag(RWTexture2D<uint> target, uint2 index, uint tagBit) {
	target[index] |= (1 << tagBit);
}

void SetBoundary2D(RWTexture2D<uint> target, uint2 index, bool isBoundary, uint tagBit) {
	target[index] |= ((isBoundary ? 1 : 0) << tagBit);
}

bool GetBoundary2D(RWTexture2D<uint> target, uint2 index, uint tagBit) {
	return (target[index] & (1 << tagBit)) != 0;
}

bool GetBoundaryR2D(Texture2D<uint> target, uint2 index, uint tagBit) {
	return (target[index] & (1 << tagBit)) != 0;
}

#endif