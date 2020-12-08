#ifndef PAINT_PARAMS
#define PAINT_PARAMS

// Canvas Options
uniform uint _sizeX, _sizeY;
uniform sampler2D_float _Canvas;

// General Options
uniform float _timeStep;
uniform float4 _channelMask;
uniform int _clamp;
//uniform int _clampStroke;

// Brush parameters
uniform float2 _brushPos;
uniform float _brushSize;
uniform float _brushIntensity;
uniform int _brushType; // 0-Image 1-Gaussian ...
uniform sampler2D _brushTex;
uniform float _brushFalloff;
uniform float _brushHardness;
uniform float4x4 _brushMatrix;
uniform float _aspect;

// Painting paramters
uniform int _paintMode;
uniform float4 _paintColor;
uniform float _paintSmoothBias; // 0-SmoothenDetails 4-FlattenArea
uniform float _paintTarget;
//uniform int _paintClampMode; // 0-None 1-0to1 2-0toIntensity

// Blend Parameters
uniform sampler2D_float _blendTex;
uniform int _blendMode;
uniform float _blendAmount;

// Multi-Channel Blend Parameters
uniform int _multiTexCount, _multiTexIndex;
uniform int _multiChannelIndex;
uniform sampler2D_float _multiChannelTex;
uniform float4 _multiLastTexMaskInv;

// Multi-Channel Color Viz Parameters
#if ENABLE_TEXTURE_ARRAYS && (SHADER_API_D3D11 || SHADER_API_D3D12 || SHADER_API_VULKAN || SHADER_API_GLCORE || SHADER_API_METAL)
UNITY_DECLARE_TEX2DARRAY(_multiTextures);
uniform float4 _multiChannelColors[128];
#else
uniform sampler2D_float _canvasTex1, _canvasTex2, _canvasTex3, _canvasTex4;
uniform float4 _multiChannelColors[16];
#endif


// Modification Parameters
uniform float _modBrightness = 0, _modContrast = 1;
uniform float4 _modTintCol = float4(1, 1, 1, 1);

// Channel Modification Parameters
// Ints pointing to the channel to represent: 0-black - 1-r - 2-g - 3-b - 4-a - 5-white
uniform int _modR = 1, _modG = 2, _modB = 3, _modA = 4;
uniform float4 _modChOffset = float4(0, 0, 0, 0);
uniform float4 _modChScale = float4(1, 1, 1, 1);


// Visualization
uniform half4 _VizColor;
uniform float _VizStrength = 1;
uniform float _VizStrengthCurvature = 1;
uniform float _VizOutlineRadius;
uniform float _VizOutlineThickness;

// Projection
uniform sampler2D _projCanvasTex;
uniform half4 _projTintCol;
uniform float _projStrength;

#endif