using System;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_ReadAndLockRecord : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private int mOwnerId;

	private GameDataTable sakeManager;

	private RequestState readAndLockRecordState;

	private Action<GripNetwork.Result, GripField[]> mReadAndLockFinishedCallback;

	public void ReadAndLockRecord(string tableName, int ownerId, Action<GripNetwork.Result, GripField[]> readAndLockFinishedCallback)
	{
		mReadAndLockFinishedCallback = readAndLockFinishedCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, null);
				return;
			}
			mTableName = tableName;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			mOwnerId = ownerId;
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void Update()
	{
		try
		{
			if (readAndLockRecordState != RequestState.Complete)
			{
				readAndLockRecordState = sakeManager.ReadAndLockRecord(mOwnerId);
				return;
			}
			if (sakeManager.Result == SakeRequestResult.RecordNotFound)
			{
				WhenDone(GripNetwork.Result.RecordNotFound, null);
				return;
			}
			if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, null);
				return;
			}
			Record readAndLock_Record = sakeManager.ReadAndLock_Record;
			int count = readAndLock_Record.Fields.Count;
			GripField[] array = new GripField[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new GripField();
				GripField.SakeFieldToGripField(readAndLock_Record.Fields[i], array[i]);
			}
			WhenDone(GripNetwork.Result.Success, array);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void WhenDone(GripNetwork.Result result, GripField[] fields)
	{
		Action<GripNetwork.Result, GripField[]> action = mReadAndLockFinishedCallback;
		mReadAndLockFinishedCallback = null;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(result, fields);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
