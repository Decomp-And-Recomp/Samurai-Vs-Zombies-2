¸ZShader "Griptonite/Texture/Base" {
Properties {
 _MainTex ("Base (RGB) RefStrength (A)", 2D) = "white" {}
 _Cube ("Reflection Cubemap(RGB) Alpha(A)", CUBE) = "_Skybox" { TexGen CubeReflect }
 _Color ("Main Color", Color) = (1,1,1,1)
 _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
 _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0)
 _RimPower ("Rim Power", Range(0.5,8)) = 3
}
SubShader { 
 Pass {
  Name "OPAQUE_UNLIT_REFLECT"
  Tags { "RenderType"="Opaque" }
  Fog { Mode Off }
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  mat3 tmpvar_1;
  tmpvar_1[0] = _Object2World[0].xyz;
  tmpvar_1[1] = _Object2World[1].xyz;
  tmpvar_1[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 * (normalize(_glesNormal) * unity_Scale.w));
  highp vec3 i_3;
  i_3 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform sampler2D _MainTex;
uniform samplerCube _Cube;
void main ()
{
  mediump vec4 tmpvar_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec4 tmpvar_3;
  tmpvar_3 = textureCube (_Cube, xlv_TEXCOORD1);
  lowp vec4 col_4;
  col_4.w = tmpvar_2.w;
  col_4.xyz = (tmpvar_2.xyz * 0.500000);
  col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
  tmpvar_1 = col_4;
  gl_FragData[0] = tmpvar_1;
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
 Pass {
  Name "OPAQUE_UNLIT_REFLECT-COLOR-REFLCOL"
  Tags { "RenderType"="Opaque" }
  Fog { Mode Off }
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  mat3 tmpvar_1;
  tmpvar_1[0] = _Object2World[0].xyz;
  tmpvar_1[1] = _Object2World[1].xyz;
  tmpvar_1[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 * (normalize(_glesNormal) * unity_Scale.w));
  highp vec3 i_3;
  i_3 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform lowp vec4 _ReflectColor;
uniform sampler2D _MainTex;
uniform samplerCube _Cube;
uniform lowp vec4 _Color;
void main ()
{
  mediump vec4 tmpvar_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = (texture2D (_MainTex, xlv_TEXCOORD0) * _Color);
  lowp vec4 tmpvar_3;
  tmpvar_3 = (textureCube (_Cube, xlv_TEXCOORD1) * _ReflectColor);
  lowp vec4 col_4;
  col_4.w = tmpvar_2.w;
  col_4.xyz = (tmpvar_2.xyz * 0.500000);
  col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
  tmpvar_1 = col_4;
  gl_FragData[0] = tmpvar_1;
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
 Pass {
  Name "ABLEND_UNLIT_REFLECT"
  Tags { "RenderType"="Transparent" }
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

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  mat3 tmpvar_1;
  tmpvar_1[0] = _Object2World[0].xyz;
  tmpvar_1[1] = _Object2World[1].xyz;
  tmpvar_1[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 * (normalize(_glesNormal) * unity_Scale.w));
  highp vec3 i_3;
  i_3 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform sampler2D _MainTex;
uniform samplerCube _Cube;
void main ()
{
  mediump vec4 tmpvar_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec4 tmpvar_3;
  tmpvar_3 = textureCube (_Cube, xlv_TEXCOORD1);
  lowp vec4 col_4;
  col_4.w = tmpvar_2.w;
  col_4.xyz = (tmpvar_2.xyz * 0.500000);
  col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
  tmpvar_1 = col_4;
  gl_FragData[0] = tmpvar_1;
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
 Pass {
  Name "ABLEND_UNLIT_REFLECT-COLOR-REFLCOL"
  Tags { "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  mat3 tmpvar_1;
  tmpvar_1[0] = _Object2World[0].xyz;
  tmpvar_1[1] = _Object2World[1].xyz;
  tmpvar_1[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 * (normalize(_glesNormal) * unity_Scale.w));
  highp vec3 i_3;
  i_3 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = (i_3 - (2.00000 * (dot (tmpvar_2, i_3) * tmpvar_2)));
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform lowp vec4 _ReflectColor;
uniform sampler2D _MainTex;
uniform samplerCube _Cube;
uniform lowp vec4 _Color;
void main ()
{
  mediump vec4 tmpvar_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = (texture2D (_MainTex, xlv_TEXCOORD0) * _Color);
  lowp vec4 tmpvar_3;
  tmpvar_3 = (textureCube (_Cube, xlv_TEXCOORD1) * _ReflectColor);
  lowp vec4 col_4;
  col_4.w = tmpvar_2.w;
  col_4.xyz = (tmpvar_2.xyz * 0.500000);
  col_4.xyz = (col_4.xyz + ((tmpvar_3.xyz * tmpvar_3.w) * 0.500000));
  tmpvar_1 = col_4;
  gl_FragData[0] = tmpvar_1;
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
 Pass {
  Name "OPAQUE_UNLIT_RIM"
  Tags { "RenderType"="Opaque" }
  Fog { Mode Off }
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD2;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = (_WorldSpaceCameraPos - (_Object2World * _glesVertex).xyz);
  xlv_TEXCOORD2 = normalize(_glesNormal);
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD2;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform lowp float _RimPower;
uniform lowp vec4 _RimColor;
uniform sampler2D _MainTex;
void main ()
{
  mediump vec4 tmpvar_1;
  lowp float rim_2;
  lowp vec4 c_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture2D (_MainTex, xlv_TEXCOORD0);
  c_3.w = tmpvar_4.w;
  highp float tmpvar_5;
  tmpvar_5 = (1.00000 - clamp (dot (normalize(xlv_TEXCOORD1), xlv_TEXCOORD2), 0.00000, 1.00000));
  rim_2 = tmpvar_5;
  lowp float tmpvar_6;
  tmpvar_6 = pow (rim_2, _RimPower);
  c_3.xyz = ((tmpvar_4.xyz * (1.00000 - tmpvar_6)) + (_RimColor.xyz * tmpvar_6));
  tmpvar_1 = c_3;
  gl_FragData[0] = tmpvar_1;
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
 Pass {
  Name "OPAQUE_UNLIT_VCOL"
  Tags { "RenderType"="Opaque" }
  Fog { Mode Off }
Program "vp" {
SubProgram "gles " {
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec2 xlv_TEXCOORD0;
varying highp vec4 xlv_COLOR;

uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesColor;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_COLOR = _glesColor;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
}



#endif
#ifdef FRAGMENT

varying highp vec2 xlv_TEXCOORD0;
varying highp vec4 xlv_COLOR;
uniform sampler2D _MainTex;
void main ()
{
  mediump vec4 c_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
  c_1 = tmpvar_2;
  highp vec3 tmpvar_3;
  tmpvar_3 = (c_1.xyz * xlv_COLOR.xyz);
  c_1.xyz = tmpvar_3;
  gl_FragData[0] = c_1;
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
 Pass {
  Name "ALPHA_UNLIT_VCOL"
  Tags { "RenderType"="Transparent" }
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
varying highp vec4 xlv_COLOR;

uniform highp vec4 _MainTex_ST;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesColor;
attribute vec4 _glesVertex;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_COLOR = _glesColor;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
}



#endif
#ifdef FRAGMENT

varying highp vec2 xlv_TEXCOORD0;
varying highp vec4 xlv_COLOR;
uniform sampler2D _MainTex;
uniform lowp vec4 _Color;
void main ()
{
  mediump vec4 c_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = (texture2D (_MainTex, xlv_TEXCOORD0) * _Color);
  c_1 = tmpvar_2;
  highp vec3 tmpvar_3;
  tmpvar_3 = (c_1.xyz * xlv_COLOR.xyz);
  c_1.xyz = tmpvar_3;
  highp float tmpvar_4;
  tmpvar_4 = (c_1.w * xlv_COLOR.w);
  c_1.w = tmpvar_4;
  gl_FragData[0] = c_1;
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
}