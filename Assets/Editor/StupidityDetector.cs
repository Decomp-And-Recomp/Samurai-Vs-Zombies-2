using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class StupidityDetector : Editor
{
	[MenuItem("Stupid/Check for Stupid")]
	public static void Stupid()
	{
		foreach (string file in Directory.GetFiles("Assets/Resources", "*", SearchOption.AllDirectories))
		{
			if (file.EndsWith("asset") && File.Exists(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file.ToLower()) + ".prefab")))
			{
				UnityEngine.Debug.LogError(file);
			}
		}
	}
}