using UnityEngine;

[DataBundleClass(Category = "Design")]
public class IconSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D icon;
}
