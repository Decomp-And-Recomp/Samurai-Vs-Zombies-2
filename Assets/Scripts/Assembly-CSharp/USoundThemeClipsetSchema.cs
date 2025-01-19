using UnityEngine;

[DataBundleClass(Category = "Audio")]
public class USoundThemeClipsetSchema
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 200)]
	public int UnimportantKey;

	[DataBundleField(ColumnWidth = 500, StaticResource = true)]
	public AudioClip audioClip;

	[DataBundleField(ColumnWidth = 100)]
	public bool excludeOnLowEnd;

	public bool dontPreloadSound;

	public static implicit operator bool(USoundThemeClipsetSchema obj)
	{
		return obj != null;
	}
}
