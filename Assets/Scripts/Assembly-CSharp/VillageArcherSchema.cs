using UnityEngine;

[DataBundleClass(Category = "Design")]
public class VillageArcherSchema
{
	[DataBundleKey]
	public int level;

	[DataBundleSchemaFilter(typeof(CharacterSchema), false)]
	public DataBundleRecordKey character_1;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.InGame)]
	public GameObject rangedWeaponPrefab_1;

	[DataBundleSchemaFilter(typeof(ProjectileSchema), false)]
	public DataBundleRecordKey projectile_1;

	public float bowRange_1;

	public float bowDamage_1;

	public float attackFrequency_1;

	[DataBundleSchemaFilter(typeof(CharacterSchema), false)]
	public DataBundleRecordKey character_2;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.InGame)]
	public GameObject rangedWeaponPrefab_2;

	[DataBundleSchemaFilter(typeof(ProjectileSchema), false)]
	public DataBundleRecordKey projectile_2;

	public float bowRange_2;

	public float bowDamage_2;

	public float attackFrequency_2;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D icon;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey upgradeDescription;

	public int costCoins;

	public int costGems;
}
