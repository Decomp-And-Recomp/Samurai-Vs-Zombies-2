using System;
using UnityEngine;

[AddComponentMenu("Glui State/State Prefab")]
public class GluiState_Prefab : GluiStateBase
{
	public GameObject prefabToSpawn;

	protected GameObject statePrefab;

	public override void InitState(Action<GameObject> whenDone)
	{
		statePrefab = AttachPrefab(prefabToSpawn, base.gameObject);
		processes.AddStateProcesses(statePrefab);
		whenDone(statePrefab);
	}

	public override void DestroyState()
	{
		if (statePrefab != null)
		{
			UnityEngine.Object.DestroyImmediate(statePrefab);
		}
		processes.Clear();
	}

	protected GameObject AttachPrefab(GameObject prefab, GameObject parent)
	{
		if (prefab == null)
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
		ApplyTransform(gameObject, base.gameObject);
		return gameObject;
	}
}
