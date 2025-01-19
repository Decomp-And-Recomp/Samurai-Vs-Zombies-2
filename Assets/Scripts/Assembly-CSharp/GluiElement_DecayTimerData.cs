using UnityEngine;

[AddComponentMenu("Glui Data/GluiElement DecayTimerData")]
public class GluiElement_DecayTimerData : GluiElement_DataAdaptor<DataAdaptor_DecayTimerData>
{
	public bool updateInterval;

	public float updateIntervalSeconds;

	public override void OnSafeEnable()
	{
		Draw();
		if (updateInterval)
		{
			InvokeRepeating("Draw", updateIntervalSeconds, updateIntervalSeconds);
		}
	}

	public override void OnDisable()
	{
		CancelInvoke();
		base.OnDisable();
	}

	public void Draw()
	{
		if (SingletonSpawningMonoBehaviour<DecaySystem>.Exists)
		{
			adaptor.SetData(SingletonSpawningMonoBehaviour<DecaySystem>.Instance.Find(watcher.PersistentEntryToWatch));
		}
	}
}
