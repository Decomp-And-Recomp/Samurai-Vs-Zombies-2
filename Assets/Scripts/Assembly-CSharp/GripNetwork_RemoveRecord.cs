using System;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_RemoveRecord : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private int mRecord;

	private GameDataTable sakeManager;

	private RequestState createRecordState;

	private Action<GripNetwork.Result, int> mRecordRemoveCallback;

	public void RemoveRecord(string tableID, int recordID, Action<GripNetwork.Result, int> recordRemoveCallback)
	{
		mRecordRemoveCallback = recordRemoveCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, -1);
				return;
			}
			mTableName = tableID;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			mRecord = recordID;
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, -1);
		}
		GripNetwork_RequestManager.Instance.QueueRequest(this);
	}

	private void Update()
	{
		try
		{
			if (createRecordState != RequestState.Complete)
			{
				createRecordState = sakeManager.DeleteRecord(mRecord);
			}
			else if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, -1);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success, mRecord);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, -1);
		}
	}

	private void WhenDone(GripNetwork.Result result, int recordID)
	{
		Action<GripNetwork.Result, int> action = mRecordRemoveCallback;
		mRecordRemoveCallback = null;
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
