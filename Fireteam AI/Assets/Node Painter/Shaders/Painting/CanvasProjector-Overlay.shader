Shader "Projector/CanvasOverlay" 
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
			
			half4 frag (v2f IN) : SV_Target
			{
				clip (IN.uv.x);
				clip (1-IN.uv.x);
				clip (IN.uv.y);
				clip (1-IN.uv.y);
				//clip (-abs(IN.uv.x-0.5));
				//clip (-abs(IN.uv.y-0.5));
				
				float4 projCol = tex2D(_projCanvasTex, IN.uv);
				projCol.rgb = lerp (projCol.rgb, projCol.rgb + 1, 1-projCol.a);
				projCol *= _projTintCol;
				projCol.a *= _projStrength;
				return projCol;
			}
			ENDCG
		}
	}
}