#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable
// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Decal" 
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DecalTex ("Decal (RGBA)", 2D) = "black" {}
    }
    SubShader
    {
        LOD 250
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "FORWARD"
            Tags { "LIGHTMODE"="ForwardBase" "RenderType"="Opaque" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            float4 _DecalTex_ST;
            sampler2D _MainTex;
            #include "UnityLightingCommon.cginc"
            sampler2D _DecalTex;
            float4 _Color;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float3 texcoord2 : TEXCOORD2;
                float3 texcoord1 : TEXCOORD1;
                float4 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float3 shlight_1;
                float4 tmpvar_2;
                float3 tmpvar_3;
                float3 tmpvar_4;
                tmpvar_2.xy = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                tmpvar_2.zw = ((v.texcoord0.xy * _DecalTex_ST.xy) + _DecalTex_ST.zw);
                float3x3 tmpvar_5;
                tmpvar_5[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_5[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_5[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_6;
                tmpvar_6 = mul(tmpvar_5, (normalize(v.normal) * 1.0));
                tmpvar_3 = tmpvar_6;
                float4 tmpvar_7;
                tmpvar_7.w = 1.00000;
                tmpvar_7.xyz = tmpvar_6;
                float3 tmpvar_8;
                float4 normal_9;
                normal_9 = tmpvar_7;
                float3 x3_10;
                float vC_11;
                float3 x2_12;
                float3 x1_13;
                float tmpvar_14;
                tmpvar_14 = dot (unity_SHAr, normal_9);
                x1_13.x = tmpvar_14;
                float tmpvar_15;
                tmpvar_15 = dot (unity_SHAg, normal_9);
                x1_13.y = tmpvar_15;
                float tmpvar_16;
                tmpvar_16 = dot (unity_SHAb, normal_9);
                x1_13.z = tmpvar_16;
                float4 tmpvar_17;
                tmpvar_17 = (normal_9.xyzz * normal_9.yzzx);
                float tmpvar_18;
                tmpvar_18 = dot (unity_SHBr, tmpvar_17);
                x2_12.x = tmpvar_18;
                float tmpvar_19;
                tmpvar_19 = dot (unity_SHBg, tmpvar_17);
                x2_12.y = tmpvar_19;
                float tmpvar_20;
                tmpvar_20 = dot (unity_SHBb, tmpvar_17);
                x2_12.z = tmpvar_20;
                float tmpvar_21;
                tmpvar_21 = ((normal_9.x * normal_9.x) - (normal_9.y * normal_9.y));
                vC_11 = tmpvar_21;
                float3 tmpvar_22;
                tmpvar_22 = (unity_SHC.xyz * vC_11);
                x3_10 = tmpvar_22;
                tmpvar_8 = ((x1_13 + x2_12) + x3_10);
                shlight_1 = tmpvar_8;
                tmpvar_4 = shlight_1;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = tmpvar_2;
                o.texcoord1 = tmpvar_3;
                o.texcoord2 = tmpvar_4;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 c_1;
                float4 decal_2;
                float4 c_3;
                float4 tmpvar_4;
                tmpvar_4 = tex2D (_MainTex, i.texcoord0.xy);
                c_3.w = tmpvar_4.w;
                float4 tmpvar_5;
                tmpvar_5 = tex2D (_DecalTex, i.texcoord0.zw);
                decal_2 = tmpvar_5;
                float3 tmpvar_6;
                tmpvar_6 = lerp (tmpvar_4.xyz, decal_2.xyz, decal_2.www);
                c_3.xyz = tmpvar_6;
                float4 tmpvar_7;
                tmpvar_7 = (c_3 * _Color);
                c_3 = tmpvar_7;
                float4 c_8;
                c_8.xyz = ((tmpvar_7.xyz * _LightColor0.xyz) * (max (0.000000, dot (i.texcoord1, _WorldSpaceLightPos0.xyz)) * 2.00000));
                c_8.w = tmpvar_7.w;
                c_1.w = c_8.w;
                c_1.xyz = (c_8.xyz + (tmpvar_7.xyz * i.texcoord2));
                return c_1;
            }
            ENDCG
        }
        Pass
        {
            Name "FORWARD"
            Tags { "LIGHTMODE"="ForwardAdd" "RenderType"="Opaque" }
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
            float4 _DecalTex_ST;
            sampler2D _MainTex;
            sampler2D _LightTexture0;
            #include "UnityLightingCommon.cginc"
            sampler2D _DecalTex;
            float4 _Color;
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
                float4 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float4 tmpvar_1;
                float3 tmpvar_2;
                float3 tmpvar_3;
                tmpvar_1.xy = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                tmpvar_1.zw = ((v.texcoord0.xy * _DecalTex_ST.xy) + _DecalTex_ST.zw);
                float3x3 tmpvar_4;
                tmpvar_4[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_4[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_4[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_5;
                tmpvar_5 = mul(tmpvar_4, (normalize(v.normal) * 1.0));
                tmpvar_2 = tmpvar_5;
                float3 tmpvar_6;
                tmpvar_6 = (_WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
                tmpvar_3 = tmpvar_6;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = tmpvar_1;
                o.texcoord1 = tmpvar_2;
                o.texcoord2 = tmpvar_3;
                o.texcoord3 = mul(unity_WorldToLight, mul(unity_ObjectToWorld, v.vertex)).xyz;
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 c_1;
                float3 lightDir_2;
                float4 decal_3;
                float4 c_4;
                float4 tmpvar_5;
                tmpvar_5 = tex2D (_MainTex, i.texcoord0.xy);
                c_4.w = tmpvar_5.w;
                float4 tmpvar_6;
                tmpvar_6 = tex2D (_DecalTex, i.texcoord0.zw);
                decal_3 = tmpvar_6;
                float3 tmpvar_7;
                tmpvar_7 = lerp (tmpvar_5.xyz, decal_3.xyz, decal_3.www);
                c_4.xyz = tmpvar_7;
                float4 tmpvar_8;
                tmpvar_8 = (c_4 * _Color);
                c_4 = tmpvar_8;
                float3 tmpvar_9;
                tmpvar_9 = normalize(i.texcoord2);
                lightDir_2 = tmpvar_9;
                float tmpvar_10;
                tmpvar_10 = dot (i.texcoord3, i.texcoord3);
                float4 c_11;
                c_11.xyz = ((tmpvar_8.xyz * _LightColor0.xyz) * ((max (0.000000, dot (i.texcoord1, lightDir_2)) * tex2D (_LightTexture0, tmpvar_10.xx).w) * 2.00000));
                c_11.w = tmpvar_8.w;
                c_1.xyz = c_11.xyz;
                c_1.w = 0.000000;
                return c_1;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}