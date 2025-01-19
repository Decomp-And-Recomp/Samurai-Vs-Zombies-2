using UnityEngine;

[AddComponentMenu("Glui Process/Process WaitForPersistentData")]
public class GluiProcess_WaitForPersistentData : GluiProcessSingleState
{
	public string valueToWaitFor;

	public GluiPersistentDataWatcher persistentDataToWatchFor;

	public string actionOnMatchingAlready;

	public string actionOnWaitForMatch;

	public string actionOnMatchingAfterWait;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		return StartWatching();
	}

	public override void ProcessInterrupt()
	{
		StopWatching();
		base.ProcessInterrupt();
	}

	private bool StartWatching()
	{
		if (persistentDataToWatchFor.PersistentEntryToWatch != string.Empty)
		{
			if (!CompareData(SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(persistentDataToWatchFor.PersistentEntryToWatch)))
			{
				GluiActionSender.SendGluiAction(actionOnWaitForMatch, base.gameObject, null);
				persistentDataToWatchFor.StartWatching();
				persistentDataToWatchFor.Event_WatchedDataChanged += Done;
				return true;
			}
			GluiActionSender.SendGluiAction(actionOnMatchingAlready, base.gameObject, null);
			return false;
		}
		return false;
	}

	private void StopWatching()
	{
		persistentDataToWatchFor.Event_WatchedDataChanged -= Done;
		persistentDataToWatchFor.StopWatching();
	}

	private bool CompareData(object data)
	{
		return (string)data == valueToWaitFor;
	}

	private void Done(object data)
	{
		if (CompareData(data))
		{
			GluiActionSender.SendGluiAction(actionOnMatchingAfterWait, base.gameObject, null);
			StopWatching();
			ProcessDone();
		}
	}
}
