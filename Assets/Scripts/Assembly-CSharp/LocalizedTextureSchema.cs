using System.IO;
using UnityEngine;

[DataBundleClass(Localizable = true)]
public class LocalizedTextureSchema
{
	[DataBundleKey(ColumnWidth = 200)]
	public string id;

	[DataBundleField(ColumnWidth = 200, StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D texture;

	public static string GetLocalizedPath(string tableName, string defaultPath)
	{
		if (!string.IsNullOrEmpty(defaultPath))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(defaultPath);
			string value = DataBundleRuntime.Instance.GetValue<string>(typeof(LocalizedTextureSchema), tableName, fileNameWithoutExtension, "texture", true);
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
		}
		return defaultPath;
	}
}
