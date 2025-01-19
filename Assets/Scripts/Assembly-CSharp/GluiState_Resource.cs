using System;
using UnityEngine;

[AddComponentMenu("Glui State/State Resource")]
public class GluiState_Resource : GluiStateBase
{
	public string resourceToLoad;

	public bool unloadUnusedAssets;

	protected GameObject statePrefab;

	public override void InitState(Action<GameObject> whenDone)
	{
		if (base.gameObject.name.Equals("Tutorial_MP01_CollectionsA") || base.gameObject.name.Equals("Tutorial_MP02_CollectionsB") || base.gameObject.name.Equals("Tutorial_MP03_SoulJar") || base.gameObject.name.Equals("Tutorial_Tap_Enemy"))
		{
			SingletonMonoBehaviour<InputManager>.Instance.tutorialPopupEnabled = true;
		}
		statePrefab = AttachPrefab(resourceToLoad, base.gameObject);
		processes.AddStateProcesses(statePrefab);
		whenDone(statePrefab);
	}

	public override void DestroyState()
	{
		SingletonMonoBehaviour<InputManager>.Instance.tutorialPopupEnabled = false;
		if (statePrefab != null)
		{
			UnityEngine.Object.DestroyImmediate(statePrefab);
			statePrefab = null;
		}
		processes.Clear();
	}

	protected GameObject AttachPrefab(string resourceToLoad, GameObject parent)
	{
		if (resourceToLoad == string.Empty)
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(resourceToLoad));
		ApplyTransform(gameObject, base.gameObject);
		return gameObject;
	}
}
