Shader "Hidden/Shader_Fade"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_Fade ("Fade", Vector) = (0.01, 0.01, 0, 0)
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _MainTex;
			float3 _Fade;

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				return float4(tex2D(_MainTex, i.uv).rgb - _Fade, 1);
			}
			ENDCG
		}
	}
}
