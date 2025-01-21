using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

namespace ULegacyRipper
{
    public class ULegacyPostProcessor : Editor
    {
        //[UnityEditor.Callbacks.DidReloadScripts]
        private static void PromptProcess()
        {
            if (PlayerPrefs.GetInt("ULEGACY_RIPPER_POST_PROCESSED") == 0)
            {
                if (EditorUtility.DisplayDialog("Post Processor", "Exported project has been fully imported. would you like to run the Post Processor? (HIGHLY RECOMMENDED, ports Lightmaps, Shaders, NavMeshes, Meshes, ETC)", "Yes", "No"))
                {
                    ULegacyUtils.TryMethod(PostProcess, "Post processing");
                    EditorUtility.ClearProgressBar();

                    AssetDatabase.Refresh();
                }

                //uncomment once testing is over
                //PlayerPrefs.SetInt("ULEGACY_RIPPER_POST_PROCESSED", 1);
            }
        }

        private static void PostProcess()
        {
            ULegacyUtils.TryMethod(TranslateShaders, "Shader translation");
            ULegacyUtils.TryMethod(GenerateLightmapData, "Lightmap generation");
            ULegacyUtils.TryMethod(RebuildNavMeshes, "NavMesh reconstruction");
            ULegacyUtils.TryMethod(ConvertMeshes, "Mesh conversion");
        }

        private static void TranslateShaders()
        {
            //handled in ULegacyShaderTranslator (see class below)
            //quite complicated as to avoid any and all compiler errors
            //surface shaders are very limited, for proper support we require a code extraction tool from all the surface generated code.

            string[] shaders = ULegacyUtils.GetAllAssets("t:Shader");

            for (int i = 0; i < shaders.Length; i++)
            {
                EditorUtility.DisplayProgressBar("ULegacy Post Processor", "Translating " + Path.GetFileName(shaders[i]), i == 0 ? 0 : (i / (float)shaders.Length));
                ULegacyShaderTranslator.TranslateShader(shaders[i]);
            }
        }

        private static void GenerateLightmapData()
        {
            //generate 2017.4.40f1's LightMapData asset based on all scenes containing any lightmap textures
            //this requires recreation of the editor write function in the post processor
            //finally point the scene to the generated lightmap
        }

        private static void RebuildNavMeshes()
        {
            //extract vertices/indices from individual areas in the old navmesh meshdata
            //place meshes onto scene, generate navmesh settings, mark everything as non-static, begin baking
            //revert all changes, point scene to navmeshdata
        }

        private static void ConvertMeshes()
        {
            //extremely complicated, requires project-wide asset searching for consistent patterns.
            //deduplication will also be ran

            //if the mesh is consistently used as a single object it will be exported as an obj. the obj's default material will be whatever the most commonly chosen material is.

            //if the mesh is a completely consistent "scene" (same objects, same materials, same positions) it will be exported as an fbx and all instances will be replaced with the fbx
            //depending on a threshold of differentials between scenes and number of times used, they will either be exported as a separate fbx or marked as a modified version of a base fbx
        }
    }

    public static class ULegacyUtils
    {
        public static void TryMethod(Action method, string function)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                if (!EditorUtility.DisplayDialog("Post Processing Error", "An error has occured during " + function + " Continue post processing?\nERROR:\n" + e.ToString(), "Continue", "Cancel"))
                {
                    throw new Exception("Post Processing Cancelled");
                }
            }
        }

        public static string[] GetAllAssets(string filter)
        {
            return AssetDatabase.FindAssets(filter).Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
        }
    }
}