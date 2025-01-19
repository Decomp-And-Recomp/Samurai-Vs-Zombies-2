using UnityEngine;

[DataBundleClass(Category = "GLUI", Comment = "Set image tags for GluiText.")]
public class GluiText_ImageSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D icon;

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(GluiText_ImageSchema), tableName, id, "icon", true);
	}
}
