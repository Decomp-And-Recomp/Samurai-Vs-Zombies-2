using System;
using System.Collections.Generic;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_CreateRecord : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private Record mRecord;

	private GameDataTable sakeManager;

	private RequestState createRecordState;

	private Action<GripNetwork.Result, int> mRecordCreateCallback;

	public void CreateRecord(string tableID, GripField[] fields, Action<GripNetwork.Result, int> recordCreateCallback)
	{
		mRecordCreateCallback = recordCreateCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			GripField[] array = fields ?? new GripField[0];
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, -1);
				return;
			}
			mTableName = tableID;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			List<Field> list = new List<Field>();
			GripField[] array2 = array;
			foreach (GripField gripField in array2)
			{
				Field field = new Field();
				GripField.GripFieldToSakeField(gripField, field);
				list.Add(field);
			}
			mRecord = new Record(list);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, -1);
		}
		Update();
	}

	private void Update()
	{
		try
		{
			if (createRecordState != RequestState.Complete)
			{
				createRecordState = sakeManager.CreateRecord(mRecord);
			}
			else if (sakeManager.Result == SakeRequestResult.RecordLimitReached)
			{
				WhenDone(GripNetwork.Result.RecordLimitReached, -1);
			}
			else if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, 0);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success, sakeManager.CreateRecord_RecordId);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, -1);
		}
	}

	private void WhenDone(GripNetwork.Result result, int recordID)
	{
		Action<GripNetwork.Result, int> action = mRecordCreateCallback;
		mRecordCreateCallback = null;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(result, recordID);
		}
		ObjectUtils.DestroyImmediate(base.transform.gameObject);
	}
}
