using UnityEngine;

[DataBundleClass(Category = "Design", Comment = "Describes armor stats per level")]
public class ArmorLevelSchema
{
	[DataBundleKey]
	public int level;

	public int costCoins;

	public int costGems;

	public float meleeDamageModifier;

	public float rangedDamageModifier;

	public float meleeBlockRatio;

	public float rangedBlockRatio;

	public float reflectDamageRatio;

	[DataBundleField(StaticResource = true)]
	public Material characterMaterial;

	[DataBundleField(StaticResource = true)]
	public GameObject blockFX;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D icon;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey title;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey desc;

	[DataBundleField(StaticResource = true)]
	public GameObject visualFX;

	public int defenseRating;

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(ArmorLevelSchema), tableName, level.ToString(), "icon", true);
	}

	public static string ModifierString(float modifier, bool reverse)
	{
		int num = ((!reverse) ? Mathf.RoundToInt(modifier * 100f) : Mathf.RoundToInt((1f - modifier) * 100f));
		return string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "percent"), num);
	}
}
