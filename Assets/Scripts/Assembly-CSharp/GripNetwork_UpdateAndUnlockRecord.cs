using System;
using System.Collections.Generic;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_UpdateAndUnlockRecord : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private Record mRecord;

	private int mRecordID;

	private GameDataTable sakeManager;

	private RequestState updateRecordState;

	private Action<GripNetwork.Result> mRecordUpdateCallback;

	public void UpdateAndUnlockRecord(string tableID, int recordID, GripField[] fields, Action<GripNetwork.Result> recordUpdateCallback)
	{
		mRecordUpdateCallback = recordUpdateCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed);
				return;
			}
			mTableName = tableID;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			mRecordID = recordID;
			List<Field> list = new List<Field>();
			foreach (GripField gripField in fields)
			{
				Field field = new Field();
				GripField.GripFieldToSakeField(gripField, field);
				list.Add(field);
			}
			mRecord = new Record(list);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed);
		}
		GripNetwork_RequestManager.Instance.QueueRequest(this);
	}

	public void UnlockRecord(string tableID, int recordID, Action<GripNetwork.Result> recordUpdateCallback)
	{
		mRecordUpdateCallback = recordUpdateCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed);
				return;
			}
			mTableName = tableID;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			mRecordID = recordID;
			List<Field> list = new List<Field>();
			list.Clear();
			mRecord = new Record(list);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed);
		}
		GripNetwork_RequestManager.Instance.QueueRequest(this);
	}

	private void Update()
	{
		try
		{
			if (updateRecordState != RequestState.Complete)
			{
				updateRecordState = sakeManager.UpdateAndUnlockRecord(mRecordID, mRecord);
			}
			else if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed);
		}
	}

	private void WhenDone(GripNetwork.Result result)
	{
		Action<GripNetwork.Result> action = mRecordUpdateCallback;
		mRecordUpdateCallback = null;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(result);
		}
		ObjectUtils.DestroyImmediate(base.transform.gameObject);
	}
}
