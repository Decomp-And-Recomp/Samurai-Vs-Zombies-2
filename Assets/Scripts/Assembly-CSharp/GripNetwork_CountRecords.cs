using System;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_CountRecords : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private string mSqlStyleFilter;

	private GameDataTable sakeManager;

	private RequestState searchRecordsState;

	private Action<GripNetwork.Result, int> mCountRecordCallback;

	public void CountRecords(string tableName, string sqlStyleFilter, Action<GripNetwork.Result, int> countRecordCallback)
	{
		mCountRecordCallback = countRecordCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, 0);
				return;
			}
			mTableName = tableName;
			sakeManager = new GameDataTable(GripNetwork.GameSpyAccountManager.SecurityToken, mTableName);
			mSqlStyleFilter = sqlStyleFilter;
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, 0);
		}
	}

	private void Update()
	{
		try
		{
			if (searchRecordsState != RequestState.Complete)
			{
				searchRecordsState = sakeManager.GetRecordCount(mSqlStyleFilter);
			}
			else if (sakeManager.Result == SakeRequestResult.RecordNotFound || (sakeManager.Result == SakeRequestResult.Success && sakeManager.GetRecordCount_Count == 0))
			{
				WhenDone(GripNetwork.Result.RecordNotFound, 0);
			}
			else if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, 0);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success, sakeManager.GetRecordCount_Count);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, 0);
		}
	}

	private void WhenDone(GripNetwork.Result result, int count)
	{
		Action<GripNetwork.Result, int> action = mCountRecordCallback;
		mCountRecordCallback = null;
		switch (result)
		{
		}
		if (action != null)
		{
			action(result, count);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
