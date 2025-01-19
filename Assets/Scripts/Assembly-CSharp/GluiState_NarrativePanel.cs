using System;
using GripTech;
using UnityEngine;

[AddComponentMenu("Glui State/State Narrative Panel")]
public class GluiState_NarrativePanel : GluiStateBase, IGluiActionHandler
{
	public static string resourceToLoad;

	public bool unloadUnusedAssets;

	protected GameObject statePrefab;

	protected string resourcePath;

	public static bool ShowIfSet()
	{
		if (!string.IsNullOrEmpty(resourceToLoad))
		{
			GluiActionSender.SendGluiAction("ALERT_NARRATIVE_PANEL", null, null);
			return true;
		}
		return false;
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "MENU_TRANSITION_NEXT")
		{
			GluiActionSender.SendGluiAction("ALERT_CLEAR", sender, data);
			return true;
		}
		return false;
	}

	public override void InitState(Action<GameObject> whenDone)
	{
		SingletonMonoBehaviour<InputManager>.Instance.tutorialPopupEnabled = true;
		if (string.IsNullOrEmpty(resourcePath))
		{
			resourcePath = resourceToLoad;
			resourceToLoad = null;
		}
		statePrefab = AttachPrefab(resourcePath, base.gameObject);
		if (statePrefab != null)
		{
			processes.AddStateProcesses(statePrefab);
		}
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

	protected GameObject AttachPrefab(string resource, GameObject parent)
	{
		if (string.IsNullOrEmpty(resource))
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceLoader.Load(resource));
		ApplyTransform(gameObject, base.gameObject);
		return gameObject;
	}
}
