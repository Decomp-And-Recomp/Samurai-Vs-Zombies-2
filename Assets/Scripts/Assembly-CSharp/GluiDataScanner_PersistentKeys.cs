public abstract class GluiDataScanner_PersistentKeys : GluiDataScanner
{
	private GluiPersistentDataWatcher watcher = new GluiPersistentDataWatcher();

	private GluiPersistentDataWatcher watcherSecondary = new GluiPersistentDataWatcher();

	public string PersistentData_Source_For_Key;

	public string PersistentData_Source_For_Key_Secondary;

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

	public override void OnSafeEnable()
	{
		StartWatcher(watcher, PersistentData_Source_For_Key);
		StartWatcher(watcherSecondary, PersistentData_Source_For_Key_Secondary);
		base.OnSafeEnable();
	}

	public virtual void OnDisable()
	{
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
