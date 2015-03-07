Shader "Infinite Runner/Diffuse Curve Specular" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_AdditiveColor ("Additive Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SpecColor("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Float) = 10
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
			#include "UnityCG.cginc"
						
			uniform float4 _Color;
			uniform float4 _AdditiveColor;
			uniform float4 _SpecColor;
			uniform float _Shininess;
			uniform float4 _FadeOutColor;
			uniform float4 _NearCurve;
			uniform float4 _FarCurve;
			uniform float _Dist;

			uniform float4 _LightColor0;
			
			struct fragmentInput
			{
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
				float distanceSquared : TEXCOORD2;
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

				o.posWorld = mul(UNITY_MATRIX_P, pos);
				o.normalDir = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);

				return o;
			}
			
			fixed4 frag(fragmentInput i) : COLOR
			{
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float atten = 2.0;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuseReflection = atten * _LightColor0.xyz * max(0.0, dot(i.normalDir, lightDirection));
				float3 specularReflection = atten * _LightColor0.xyz * _SpecColor.rgb * max(0.0, dot(i.normalDir, lightDirection)) * pow(max(0.0, dot(reflect(-lightDirection, i.normalDir), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT + _AdditiveColor;

				return lerp(float4(lightFinal * _Color.rgb, 1.0), _FadeOutColor, max(i.distanceSquared  / _FarCurve.z, 0.0));
			}
			
			ENDCG
        } // end pass
	} 
	FallBack "Diffuse"
}
