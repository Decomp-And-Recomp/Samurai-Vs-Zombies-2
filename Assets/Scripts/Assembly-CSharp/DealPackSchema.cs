using UnityEngine;

[DataBundleClass(Category = "Design")]
public class DealPackSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D icon;

	public string cost;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey description;

	public bool showInStore = true;

	public string items;
}
