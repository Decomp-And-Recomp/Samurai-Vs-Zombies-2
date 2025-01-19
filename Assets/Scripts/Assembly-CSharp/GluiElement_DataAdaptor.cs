public abstract class GluiElement_DataAdaptor<T> : GluiElement_Base, IGluiElement_DataAdaptor where T : DataAdaptorBase, new()
{
	public T adaptor = new T();

	public GluiPersistentDataWatcher watcher;

	public override void SetGluiCustomElementData(object data)
	{
		if (data != null)
		{
			adaptor.SetData(data);
		}
		else
		{
			adaptor.SetDefaultData();
		}
	}

	public override void OnSafeEnable()
	{
		ForceUpdate();
	}

	public override void ForceUpdate()
	{
		if (watcher.PersistentEntryToWatch != string.Empty)
		{
			watcher.StartWatching();
			watcher.Event_WatchedDataChanged += HandleWatcherEvent_PersistentDataChanged;
			object data = watcher.GetData();
			SetGluiCustomElementData(data);
		}
	}

	protected virtual void HandleWatcherEvent_PersistentDataChanged(object data)
	{
		SetGluiCustomElementData(data);
	}

	public virtual void OnDisable()
	{
		watcher.StopWatching();
	}
}
