#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable
// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Mobile/Dual Texture Blend + Vertex Color" 
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _BlendTex1 ("Texture 2", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
    
    CGPROGRAM
    #pragma surface surf Lambert forwardadd
    
    sampler2D _MainTex, _BlendTex1;
    
    struct Input {
        float2 uv_MainTex;
        float4 color : COLOR;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 c = (tex2D (_MainTex, IN.uv_MainTex) * IN.color.w) + (tex2D (_BlendTex1, IN.uv_MainTex) * (1.00000 - IN.color.w));
        o.Albedo = c.rgb;
        o.Alpha = c.a;
    }
    ENDCG
    }
    Fallback "Diffuse"
}