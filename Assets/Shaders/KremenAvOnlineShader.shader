// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 //Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kremen"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" "RenderType" = "Opaque" } // pass for 
													// 4 vertex lights, ambient light & first pixel light
			//Blend One One
			CGPROGRAM
			#pragma multi_compile_fwdbase 
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" 
			uniform float4 _LightColor0;
			// color of light source (from "Lighting.cginc")

			// User-specified properties

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct vertexInput {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
			};

			v2f vert(vertexInput v)
			{
				v2f o;

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

				o.worldPos = mul(modelMatrix, v.vertex);
				//o.normalDir = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

				float4 frag(v2f i) : COLOR
				{
					float3 color = tex2D(_MainTex,i.uv).rgb*UNITY_LIGHTMODEL_AMBIENT.rgb;
					return float4(color,1.0);
				}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"


		Pass{

			Tags{ "LightMode" = "ForwardAdd"}
			// pass for additional light sources
			Blend One One // additive blending 

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 
			#include "UnityCG.cginc" 
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdadd_fullshadows //nolightmap nodirlightmap nodynlightmap novertexlight
			// shadow helper functions and macros
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

			//uniform float4 _LightColor0; //included in Lighting.cginc
			sampler2D _MainTex;
			
			struct vertexInput {
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
			};

			v2f vert(vertexInput v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(UNITY_MATRIX_M, v.vertex);

				TRANSFER_SHADOW(o);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
				float distanceToLight = length(vertexToLightSource);
				if (distanceToLight > 5)
				{
					return (0, 0, 0, 0);
				}
				float shadow = SHADOW_ATTENUATION(i);
				float3 color = tex2D(_MainTex, i.pos).rgb*_LightColor0*shadow;
				return float4(color, 1.0);
			}
			ENDCG
		}


	}
}