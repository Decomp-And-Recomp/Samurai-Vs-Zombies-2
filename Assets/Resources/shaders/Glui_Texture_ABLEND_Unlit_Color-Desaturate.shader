ˆShader "Glui/Texture/ABLEND/Unlit_Color-Desaturate" {
Properties {
 _MainTex ("Texture", 2D) = "white" {}
 _Color ("Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  Cull Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec2 xlv_TEXCOORD0;

uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
}



#endif
#ifdef FRAGMENT

varying highp vec2 xlv_TEXCOORD0;
uniform sampler2D _MainTex;
uniform highp vec4 _Color;
void main ()
{
  mediump vec4 texcol_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
  texcol_1 = tmpvar_2;
  texcol_1.xyz = vec3(dot (texcol_1.xyz, vec3(0.300000, 0.590000, 0.110000)));
  highp vec3 tmpvar_3;
  tmpvar_3 = (_Color.xyz * texcol_1.xyz);
  texcol_1.xyz = tmpvar_3;
  highp float tmpvar_4;
  tmpvar_4 = (texcol_1.w * _Color.w);
  texcol_1.w = tmpvar_4;
  gl_FragData[0] = texcol_1;
}



#endif"
}
}
Program "fp" {
SubProgram "gles " {
"!!GLES"
}
}
 }
}
Fallback "VertexLit"
}