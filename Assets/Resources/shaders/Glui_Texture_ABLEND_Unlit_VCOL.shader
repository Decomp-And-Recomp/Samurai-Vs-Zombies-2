Shader "Glui/Texture/ABLEND/Unlit/VCOL" 
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 color : COLOR;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.color = v.color;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 texcol_1;
                float4 tmpvar_2;
                tmpvar_2 = tex2D (_MainTex, i.texcoord0);
                texcol_1 = tmpvar_2;
                float3 tmpvar_3;
                tmpvar_3 = (i.color.xyz * texcol_1.xyz);
                texcol_1.xyz = tmpvar_3;
                float tmpvar_4;
                tmpvar_4 = (texcol_1.w * i.color.w);
                texcol_1.w = tmpvar_4;
                return texcol_1;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}