using UnityEngine;

[DataBundleClass(Category = "Design")]
public class HeroSchema
{
	public delegate float InfiniteUpgradeAccessor(HeroSchema s);

	public delegate float LevelValueAccessor(HeroLevelSchema ls);

	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey desc;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey store_levelup;

	[DataBundleSchemaFilter(typeof(CharacterSchema), false)]
	public DataBundleRecordKey resources;

	[DataBundleSchemaFilter(typeof(CharacterSchema), false)]
	public DataBundleRecordKey resourcesGhost;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D icon;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D storePortrait;

	[DataBundleSchemaFilter(typeof(WeaponSchema), false)]
	[DataBundleRecordTableFilter("Weapons")]
	public DataBundleRecordKey meleeWeapon;

	[DataBundleSchemaFilter(typeof(WeaponSchema), false)]
	[DataBundleRecordTableFilter("Weapons")]
	public DataBundleRecordKey rangedWeapon;

	public string infiniteUpgradeCost;

	public float infiniteUpgradeHealth;

	public float infiniteUpgradeHealthRecovery;

	[DataBundleSchemaFilter(typeof(HeroLevelSchema), false)]
	public DataBundleRecordTable levels;

	[DataBundleSchemaFilter(typeof(AbilitiesListSchema), false)]
	public DataBundleRecordTable PotentialAbilties;

	[DataBundleSchemaFilter(typeof(ArmorLevelSchema), false)]
	public DataBundleRecordTable armorLevels;

	public int allySlots;

	public int abilitySlots;

	public bool canMeleeFliers;

	public int waveToUnlock;

	public bool purchaseToUnlock;

	public bool hideUntilUnlocked;

	public bool overrideRequirements;

	public bool disabled;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey unlockAchievement;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey upgradeAchievement;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey meleeAchievement;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey rangedAchievement;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey leadershipAchievement;

	public string unlockPlayhavenRequest;

	[DataBundleDefaultValue(1.3f)]
	public float backPedalTime;

	public bool blocksHeroMovement;

	public int defenseRating;

	public HeroLevelSchema[] Levels { get; set; }

	public AbilitySchema[] Abilities { get; private set; }

	public ArmorLevelSchema[] ArmorLevels { get; set; }

	public WeaponSchema MeleeWeapon { get; private set; }

	public WeaponSchema RangedWeapon { get; private set; }

	public bool Locked
	{
		get
		{
			return !Purchased || Singleton<Profile>.Instance.highestUnlockedWave < waveToUnlock;
		}
	}

	public bool Purchased
	{
		get
		{
			return !purchaseToUnlock || Singleton<Profile>.Instance.GetHeroPurchased(id);
		}
	}

	public string IconPath { get; private set; }

	public HeroStarsSchema HeroStarsSchema { get; set; }

	public HeroLevelSchema GetLevel(int level)
	{
		if (Levels != null && Levels.Length > 0)
		{
			return Levels[Mathf.Clamp(level - 1, 0, Levels.Length - 1)];
		}
		return null;
	}

	public ArmorLevelSchema GetArmorLevel(int level)
	{
		if (ArmorLevels != null && ArmorLevels.Length > 0)
		{
			return ArmorLevels[Mathf.Clamp(level - 1, 0, ArmorLevels.Length - 1)];
		}
		return null;
	}

	public float MaxHealth(int level)
	{
		level--;
		int num = Levels.Length - 1;
		if (level <= num)
		{
			return Levels[level].health;
		}
		return InfiniteUpgrades.Extrapolate(Levels[num].health, infiniteUpgradeHealth, level - num);
	}

	public float HealthRecovery(int level)
	{
		level--;
		int num = Levels.Length - 1;
		if (level <= num)
		{
			return Levels[level].healthRecovery;
		}
		return InfiniteUpgrades.Extrapolate(Levels[num].healthRecovery, infiniteUpgradeHealthRecovery, level - num);
	}

	public float Extrapolate(LevelValueAccessor accessor, InfiniteUpgradeAccessor upgradeAccessor)
	{
		return Extrapolate(Singleton<Profile>.Instance.GetHeroLevel(id), accessor, upgradeAccessor);
	}

	public float Extrapolate(int level, LevelValueAccessor accessor, InfiniteUpgradeAccessor upgradeAccessor)
	{
		level = Mathf.Max(0, level - 1);
		int num = Levels.Length - 1;
		if (level <= num)
		{
			return accessor(Levels[level]);
		}
		return InfiniteUpgrades.Extrapolate(accessor(Levels[num]), upgradeAccessor(this), level - num);
	}

	public void Initialize(string tableName)
	{
		Levels = levels.InitializeRecords<HeroLevelSchema>();
		if (PotentialAbilties != null)
		{
			AbilitiesListSchema[] array = PotentialAbilties.InitializeRecords<AbilitiesListSchema>();
			Abilities = new AbilitySchema[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Abilities[i] = Singleton<AbilitiesDatabase>.Instance[array[i].ability.Key];
			}
		}
		MeleeWeapon = meleeWeapon.InitializeRecord<WeaponSchema>();
		if (MeleeWeapon != null)
		{
			MeleeWeapon.Initialize();
		}
		RangedWeapon = rangedWeapon.InitializeRecord<WeaponSchema>();
		if (RangedWeapon != null)
		{
			RangedWeapon.Initialize();
		}
		if (!DataBundleRecordTable.IsNullOrEmpty(armorLevels))
		{
			ArmorLevels = armorLevels.InitializeRecords<ArmorLevelSchema>();
			ArmorLevelSchema[] array2 = ArmorLevels;
			foreach (ArmorLevelSchema armorLevelSchema in array2)
			{
				armorLevelSchema.Initialize(armorLevels.RecordTable);
			}
		}
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(HeroSchema), tableName, id, "icon", true);
		HeroStarsSchema = DataBundleRuntime.Instance.InitializeRecord<HeroStarsSchema>("HeroStars", id);
	}

	public void LoadCachedResources(int meleeLevel, int rangedLevel, int armorLevel, bool frontEnd)
	{
		if (MeleeWeapon != null)
		{
			MeleeWeapon.LoadCachedResources(meleeLevel);
			if (frontEnd)
			{
				WeaponLevelSchema weaponLevelSchema = MeleeWeapon.Levels[Mathf.Clamp(meleeLevel, 0, MeleeWeapon.Levels.Length - 1)];
				string iconPath = weaponLevelSchema.IconPath;
				ResourceCache.GetCachedResource(iconPath, 1);
			}
		}
		if (RangedWeapon != null)
		{
			RangedWeapon.LoadCachedResources(rangedLevel);
			if (frontEnd)
			{
				WeaponLevelSchema weaponLevelSchema2 = RangedWeapon.Levels[Mathf.Clamp(rangedLevel, 0, RangedWeapon.Levels.Length - 1)];
				string iconPath2 = weaponLevelSchema2.IconPath;
				ResourceCache.GetCachedResource(iconPath2, 1);
			}
		}
		ArmorLevelSchema armorLevel2 = GetArmorLevel(armorLevel);
		if (armorLevel2 != null)
		{
			string tableRecordKey = DataBundleRuntime.TableRecordKey(armorLevels.RecordTable, armorLevel2.level.ToString());
			ResourceCache.LoadCachedResources(armorLevel2, tableRecordKey);
			if (frontEnd)
			{
				string iconPath3 = armorLevel2.IconPath;
				ResourceCache.GetCachedResource(iconPath3, 1);
			}
		}
	}

	public void UnloadCachedResources(int meleeLevel, int rangedLevel, int armorLevel)
	{
		if (MeleeWeapon != null)
		{
			MeleeWeapon.UnloadCachedResources();
			WeaponLevelSchema weaponLevelSchema = MeleeWeapon.Levels[Mathf.Clamp(meleeLevel, 0, MeleeWeapon.Levels.Length - 1)];
			string iconPath = weaponLevelSchema.IconPath;
			ResourceCache.UnCache(iconPath);
		}
		if (RangedWeapon != null)
		{
			RangedWeapon.UnloadCachedResources();
			WeaponLevelSchema weaponLevelSchema2 = RangedWeapon.Levels[Mathf.Clamp(rangedLevel, 0, RangedWeapon.Levels.Length - 1)];
			string iconPath2 = weaponLevelSchema2.IconPath;
			ResourceCache.UnCache(iconPath2);
		}
		if (ArmorLevels != null)
		{
			ArmorLevelSchema[] array = ArmorLevels;
			foreach (ArmorLevelSchema armorLevelSchema in array)
			{
				string tableRecordKey = DataBundleRuntime.TableRecordKey(armorLevels.RecordTable, armorLevelSchema.level.ToString());
				ResourceCache.UnloadCachedResources(armorLevelSchema, tableRecordKey);
			}
			ArmorLevelSchema armorLevel2 = GetArmorLevel(armorLevel);
			if (armorLevel2 != null)
			{
				string iconPath3 = armorLevel2.IconPath;
				ResourceCache.UnCache(iconPath3);
			}
		}
	}
}
