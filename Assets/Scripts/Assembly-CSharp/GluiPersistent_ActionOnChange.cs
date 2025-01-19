using System;
using UnityEngine;

[AddComponentMenu("Glui Data/Persistent Adapter SendAction")]
public class GluiPersistent_ActionOnChange : SafeEnable_Monobehaviour
{
	[Serializable]
	public class LookupData
	{
		public string persistentValueToMatch;

		public string[] actions;
	}

	public LookupData[] lookupData;

	public GluiPersistentDataWatcher watcher;

	private string lastValue = string.Empty;

	public bool ignoreFirstValue = true;

	public bool ignoreSameValue = true;

	private string GetPersistentValue()
	{
		return (string)watcher.GetData();
	}

	public override void OnSafeEnable()
	{
		watcher.StartWatching();
		watcher.Event_WatchedDataChanged += HandleWatcherEvent_WatchedDataChanged;
		if (ignoreFirstValue)
		{
			lastValue = (string)watcher.GetData();
		}
		else
		{
			UpdateOnDataChange();
		}
	}

	private void HandleWatcherEvent_WatchedDataChanged(object data)
	{
		UpdateOnDataChange();
	}

	private void OnDisable()
	{
		watcher.StopWatching();
	}

	private void UpdateOnDataChange()
	{
		string text = (string)watcher.GetData();
		if (!ignoreSameValue || !(lastValue == text))
		{
			lastValue = text;
			LookupData data = FindDataSet(text);
			DoAction(data);
		}
	}

	private LookupData FindDataSet(string lookupValue)
	{
		if (lookupValue == string.Empty)
		{
			return null;
		}
		LookupData[] array = this.lookupData;
		foreach (LookupData lookupData in array)
		{
			if (lookupData.persistentValueToMatch == lookupValue)
			{
				return lookupData;
			}
		}
		return null;
	}

	private void DoAction(LookupData data)
	{
		if (data != null)
		{
			string[] actions = data.actions;
			foreach (string action in actions)
			{
				GluiActionSender.SendGluiAction(action, base.gameObject, null);
			}
		}
	}
}
