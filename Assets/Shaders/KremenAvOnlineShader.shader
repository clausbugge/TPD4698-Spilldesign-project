﻿Shader "Kremen"
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
				float4 pos : SV_POSITION; //this shit does stuff, even when not used explicitly in fragment shader
				float2 uv : TEXCOORD0;
				//float4 worldPos : TEXCOORD1;
			};

			v2f vert(vertexInput v)
			{
				v2f o;
				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;
				//o.worldPos = mul(modelMatrix, v.vertex);
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
			//#include "UnityShaderVariables.cginc" //included in autolight.cginc I think
			//uniform float4 _LightColor0; //included in Lighting.cginc
			sampler2D _MainTex;
			float4 _MainTex_ST;
			struct vertexInput {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD2;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
				float2 uv : TEXCOORD2;
			};

			v2f vert(vertexInput v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				TRANSFER_SHADOW(o);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
				float distanceToLight = length(vertexToLightSource);
				float lightRange = 1 / _LightPositionRange.w;
				float attenuation = 1 - (distanceToLight / lightRange);
				
				//SMOOTH OUTWARD ATTENUATION
				/*attenuation *= 100;
				int rest = attenuation % 20;
				attenuation -= rest;
				attenuation /= 100;*/
				
				//PIXEL LIGHT WITH/WITHOUT SMOOTH ATTENUATION
				float3 pixelAttenuation = float3(1.0-length(vertexToLightSource.x/lightRange), 1.0 - length(vertexToLightSource.y / lightRange), 1.0 - length(vertexToLightSource.z / lightRange));
				pixelAttenuation*= 1000; //multiply by 1000 to get better mod possibilities
				int modValue = 400/(lightRange); //this gives approx 5-6 pixels per light range
				int3 pixelRemainder = int3(pixelAttenuation.x % modValue, pixelAttenuation.y % modValue, pixelAttenuation.z % modValue);
				pixelAttenuation.x -= pixelRemainder.x - (modValue);
				pixelAttenuation.y -= pixelRemainder.y - (modValue);
				pixelAttenuation.z -= pixelRemainder.z - (modValue);
				pixelAttenuation /= 1000;
				//WITH ATTENUATION
				attenuation *= pixelAttenuation.x*pixelAttenuation.y*pixelAttenuation.z*3;
				//WITHOUT ATTENUATION
				//attenuation = pixelAttenuation.x*pixelAttenuation.y*pixelAttenuation.z;
				//need discard if no attenuation, not 100% sure why. maybe floating point error
				//NO DISCARD with attenuation
				if (attenuation < 0.1)
				{
					//discard;
				}

				float shadow = SHADOW_ATTENUATION(i);
				float3 color = tex2D(_MainTex, i.uv).rgb*_LightColor0*shadow*attenuation; //***use pos instead of uv to highlight light colors more***
				return float4(color, 1.0);
			}
			ENDCG
		}


	}
}