Shader "Griptonite/Texture/ABLEND/Diffuse_VCOL" 
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MainTexInt ("Base Intensity", Range(0,4)) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
    
    CGPROGRAM
    #pragma surface surf Lambert forwardadd alpha:fade
    
    sampler2D _MainTex;
    fixed4 _Color;
    float _MainTexInt;
    
    struct Input {
        float2 uv_MainTex;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb * _MainTexInt;
        o.Alpha = c.a;
    }
    ENDCG
    }
    Fallback "Diffuse"
}