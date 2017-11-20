// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'


Shader "Custom/PurePixelLightNoBullshit"
{
//	UNITY_SHADER_NO_UPGRADE
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
			//Blend One One
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
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"


			Pass{

			Tags{ "LightMode" = "ForwardAdd" }
			Blend One One // additive blending 

			CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 


			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
#pragma multi_compile_lightpass// multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap //dunno what these other do. dunno if they improve performance
			// shadow helper functions and macros
#include "UnityCG.cginc" 
#include "AutoLight.cginc"
#include "Lighting.cginc"
//#include "UnityShaderVariables.cginc"
//#include "UnityPBSLighting.cginc"

			//#include "UnityShaderVariables.cginc" //included in autolight.cginc I think
			//uniform float4 _LightColor0; //included in Lighting.cginc
		uniform sampler2D _MainTex;
		uniform float4x4 _LightMatrix0;
		//uniform sampler2D _LightTexture0;
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
			float4 vertex : TEXCOORD6;
			float4 pos : SV_POSITION;
			float4 posWorld : TEXCOORD0;
			// position of the vertex (and fragment) in world space 
			float4 posLight : TEXCOORD1;
			// position of the vertex (and fragment) in light space
			float3 normalDir : TEXCOORD2;
			// surface normal vector in world space

			//SHADOW_COORDS(3)
			LIGHTING_COORDS(4, 5)
			//float2 uv : TEXCOORD2;
			float3 normal : NORMAL;
		};

		v2f vert(vertexInput v)
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.posWorld = mul(UNITY_MATRIX_M, v.vertex);
			o.posLight = mul(_LightMatrix0, o.posWorld);
			//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			//TRANSFER_SHADOW(o);
			
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			o.normal = v.normal;
			o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
			
			o.vertex = v.vertex;
			return o;
		}

		fixed4 frag(v2f i) : COLOR
		{
			
			float4 vertexWorld = mul(unity_ObjectToWorld, i.vertex);

			float3 lightCoord = mul(_LightMatrix0, vertexWorld).xyz;
			float3 toLight = _WorldSpaceLightPos0.xyz - vertexWorld.xyz;

			float lightRange = length(toLight) / length(lightCoord);


			float shadow = SHADOW_ATTENUATION(i);

			float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;

			float distanceToLight = length(vertexToLightSource);

			//float lightRange = 1 / _LightPositionRange.w;

			vertexToLightSource *= 1000; //higher mod possibilities. _PixelTightness has be to higher for to compensate
			int modValue = _PixelTightness;
			int3 pixelRemainder = int3(vertexToLightSource.x % modValue, vertexToLightSource.y % modValue, vertexToLightSource.z % modValue);
			vertexToLightSource.x -= pixelRemainder.x - modValue;
			vertexToLightSource.y -= pixelRemainder.y - modValue;
			vertexToLightSource.z -= pixelRemainder.z - modValue;
			vertexToLightSource /= 1000;

			distanceToLight = length(float3(vertexToLightSource.x, vertexToLightSource.y, vertexToLightSource.z));

			float attenuation = (1 -(distanceToLight / lightRange)) * 3;
			//float attenuation = (1 -(distanceToLight / lightRange)) * 4;
			//attenuation = lightRange;

			//attenuation = LIGHT_ATTENUATION(i)*9;
			if (i.normal.x > -0.5 && i.posWorld.z <= _WorldSpaceLightPos0.z)
			{
				discard;
			}
			if (attenuation < _DiscardThreshold)
			{
				discard;
			}
			float3 finalColor = _LightColor0;
			finalColor = _LightColor0*attenuation*_PixelColorShades;
			finalColor = float3(round(finalColor.r), round(finalColor.g), round(finalColor.b)) / _PixelColorShades;
#if POINT
			return float4(finalColor *shadow, 1.0); //***use pos instead of uv to highlight light colors more***
#endif
			float4 returnValue = float4(normalize(tex2D(_LightTexture0, i.posLight.xy / i.posLight.w + float2(0.5, 0.5))).a*finalColor*shadow, 1.0);
			if (normalize(tex2D(_LightTexture0, i.posLight.xy / i.posLight.w + float2(0.5, 0.5))).a >= 0.2)
			{
				return returnValue;
				
			}
			
			discard;
			return returnValue;
			//float distance = length(vertexToLightSource);
			//attenuation = 1.0 / distance; // linear attenuation 
			//float3 finalColor = attenuation* _LightColor0.rgb;
			//finalColor = _LightColor0*attenuation*_PixelColorShades;
			//finalColor = float3(round(finalColor.r), round(finalColor.g), round(finalColor.b)) / _PixelColorShades;
			//float cookieAttenuation = tex2D(_LightTexture0, i.posLight.xy / i.posLight.w + float2(0.5, 0.5)).a;
			//cookieAttenuation = normalize(cookieAttenuation);
			//cookieAttenuation = 1;
			////cookieAttenuation = -1;
			//if (cookieAttenuation == -1)
			//{
			//	//discard;
			//}
			
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