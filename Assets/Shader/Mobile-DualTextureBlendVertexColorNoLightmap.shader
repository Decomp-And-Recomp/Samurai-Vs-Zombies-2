Shader "Mobile/Dual Texture Blend + Vertex Color + No Lightmaps" 
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _BlendTex1 ("Texture 2", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "RenderType"="Opaque" }
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BlendTex1;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float2 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord0 = v.texcoord0.xy;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 tmpvar_1;
                float4 tmpvar_2;
                tmpvar_2 = tex2D (_MainTex, i.texcoord0);
                float4 tmpvar_3;
                tmpvar_3 = tex2D (_BlendTex1, i.texcoord0);
                tmpvar_1 = (((tmpvar_2 * i.color.w) + (tmpvar_3 * (1.00000 - i.color.w))) * i.color);
                return tmpvar_1;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}