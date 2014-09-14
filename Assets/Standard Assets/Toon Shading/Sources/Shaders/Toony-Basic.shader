Shader "Toon/Basic" {
	Properties {
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ToonShade ("ToonShader Cubemap(RGB)", CUBE) = "" { Texgen CubeNormal }
	}


	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Name "BASE"
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			samplerCUBE _ToonShade;
			float4 _MainTex_ST;
			float4 _Color;

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 wnormal: TEXCOORD1;
				float4 wpos : TEXCOORD2;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				//o.wnormal = mul(UNITY_MATRIX_MV, float4(v.normal,0));
				o.wpos = mul(_Object2World, v.vertex);
				o.wpos.w = o.pos.z/20.0;
				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				//float4 col = _Color * tex2D(_MainTex, i.texcoord);
				//float4 cube = texCUBE(_ToonShade, i.wnormal);
				float xyz = cos(i.wpos.x*8) * cos(i.wpos.y*8) * cos(i.wpos.z*8);
				clip(xyz);
				float timepass = (sin( i.wpos.y - _Time.w ) + 1) * 0.5;
				float impact = 0.9 -i.wpos.w + pow(timepass,50);
				return float4( impact, impact, impact, 1 );
			}
			ENDCG
		}
	} 

	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Name "BASE"
			Cull Off
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine texture * constant
			} 
			SetTexture [_ToonShade] {
				combine texture * previous DOUBLE, previous
			}
		}
	} 
	
	Fallback "VertexLit"
}
