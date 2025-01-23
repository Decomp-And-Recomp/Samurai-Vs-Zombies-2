using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ULegacyRipper
{
    public static class ULegacyShaderTranslator
    {
        private class Shader
        {
            public string name;

            public List<string> properties = new List<string>();

            public List<SubShader> subShaders = new List<SubShader>();

            public string fallback;

            public List<string> includes = new List<string>();
        }

        private class SubShader
        {
            public List<string> tags = new List<string>();

            public List<SubShaderPass> passes = new List<SubShaderPass>();
        }

        private class SubShaderPass
        {
            public List<string> tags = new List<string>();

            public List<string> vertexVariables, fragmentVariables;

            public List<string> vertex = new List<string>(),
            fragment = new List<string>();
        }

        private const string OutputVariable = "xlv_",
        InputVariable = "_gles", UnityVariable = "unity_",
        GLState = "glstate_", GLVariable = "gl_";

        private static Dictionary<string, string> TypeReplacements = new Dictionary<string, string>
        {
            //matrices
            {"mat3", "float3x3"},
            {"mat4", "float4x4"},

            //vertices
            {"vec2", "float2"},
            {"vec3", "float3"},
            {"vec4", "float4"},

            //samplers
            {"samplerCube", "samplerCUBE"}
        };

        private static Dictionary<string, string> MethodReplacements = new Dictionary<string, string>
        {
            //gles -> unity variables
            {"_Object2World", "unity_ObjectToWorld"},
            {"_World2Object", "unity_WorldToObject"},

            //gles variable modifiers, not applicable in cg
            {"lowp ", ""},
            {"mediump ", ""},
            {"highp ", ""},

            //math functions
            {"mix", "lerp"},
            {"inversesqrt", "rsqrt"},
        };

        private static Dictionary<string, string> VertexMethodReplacements = new Dictionary<string, string>
        {
            {"(gl_ModelViewProjectionMatrix * _glesVertex)", "UnityObjectToClipPos(v.vertex)"},
            {"gl_TextureMatrix", "UNITY_MATRIX_TEXTURE"},
            {"gl_Position", "o.vertex"},
            {"(unity_ObjectToWorld *", "mul(unity_ObjectToWorld,"},
            {"(unity_WorldToObject *", "mul(unity_WorldToObject,"},
            {"(UNITY_MATRIX_TEXTURE0 *", "mul(UNITY_MATRIX_TEXTURE0,"},
            {"(tmpvar_1 * (normalize(_glesNormal) * unity_Scale.w))", "mul(tmpvar_1, (normalize(_glesNormal) * unity_Scale.w))"},
            {"(tmpvar_2 * (normalize(_glesNormal) * unity_Scale.w))", "mul(tmpvar_2, (normalize(_glesNormal) * unity_Scale.w))"},
            {"(tmpvar_3 * (normalize(_glesNormal) * unity_Scale.w))", "mul(tmpvar_3, (normalize(_glesNormal) * unity_Scale.w))"},
            {"(tmpvar_4 * (normalize(_glesNormal) * unity_Scale.w))", "mul(tmpvar_4, (normalize(_glesNormal) * unity_Scale.w))"},
            {"(tmpvar_5 * (normalize(_glesNormal) * unity_Scale.w))", "mul(tmpvar_5, (normalize(_glesNormal) * unity_Scale.w))"},
            {"(_LightMatrix0 * ", "mul(unity_WorldToLight, "}//
        };

        private static Dictionary<string, string> FragmentMethodReplacements = new Dictionary<string, string>
        {
            {"gl_FragData[0] =", "return"},
            {"texture2D", "tex2D"},
            {"textureCube", "texCUBE"},
            {"tex2DProj", "tex2Dproj"},
        };

        private static List<string> BuiltInVariables = new List<string>
        {
            "_WorldSpaceCameraPos",
            "_ProjectionParams",
            "_ScreenParams",
            "_ZBufferParams",
            "_Time",
            "_SinTime",
            "_CosTime",
            "_LightColor0",
            "_WorldSpaceLightPos0",
            "_LightColor",
            "_TextureSampleAdd"
        };

        private static string RunReplacements(string original, Dictionary<string, string> replacements, string prefix = "", string postfix = "")
        {
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                original = original.Replace(replacement.Key, prefix + replacement.Value + postfix);
            }

            return original;
        }

        public static void TranslateShader(string assetPath)
        {
            Shader shader = ReadCompiledShader(File.ReadAllLines(assetPath));

            if (shader != null)
            {
                File.WriteAllText(assetPath, WriteCompiledShader(shader));
            }
        }

        private static string WriteCompiledShader(Shader shader)
        {
            IndentedWriter writer = new IndentedWriter();

            writer.WriteLine(shader.name);

            writer.BeginBraces();
            {
                writer.WriteLine("Properties");

                writer.BeginBraces();
                {
                    writer.WriteLines(shader.properties);
                }
                writer.EndBraces();

                foreach (SubShader subShader in shader.subShaders)
                {
                    writer.WriteLine("SubShader");

                    writer.BeginBraces();
                    {
                        writer.WriteLines(subShader.tags);

                        foreach (SubShaderPass pass in subShader.passes)
                        {
                            writer.WriteLine("Pass");

                            Dictionary<string, string> outputReplacements = new Dictionary<string, string>(),
                            inputReplacements = new Dictionary<string, string>();

                            writer.BeginBraces();
                            {
                                writer.WriteLines(pass.tags);
                                writer.WriteLine("CGPROGRAM");

                                writer.WriteLine("#pragma vertex vert");
                                writer.WriteLine("#pragma fragment frag");
                            
                                writer.WriteLine("#include \"UnityCG.cginc\"");

                                bool writtenCommonLighting = false;

                                List<string> variables = new List<string>();

                                foreach (string variable in pass.vertexVariables)
                                {
                                    string variableName = ExtractVariableName(variable);

                                    if (!writtenCommonLighting && variableName == "_LightColor0")
                                    {
                                        writer.WriteLine("#include \"UnityLightingCommon.cginc\"");
                                        writtenCommonLighting = true;
                                    }

                                    if (variables.Contains(variableName))
                                    {
                                        continue;
                                    }

                                    if (IsNormalVariable(variableName))
                                    {
                                        writer.WriteLine(RunReplacements(ExtractVariableType(variable), TypeReplacements) + " " + variableName + ";");
                                        variables.Add(variableName);
                                    }
                                }
                                
                                foreach (string variable in pass.fragmentVariables)
                                {
                                    string variableName = ExtractVariableName(variable);

                                    if (!writtenCommonLighting && variableName == "_LightColor0")
                                    {
                                        writer.WriteLine("#include \"UnityLightingCommon.cginc\"");
                                        writtenCommonLighting = true;
                                    }

                                    if (variables.Contains(variableName))
                                    {
                                        continue;
                                    }

                                    if (IsNormalVariable(variableName))
                                    {
                                        writer.WriteLine(RunReplacements(ExtractVariableType(variable), TypeReplacements) + " " + variableName + ";");
                                        variables.Add(variableName);
                                    }
                                }

                                writer.WriteLine("struct appdata_t");

                                writer.BeginBraces();
                                {
                                    foreach (string variable in pass.vertexVariables)
                                    {
                                        string variableName = ExtractVariableName(variable);

                                        if (variableName.StartsWith(InputVariable))
                                        {
                                            string name = ExtractInputVariableName(variable);

                                            writer.WriteLine(RunReplacements(ExtractVariableType(variable), TypeReplacements) + " " + name + " : " + ExtractInputVariableDirective(variable) + ";");
                                            inputReplacements.Add(variableName, name);
                                        }
                                    }
                                }
                                writer.EndBracesStruct();

                                writer.WriteLine("struct v2f");

                                writer.BeginBraces();
                                {
                                    List<string> outputs = new List<string>(); //failsafe incase one program or the other doesn't contain any number of output variables

                                    foreach (string variable in pass.vertexVariables)
                                    {
                                        string variableName = ExtractVariableName(variable);

                                        if (variableName.StartsWith(OutputVariable) && !outputs.Contains(variable))
                                        {
                                            outputs.Add(variable);
                                        }
                                    }

                                    foreach (string variable in pass.fragmentVariables)
                                    {
                                        string variableName = ExtractVariableName(variable);

                                        if (variableName.StartsWith(OutputVariable) && !outputs.Contains(variable))
                                        {
                                            outputs.Add(variable);
                                        }
                                    }

                                    foreach (string output in outputs)
                                    {
                                        string name = ExtractOutputVariableName(output);

                                        writer.WriteLine(RunReplacements(ExtractVariableType(output), TypeReplacements) + " " + name + " : " + ExtractOutputVariableDirective(output) + ";");
                                        outputReplacements.Add(ExtractVariableName(output), name);
                                    }

                                    writer.WriteLine("float4 vertex : POSITION;");
                                }
                                writer.EndBracesStruct();

                                writer.WriteLine("v2f vert(appdata_t v)");

                                writer.BeginBraces();
                                {
                                    List<string> vertexFunction = new List<string>
                                    {
                                        "v2f o;"
                                    };

                                    foreach (string line in pass.vertex)
                                    {
                                        vertexFunction.Add
                                        (
                                            RunReplacements
                                            (
                                                RunReplacements
                                                (
                                                    RunReplacements
                                                    (
                                                        RunReplacements
                                                        (
                                                            RunReplacements
                                                            (
                                                                line, MethodReplacements
                                                            ),
                                                            VertexMethodReplacements
                                                        ),
                                                        TypeReplacements
                                                    ),
                                                    inputReplacements, "v."
                                                ),
                                                outputReplacements, "o."
                                            )
                                        );
                                    }

                                    vertexFunction.Add("return o;");

                                    writer.WriteLines(vertexFunction);
                                }
                                writer.EndBraces();

                                writer.WriteLine("float4 frag(v2f i) : SV_TARGET");

                                writer.BeginBraces();
                                {
                                    List<string> fragmentFunction = new List<string>();

                                    foreach (string line in pass.fragment)
                                    {
                                        fragmentFunction.Add
                                        (
                                            RunReplacements
                                            (
                                                RunReplacements
                                                (
                                                    RunReplacements
                                                    (
                                                        RunReplacements(line, MethodReplacements), FragmentMethodReplacements
                                                    ),
                                                    TypeReplacements
                                                ),
                                                outputReplacements, "i."
                                            )
                                        );
                                    }

                                    writer.WriteLines(fragmentFunction);
                                }
                                writer.EndBraces();

                                writer.WriteLine("ENDCG");
                            }
                            writer.EndBraces();
                        }
                    }
                    writer.EndBraces();
                }   
            }
            
            writer.WriteLine(shader.fallback);
            writer.EndBraces();

            return writer.ToString();
        }
        
        private static string ExtractVariableType(string variable)
        {
            string[] variableModifiers = variable.Split(' ');

            return variableModifiers[variableModifiers.Length - 2];
        }

        private static string ExtractVariableName(string variable)
        {
            string[] variableModifiers = variable.Split(' ');

            return variableModifiers[variableModifiers.Length - 1].Split(';')[0];
        }
        
        private static bool IsNormalVariable(string variableName)
        {
            return !variableName.StartsWith(OutputVariable) && !variableName.StartsWith(InputVariable)
            && !variableName.StartsWith(UnityVariable)&& !variableName.StartsWith(GLState)
            && !variableName.StartsWith(GLVariable) && !BuiltInVariables.Contains(variableName);
        }

        private static string ExtractInputVariableName(string variable)
        {
            string name = ExtractVariableName(variable);

            return name.Substring(InputVariable.Length).ToLower().Replace("multi", "");
        }

        private static string ExtractOutputVariableName(string variable)
        {
            string name = ExtractVariableName(variable);

            return name.Substring(OutputVariable.Length).ToLower().Replace("multi", "");
        }

        private static string ExtractInputVariableDirective(string variable)
        {
            string name = ExtractVariableName(variable);

            return name.Substring(InputVariable.Length).ToUpper().Replace("VERTEX", "POSITION").Replace("MULTI", "");
        }

        private static string ExtractOutputVariableDirective(string variable)
        {
            string name = ExtractVariableName(variable);

            return name.Substring(OutputVariable.Length).ToUpper().Replace("VERTEX", "POSITION").Replace("MULTI", "");
        }

        private static Shader ReadCompiledShader(string[] content)
        {
            IndentedReader reader = new IndentedReader(content);

            try
            {
                reader.SearchFile("Program");
            }
            catch
            {
                return null;
            }

            Shader shader = new Shader();
            string nameLine;
            
            do
            {
                nameLine = reader.ReadLine();
            }
            while (!nameLine.Contains("Shader"));

            shader.name = nameLine.Substring(nameLine.IndexOf("Shader")).Replace("{", ""); //skip invalid characters

            reader.Search("Properties");
            shader.properties = reader.ReadUntil("}");

            while (true)
            {
                try
                {
                    reader.Search("SubShader");

                    SubShader subShader = new SubShader();

                    int previousIndex = reader.lineIndex;
                    subShader.tags = reader.ReadUntil("Pass");

                    reader.lineIndex = previousIndex;

                    while (true)
                    {
                        try
                        {
                            reader.Search("Pass");
                            SubShaderPass pass = new SubShaderPass();

                            int currentIndentation = reader.indentationLevel;

                            while (true)
                            {
                                string line = reader.ReadLine();

                                if (line.StartsWith("Program"))
                                {
                                    break;
                                }

                                pass.tags.Add(line);

                                if (reader.indentationLevel < currentIndentation)
                                {
                                    break;
                                }
                            }

                            reader.Search("#ifdef VERTEX");
                            pass.vertexVariables = reader.ReadUntil("void", "#");

                            reader.Search("{");
                            pass.vertex = reader.ReadUntil("}");

                            reader.Search("#ifdef FRAGMENT");
                            pass.fragmentVariables = reader.ReadUntil("void", "#");

                            reader.Search("{");
                            pass.fragment = reader.ReadUntil("}");

                            subShader.passes.Add(pass);
                        }
                        catch
                        {
                            break;
                        }
                    }

                    shader.subShaders.Add(subShader);
                }
                catch
                {
                    break;
                }
            }
            
            try
            {
                shader.fallback = reader.SearchFile("Fallback");
            } catch {}

            return shader;
        }
    }
}