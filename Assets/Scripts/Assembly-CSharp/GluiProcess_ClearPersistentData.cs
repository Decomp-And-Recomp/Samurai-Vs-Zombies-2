using UnityEngine;

[AddComponentMenu("Glui Process/Process Clear Persistent Data")]
public class GluiProcess_ClearPersistentData : GluiProcessSingleState
{
	public string[] persistentNames;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		string[] array = persistentNames;
		foreach (string text in array)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove(text);
		}
		return false;
	}
}
