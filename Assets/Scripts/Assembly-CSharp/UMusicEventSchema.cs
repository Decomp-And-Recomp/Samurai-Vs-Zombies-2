[DataBundleClass(Category = "Audio")]
public class UMusicEventSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "MusicEventEnum")]
	[DataBundleField(ColumnWidth = 300)]
	public DataBundleRecordKey eventName;

	[DataBundleSchemaFilter(typeof(UMusicSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordKey musicClip;

	[DataBundleField(ColumnWidth = 150)]
	public UMusicManager.MusicSourceID musicSource;

	[DataBundleField(ColumnWidth = 180)]
	public UMusicManager.FadeStyle fadeStyle;

	[DataBundleField(ColumnWidth = 80)]
	[DataBundleDefaultValue(0f)]
	public float fadeOutTimes;

	[DataBundleField(ColumnWidth = 80)]
	[DataBundleDefaultValue(0f)]
	public float fadeInTimes;

	public static implicit operator bool(UMusicEventSchema obj)
	{
		return obj != null;
	}
}
