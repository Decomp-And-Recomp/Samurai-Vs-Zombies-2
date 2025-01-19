using System.Collections.Generic;

public class CharmsDatabase : Singleton<CharmsDatabase>
{
	private List<DataBundleRecordHandle<CharmSchema>> mData;

	private Dictionary<string, List<string>> mAllIDs = new Dictionary<string, List<string>>();

	public static string UdamanTableName
	{
		get
		{
			return "Charms";
		}
	}

	public List<string> allIDsForActivePlayMode
	{
		get
		{
			return GetIDsForPlayMode(GetCharmsPlayMode());
		}
	}

	public CharmSchema this[string charmID]
	{
		get
		{
			DataBundleRecordHandle<CharmSchema> dataBundleRecordHandle = mData.Find((DataBundleRecordHandle<CharmSchema> c) => c.Data.id.Equals(charmID));
			return (dataBundleRecordHandle == null) ? null : dataBundleRecordHandle.Data;
		}
	}

	public List<CharmSchema> AllPlayerAvailableCharms
	{
		get
		{
			List<CharmSchema> list = new List<CharmSchema>();
			List<string> list2 = allIDsForActivePlayMode;
			foreach (string item in list2)
			{
				list.Add(this[item]);
			}
			return list;
		}
	}

	public IEnumerable<CharmSchema> AllCharmsForActivePlayMode
	{
		get
		{
			List<CharmSchema> list = new List<CharmSchema>();
			List<string> list2 = allIDsForActivePlayMode;
			foreach (DataBundleRecordHandle<CharmSchema> mDatum in mData)
			{
				if (list2.Contains(mDatum.Data.id))
				{
					list.Add(mDatum.Data);
				}
			}
			return list;
		}
	}

	public CharmsDatabase()
	{
		ResetCachedData();
		CacheSimpleIDList();
	}

	public void ResetCachedData()
	{
		mData = new List<DataBundleRecordHandle<CharmSchema>>();
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return;
		}
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys(typeof(CharmSchema), UdamanTableName))
		{
			DataBundleRecordHandle<CharmSchema> dataBundleRecordHandle = new DataBundleRecordHandle<CharmSchema>(UdamanTableName, item);
			dataBundleRecordHandle.Data.Initialize(UdamanTableName);
			mData.Add(dataBundleRecordHandle);
		}
		CacheSimpleIDList();
	}

	private string GetCharmsPlayMode()
	{
		if (string.IsNullOrEmpty(Singleton<Profile>.Instance.playModeSubSection))
		{
			return "classic";
		}
		return Singleton<Profile>.Instance.playModeSubSection;
	}

	public List<string> GetIDsForPlayMode(string playmode)
	{
		return mAllIDs[playmode];
	}

	public bool Contains(string id)
	{
		foreach (KeyValuePair<string, List<string>> mAllID in mAllIDs)
		{
			foreach (string item in mAllID.Value)
			{
				if (string.Compare(item, id, true) == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void LoadFrontEndData()
	{
		foreach (DataBundleRecordHandle<CharmSchema> mDatum in mData)
		{
			mDatum.Load(DataBundleResourceGroup.FrontEnd, false, null);
		}
	}

	public void LoadInGameData(string id, bool unloadUnused)
	{
		foreach (DataBundleRecordHandle<CharmSchema> mDatum in mData)
		{
			if (string.Equals(id, mDatum.Data.id))
			{
				mDatum.Load(DataBundleResourceGroup.InGame, true, null);
			}
			else if (unloadUnused)
			{
				mDatum.Unload();
			}
		}
	}

	public void UnloadData()
	{
		foreach (DataBundleRecordHandle<CharmSchema> mDatum in mData)
		{
			mDatum.Unload();
		}
	}

	private void CacheSimpleIDList()
	{
		mAllIDs.Clear();
		foreach (DataBundleRecordHandle<CharmSchema> mDatum in mData)
		{
			List<string> value = null;
			if (!mAllIDs.TryGetValue(mDatum.Data.playmode.Key, out value))
			{
				value = new List<string>();
				mAllIDs.Add(mDatum.Data.playmode.Key, value);
			}
			value.Add(mDatum.Data.id);
		}
	}
}
