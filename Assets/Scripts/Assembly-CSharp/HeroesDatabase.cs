using System;
using System.Collections.Generic;

public class HeroesDatabase : Singleton<HeroesDatabase>
{
	private List<DataBundleRecordHandle<HeroSchema>> mData = new List<DataBundleRecordHandle<HeroSchema>>();

	private string[] mAllIDs;

	public static string UdamanTableName
	{
		get
		{
			return "Heroes";
		}
	}

	public string[] AllIDs
	{
		get
		{
			return mAllIDs;
		}
	}

	public List<DataBundleRecordHandle<HeroSchema>> AllHeroes
	{
		get
		{
			return mData;
		}
	}

	public HeroSchema this[string id]
	{
		get
		{
			DataBundleRecordHandle<HeroSchema> dataBundleRecordHandle = mData.Find((DataBundleRecordHandle<HeroSchema> d) => d.Data.id.Equals(id, StringComparison.OrdinalIgnoreCase));
			return (dataBundleRecordHandle == null) ? null : dataBundleRecordHandle.Data;
		}
	}

	public HeroesDatabase()
	{
		ResetCachedData();
	}

	public void ResetCachedData()
	{
		mData.Clear();
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return;
		}
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys<HeroSchema>(UdamanTableName))
		{
			DataBundleRecordHandle<HeroSchema> dataBundleRecordHandle = new DataBundleRecordHandle<HeroSchema>(UdamanTableName, item);
			dataBundleRecordHandle.Data.Initialize(UdamanTableName);
			mData.Add(dataBundleRecordHandle);
		}
		CacheSimpleIDList();
	}

	public bool Contains(string id)
	{
		return Array.Find(mAllIDs, (string s) => string.Compare(s, id, true) == 0) != null;
	}

	public int GetMaxLevel(string heroID)
	{
		HeroSchema heroSchema = this[heroID];
		if (heroSchema != null)
		{
			return heroSchema.Levels.Length;
		}
		return -1;
	}

	public void LoadFrontEndData()
	{
		foreach (DataBundleRecordHandle<HeroSchema> mDatum in mData)
		{
			mDatum.Load(DataBundleResourceGroup.FrontEnd, false, delegate(HeroSchema s)
			{
				int meleeWeaponLevel = Singleton<Profile>.Instance.GetMeleeWeaponLevel(s.id);
				int rangedWeaponLevel = Singleton<Profile>.Instance.GetRangedWeaponLevel(s.id);
				int armorLevel = Singleton<Profile>.Instance.GetArmorLevel(s.id);
				s.LoadCachedResources(meleeWeaponLevel, rangedWeaponLevel, armorLevel, true);
			});
		}
	}

	public void LoadInGameData(string id, int ownerId)
	{
		DataBundleResourceGroup groupToLoad = ((!WeakGlobalMonoBehavior<InGameImpl>.Exists) ? DataBundleResourceGroup.Preview : DataBundleResourceGroup.InGame);
		foreach (DataBundleRecordHandle<HeroSchema> mDatum in mData)
		{
			if (string.Equals(id, mDatum.Data.id))
			{
				int swordLevel = ((ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.meleeLevel : Singleton<Profile>.Instance.GetMeleeWeaponLevel(mDatum.Data.id));
				int bowLevel = ((ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowLevel : Singleton<Profile>.Instance.GetRangedWeaponLevel(mDatum.Data.id));
				int armorLevel = ((ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.armorLevel : Singleton<Profile>.Instance.GetArmorLevel(mDatum.Data.id));
				mDatum.Load(groupToLoad, true, delegate(HeroSchema s)
				{
					s.LoadCachedResources(swordLevel, bowLevel, armorLevel, false);
				});
			}
		}
	}

	public void UnloadData()
	{
		foreach (DataBundleRecordHandle<HeroSchema> mDatum in mData)
		{
			CharacterSchema characterSchema = CharacterSchema.Initialize(mDatum.Data.resources);
			if (characterSchema != null)
			{
				SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.UnloadSoundTheme(characterSchema.soundTheme);
			}
			int meleeWeaponLevel = Singleton<Profile>.Instance.GetMeleeWeaponLevel(mDatum.Data.id);
			int rangedWeaponLevel = Singleton<Profile>.Instance.GetRangedWeaponLevel(mDatum.Data.id);
			int armorLevel = Singleton<Profile>.Instance.GetArmorLevel(mDatum.Data.id);
			mDatum.Data.UnloadCachedResources(meleeWeaponLevel, rangedWeaponLevel, armorLevel);
			mDatum.Unload();
		}
	}

	private void CacheSimpleIDList()
	{
		mAllIDs = new string[mData.Count];
		int num = 0;
		foreach (DataBundleRecordHandle<HeroSchema> mDatum in mData)
		{
			mAllIDs[num++] = mDatum.Data.id;
		}
	}
}
