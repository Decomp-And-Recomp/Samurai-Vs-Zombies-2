using UnityEngine;

[DataBundleClass(Category = "Design")]
public class UpgradesSchema
{
	[DataBundleKey(ColumnWidth = 160)]
	public string id;

	public bool globalStoreGroup;

	public int numUpgradeLevels;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey name;

	public float startingAmount;

	public float amountLevel1;

	public string costLevel1;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D iconLevel1;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey descLevel1;

	public float amountLevel2;

	public string costLevel2;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D iconLevel2;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey descLevel2;

	public float amountLevel3;

	public string costLevel3;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D iconLevel3;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey descLevel3;

	public float amountLevel4;

	public string costLevel4;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D iconLevel4;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey descLevel4;
}
