#ifndef PAINT_UTIL
#define PAINT_UTIL

#include "PaintParameters.cginc"

#pragma target 3.0

// BRUSH FUNCTIONS

float gaussian(float sqr)
{
	return max(0, exp(-_brushHardness*sqr));
}

float exponential(float sqr)
{
	return max(0, pow(1 - sqr, 7));
}

float simple_brushFalloff(float sqr)
{
	return max(0, 1 - sqrt(sqr));
}

float calcFalloff(float2 dist)
{
	return max(0, dot(dist, dist) - _brushFalloff);
}

// TESTS

int testEquals(int value, int target)
{
	return max(0, -abs(value - target) + 1);
}
int testGreaterEquals(int value, int target)
{
	return max(0, min(1, value - target + 1));
}
int testLesserEquals(int value, int target)
{
	return max(0, min(1, target - value + 1));
}
float toggle(int value, int toggle, float base, float mod)
{
	return lerp(base, mod, testEquals(value, toggle));
}

// UTILITIES

float foldThickness01(float value, float center, float thickness, float factor)
{
	return clamp(-abs(value - center) / thickness * factor + factor, 0, 1);
}

float sampleBrushOutline(float distance)
{
	return foldThickness01(distance, _VizOutlineRadius, min (_VizOutlineThickness / _brushSize, 0.05), 2);
}

float modulateVizIntensity(float intensity) 
{
	return lerp(intensity, -(intensity - 1)*(intensity - 1) + 1, _VizStrengthCurvature);
}

float2 getBrushUV(float2 uv)
{
	uv = _brushPos.xy - uv;
	uv.y = uv.y * _aspect;
	uv = uv / _brushSize;
	uv = mul(uv, _brushMatrix);
	return uv;
}

float2 getBrushUVStatic(float2 uv)
{
	uv = float2(0.5, 0.5) - uv;
	uv = mul(uv, _brushMatrix);
	uv.y = uv.y * _aspect;
	return uv;
}

// MODIFICATIONS

float4 mask(float4 base, float4 mod)
{
	return lerp(base, mod, _channelMask);
}

float4 clamp(float4 col)
{
	return lerp(col, clamp(col, 0, 0.99999f), _clamp);
}

float4 shuffleChannels(float4 col, int4 shuffle)
{
	float channels[6] = { col.r, col.g, col.b, col.a, 0, 1 };
	return float4(channels[shuffle.r], channels[shuffle.g], channels[shuffle.b], channels[shuffle.a]);
}

float4 shuffleChannels(float4 col, int r, int g, int b, int a)
{
	float channels[6] = { col.r, col.g, col.b, col.a, 0, 1 };
	return float4(channels[r], channels[g], channels[b], channels[a]);
}

float4 modifyColor(float4 col, float contrast, float brightness, float4 tint)
{
	col = col * contrast + brightness;
	return col * tint;
}

float4 modifyColor(float4 col)
{
#if MOD_CHANNEL
	col = shuffleChannels(col, _modR, _modG, _modB, _modA) * _modChScale + _modChOffset;
#endif
	return modifyColor(col, _modContrast, _modBrightness, _modTintCol);
}



#if CALC_BRANCHES // Calculate all branches instead of branching. Performance-critical

float4 blendPaint(float4 base, float brush, float4 med)
{
	float4 blendedCol = float4(0, 0, 0, 0);
	blendedCol += testEquals(_paintMode, -1) * lerp(base, base + _paintColor, brush); // Add Static
	blendedCol += testEquals(_paintMode, 0) * lerp(base, base + _paintColor, 0.05f * brush); // Add
	blendedCol += testEquals(_paintMode, 4) * lerp(base, _paintColor * _paintTarget, 0.1f * _paintColor * brush); // Lerp
	blendedCol += testEquals(_paintMode, 6) * lerp(base, _paintColor * _paintTarget, brush); // Replace
	blendedCol += testEquals(_paintMode, 7) * lerp(base, med, brush); // Smoothen
	blendedCol += testEquals(_paintMode, 8) * lerp(base, (base - med)*1.1f + med, brush); // Contrast
	return blendedCol;
}


float4 blendColor(float4 base, float4 col, float4 med)
{
	float4 blendedCol = float4(0, 0, 0, 0);
	blendedCol += testEquals(_blendMode,-1) * lerp(base, base + col, _blendAmount); // Add Static
	blendedCol += testEquals(_blendMode, 0) * lerp(base, base + col, 0.05f * _blendAmount); // Add
	blendedCol += testEquals(_blendMode, 1) * lerp(base, base - col, 0.05f * _blendAmount); // Substract
	blendedCol += testEquals(_blendMode, 2) * lerp(base, base * col, _blendAmount); // Multiply
	blendedCol += testEquals(_blendMode, 3) * lerp(base, base / max (col, 0.0001f), _blendAmount); // Divide
	blendedCol += testEquals(_blendMode, 4) * lerp(base, col, 0.1f * col * _blendAmount); // Lerp
	blendedCol += testEquals(_blendMode, 5) * lerp(base, ((col - 0.5f) + (base - 0.5f)) / 2 + 0.5f, 0.1f * _blendAmount); // Overlay
	blendedCol += testEquals(_blendMode, 6) * lerp(base, col, min(1, max(0, _blendAmount))); // Replace
	blendedCol += testEquals(_blendMode, 7) * lerp(base, med, _blendAmount); // Smoothen
	blendedCol += testEquals(_blendMode, 8) * lerp(base, (base - med)*1.1f + med, _blendAmount); // Contrast
	return blendedCol;
}

//float4 clampColor (float4 colA, float4 colMax, uniform int clampMode)
//{
//	float4 clampedCol = float4(0,0,0,0);
//	clampedCol += testEquals (clampMode, 0) * colA; // None
//	clampedCol += testEquals (clampMode, 1) * clamp (colA, 0, 0.999999); // Max
//	clampedCol += testEquals (clampMode, 2) * clamp (colA, 0, colMax); // Stroke
//	return clampedCol;
//}

float sampleBrush(float2 uv)
{
	float brushValue = 0;
	brushValue += testEquals(_brushType, 0) * tex2D(_brushTex, float2(0.5, 0.5) + uv).a;
	brushValue += testEquals(_brushType, 1) * max(0, gaussian(calcFalloff(uv * 4)) - gaussian(1.9f*1.9f-_brushFalloff));
	return brushValue;
}

#else

float4 blendPaint(float4 base, float brush, float4 med)
{
	if (_paintMode == -1) // Add Static
		return lerp(base, base + _paintColor, brush);
	if (_paintMode == 0) // Add
		return lerp(base, base + _paintColor, 0.05f * brush);
	if (_paintMode == 4) // Lerp
		return lerp(base, _paintColor * _paintTarget, 0.1f * _paintColor * brush);
	if (_paintMode == 6) // Replace
		return lerp(base, _paintColor * _paintTarget, min(1, max(0, brush)));
	if (_paintMode == 7) // Smoothen
		return lerp(base, med, brush);
	if (_paintMode == 8) // Contrast
		return lerp(base, (base - med)*1.1f + med, brush);
	return float4(1, 0, 1, 0);
}

float4 blendColor(float4 base, float4 col, float4 med)
{
	if (_blendMode == -1) // Add Static
		return lerp(base, base + col, _blendAmount);
	if (_blendMode == 0) // Add
		return lerp(base, base + col, 0.05f * _blendAmount);
	if (_blendMode == 1) // Substract
		return lerp(base, base - col, 0.05f * _blendAmount);
	if (_blendMode == 2) // Multiply
		return lerp(base, base * col, _blendAmount);
	if (_blendMode == 3) // Divide
		return lerp(base, base / col, _blendAmount);
	if (_blendMode == 4) // Lerp
		return lerp(base, col, 0.1f * col * _blendAmount);
	if (_blendMode == 5) // Overlay
		return lerp(base, ((col - 0.5f) + (base - 0.5f)) / 2 + 0.5f, 0.1f * _blendAmount);
	if (_blendMode == 6) // Replace
		return lerp(base, col, _blendAmount);
	if (_blendMode == 7) // Smoothen
		return lerp(base, med, _blendAmount);
	if (_blendMode == 8) // Contrast
		return lerp(base, (base - med)*1.1f + med, _blendAmount);
	return float4(1, 0, 1, 0);
}

//float4 clampColor (float4 colA, float4 colMax, uniform int clampMode)
//{
//	if (clampMode == 0) // None
//		return colA;
//	if (clampMode == 1) // Max
//		return clamp (colA, 0, 0.999999);
//	if (clampMode == 2) // Stroke
//		return clamp (colA, 0, colMax);
//	return float4(1, 0, 1, 0);
//}

float sampleBrush(float2 uv)
{
	//if (type == 0) // Texture Brush
	//	return tex2D(tex, float2(0.5, 0.5) + dist).a * intensity;
	if (_brushType == 1) // Gaussian Function
		return max(0, gaussian(calcFalloff(uv * 4)) - gaussian(1.9f*1.9f-_brushFalloff));
	//if (_brushType == 2) // Round Function
	//	return exponential(calcFalloff(uv * 4));
	// Texture Brush
	return tex2D(_brushTex, float2(0.5, 0.5) + uv).a;
}

#endif

#endif