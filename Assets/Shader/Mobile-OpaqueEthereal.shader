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
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150

    CGPROGRAM
    #pragma surface surf Lambert noforwardadd

    sampler2D _MainTex;
    float4 _RimColor;
    float _RimPower;

    struct Input {
        float2 uv_MainTex;
        float3 viewDir;
    };

    void surf (Input IN, inout SurfaceOutput o) {
        float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb + (_RimColor.rgb * pow(rim, _RimPower));
        o.Alpha = c.a;
    }
    ENDCG
    }

    Fallback "Mobile/VertexLit"
    }