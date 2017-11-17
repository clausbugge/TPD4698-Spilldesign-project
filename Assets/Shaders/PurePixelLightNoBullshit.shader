﻿Shader "Custom/PurePixelLightNoBullshit"
{
	Properties
	{
		_TopPillarColor("TopPillarColor", COLOR) = (0,0,0,0)
		_MainTex("Texture", 2D) = "white" {}
		_PixelTightness("PixelTightness",int) = 150
		_PixelColorShades("PixelColorShades",Range(2,256)) = 8
		_DiscardThreshold("DiscardThreshold",Range(0.001,0.2)) = 0.01
		_WallHeight("WallHeight", Range(-0.1,-3)) = -1.8
		
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
		float4 _TopPillarColor;
		float _WallHeight;
		struct vertexInput {
			float4 vertex : POSITION;
			float4 uv : TEXCOORD0;
			
			float3 normal : NORMAL;
		};
		struct v2f {
			float4 pos : SV_POSITION; //this shit does stuff, even when not used explicitly in fragment shader
			float3 worldPos : TEXCOORD1;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
		};

		v2f vert(vertexInput v)
		{
			v2f o;
			float4x4 modelMatrix = unity_ObjectToWorld;
			float4x4 modelMatrixInverse = unity_WorldToObject;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			return o;
		}

		float4 frag(v2f i) : COLOR
		{
			float3 color = tex2D(_MainTex,i.uv).rgb*UNITY_LIGHTMODEL_AMBIENT.rgb;
			return (i.normal.x > -0.5 && i.worldPos.z < _WallHeight) ?
				_TopPillarColor : 
				float4(color, 1.0);
		}
			ENDCG
		}
			//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"


			Pass{

			Tags{ "LightMode" = "ForwardAdd" }
			Blend One One // additive blending 

			CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 
#include "UnityCG.cginc" 
#include "UnityCG.cginc"
#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
#pragma multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap //dunno what these other do. dunno if they improve performance
			// shadow helper functions and macros
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
			//#include "UnityShaderVariables.cginc" //included in autolight.cginc I think
			//uniform float4 _LightColor0; //included in Lighting.cginc
		sampler2D _MainTex;
		float4 _MainTex_ST;
		int _PixelTightness;
		int _PixelColorShades;
		float _DiscardThreshold;

		struct vertexInput {
			float4 vertex : POSITION;
			float4 uv : TEXCOORD2;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float4 worldPos : TEXCOORD0;
			SHADOW_COORDS(1)
			float2 uv : TEXCOORD2;
			float3 normal : NORMAL;
		};

		v2f vert(vertexInput v)
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			TRANSFER_SHADOW(o);
			o.normal = v.normal;
			return o;
		}

		fixed4 frag(v2f i) : COLOR
		{

			float shadow = SHADOW_ATTENUATION(i);
			float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
			float distanceToLight = length(vertexToLightSource);
			float lightRange = 1 / _LightPositionRange.w;

			vertexToLightSource *= 1000; //higher mod possibilities. _PixelTightness has be to higher for to compensate
			int modValue = _PixelTightness;
			int3 pixelRemainder = int3(vertexToLightSource.x % modValue, vertexToLightSource.y % modValue, vertexToLightSource.z % modValue);
			vertexToLightSource.x -= pixelRemainder.x - modValue;
			vertexToLightSource.y -= pixelRemainder.y - modValue;
			vertexToLightSource.z -= pixelRemainder.z - modValue;
			vertexToLightSource /= 1000;

			distanceToLight = length(float3(vertexToLightSource.x, vertexToLightSource.y, vertexToLightSource.z));
			float attenuation = (1 - (distanceToLight / lightRange))*3;
			if (i.normal.x > -0.5 && i.worldPos.z <= _WorldSpaceLightPos0.z)
			{
				discard;
			}
			if (attenuation < _DiscardThreshold)
			{
				discard;
			}
			float3 finalColor = _LightColor0*attenuation;
			finalColor = _LightColor0*attenuation*_PixelColorShades;
			finalColor = float3(round(finalColor.r), round(finalColor.g), round(finalColor.b)) / _PixelColorShades;
			return float4(tex2D(_MainTex, i.pos).rgb*finalColor *shadow, 1.0); //***use pos instead of uv to highlight light colors more***

		}
			ENDCG
		}
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On ZTest Less Cull Off
			Offset 1, 1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
				ENDCG
			}
		}
}