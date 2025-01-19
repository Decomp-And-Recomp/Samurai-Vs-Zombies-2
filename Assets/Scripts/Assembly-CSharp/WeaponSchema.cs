using UnityEngine;

[DataBundleClass(Category = "Design")]
public class WeaponSchema
{
	public delegate float InfiniteUpgradeAccessor(WeaponSchema s);

	public delegate float LevelValueAccessor(WeaponLevelSchema ls);

	[DataBundleKey]
	public string id;

	public bool isRanged;

	public bool isBladeWeapon;

	public bool isDualWield;

	public string infiniteUpgradeCost;

	public float infiniteUpgradeDamage;

	[DataBundleSchemaFilter(typeof(WeaponLevelSchema), false)]
	public DataBundleRecordTable levels;

	public int defenseRating;

	public WeaponLevelSchema[] Levels { get; set; }

	public WeaponLevelSchema GetLevel(int level)
	{
		if (Levels != null && Levels.Length > 0)
		{
			return Levels[Mathf.Clamp(level - 1, 0, Levels.Length - 1)];
		}
		return null;
	}

	public float Damage(int level)
	{
		int num = Levels.Length;
		if (level <= num)
		{
			return Levels[level - 1].damage;
		}
		return InfiniteUpgrades.Extrapolate(Levels[num - 1].damage, infiniteUpgradeDamage, level - num);
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

	public void Initialize()
	{
		Levels = levels.InitializeRecords<WeaponLevelSchema>();
		WeaponLevelSchema[] array = Levels;
		foreach (WeaponLevelSchema weaponLevelSchema in array)
		{
			weaponLevelSchema.Initialize(levels.RecordTable);
		}
	}

	public void LoadCachedResources(int level)
	{
		WeaponLevelSchema level2 = GetLevel(level);
		if (level2 != null)
		{
			string tableRecordKey = DataBundleRuntime.TableRecordKey(levels.RecordTable, level2.level.ToString());
			ResourceCache.LoadCachedResources(level2, tableRecordKey);
		}
	}

	public void UnloadCachedResources()
	{
		if (Levels != null)
		{
			WeaponLevelSchema[] array = Levels;
			foreach (WeaponLevelSchema weaponLevelSchema in array)
			{
				string tableRecordKey = DataBundleRuntime.TableRecordKey(levels.RecordTable, weaponLevelSchema.level.ToString());
				ResourceCache.UnloadCachedResources(weaponLevelSchema, tableRecordKey);
			}
		}
	}
}
