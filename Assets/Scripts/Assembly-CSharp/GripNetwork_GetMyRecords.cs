using System;
using System.Collections.Generic;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_GetMyRecords : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private string[] mFieldNames;

	private GameDataTable sakeManager;

	private RequestState searchRecordsState;

	private Action<GripNetwork.Result, GripField[,]> mSearchRecordCallback;

	public void GetMyRecords(string tableName, string[] fieldNames, Action<GripNetwork.Result, GripField[,]> searchUpdateCallback)
	{
		mSearchRecordCallback = searchUpdateCallback;
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
			mFieldNames = fieldNames;
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
			if (searchRecordsState != RequestState.Complete)
			{
				searchRecordsState = sakeManager.GetMyRecords(mFieldNames);
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
			List<Record> getMyRecords_Records = sakeManager.GetMyRecords_Records;
			int count = getMyRecords_Records.Count;
			int num = ((count > 0) ? getMyRecords_Records[0].Fields.Count : 0);
			GripField[,] array = new GripField[count, num];
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < num; j++)
				{
					array[i, j] = new GripField();
					GripField.SakeFieldToGripField(getMyRecords_Records[i].Fields[j], array[i, j]);
				}
			}
			WhenDone(GripNetwork.Result.Success, array);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void WhenDone(GripNetwork.Result result, GripField[,] fields)
	{
		Action<GripNetwork.Result, GripField[,]> action = mSearchRecordCallback;
		mSearchRecordCallback = null;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(result, fields);
		}
		ObjectUtils.DestroyImmediate(base.transform.gameObject);
	}
}
