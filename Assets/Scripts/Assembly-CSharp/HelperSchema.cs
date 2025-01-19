using UnityEngine;

[DataBundleClass(Category = "Design")]
public class HelperSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey displayName;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey desc;

	[DataBundleRecordTableFilter("Character")]
	[DataBundleSchemaFilter(typeof(CharacterSchema), false, DontFollowRecordLink = true)]
	public DataBundleRecordKey resources;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.FrontEnd | DataBundleResourceGroup.Preview))]
	public Texture2D HUDIcon;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.FrontEnd | DataBundleResourceGroup.Preview))]
	public Texture2D championIcon;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.FrontEnd | DataBundleResourceGroup.Preview))]
	public Texture2D lockedIcon;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.FrontEnd | DataBundleResourceGroup.Preview))]
	public Texture2D platinumIcon;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject meleeWeaponPrefab;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject rangedWeaponPrefab;

	[DataBundleRecordTableFilter("Lane")]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	public DataBundleRecordKey lane;

	public int waveToUnlock;

	public int availableAtWave;

	public float resourcesCost;

	public float cooldownTimer;

	public float attackFrequency;

	public bool usesBladeWeapon;

	public bool hideInStore;

	public bool hideInEquip;

	public bool unique;

	public bool exploseOnMelee;

	public bool enemyIgnoresMe;

	public bool canMeleeFliers;

	public bool canMeleeProjectiles;

	public bool isMount;

	public bool isMounted;

	public bool blocksHeroMovement;

	public bool canBeEaten;

	public bool isCharger;

	public int award;

	public int resourceDropMin;

	public int resourceDropMax;

	public int valueInCoins;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey levelMatchOtherHelper;

	[DataBundleSchemaFilter(typeof(HelperLevelSchema), false)]
	public DataBundleRecordTable levels;

	[DataBundleSchemaFilter(typeof(HeroSchema), false)]
	public DataBundleRecordKey requiredHero;

	public int goldenHelperProbability;

	public float goldenHelperHealthMultiplier;

	public float goldenHelperDamageMultiplier;

	public float goldenHelperSizeScale;

	public string goldenHelperCostToUnlock;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey upgradeAchievement;

	public string unlockPlayhavenRequest;

	public int summonIndex;

	public int defenseRating;

	public HelperLevelSchema[] Levels { get; set; }

	public HelperLevelSchema CurLevel
	{
		get
		{
			return Singleton<HelpersDatabase>.Instance.GetHelperLevelData(this);
		}
	}

	public HelperLevelSchema NextLevel
	{
		get
		{
			int num = Mathf.Clamp(Singleton<Profile>.Instance.GetRawHelperLevel(id), 0, Levels.Length - 1);
			return Levels[num];
		}
	}

	public bool Locked
	{
		get
		{
			if (Singleton<Profile>.Instance.GetRawHelperLevel(id) == 0 && Singleton<Profile>.Instance.highestUnlockedWave < waveToUnlock)
			{
				return true;
			}
			return false;
		}
	}

	public string IconPath { get; private set; }

	public string LockedIconPath { get; private set; }

	public string PlatinumIconPath { get; private set; }

	public string ChampionIconPath { get; private set; }

	public HelperLevelSchema GetLevel(int level)
	{
		level = Mathf.Clamp(level - 1, 0, Levels.Length - 1);
		return Levels[level];
	}

	public Texture2D TryGetChampionIcon()
	{
		if (championIcon != null)
		{
			return championIcon;
		}
		if (!string.IsNullOrEmpty(ChampionIconPath))
		{
			return LoadIcon(ChampionIconPath);
		}
		return null;
	}

	public Texture2D TryGetLockedIcon()
	{
		if (lockedIcon != null)
		{
			return lockedIcon;
		}
		if (!string.IsNullOrEmpty(LockedIconPath))
		{
			return LoadIcon(LockedIconPath);
		}
		if (HUDIcon != null)
		{
			return HUDIcon;
		}
		if (!string.IsNullOrEmpty(IconPath))
		{
			return LoadIcon(IconPath);
		}
		return null;
	}

	public Texture2D TryGetPlatinumIcon()
	{
		if (platinumIcon != null)
		{
			return platinumIcon;
		}
		if (!string.IsNullOrEmpty(PlatinumIconPath))
		{
			return LoadIcon(PlatinumIconPath);
		}
		if (HUDIcon != null)
		{
			return HUDIcon;
		}
		if (!string.IsNullOrEmpty(IconPath))
		{
			return LoadIcon(IconPath);
		}
		return null;
	}

	public Texture2D TryGetHUDIcon()
	{
		if (Singleton<Profile>.Instance.GetHelperLevel(id) > Helper.kPlatinumLevel)
		{
			return TryGetPlatinumIcon();
		}
		if (HUDIcon != null)
		{
			return HUDIcon;
		}
		if (!string.IsNullOrEmpty(IconPath))
		{
			return LoadIcon(IconPath);
		}
		return null;
	}

	public Texture2D LoadIcon(string path)
	{
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(path, 1);
		if (cachedResource != null)
		{
			return cachedResource.Resource as Texture2D;
		}
		return null;
	}

	public void Initialize(string tableName)
	{
		if (!DataBundleRecordTable.IsNullOrEmpty(levels))
		{
			Levels = levels.InitializeRecords<HelperLevelSchema>();
			HelperLevelSchema[] array = Levels;
			foreach (HelperLevelSchema helperLevelSchema in array)
			{
				helperLevelSchema.Initialize(tableName);
			}
		}
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(HelperSchema), tableName, id, "HUDIcon", true);
		LockedIconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(HelperSchema), tableName, id, "lockedIcon", true);
		PlatinumIconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(HelperSchema), tableName, id, "platinumIcon", true);
		ChampionIconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(HelperSchema), tableName, id, "championIcon", true);
	}
}
