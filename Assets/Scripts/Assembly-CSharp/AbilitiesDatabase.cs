using System.Collections.Generic;
using UnityEngine;

public class AbilitiesDatabase : Singleton<AbilitiesDatabase>
{
	public abstract class AbilityData
	{
		public delegate void OnBuildStoreDataFunc(string id, List<StoreData.Item> items);

		public string id;

		public OnBuildStoreDataFunc OnBuildStoreData;

		public OnAbilityActivateFunc OnActivate;

		public OnAbilityExecuteFunc OnExecute;

		public SDFTreeNode registryData;

		public AbilitySchema schema { get; private set; }

		public DataBundleRecordHandle<AbilitySchema> handle { get; private set; }

		public AbilityData(string abId)
		{
			id = abId;
			handle = new DataBundleRecordHandle<AbilitySchema>(UdamanTableName, abId);
			schema = handle.Data;
			if (schema != null)
			{
				schema.Initialize(UdamanTableName);
			}
		}

		public void Initialize(OnAbilityActivateFunc activateFunc, OnAbilityExecuteFunc execFunc, OnBuildStoreDataFunc storeBuildDataFunc)
		{
			OnActivate = delegate(Character executor)
			{
				activateFunc(executor);
			};
			OnExecute = delegate(Character executor)
			{
				execFunc(executor);
			};
			OnBuildStoreData = storeBuildDataFunc;
		}

		public void Initialize(OnAbilityExecuteFunc execFunc, OnBuildStoreDataFunc storeBuildDataFunc)
		{
			OnExecute = delegate(Character executor)
			{
				execFunc(executor);
			};
			OnBuildStoreData = storeBuildDataFunc;
		}

		public void EnsureDataCached()
		{
			if (registryData == null)
			{
				registryData = SDFTree.LoadFromResources("TextDB/Abilities/" + id);
			}
		}
	}

	private class AbilityDataHandler<T> : AbilityData where T : AbilityHandler, new()
	{
		private T mAbilityHandler = new T();

		public AbilityDataHandler(string strID, OnBuildStoreDataFunc onBuildStoreData)
			: base(strID)
		{
			mAbilityHandler.schema = base.schema;
			Initialize(delegate(Character executor)
			{
				mAbilityHandler.Activate(executor);
			}, delegate(Character executor)
			{
				mAbilityHandler.Execute(executor);
			}, onBuildStoreData);
		}
	}

	private class AbilityDataComponent<T> : AbilityData where T : AbilityHandlerComponent
	{
		public AbilityDataComponent(string strID, OnBuildStoreDataFunc onBuildStoreData)
			: base(strID)
		{
			Initialize(delegate(Character executor)
			{
				AttachAbilityHandler(base.schema, executor);
			}, onBuildStoreData);
		}

		public static void AttachAbilityHandler(AbilitySchema abSchema, Character executor)
		{
			GameObject gameObject = abSchema.prefab;
			if (executor != null && !executor.LeftToRight && abSchema.rightToLeftPrefab != null)
			{
				gameObject = abSchema.rightToLeftPrefab;
			}
			GameObject gameObject2 = ((!(gameObject != null)) ? new GameObject(abSchema.id + "_handler") : GameObjectPool.DefaultObjectPool.Acquire(gameObject));
			T val = gameObject2.GetComponent<T>();
			if ((Object)val == (Object)null)
			{
				val = gameObject2.AddComponent<T>();
			}
			val.Init(abSchema, executor);
		}
	}

	public delegate void OnAbilityActivateFunc(Character executor);

	public delegate void OnAbilityExecuteFunc(Character executor);

	private List<AbilityData> mData;

	private string[] mAllIDs;

	private List<string> mGlobalIDs;

	public static string UdamanTableName
	{
		get
		{
			return "Abilities";
		}
	}

	public string[] allIDs
	{
		get
		{
			return mAllIDs;
		}
	}

	public List<string> GlobalAbilityIDs
	{
		get
		{
			return mGlobalIDs;
		}
	}

	public AbilitySchema[] AllAbilitiesForActiveHero
	{
		get
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID];
			return heroSchema.Abilities;
		}
	}

	public AbilitySchema this[string id]
	{
		get
		{
			AbilityData abilityData = Seek(id);
			if (abilityData != null)
			{
				return abilityData.schema;
			}
			return null;
		}
	}

	public AbilitiesDatabase()
	{
		ResetCachedData();
	}

	public void ResetCachedData()
	{
		mData = new List<AbilityData>();
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			mData.Add(new AbilityDataHandler<KatanaSlashHandler>("KatanaSlash", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<SummonLightningHandler>("SummonLightning", StoreAvailability_Abilities.GetAbilityUpgrade_SummonLightning));
			mData.Add(new AbilityDataComponent<LethargyHandler>("Lethargy", StoreAvailability_Abilities.GetAbilityUpgrade_Lethargy));
			mData.Add(new AbilityDataHandler<DivineInterventionHandler>("DivineIntervention", StoreAvailability_Abilities.GetAbilityUpgrade_DivineIntervention));
			mData.Add(new AbilityDataComponent<SummonTornadoHandler>("SummonTornado", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<TroopTrampleHandler>("GiantWave", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<DivineWindHandler>("DivineWind", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<ThunderStrikeHandler>("ThunderStrike", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<DaggerBarrageHandler>("DaggerBarrage", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<ExplosiveCartHandler>("ExplosiveCart", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<DragonDamageHandler>("DragonDamage", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<FlashBombHandler>("FlashBomb", StoreAvailability_Abilities.GetAbilityUpgrade_FlashBomb));
			mData.Add(new AbilityDataComponent<SoulBurnHandler>("SoulBurn", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<MysticFlameHandler>("MysticFlame", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<RepelHandler>("Repel", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<SetTrapHandler>("SetTrap", StoreAvailability_Abilities.GetAbilityUpgrade_SetTrap));
			mData.Add(new AbilityDataComponent<InspireHandler>("Inspire", StoreAvailability_Abilities.GetAbilityUpgrade_Inspire));
			mData.Add(new AbilityDataComponent<InvincibilityHandler>("Invincibility", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataComponent<DestructionHandler>("Destruction", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<TagTeamHandler>("TagTeam", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<FriendshipHandler>("Friendship", StoreAvailability_Abilities.GetAbilityUpgrade_DamageOnly));
			mData.Add(new AbilityDataHandler<LegendaryStrikeLightningHandler>("LSLightningStrike", null));
			mData.Add(new AbilityDataComponent<LegendaryStrikeArrowHandler>("LSArrowVolley", null));
			mData.Add(new AbilityDataHandler<LegendaryStrikeRaiseDeadHandler>("LSRaiseDead", null));
			mData.Add(new AbilityDataComponent<LegendaryStrikeTornadoHandler>("LSTornado", null));
			CacheSimpleIDList();
			mGlobalIDs = DataBundleRuntime.Instance.GetRecordKeys(typeof(AbilitiesListSchema), "AbilitiesGlobal", false);
		}
	}

	private void OnExecute_GraveHands(Character activator)
	{
	}

	private void OnExecute_GroundShock(Character activator)
	{
	}

	public void GetAbilityInfoByName(string abilityID, out string animName, out OnAbilityActivateFunc activateFunc, out OnAbilityExecuteFunc execFunc)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData.schema != null)
		{
			animName = abilityData.schema.animName;
		}
		else
		{
			animName = string.Empty;
		}
		execFunc = abilityData.OnExecute;
		activateFunc = abilityData.OnActivate;
	}

	public bool Contains(string id)
	{
		string[] array = mAllIDs;
		foreach (string strA in array)
		{
			if (string.Compare(strA, id, true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public string GetAttribute(string abilityID, string attributeName)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData == null)
		{
			return string.Empty;
		}
		abilityData.EnsureDataCached();
		int level = Mathf.Clamp(Singleton<Profile>.Instance.GetAbilityLevel(abilityID), 1, abilityData.registryData.childCount);
		return GetAttribute(abilityID, attributeName, level);
	}

	public string GetNextLevelAttribute(string abilityID, string attributeName)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData == null)
		{
			return string.Empty;
		}
		abilityData.EnsureDataCached();
		int level = Mathf.Clamp(Singleton<Profile>.Instance.GetAbilityLevel(abilityID) + 1, 1, abilityData.registryData.childCount);
		return GetAttribute(abilityID, attributeName, level);
	}

	public T Extrapolate<T>(string abilityID, string infiniteUpgradableAttributeName, string attributeName)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData == null)
		{
			return default(T);
		}
		abilityData.EnsureDataCached();
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		if (infiniteUpgradableAttributeName == string.Empty || !abilityData.registryData.hasAttribute(infiniteUpgradableAttributeName))
		{
			return InfiniteUpgrades.SnapToHighest<T>(abilityData.registryData, attributeName, abilityLevel);
		}
		return InfiniteUpgrades.Extrapolate<T>(abilityData.registryData, infiniteUpgradableAttributeName, attributeName, abilityLevel);
	}

	public int GetMaxLevel(string abilityID)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData != null)
		{
			if (abilityData.schema != null)
			{
				return abilityData.schema.levelData.Length;
			}
			abilityData.EnsureDataCached();
			for (int i = 1; i < 1000; i++)
			{
				if (abilityData.registryData.to(i) == null)
				{
					return i - 1;
				}
			}
		}
		return -1;
	}

	public List<HeroSchema> HeroesUsingAbility(string abilityID)
	{
		string text = abilityID + ": ";
		List<HeroSchema> list = new List<HeroSchema>();
		string[] array = Singleton<HeroesDatabase>.Instance.AllIDs;
		foreach (string text2 in array)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[text2];
			AbilitySchema[] abilities = heroSchema.Abilities;
			foreach (AbilitySchema abilitySchema in abilities)
			{
				if (abilityID == abilitySchema.id)
				{
					list.Add(heroSchema);
					text = text + text2 + ", ";
					break;
				}
			}
		}
		return list;
	}

	private void CacheSimpleIDList()
	{
		mAllIDs = new string[mData.Count];
		mGlobalIDs = new List<string>();
		int num = 0;
		foreach (AbilityData mDatum in mData)
		{
			mAllIDs[num++] = mDatum.id;
		}
	}

	private AbilityData Seek(string abilityID)
	{
		foreach (AbilityData mDatum in mData)
		{
			if (mDatum.id == abilityID)
			{
				return mDatum;
			}
		}
		return null;
	}

	public AbilitySchema GetSchema(string abilityID)
	{
		foreach (AbilityData mDatum in mData)
		{
			if (mDatum.schema != null && mDatum.id == abilityID)
			{
				return mDatum.schema.ShallowCopy();
			}
		}
		return null;
	}

	private string GetAttribute(string abilityID, string attributeName, int level)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData == null)
		{
			return string.Empty;
		}
		abilityData.EnsureDataCached();
		if (abilityData.registryData.hasAttribute(attributeName))
		{
			return Singleton<Localizer>.Instance.Parse(abilityData.registryData[attributeName]);
		}
		SDFTreeNode sDFTreeNode = abilityData.registryData.to(level);
		if (sDFTreeNode != null && sDFTreeNode.hasAttribute(attributeName))
		{
			return Singleton<Localizer>.Instance.Parse(sDFTreeNode[attributeName]);
		}
		return string.Empty;
	}

	public void BuildStoreData(string abilityID, List<StoreData.Item> items)
	{
		AbilityData abilityData = Seek(abilityID);
		if (abilityData != null)
		{
			abilityData.OnBuildStoreData(abilityID, items);
		}
	}

	public void LoadFrontEndData()
	{
		foreach (AbilityData mDatum in mData)
		{
			mDatum.handle.Load(DataBundleResourceGroup.FrontEnd, false, null);
		}
	}

	public void LoadInGameData(List<string> ids, bool unloadNotFound)
	{
		List<string> list = ids;
		if (Singleton<Profile>.Instance.inMultiplayerWave)
		{
			list = new List<string>();
			foreach (string id in ids)
			{
				list.Add(id);
			}
			for (int i = 0; i < 4; i++)
			{
				list.Add(PlayerWaveEventData.LegendaryStrikeID[i]);
			}
			if (Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null)
			{
				foreach (string abilityId in Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList)
				{
					list.Add(abilityId);
				}
			}
		}
		foreach (AbilityData mDatum in mData)
		{
			if (list.Contains(mDatum.handle.Data.id))
			{
				mDatum.handle.Load(DataBundleResourceGroup.InGame, true, null);
			}
			else if (unloadNotFound)
			{
				mDatum.handle.Unload();
			}
		}
	}

	public void UnloadData()
	{
		foreach (AbilityData mDatum in mData)
		{
			mDatum.handle.Unload();
		}
	}
}
