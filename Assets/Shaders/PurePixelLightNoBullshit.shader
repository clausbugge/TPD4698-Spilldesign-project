// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'


Shader "Custom/PurePixelLightNoBullshit"
{
//	UNITY_SHADER_NO_UPGRADE
	Properties
	{
		_TopPillarColor("TopPillarColor", COLOR) = (0,0,0,0)
		//_MainTex("Texture", 2D) = "white" {}
		_PixelTightness("PixelTightness",int) = 150
		_PixelColorShades("PixelColorShades",Range(2,256)) = 8
		_DiscardThreshold("DiscardThreshold",Range(0.001,0.2)) = 0.01
		_WallHeight("WallHeight", Range(-0.1,-3)) = -1.8
		_BumpMap("BumpMap",2D) = "bump"{}
		_BumpMapZoom("BumpMapZoom",int) = 100000
		_BumpMapInfluence("BumpMapInfluence",Range(-1.5,1.5)) = 0.1
		
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
			o.normal = v.normal;
			return o;
		}

		fixed4 frag(v2f i) : COLOR
		{
			if (i.normal.x > -0.5 && i.worldPos.z < _WallHeight)
			{
				return _TopPillarColor;
			}
			return fixed4(UNITY_LIGHTMODEL_AMBIENT.rgb, 1.0);
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
		//uniform sampler2D _MainTex;
		uniform float4x4 _LightMatrix0;
		//uniform sampler2D _LightTexture0;
		float4 _MainTex_ST;
		int _PixelTightness;
		int _PixelColorShades;
		float _DiscardThreshold;
		sampler2D _BumpMap;
		int _BumpMapZoom;
		float _BumpMapInfluence;
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


			SHADOW_COORDS(3)
			float3 normal : NORMAL;
		};

		v2f vert(vertexInput v)
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.posWorld = mul(UNITY_MATRIX_M, v.vertex);
			o.posLight = mul(_LightMatrix0, o.posWorld);
			TRANSFER_SHADOW(o);
			
			o.normal = v.normal;
			
			o.vertex = v.vertex;
			return o;
		}

		fixed4 frag(v2f i) : COLOR
		{
			
			fixed4 vertexWorld = mul(unity_ObjectToWorld, i.vertex);
			fixed3 lightCoord = mul(_LightMatrix0, vertexWorld).xyz;
			float3 toLight = _WorldSpaceLightPos0.xyz - vertexWorld.xyz;

			fixed lightRange = length(toLight) / length(lightCoord);
			fixed shadow = SHADOW_ATTENUATION(i);
			fixed3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
			fixed distanceToLight = length(vertexToLightSource);

			vertexToLightSource *= 100; //higher mod possibilities. _PixelTightness has be to higher for to compensate
			int modValue = _PixelTightness;
			int3 pixelRemainder = int3(
				vertexToLightSource.x % modValue, 
				vertexToLightSource.y % modValue, 
				vertexToLightSource.z % modValue);
			vertexToLightSource.x -= pixelRemainder.x - modValue;
			vertexToLightSource.y -= pixelRemainder.y - modValue;
			vertexToLightSource.z -= pixelRemainder.z - modValue;
			vertexToLightSource /= 100;

			distanceToLight = length(fixed3(vertexToLightSource.x, vertexToLightSource.y, vertexToLightSource.z));

			fixed attenuation = (1 -(distanceToLight / lightRange)) * 3;

			if ((i.normal.x > -0.5 && i.posWorld.z <= _WorldSpaceLightPos0.z) ||
				attenuation < _DiscardThreshold)
			{
				return fixed4(0,0,0,0);
			}
			fixed3 finalColor = _LightColor0*attenuation*_PixelColorShades;
			




			if (i.normal.z <= -0.9)
			{
				fixed2 test = i.posWorld * 100;
				int modValueTwo = _PixelTightness;
				test.x -= (test.x %modValueTwo) - modValueTwo;
				test.y -= (test.y %modValueTwo) - modValueTwo;

				fixed bump = (tex2D(_BumpMap, test / (100*_BumpMapZoom))).a;
				//finalColor *= bump;
				//bump *= 16; //I don't think 16 should be modified
				//int result = bump;

				finalColor -= int(bump*16) * _BumpMapInfluence;

				if (i.posWorld.x < 0) //soooooooo.. this is a hack and I have no idea why/how it works. why isn't +=1 enough?
				{
					i.posWorld.x += 100;
				}
				if (i.posWorld.y < 0)
				{
					i.posWorld.y += 100;
				}
				//finalColor += bump*0.1;
				i.posWorld.x *= 100;
				i.posWorld.y *= 100;
				fixed2 chessChecker = fixed2(abs(i.posWorld.x) % 100, abs(i.posWorld.y) % 100);
				//a:
				/*if (chessChecker.x > 50 && chessChecker.y > 50)
				{
				finalColor *= 0.5;
				}
				if (chessChecker.x < 50 && chessChecker.y < 50)
				{
				finalColor *= 0.5;
				}*/
				//b:
				if (chessChecker.x > 50 && chessChecker.y > 50)
				{
					finalColor *= 0.5;
				}
				//c:
				/*if (chessChecker.x > 50 || chessChecker.y > 50)
				{
				finalColor *= 0.5;
				}*/
				//d:
				/*if (chessChecker.x > 5 && chessChecker.y > 5)
				{
				finalColor *= 0.5;
				}*/
				//e:
				/*if (chessChecker.x > 85 || chessChecker.y > 85)
				{
				finalColor *= 0.5;
				}*/
			}

			
			
			finalColor = fixed3(round(finalColor.r), round(finalColor.g), round(finalColor.b)) / _PixelColorShades;

#if POINT
			return fixed4(finalColor *shadow, 1.0);
#endif
#if SPOT
			fixed cookieAlpha = normalize(tex2D(_LightTexture0, i.posLight.xy / i.posLight.w + fixed2(0.5, 0.5))).a; //atleast I think it's cookieAlpha
			fixed4 returnValue = fixed4(cookieAlpha*finalColor*shadow, 1.0);
			if (cookieAlpha >= 0.1)
			{
				return returnValue;
			}
#endif
			return fixed4(0, 0, 0, 0);
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