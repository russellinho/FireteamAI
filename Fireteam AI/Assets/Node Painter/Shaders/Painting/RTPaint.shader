Shader "Hidden/NodePainter_RTPaint" 
{
	CGINCLUDE
	#pragma target 3.0
	#pragma vertex vert

	#include "UnityCG.cginc"
	#include "PaintUtility.cginc"
	#include "PaintParameters.cginc"

	#pragma multi_compile __ CALC_BRANCHES

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert (appdata_base v)
	{
		v2f OUT;
		OUT.pos = UnityObjectToClipPos (v.vertex);
		OUT.uv = v.texcoord.xy;
		return OUT;
	}

	ENDCG

	SubShader 
	{
		Pass 
		{ // Paint
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment Paint

			float4 Paint (v2f IN) : COLOR0
			{
				float4 canvasCol = tex2D (_Canvas, IN.uv);

				// Calculate Brush Value
				float brushValue = sampleBrush (getBrushUV(IN.uv)) * _brushIntensity * _timeStep;

				// Calculate medium in case it's needed
				float4 medium = tex2Dlod (_Canvas, float4 (IN.uv.x, IN.uv.y, 0, _paintSmoothBias));

				// Apply brush function
				float4 modCol = blendPaint(canvasCol, brushValue, medium);

				// Mask and clamp
				return clamp(mask(canvasCol, modCol));
			}

			ENDCG
		}

		Pass 
		{ // Single Blend Pass
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment Blend

			float4 Blend (v2f IN) : COLOR0
			{
				float4 canvasCol = tex2D (_Canvas, IN.uv);
				float4 blendCol = tex2D (_blendTex, IN.uv);

				// Blend
				float4 modCol = blendColor(canvasCol, blendCol, 0);

				// Mask and clamp
				return clamp(mask(canvasCol, modCol));
			}

			ENDCG
		}

		Pass 
		{ // Modification Pass
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment Modify
			#pragma multi_compile __ MOD_CHANNEL

			float4 Modify (v2f IN) : COLOR0
			{
				float4 canvasCol = tex2D (_Canvas, IN.uv);

				// Modify
				float4 modCol = modifyColor (canvasCol);

				// Mask and clamp
				return clamp(mask(canvasCol, modCol));
			}

			ENDCG
		}

		Pass 
		{ // Single Blend and Modification Pass
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment BlendModify

			#pragma multi_compile __ MOD_CHANNEL

			float4 BlendModify (v2f IN) : COLOR0
			{
				float4 canvasCol = tex2D (_Canvas, IN.uv);
				float4 blendCol = tex2D (_blendTex, IN.uv);
				
				// Blend
				float4 modCol = blendColor (canvasCol, blendCol, 0);
				
				// Modify
				modCol = modifyColor (modCol);

				// Mask and clamp
				return clamp(mask(canvasCol, modCol));
			}

			ENDCG
		}

		Pass 
		{ // Multi Color Blend Pass for Visualization
			// Responsible for blending all channels of a Multi-Format canvas together
			// In practice, adds channel colors together multiplied by their respective channel values
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment MultiColorBlend

			#pragma multi_compile __ ENABLE_TEXTURE_ARRAYS

			float4 addChannelColors (float4 canvasCol, float4 texValue, int texIndex) 
			{
				// Decide on channel source
				int texExists = testGreaterEquals (_multiTexCount, texIndex+1);
				float4 texChannels = lerp (float4(0,0,0,0), texValue, texExists);

				// Ignore unused channels
				int lastTex = testEquals (_multiTexCount, texIndex+1);
				texChannels = lerp (texChannels, 0, _multiLastTexMaskInv*lastTex);

				// Replace colors
				canvasCol = lerp (canvasCol, _multiChannelColors[texIndex*4+0], texChannels.r);
				canvasCol = lerp (canvasCol, _multiChannelColors[texIndex*4+1], texChannels.g);
				canvasCol = lerp (canvasCol, _multiChannelColors[texIndex*4+2], texChannels.b);
				canvasCol = lerp (canvasCol, _multiChannelColors[texIndex*4+3], texChannels.a);
				return canvasCol;
			}

			float4 MultiColorBlend (v2f IN) : COLOR0
			{
				float4 canvasCol = float4(0, 0, 0, 0);

				// Add channel colors based on source canvas textures
			#if ENABLE_TEXTURE_ARRAYS && (SHADER_API_D3D11 || SHADER_API_D3D12 || SHADER_API_VULKAN || SHADER_API_GLCORE || SHADER_API_METAL)
				for (int i = 0; i < _multiTexCount; i++)
					canvasCol = addChannelColors (canvasCol, UNITY_SAMPLE_TEX2DARRAY (_multiTextures, float3(IN.uv, i)), i);
			#else
				canvasCol = addChannelColors (canvasCol, tex2D (_canvasTex1, IN.uv), 0);
				canvasCol = addChannelColors (canvasCol, tex2D (_canvasTex2, IN.uv), 1);
				canvasCol = addChannelColors (canvasCol, tex2D (_canvasTex3, IN.uv), 2);
				canvasCol = addChannelColors (canvasCol, tex2D (_canvasTex4, IN.uv), 3);
			#endif
				return canvasCol;
			}

			ENDCG
		}

		Pass 
		{ // Multi-Channel Blend Pass with Normalization
			// Responsible for applying changes to a single channel while keeping all other channels normalized
			// Applied on every canvas texture with reference to the current channel
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment MultiBlendNormalized


			float4 MultiBlendNormalized(v2f IN) : COLOR0
			{
				float4 canvasCol = tex2D (_Canvas, IN.uv);

				// Get and apply blend on the current channel (CONSTANT for each texture)
				float blend = tex2D (_blendTex, IN.uv)[_multiChannelIndex];
				float value = tex2D (_multiChannelTex, IN.uv)[_multiChannelIndex];
				float blendedCol = blendColor (value, blend, 0);

				// Substract the proportional difference from all the other channels
				float diff = blendedCol - value;
				float4 normCol = canvasCol - (diff * canvasCol);

				// Clamp and Mask based on current channel
				return clamp(mask(normCol, blendedCol));
			}

			ENDCG
		}

		Pass 
		{ // Crop/Expand to Rect Pass
			// Responsible for cropping or expanding a canvas to a specified new size
			ZTest Always Cull Off ZWrite Off Fog { Mode off }

			CGPROGRAM
			#pragma fragment CropRect

			uniform float4 sourceRect;
			uniform float4 targetRect;

			float4 CropRect(v2f IN) : COLOR0
			{
				float2 uv = IN.uv;

				uv *= sourceRect.zw;
				uv += sourceRect.xy;

				clamp(uv, 0, 1);

				uv -= targetRect.xy;
				uv /= targetRect.zw;

				return tex2D (_Canvas, uv);
			}

			ENDCG
		}
	}
	
	Fallback off
}