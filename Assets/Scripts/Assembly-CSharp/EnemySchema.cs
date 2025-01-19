using UnityEngine;

[DataBundleClass(Category = "Design")]
public class EnemySchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(CharacterSchema), false)]
	public DataBundleRecordKey resources;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject rangedWeaponPrefab;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject meleeWeaponPrefab;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("Projectile")]
	public DataBundleRecordKey projectile;

	public int health;

	public float speedMin;

	public float speedMax;

	public float meleeRange;

	public float bowRange;

	public int meleeDamage;

	public int bowDamage;

	public float attackFrequency;

	public int knockbackPower;

	public int knockbackResistance;

	public float damageBuffPercent;

	public float eatCooldown;

	public int award;

	public int resourceDropMin;

	public int resourceDropMax;

	public int previewPriority;

	public bool usesBladeWeapon;

	public bool boss;

	public bool flying;

	public bool canMeleeFliers;

	public bool exploseOnMelee;

	public bool enemyIgnoresMe;

	public bool gateRusher;

	public bool blocksHeroMovement;

	public float previewScale;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("Lane")]
	public DataBundleRecordKey lane;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	public DataBundleRecordKey spawnOnDeath;

	public int spawnOnDeathCount;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey specialsDesc;
}
