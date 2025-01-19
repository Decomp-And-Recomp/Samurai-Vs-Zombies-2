Shader "Griptonite/Texture/ABLEND/Unlit_VCOL" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader { 
 UsePass "Griptonite/Texture/Base/ALPHA_UNLIT_VCOL"
}
}