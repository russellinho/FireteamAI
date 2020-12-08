Shader "Hidden/BrushGUIViz"
{
	SubShader 
	{
		Tags { "ForceSupported" = "True" }

		Pass 
		{
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "PaintUtility.cginc"
			#include "PaintParameters.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 clipUV : TEXCOORD1;
			};

			uniform float4x4 unity_GUIClipTextureMatrix;
			sampler2D _GUIClipTexture;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				float4 eyePos = float4(UnityObjectToViewPos(v.vertex), 1);
				o.clipUV = mul(unity_GUIClipTextureMatrix, eyePos);

				o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				// Modulate intensity to improve visibility
				float intensity = modulateVizIntensity(_brushIntensity);

				// Calculate Brush UV
				float2 brushUV = getBrushUVStatic(i.texcoord.xy);

				// Calculate Brush Value
				float brushValue = sampleBrush(brushUV) * intensity;

				// Outline
				float outlineValue = sampleBrushOutline(length(brushUV));

				// Mix and tint
				float4 col = max (brushValue, outlineValue) * _VizColor;
				col.a *= tex2D(_GUIClipTexture, i.clipUV).a * _VizStrength;
				return col;
			}
			ENDCG
		}
	}
}
