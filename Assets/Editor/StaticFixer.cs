using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class StaticFixer : Editor
{
	[MenuItem("Static Fixes/Fix Static Objects")]
	public static void Fix()
	{
		foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
		{
			MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

			if (renderer != null)
			{
				gameObject.isStatic = renderer.isPartOfStaticBatch;
				continue;
			}

			gameObject.isStatic = false;
		}

		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}
}
