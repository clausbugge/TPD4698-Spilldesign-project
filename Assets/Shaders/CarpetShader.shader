Shader "SingleColorNoCulling"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag 
			fixed4 _Color;
			struct vertexInput 
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD2;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(vertexInput v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{

				return _Color;
			}
	
			ENDCG
		}
	}
}