using UnityEngine;

[AddComponentMenu("Glui Action/Listener - SetPersistent")]
public class GluiActionListener_SetPersistentData : GluiListenerSingle
{
	public string PersistentName;

	public string PersistentValue;

	protected override void OnTrigger(GameObject sender, object data)
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(PersistentName, PersistentValue);
	}
}
