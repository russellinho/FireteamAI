Shader "Projector/BrushOverlay" 
{
	Subshader 
	{
		Tags { "Queue"="Transparent" }

		Pass 
		{
			Lighting Off 
			Blend SrcAlpha OneMinusSrcAlpha 
			Cull Off 
			ZWrite Off 
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "PaintUtility.cginc"
			#include "PaintParameters.cginc"

			uniform float4x4 unity_Projector;
			
			struct v2f 
			{
				float4 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uv = mul (unity_Projector, vertex);
				return o;
			}
			
			float4 frag (v2f IN) : SV_Target
			{
				// Modulate intensity to improve visibility
				float intensity = modulateVizIntensity(_brushIntensity);

				// Calculate Brush UV
				float2 brushUV = getBrushUVStatic(UNITY_PROJ_COORD(IN.uv));

				// Calculate Brush Value
				float brushValue = sampleBrush(brushUV) * intensity;

				// Outline
				float outlineValue = sampleBrushOutline(length(brushUV));

				// Mix and tint
				float4 projCol = max(brushValue, outlineValue);
				projCol.rgb = lerp(projCol.rgb, projCol.rgb + 1, 1 - projCol.a) * _VizColor;
				projCol.a *= _VizStrength;
				return projCol;
			}
			ENDCG
		}
	}
}