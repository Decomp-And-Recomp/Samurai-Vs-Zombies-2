using System;
using System.Collections.Generic;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_SearchRecords : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mTableName;

	private string[] mFieldNames;

	private string mSqlStyleFilter;

	private string mSqlStyleSort;

	private int[] mProfileIds;

	private int mNumRecordsPerPage;

	private int mPageNumber;

	private GameDataTable sakeManager;

	private RequestState searchRecordsState;

	private Action<GripNetwork.Result, GripField[,]> mSearchRecordCallback;

	public void SearchRecords(string tableName, string[] fieldNames, string sqlStyleFilter, string sqlStyleSort, int[] profileIds, int numRecordsPerPage, int pageNumber, Action<GripNetwork.Result, GripField[,]> searchUpdateCallback)
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
			mSqlStyleFilter = sqlStyleFilter;
			mSqlStyleSort = sqlStyleSort;
			mProfileIds = profileIds;
			mNumRecordsPerPage = numRecordsPerPage;
			mPageNumber = pageNumber;
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
				searchRecordsState = sakeManager.Search(mFieldNames, mSqlStyleFilter, mSqlStyleSort, mProfileIds, mNumRecordsPerPage, mPageNumber);
				return;
			}
			if (sakeManager.Result == SakeRequestResult.RecordNotFound || (sakeManager.Result == SakeRequestResult.Success && (sakeManager.Search_Records == null || sakeManager.Search_Records.Count == 0)))
			{
				WhenDone(GripNetwork.Result.RecordNotFound, null);
				return;
			}
			if (sakeManager.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, null);
				return;
			}
			List<Record> search_Records = sakeManager.Search_Records;
			int count = search_Records.Count;
			int num = ((count > 0) ? search_Records[0].Fields.Count : 0);
			GripField[,] array = new GripField[count, num];
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < num; j++)
				{
					array[i, j] = new GripField();
					GripField.SakeFieldToGripField(search_Records[i].Fields[j], array[i, j]);
				}
			}
			WhenDone(GripNetwork.Result.Success, array);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void WhenDone(GripNetwork.Result result, GripField[,] gripFields)
	{
		AJavaTools.UI.StopIndeterminateProgress();
		Action<GripNetwork.Result, GripField[,]> action = mSearchRecordCallback;
		mSearchRecordCallback = null;
		switch (result)
		{
		}
		if (action != null)
		{
			action(result, gripFields);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
