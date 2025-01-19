using UnityEngine;

[AddComponentMenu("Glui Data/Persistent Data Log")]
public class GluiPersistentDataLog : GluiLog_Base
{
	public static GluiPersistentDataLog Instance;

	public void Awake()
	{
		Instance = this;
	}

	public void OnAdded_Internal(GluiPersistentDataCache.PersistentData node)
	{
	}

	public void OnUpdated_Internal(GluiPersistentDataCache.PersistentData node)
	{
	}

	public void OnRemoved_Internal(string nodeName)
	{
	}

	public static void OnAdded(GluiPersistentDataCache.PersistentData node)
	{
		if (Instance != null)
		{
			Instance.OnAdded_Internal(node);
		}
	}

	public static void OnUpdated(GluiPersistentDataCache.PersistentData node)
	{
		if (Instance != null)
		{
			Instance.OnUpdated_Internal(node);
		}
	}

	public static void OnRemoved(string nodeName)
	{
		if (Instance != null)
		{
			Instance.OnRemoved_Internal(nodeName);
		}
	}
}
