áShader "Glui/Texture4x8bit/ABLEND/Unlit/VCOL" {
Properties {
 _MainTex ("Texture", 2D) = "white" {}
 _ChannelLookup ("Channel Lookup Texture", 2D) = "black" {}
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

varying highp vec4 xlv_COLOR;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;

uniform highp vec4 _MainTex_ST;
uniform highp vec4 _ChannelLookup_ST;
attribute vec4 _glesMultiTexCoord1;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesColor;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = ((_glesMultiTexCoord1.xy * _ChannelLookup_ST.xy) + _ChannelLookup_ST.zw);
  xlv_COLOR = _glesColor;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_COLOR;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform sampler2D _MainTex;
uniform sampler2D _ChannelLookup;
void main ()
{
  mediump vec4 channel_1;
  mediump vec4 texcol_2;
  lowp vec4 tmpvar_3;
  tmpvar_3 = texture2D (_MainTex, xlv_TEXCOORD0);
  texcol_2 = tmpvar_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture2D (_ChannelLookup, xlv_TEXCOORD1);
  channel_1 = tmpvar_4;
  mediump float tmpvar_5;
  tmpvar_5 = dot (texcol_2, channel_1);
  highp float tmpvar_6;
  tmpvar_6 = (xlv_COLOR.w * tmpvar_5);
  texcol_2.w = tmpvar_6;
  highp vec3 tmpvar_7;
  tmpvar_7 = xlv_COLOR.xyz;
  texcol_2.xyz = tmpvar_7;
  gl_FragData[0] = texcol_2;
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