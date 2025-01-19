using UnityEngine;

[DataBundleClass(Category = "Design")]
public class CharmSchema
{
	[DataBundleKey]
	public string id;

	public string cost;

	[DataBundleRecordTableFilter("PlayMode")]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	public DataBundleRecordKey playmode;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey storeDesc;

	public int storePack;

	public string storePackCost;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D icon;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.InGame)]
	public Texture2D hudIcon;

	public float multiplier;

	public float criticalChance;

	public float magnetRange;

	public float magnetMinPullSpeed;

	public float magnetMaxPullSpeed;

	public float leadershipReduction;

	public float abilityCooldownReduction;

	public bool store;

	public bool hideInEquipMenu;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper;

	[DataBundleSchemaFilter(typeof(AbilitySchema), false)]
	public DataBundleRecordKey abilityToActivate;

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(CharmSchema), tableName, id, "icon", true);
	}
}
