Shader "Glui/Texture4x8bit/ABLEND/Unlit/VCOL" 
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ChannelLookup ("Channel Lookup Texture", 2D) = "black" {}
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
            float4 _ChannelLookup_ST;
            sampler2D _MainTex;
            sampler2D _ChannelLookup;
            struct appdata_t
            {
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 color : COLOR;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = ((v.texcoord1.xy * _ChannelLookup_ST.xy) + _ChannelLookup_ST.zw);
                o.color = v.color;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 channel_1;
                float4 texcol_2;
                float4 tmpvar_3;
                tmpvar_3 = tex2D (_MainTex, i.texcoord0);
                texcol_2 = tmpvar_3;
                float4 tmpvar_4;
                tmpvar_4 = tex2D (_ChannelLookup, i.texcoord1);
                channel_1 = tmpvar_4;
                float tmpvar_5;
                tmpvar_5 = dot (texcol_2, channel_1);
                float tmpvar_6;
                tmpvar_6 = (i.color.w * tmpvar_5);
                texcol_2.w = tmpvar_6;
                float3 tmpvar_7;
                tmpvar_7 = i.color.xyz;
                texcol_2.xyz = tmpvar_7;
                return texcol_2;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}