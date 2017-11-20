//Shader "CarpetAndDoors"
//{
//	Properties
//	{
//		_Color("Color", Color) = (1,1,1,1)
//	}
//		SubShader
//	{
//		Pass
//		{
//			Blend One One
//			Cull Off
//			CGPROGRAM
//			#pragma vertex vert  
//			#pragma fragment frag 
//			fixed4 _Color;
//			struct vertexInput 
//			{
//				float4 vertex : POSITION;
//				float4 uv : TEXCOORD2;
//			};
//
//			struct v2f
//			{
//				float4 pos : SV_POSITION;
//			};
//
//			v2f vert(vertexInput v)
//			{
//				v2f o;
//				o.pos = UnityObjectToClipPos(v.vertex);
//
//				return o;
//			}
//
//			fixed4 frag(v2f i) : COLOR
//			{
//
//				return _Color;
//			}
//	
//			ENDCG
//		}
//	}
//}

Shader "CarpetShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Cull Off
		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	
	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	float4 _Color;
	float4 _MainTex_ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		return _Color;
	}
		ENDCG
	}
	}
}
