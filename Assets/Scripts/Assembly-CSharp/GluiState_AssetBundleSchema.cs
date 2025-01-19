using UnityEngine;

[DataBundleClass(Category = "GLUI")]
public class GluiState_AssetBundleSchema
{
	[DataBundleKey(Schema = typeof(GluiState_AssetBundle), ColumnWidth = 200)]
	public string name;

	[DataBundleField(StaticResource = true, ColumnWidth = 300)]
	public GameObject prefab;

	[DataBundleField(ColumnWidth = 150)]
	public bool skipUnloadUnusedAssets;
}
