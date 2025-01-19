using UnityEngine;

[DataBundleClass(Category = "Design")]
public class PitSchema
{
	[DataBundleKey]
	public int level;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.InGame)]
	public GameObject ambientPrefab;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.InGame)]
	public GameObject dynamicPrefab;

	public float chanceToEnact;

	public int unlocksAtLevel;

	public string cost;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D icon;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayName;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey upgradeDescription;
}
