using System.Collections.Generic;

public class PotionsDatabase : Singleton<PotionsDatabase>
{
	private Dictionary<string, DataBundleRecordHandle<PotionSchema>> mData = new Dictionary<string, DataBundleRecordHandle<PotionSchema>>();

	private Dictionary<string, List<string>> mAllIDs = new Dictionary<string, List<string>>();

	public static string UdamanTableName
	{
		get
		{
			return "Potions";
		}
	}

	public List<string> allIDsForActivePlayMode
	{
		get
		{
			return GetIDsForPlayMode(Singleton<PlayModesManager>.Instance.selectedMode);
		}
	}

	public PotionSchema this[string id]
	{
		get
		{
			DataBundleRecordHandle<PotionSchema> value;
			mData.TryGetValue(id, out value);
			return (value == null) ? null : value.Data;
		}
	}

	public IEnumerable<DataBundleRecordHandle<PotionSchema>> AllPotions
	{
		get
		{
			return mData.Values;
		}
	}

	public IEnumerable<PotionSchema> AllPotionsForActivePlayMode
	{
		get
		{
			List<string> list = allIDsForActivePlayMode;
			List<PotionSchema> list2 = new List<PotionSchema>();
			foreach (DataBundleRecordHandle<PotionSchema> value in mData.Values)
			{
				if (list.Contains(value.Data.id))
				{
					list2.Add(value.Data);
				}
			}
			return list2;
		}
	}

	public PotionsDatabase()
	{
		ResetCachedData();
		CacheSimpleIDList();
	}

	public void ResetCachedData()
	{
		mData.Clear();
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return;
		}
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys<PotionSchema>(UdamanTableName))
		{
			DataBundleRecordHandle<PotionSchema> dataBundleRecordHandle = new DataBundleRecordHandle<PotionSchema>(UdamanTableName, item);
			dataBundleRecordHandle.Data.Initialize(UdamanTableName);
			mData.Add(dataBundleRecordHandle.Data.id, dataBundleRecordHandle);
		}
		CacheSimpleIDList();
	}

	public List<string> GetIDsForPlayMode(string playmode)
	{
		List<string> value;
		if (mAllIDs.TryGetValue(playmode, out value))
		{
			return value;
		}
		return null;
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

	public bool Execute(string id)
	{
		bool result = false;
		PotionSchema data = mData[id].Data;
		if (!string.IsNullOrEmpty(data.heal))
		{
			result = Execute_Heal(data.heal);
		}
		else if (data.leadership > 0f)
		{
			result = Execute_Leadership(data.leadership);
		}
		return result;
	}

	public void LoadFrontEndData()
	{
		foreach (DataBundleRecordHandle<PotionSchema> value in mData.Values)
		{
			value.Load(DataBundleResourceGroup.FrontEnd, false, null);
		}
	}

	public void LoadInGameData()
	{
		foreach (DataBundleRecordHandle<PotionSchema> value in mData.Values)
		{
			value.Load(DataBundleResourceGroup.InGame, true, null);
		}
	}

	public void UnloadData()
	{
		foreach (DataBundleRecordHandle<PotionSchema> value in mData.Values)
		{
			value.Unload();
		}
	}

	private void CacheSimpleIDList()
	{
		mAllIDs.Clear();
		foreach (DataBundleRecordHandle<PotionSchema> value2 in mData.Values)
		{
			List<string> value = null;
			if (!mAllIDs.TryGetValue(value2.Data.playmode.Key, out value))
			{
				value = new List<string>();
				mAllIDs.Add(value2.Data.playmode.Key, value);
			}
			value.Add(value2.Data.id);
		}
	}

	private bool Execute_Heal(string data)
	{
		int num = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Sushi");
		if (num > 0)
		{
			data = "all";
		}
		switch (data)
		{
		case "all":
		{
			bool flag = false;
			foreach (Character item in WeakGlobalInstance<CharactersManager>.Instance.allAlive)
			{
				if (item.ownerId == 0 && !item.isBase && (item.health < item.maxHealth || item.mountedHealth < item.mountedHealthMax))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				foreach (Character item2 in WeakGlobalInstance<CharactersManager>.Instance.allAlive)
				{
					if (item2.ownerId == 0 && !item2.isBase)
					{
						if (item2.health < item2.maxHealth)
						{
							item2.RecievedHealing(item2.maxHealth - item2.health);
						}
						if (item2.mountedHealth < item2.mountedHealthMax)
						{
							item2.RecievedHealing(item2.mountedHealthMax - item2.mountedHealth);
						}
					}
				}
				break;
			}
			return false;
		}
		case "hero":
			if (WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.health == WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.maxHealth && WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.mountedHealth >= WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.mountedHealthMax)
			{
				return false;
			}
			WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.RecievedHealing(WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.maxHealth);
			break;
		}
		return true;
	}

	private bool Execute_Leadership(float value)
	{
		if (WeakGlobalInstance<Leadership>.Instance.resources == WeakGlobalInstance<Leadership>.Instance.maxResources)
		{
			return false;
		}
		int num = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Tea");
		if (num > 0)
		{
			WeakGlobalInstance<Leadership>.Instance.resources = WeakGlobalInstance<Leadership>.Instance.maxResources;
		}
		else
		{
			int num2 = (int)value;
			WeakGlobalInstance<Leadership>.Instance.resources += num2;
		}
		return true;
	}
}
