Shader "Infinite Runner/Diffuse Curve" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_FadeOutColor ("Fade Out Color", Color) = (0, 0, 0, 0)
		_NearCurve ("Near Curve", Vector) = (0, 0, 0, 0)
		_FarCurve ("Far Curve", Vector) = (0, 0, 0, 0)
		_Dist ("Distance Mod", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
        Pass { // pass 0

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma multi_compile_fwdbase LIGHTMAP_OFF LIGHTMAP_ON
			#include "UnityCG.cginc"
			#ifndef SHADOWS_OFF		
			#include "AutoLight.cginc"	
			#endif
						
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_ST;
			uniform half4 	_MainTex_TexelSize;
			uniform float4 _FadeOutColor;
			uniform float4 _NearCurve;
			uniform float4 _FarCurve;
			uniform float _Dist;

			uniform float4 _LightColor0;
			
			struct fragmentInput
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uvLM : TEXCOORD1;
				float distanceSquared : TEXCOORD2;
				#ifndef SHADOWS_OFF
		        LIGHTING_COORDS(3,4)
				#endif
			};
						
			fragmentInput vert(appdata_full v)
			{
				fragmentInput o;

				// Apply the curve
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
                o.distanceSquared = pos.z * pos.z * _Dist;
                pos.x += (_NearCurve.x - max(1.0 - o.distanceSquared / _FarCurve.x, 0.0) * _NearCurve.x);
                pos.y += (_NearCurve.y - max(1.0 - o.distanceSquared / _FarCurve.y, 0.0) * _NearCurve.y);
                o.pos = mul(UNITY_MATRIX_P, pos); 
				o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1.0-o.uv.y;
				#endif
				
				#ifndef SHADOWS_OFF			  	
      			TRANSFER_VERTEX_TO_FRAGMENT(o);
				#endif

				return o;
			}
			
			fixed4 frag(fragmentInput i) : COLOR
			{
				fixed4 color = tex2D(_MainTex, i.uv);

				#ifdef LIGHTMAP_ON
				fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D (unity_Lightmap, i.uvLM));
				color.rgb *= lm;
				#endif
				
				#ifndef SHADOWS_OFF			  	
				fixed atten = LIGHT_ATTENUATION(i);
				color.rgb *= atten;
				#endif

				return lerp(color, _FadeOutColor, max(i.distanceSquared  / _FarCurve.z, 0.0));
			}
			
			ENDCG
        } // end pass
	} 
	FallBack "Diffuse"
}
