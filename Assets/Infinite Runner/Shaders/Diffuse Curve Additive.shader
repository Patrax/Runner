Shader "Infinite Runner/Diffuse Curve Additive" {
	Properties {
		_FadeOutColor ("Fade Out Color", Color) = (0, 0, 0, 0)
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_NearCurve ("Near Curve", Vector) = (0, 0, 0, 0)
		_FarCurve ("Far Curve", Vector) = (0, 0, 0, 0)
		_Dist ("Distance Mod", Float) = 0.0
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		AlphaTest Greater .01
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
				
        Pass { // pass 0
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _FadeOutColor;
			uniform float4 _TintColor;
			uniform float4 _NearCurve;
			uniform float4 _FarCurve;
			uniform float _Dist;
									
			struct fragmentInput
			{
				float4 pos : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float distanceSquared : TEXCOORD1;
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
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);

				return o;
			}
			
			fixed4 frag(fragmentInput i) : COLOR
			{
				return lerp(2.0f * i.color * _TintColor * tex2D(_MainTex, i.uv), _FadeOutColor, max(i.distanceSquared  / _FarCurve.z, 0.0));
			}
			
			ENDCG
        } // end pass
	}
}
