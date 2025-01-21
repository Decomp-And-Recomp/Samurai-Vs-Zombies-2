#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable
// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Mobile/OpaqueEthereal" 
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
    }
    SubShader
    {
        Pass
        {
            Name "FORWARD"
            Tags { "LIGHTMODE"="ForwardAdd" }
            ZWrite Off
            Fog {
            Color (0,0,0,0)
            }
            Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            float4x4 unity_WorldToLight;
            sampler2D _MainTex;
            sampler2D _LightTexture0;
            #include "UnityLightingCommon.cginc"
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float3 texcoord3 : TEXCOORD3;
                float3 texcoord2 : TEXCOORD2;
                float3 texcoord1 : TEXCOORD1;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float3 tmpvar_1;
                float3 tmpvar_2;
                float3x3 tmpvar_3;
                tmpvar_3[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_3[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_3[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_4;
                tmpvar_4 = mul(tmpvar_3, (normalize(v.normal) * 1.0));
                tmpvar_1 = tmpvar_4;
                float3 tmpvar_5;
                tmpvar_5 = (_WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
                tmpvar_2 = tmpvar_5;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = tmpvar_1;
                o.texcoord2 = tmpvar_2;
                o.texcoord3 = mul(unity_WorldToLight, mul(unity_ObjectToWorld, v.vertex)).xyz;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 c_1;
                float3 lightDir_2;
                float3 tmpvar_3;
                tmpvar_3 = normalize(i.texcoord2);
                lightDir_2 = tmpvar_3;
                float tmpvar_4;
                tmpvar_4 = dot (i.texcoord3, i.texcoord3);
                float4 c_5;
                c_5.xyz = ((tex2D (_MainTex, i.texcoord0).xyz * _LightColor0.xyz) * ((max (0.00000, dot (i.texcoord1, lightDir_2)) * tex2D (_LightTexture0, tmpvar_4.xx).w) * 2.00000));
                c_5.w = 0.00000;
                c_1.xyz = c_5.xyz;
                c_1.w = 0.00000;
                return c_1;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}