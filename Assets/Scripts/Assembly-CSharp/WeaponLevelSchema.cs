using UnityEngine;

[DataBundleClass(Category = "Design")]
public class WeaponLevelSchema
{
	[DataBundleKey]
	public int level;

	public string cost;

	public float attackRange;

	public float attackFrequency;

	public float damage;

	public int knockbackPower;

	public float DOTDamageRatio;

	public float DOTDuration;

	public float DOTInterval;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject prefab;

	[DataBundleRecordTableFilter("Projectile")]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	public DataBundleRecordKey projectile;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D icon;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D iconNoText;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey desc;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey title;

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = LocalizedTextureSchema.GetLocalizedPath("Icons", DataBundleRuntime.Instance.GetValue<string>(typeof(WeaponLevelSchema), tableName, level.ToString(), "icon", true));
	}
}
