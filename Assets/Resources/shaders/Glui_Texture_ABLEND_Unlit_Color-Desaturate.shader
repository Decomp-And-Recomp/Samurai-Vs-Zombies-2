Shader "Glui/Texture/ABLEND/Unlit_Color-Desaturate" 
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" }
        Pass
        {
            Tags { "QUEUE"="Transparent" }
            Cull Off
            Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _MainTex_ST;
            sampler2D _MainTex;
            float4 _Color;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 texcol_1;
                float4 tmpvar_2;
                tmpvar_2 = tex2D (_MainTex, i.texcoord0);
                texcol_1 = tmpvar_2;
                texcol_1.xyz = dot (texcol_1.xyz, float3(0.300000, 0.590000, 0.110000));
                float3 tmpvar_3;
                tmpvar_3 = (_Color.xyz * texcol_1.xyz);
                texcol_1.xyz = tmpvar_3;
                float tmpvar_4;
                tmpvar_4 = (texcol_1.w * _Color.w);
                texcol_1.w = tmpvar_4;
                return texcol_1;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}