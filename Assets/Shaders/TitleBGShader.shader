Shader "KremenFont" {
	Properties{
		_MainTex("Font Texture", 2D) = "white" {}
	_Color("Text Color", Color) = (1,1,1,1)
	}

		SubShader{
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

#include "UnityCG.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform fixed4 _Color;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.vertex.y += +(((sin(v.vertex.x + _Time * 100)) + 0.5)*0.005) + cos(_Time * 70)*0.0075*v.vertex.x*0.003; //magical
		o.color = v.color * _Color;
		o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = _Color*(i.vertex.y*0.0016);
		//col.r = col.r - ((col.r * 1000) % 64) / 100;
	float peace = col.r;
	peace += ((col.r * 1000) % 64) / 1000;
	col.r = col.r +(((col.r * 1000) / 64) / 1000) + ((col.r * 1000) % 1) / 1000;
	col.r = peace;
		/*col.g = col.g - ((col.g * 1000) % 64) / 100;
		col.b = col.b - ((col.b * 1000) % 64) / 100;*/
		//col.a *= tex2D(_MainTex, i.texcoord).a;
		
		return col;
	}
		ENDCG
	}
	}
}

