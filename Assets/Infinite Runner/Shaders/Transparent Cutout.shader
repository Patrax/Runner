Shader "Infinite Runner/Transparent Cutout" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_TintColor ("Tint Color", Color) = (0, 0, 0, 0)
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.0
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		
        Pass { // pass 0

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"
						
			uniform sampler2D _MainTex;
			uniform fixed4 _TintColor;
			uniform float _Cutoff;
			
			fixed4 frag(v2f_img i) : COLOR
			{
				fixed4 color = tex2D(_MainTex, i.uv);
				color.a = color.a * (1 - step(color.a, _Cutoff));

				return color * _TintColor;
			}
			
			ENDCG
        } // end pass
	} 
	FallBack "Diffuse"
}
