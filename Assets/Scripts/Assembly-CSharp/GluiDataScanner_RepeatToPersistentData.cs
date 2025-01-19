using UnityEngine;

[AddComponentMenu("Glui Data/Data Scanner Repeat To Persistent Data")]
public class GluiDataScanner_RepeatToPersistentData : GluiDataScanner_PersistentKeys
{
	public enum ScannerState
	{
		None = 0,
		NoItems = 1,
		HasItems = 2
	}

	public bool updateInterval;

	public float updateIntervalSeconds;

	public string persistentDataToSetToFirstRecord;

	public bool clearPersistentDataOnDisable = true;

	public string actionOnChangeToNoItems;

	public string actionOnChangeToItems;

	private ScannerState scannerState;

	public override void OnSafeEnable()
	{
		base.OnSafeEnable();
		if (updateInterval && updateIntervalSeconds > 0f)
		{
			InvokeRepeating("UpdateFromData", updateIntervalSeconds, updateIntervalSeconds);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (clearPersistentDataOnDisable && persistentDataToSetToFirstRecord != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove(persistentDataToSetToFirstRecord);
		}
	}

	protected override void UpdateFromData()
	{
		object[] data = base.Data;
		if (data.Length == 0)
		{
			SetScannerState(ScannerState.NoItems);
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistentDataToSetToFirstRecord, null);
		}
		else
		{
			SetScannerState(ScannerState.HasItems);
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistentDataToSetToFirstRecord, data[0]);
		}
	}

	private void SetScannerState(ScannerState newState)
	{
		if (newState != scannerState)
		{
			scannerState = newState;
			switch (newState)
			{
			case ScannerState.NoItems:
				GluiActionSender.SendGluiAction(actionOnChangeToNoItems, base.gameObject, null);
				break;
			case ScannerState.HasItems:
				GluiActionSender.SendGluiAction(actionOnChangeToItems, base.gameObject, null);
				break;
			}
		}
	}
}
