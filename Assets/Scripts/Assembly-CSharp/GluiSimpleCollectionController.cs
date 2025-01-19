using GluiCollections;
using UnityEngine;

public class GluiSimpleCollectionController : Controller
{
	public string cardPath;

	public GameObject dataSource;

	public string dataKey;

	protected object[] mData;

	public override int dataCount
	{
		get
		{
			if (mData != null)
			{
				return mData.Length;
			}
			return 0;
		}
	}

	public object[] data
	{
		get
		{
			if (mData == null)
			{
				ReloadData(null);
			}
			return mData;
		}
	}

	public override string GetCellPrefabForDataIndex(int dataIndex)
	{
		return cardPath;
	}

	public override void ReloadData(object arg)
	{
		IGluiDataSource gluiDataSource = null;
		if (dataSource != null)
		{
			gluiDataSource = (IGluiDataSource)dataSource.GetComponent(typeof(IGluiDataSource));
		}
		if (gluiDataSource == null)
		{
			gluiDataSource = (IGluiDataSource)base.gameObject.GetComponent(typeof(IGluiDataSource));
		}
		if (gluiDataSource != null)
		{
			string dataFilterKey = null;
			if (!string.IsNullOrEmpty(dataKey))
			{
				dataFilterKey = dataKey;
			}
			else if (arg is string)
			{
				dataFilterKey = (string)arg;
			}
			gluiDataSource.Get_GluiData(dataFilterKey, null, null, out mData);
		}
		else
		{
			mData = new object[0];
		}
	}

	public override object GetDataAtIndex(int index)
	{
		return mData[index];
	}
}
