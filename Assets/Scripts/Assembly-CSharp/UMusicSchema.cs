using UnityEngine;

[DataBundleClass(Category = "Audio")]
public class UMusicSchema
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 300)]
	public string name;

	[DataBundleField(ColumnWidth = 500, StaticResource = true)]
	public AudioClip musicClip;

	public static implicit operator bool(UMusicSchema obj)
	{
		return obj != null;
	}
}
