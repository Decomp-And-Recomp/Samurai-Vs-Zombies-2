using UnityEngine;

[DataBundleClass(Category = "Character")]
public class MaterialLookupSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true)]
	public Material material;
}
