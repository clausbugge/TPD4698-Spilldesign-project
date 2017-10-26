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
			Tags{ "LightMode" = "ForwardBase" "RenderType" = "Opaque" }
			CGPROGRAM
			#pragma multi_compile_fwdbase 
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" 
			uniform float4 _LightColor0;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct vertexInput {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
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
			Blend One One // additive blending 

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 
			#include "UnityCG.cginc" 
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap novertexlight //dunno what these other do. dunno if they improve performance
			// shadow helper functions and macros
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"
			//#include "UnityShaderVariables.cginc"
			//uniform float4 _LightColor0; //included in Lighting.cginc
			sampler2D _MainTex;
			//float4 _LightPositionRange;
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
				float lightRange = 1 / _LightPositionRange.w;

				//SMOOTH OUTWARD ATTENUATION
				float attenuation = 1-(distanceToLight / lightRange);
				/*attenuation *= 100;
				int rest = attenuation % 20;
				attenuation -= rest;
				attenuation /= 100;*/
				
				float3 pixelAttenuation = float3(attenuation, attenuation, attenuation);
				pixelAttenuation *= 100;
				int3 pixelRest = pixelAttenuation % 15;
				pixelAttenuation.x -= pixelRest.x;
				pixelAttenuation.y -= pixelRest.y;
				pixelAttenuation.z -= pixelRest.z;
				pixelAttenuation /= 100;

				float shadow = SHADOW_ATTENUATION(i);
				//float attenuation = 1.0 / distanceToLight*shadow;
				float3 color = mul(tex2D(_MainTex, i.pos).rgb, pixelAttenuation)*_LightColor0*shadow;//*attenuation;
				return float4(color, 1.0);
			}
			ENDCG
		}


	}
}