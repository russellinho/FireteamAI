Shader "Hidden/NP_TextureChannelMixer"
{
	SubShader 
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		Pass 
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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
			
			// Ints pointing to the channel to represent: -2:black -1:white n*4+c: tex(n)ch(c)
			uniform int shuffleR = 0, shuffleG = 1, shuffleB = 2, shuffleA = 3;
			uniform sampler2D_float texture0, texture1, texture2, texture3;

			float4 shuffleChannels (float4 col[4], int4 shuffle)
			{
				float channels[18] = { 0, 1, 
					col[0].r, col[0].g, col[0].b, col[0].a, 
					col[1].r, col[1].g, col[1].b, col[1].a,
					col[2].r, col[2].g, col[2].b, col[2].a,
					col[3].r, col[3].g, col[3].b, col[3].a 
				};
				return float4 (channels[shuffle.r+2], channels[shuffle.g+2], channels[shuffle.b+2], channels[shuffle.a+2]);
			}

			float4 frag (v2f IN) : SV_Target
			{
				float4 texCol[4] = { tex2D(texture0, IN.uv), tex2D(texture1, IN.uv), tex2D(texture2, IN.uv), tex2D(texture3, IN.uv) };
				return shuffleChannels (texCol, int4(shuffleR, shuffleG, shuffleB, shuffleA));
			}

			ENDCG
		}
	}
}
