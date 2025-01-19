using UnityEngine;

[DataBundleClass(Category = "Design")]
public class AbilitySchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true)]
	public GameObject prefab;

	[DataBundleField(StaticResource = true)]
	public GameObject rightToLeftPrefab;

	[DataBundleField(StaticResource = true)]
	public GameObject prop;

	[DataBundleField(StaticResource = true)]
	public GameObject activateFX;

	[DataBundleField(StaticResource = true)]
	public GameObject resultFX;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey description;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D icon;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D iconNoText;

	[DataBundleField]
	public float cooldown;

	[DataBundleField]
	public float AIMinRange;

	[DataBundleField]
	public float AIMaxRange;

	[DataBundleField]
	public float levelToUnlock;

	[DataBundleField]
	public string animName;

	[DataBundleField]
	public float spawnOffsetHorizontal;

	[DataBundleField]
	public float spawnOffsetVertical;

	[DataBundleField]
	public float infiniteUpgradeMagnitude;

	[DataBundleField]
	public float infiniteUpgradeCostCoins;

	[DataBundleField]
	public float infiniteUpgradeCostGems;

	[DataBundleSchemaFilter(typeof(USoundThemeSetSchema), false)]
	public DataBundleRecordKey soundTheme;

	[DataBundleRecordTableFilter("SoundThemeEnum")]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	public DataBundleRecordKey soundEvent;

	[DataBundleSchemaFilter(typeof(AbilityLevelSchema), false)]
	public DataBundleRecordTable abilityLevelData;

	[DataBundleSchemaFilter(typeof(HeroSchema), false)]
	public DataBundleRecordKey exclusiveHero;

	[DataBundleSchemaFilter(typeof(AchievementSchema), false)]
	public DataBundleRecordKey upgradeAchievement;

	public int defenseRating;

	public AbilityLevelSchema[] levelData { get; private set; }

	public DynamicEnum SoundEvent { get; private set; }

	public string IconPath { get; private set; }

	public bool EquipLocked
	{
		get
		{
			return Singleton<Profile>.Instance.GetAbilityLevel(id) == 0;
		}
	}

	public static AbilitySchema Initialize(DataBundleRecordKey record)
	{
		AbilitySchema abilitySchema = DataBundleUtils.InitializeRecord<AbilitySchema>(record);
		if (abilitySchema != null)
		{
			abilitySchema.Initialize(record.Table);
		}
		return abilitySchema;
	}

	public void Initialize(string tableName)
	{
		if (!DataBundleRecordTable.IsNullOrEmpty(abilityLevelData))
		{
			levelData = abilityLevelData.InitializeRecords<AbilityLevelSchema>();
		}
		SoundEvent = soundEvent.InitializeRecord<DynamicEnum>();
		IconPath = LocalizedTextureSchema.GetLocalizedPath("Icons", DataBundleRuntime.Instance.GetValue<string>(typeof(AbilitySchema), tableName, id, "icon", true));
	}

	public AbilitySchema ShallowCopy()
	{
		return (AbilitySchema)MemberwiseClone();
	}

	public float Extrapolate(int abLevel, LevelValueAccessor accessor)
	{
		abLevel = Mathf.Max(0, abLevel - 1);
		int num = levelData.Length - 1;
		if (abLevel <= num)
		{
			return accessor(levelData[abLevel]);
		}
		return InfiniteUpgrades.Extrapolate(accessor(levelData[num]), infiniteUpgradeMagnitude, abLevel - num);
	}
}
