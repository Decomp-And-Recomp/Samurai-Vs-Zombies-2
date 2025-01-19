using UnityEngine;

[AddComponentMenu("Glui Process/Process Set Persistent Data - Conditional")]
public class GluiProcess_SetPersistentData_Conditional : GluiProcessSingleState
{
	public string persistentName;

	public string persistentValueIfNull;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		object data = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(persistentName);
		if (data == null)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistentName, persistentValueIfNull);
		}
		return false;
	}
}
