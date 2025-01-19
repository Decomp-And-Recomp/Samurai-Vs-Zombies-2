using System;
using System.Runtime.CompilerServices;

[Serializable]
public class GluiPersistentDataWatcher
{
	public delegate void WatchedDataChangedHandler(object data);

	public string PersistentEntryToWatch;

	private bool watching;

	private bool saving;

	[method: MethodImpl(32)]
	public event WatchedDataChangedHandler Event_WatchedDataChanged;

	public void StartWatching()
	{
		if (PersistentEntryToWatch != string.Empty && !watching)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.DataChanged += HandleGluiPersistentDataCacheInstanceDataChanged;
			watching = true;
		}
	}

	public void StopWatching()
	{
		if (PersistentEntryToWatch != string.Empty && watching)
		{
			GluiPersistentDataCache instance = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance;
			if (instance != null)
			{
				instance.DataChanged -= HandleGluiPersistentDataCacheInstanceDataChanged;
			}
			watching = false;
		}
	}

	private void HandleGluiPersistentDataCacheInstanceDataChanged(GluiPersistentDataCache.PersistentData PersistentData)
	{
		if (!saving && PersistentEntryToWatch == PersistentData.name)
		{
			OnWatchedDataChanged(PersistentData.tag);
		}
	}

	public object GetData()
	{
		return SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(PersistentEntryToWatch);
	}

	public void Save(object data)
	{
		saving = true;
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(PersistentEntryToWatch, data);
		saving = false;
	}

	protected void OnWatchedDataChanged(object data)
	{
		if (this.Event_WatchedDataChanged != null)
		{
			this.Event_WatchedDataChanged(data);
		}
	}
}
