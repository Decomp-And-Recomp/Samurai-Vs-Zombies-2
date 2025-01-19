using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ULegacyPostProcessor : Editor
{
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void PromptProcess()
    {
        if (PlayerPrefs.GetInt("ULEGACY_RIPPER_POST_PROCESSED") == 0)
        {
            if (EditorUtility.DisplayDialog("Post Processor", "Exported project has been fully imported. would you like to run the Post Processor? (HIGHLY RECOMMENDED, ports Lightmaps, Shaders, NavMeshes, ETC)", "Yes", "No"))
            {

            }

            //uncomment once testing is over
            //PlayerPrefs.SetInt("ULEGACY_RIPPER_POST_PROCESSED", 1);
        }
    }
}