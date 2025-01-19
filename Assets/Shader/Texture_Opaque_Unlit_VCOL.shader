Shader "Griptonite/Texture/Opaque/Unlit_VCOL" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 UsePass "Griptonite/Texture/Base/OPAQUE_UNLIT_VCOL"
}
}