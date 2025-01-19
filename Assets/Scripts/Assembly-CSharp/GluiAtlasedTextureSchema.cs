using UnityEngine;

[DataBundleClass(DisplayName = "Glui Texture Atlas Data", Category = "GLUI", Comment = "This should never be edited manually. Use the menu commands to import atlas XML files.")]
public class GluiAtlasedTextureSchema
{
	[DataBundleField(ColumnWidth = 150, Identifier = 1)]
	[DataBundleKey]
	public string TextureName;

	[DataBundleField(ColumnWidth = 200, Identifier = 2)]
	public string SourceAssetPath;

	[DataBundleField(StaticResource = true, ColumnWidth = 100, Identifier = 3)]
	public Texture2D AtlasTexture;

	[DataBundleField(ColumnWidth = 100, Identifier = 4)]
	public float AtlasSizeX;

	[DataBundleField(ColumnWidth = 100, Identifier = 5)]
	public float AtlasSizeY;

	[DataBundleField(ColumnWidth = 100, Identifier = 6)]
	public float AtlasPosX;

	[DataBundleField(ColumnWidth = 100, Identifier = 7)]
	public float AtlasPosY;

	[DataBundleField(ColumnWidth = 100, Identifier = 8)]
	public float ActualSizeX;

	[DataBundleField(ColumnWidth = 100, Identifier = 9)]
	public float ActualSizeY;

	public Rect AtlasRect
	{
		get
		{
			return new Rect(AtlasPosX, AtlasPosY, AtlasSizeX, AtlasSizeY);
		}
	}
}
