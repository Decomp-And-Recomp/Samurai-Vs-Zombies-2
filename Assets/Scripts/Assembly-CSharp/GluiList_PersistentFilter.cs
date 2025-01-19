using UnityEngine;

[AddComponentMenu("Glui List/List From Persistent Filter")]
public class GluiList_PersistentFilter : GluiList_Base
{
	private GluiPersistentDataWatcher watcher = new GluiPersistentDataWatcher();

	private GluiPersistentDataWatcher watcherSecondary = new GluiPersistentDataWatcher();

	public string PersistentData_Source_For_Key;

	public string PersistentData_Source_For_Key_Secondary;

	public string PersistentData_Save_Quantity;

	public override string DataFilterKey
	{
		get
		{
			string text = (string)watcher.GetData();
			if (text != null)
			{
				return text;
			}
			return dataFilterKey;
		}
	}

	public override string DataFilterKeySecondary
	{
		get
		{
			string text = (string)watcherSecondary.GetData();
			if (text != null)
			{
				return text;
			}
			return dataFilterKeySecondary;
		}
	}

	protected override void PreCreateListObjects(object[] data)
	{
		if (PersistentData_Save_Quantity != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(PersistentData_Save_Quantity, data.Length);
		}
	}

	public override void OnSafeEnable()
	{
		StartWatcher(watcher, PersistentData_Source_For_Key);
		StartWatcher(watcherSecondary, PersistentData_Source_For_Key_Secondary);
		base.OnSafeEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		StopWatcher(watcher);
		StopWatcher(watcherSecondary);
	}

	private void StartWatcher(GluiPersistentDataWatcher watcher, string persistentDataName)
	{
		watcher.PersistentEntryToWatch = persistentDataName;
		watcher.StartWatching();
		watcher.Event_WatchedDataChanged += HandleWatcherEvent_WatchedDataChanged;
	}

	private void StopWatcher(GluiPersistentDataWatcher watcher)
	{
		watcher.StopWatching();
		watcher.Event_WatchedDataChanged -= HandleWatcherEvent_WatchedDataChanged;
	}

	private void HandleWatcherEvent_WatchedDataChanged(object data)
	{
		UpdateFromData();
	}
}
