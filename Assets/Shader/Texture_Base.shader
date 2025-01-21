#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable

Shader "Griptonite/Texture/Base" 
{
    Properties
    {
        _MainTex ("Base (RGB) RefStrength (A)", 2D) = "white" {}
        _Cube ("Reflection Cubemap(RGB) Alpha(A)", CUBE) = "_Skybox" { TexGen CubeReflect }
        _Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
    }
    SubShader
    {
        Pass
        {
            Name "OPAQUE_UNLIT_REFLECT-COLOR-REFLCOL"
            Tags { "RenderType"="Opaque" }
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            float4 _ReflectColor;
            sampler2D _MainTex;
            samplerCUBE _Cube;
            float4 _Color;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float3 texcoord1 : TEXCOORD1;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float3x3 tmpvar_1;
                tmpvar_1[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_1[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_1[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_2;
                tmpvar_2 = mul(tmpvar_1, (normalize(v.normal) * 1.0));
                float3 i_3;
                i_3 = (mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 tmpvar_1;
                float4 tmpvar_2;
                tmpvar_2 = (tex2D (_MainTex, i.texcoord0) * _Color);
                float4 tmpvar_3;
                tmpvar_3 = (texCUBE (_Cube, i.texcoord1) * _ReflectColor);
                float4 col_4;
                col_4.w = tmpvar_2.w;
                col_4.xyz = (tmpvar_2.xyz * 0.500000);
                col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
                tmpvar_1 = col_4;
                return tmpvar_1;
            }
            ENDCG
        }
        Pass
        {
            Name "ABLEND_UNLIT_REFLECT"
            Tags { "RenderType"="Transparent" }
            Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            sampler2D _MainTex;
            samplerCUBE _Cube;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float3 texcoord1 : TEXCOORD1;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float3x3 tmpvar_1;
                tmpvar_1[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_1[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_1[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_2;
                tmpvar_2 = mul(tmpvar_1, (normalize(v.normal) * 1.0));
                float3 i_3;
                i_3 = (mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 tmpvar_1;
                float4 tmpvar_2;
                tmpvar_2 = tex2D (_MainTex, i.texcoord0);
                float4 tmpvar_3;
                tmpvar_3 = texCUBE (_Cube, i.texcoord1);
                float4 col_4;
                col_4.w = tmpvar_2.w;
                col_4.xyz = (tmpvar_2.xyz * 0.500000);
                col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
                tmpvar_1 = col_4;
                return tmpvar_1;
            }
            ENDCG
        }
        Pass
        {
            Name "ABLEND_UNLIT_REFLECT-COLOR-REFLCOL"
            Tags { "RenderType"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            float4 _ReflectColor;
            sampler2D _MainTex;
            samplerCUBE _Cube;
            float4 _Color;
            struct appdata_t
            {
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float3 texcoord1 : TEXCOORD1;
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                float3x3 tmpvar_1;
                tmpvar_1[0] = unity_ObjectToWorld[0].xyz;
                tmpvar_1[1] = unity_ObjectToWorld[1].xyz;
                tmpvar_1[2] = unity_ObjectToWorld[2].xyz;
                float3 tmpvar_2;
                tmpvar_2 = mul(tmpvar_1, (normalize(v.normal) * 1.0));
                float3 i_3;
                i_3 = (mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 tmpvar_1;
                float4 tmpvar_2;
                tmpvar_2 = (tex2D (_MainTex, i.texcoord0) * _Color);
                float4 tmpvar_3;
                tmpvar_3 = (texCUBE (_Cube, i.texcoord1) * _ReflectColor);
                float4 col_4;
                col_4.w = tmpvar_2.w;
                col_4.xyz = (tmpvar_2.xyz * 0.500000);
                col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
                tmpvar_1 = col_4;
                return tmpvar_1;
            }
            ENDCG
        }
        Pass
        {
            Name "OPAQUE_UNLIT_RIM"
            Tags { "RenderType"="Opaque" }
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // float4x4 _Object2World;
            float4 _MainTex_ST;
            float _RimPower;
            float4 _RimColor;
            sampler2D _MainTex;
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
                float2 texcoord0 : TEXCOORD0;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                o.texcoord1 = (_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                o.texcoord2 = normalize(v.normal);
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 tmpvar_1;
                float rim_2;
                float4 c_3;
                float4 tmpvar_4;
                tmpvar_4 = tex2D (_MainTex, i.texcoord0);
                c_3.w = tmpvar_4.w;
                float tmpvar_5;
                tmpvar_5 = (1.00000 - clamp (dot (normalize(i.texcoord1), i.texcoord2), 0.00000, 1.00000));
                rim_2 = tmpvar_5;
                float tmpvar_6;
                tmpvar_6 = pow (rim_2, _RimPower);
                c_3.xyz = ((tmpvar_4.xyz * (1.00000 - tmpvar_6)) + (_RimColor.xyz * tmpvar_6));
                tmpvar_1 = c_3;
                return tmpvar_1;
            }
            ENDCG
        }
        Pass
        {
            Name "OPAQUE_UNLIT_VCOL"
            Tags { "RenderType"="Opaque" }
            Fog { Mode Off }
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
                float2 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 c_1;
                float4 tmpvar_2;
                tmpvar_2 = tex2D (_MainTex, i.texcoord0);
                c_1 = tmpvar_2;
                float3 tmpvar_3;
                tmpvar_3 = (c_1.xyz * i.color.xyz);
                c_1.xyz = tmpvar_3;
                return c_1;
            }
            ENDCG
        }
        Pass
        {
            Name "ALPHA_UNLIT_VCOL"
            Tags { "RenderType"="Transparent" }
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
                o.texcoord0 = ((v.texcoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
                return o;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float4 c_1;
                float4 tmpvar_2;
                tmpvar_2 = (tex2D (_MainTex, i.texcoord0) * _Color);
                c_1 = tmpvar_2;
                float3 tmpvar_3;
                tmpvar_3 = (c_1.xyz * i.color.xyz);
                c_1.xyz = tmpvar_3;
                float tmpvar_4;
                tmpvar_4 = (c_1.w * i.color.w);
                c_1.w = tmpvar_4;
                return c_1;
            }
            ENDCG
        }
    }
    
}