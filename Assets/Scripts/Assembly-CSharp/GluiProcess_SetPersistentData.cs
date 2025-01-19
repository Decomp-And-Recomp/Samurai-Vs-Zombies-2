using UnityEngine;

[AddComponentMenu("Glui Process/Process Set Persistent Data")]
public class GluiProcess_SetPersistentData : GluiProcessSingleState
{
	public string persistentName;

	public string persistentValue;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistentName, persistentValue);
		return false;
	}
}
