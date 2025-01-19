[DataBundleClass(Category = "Design")]
public class ProceduralWaveSchema
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 200)]
	public string waveName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey waveDisplayName;

	[DataBundleField(ColumnWidth = 200)]
	[DataBundleSchemaFilter(typeof(WaveSchema), false)]
	[DataBundleRecordTableFilter("ProceduralWaves")]
	public DataBundleRecordKey wave;

	[DataBundleSchemaFilter(typeof(HeroListSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordTable possibleHeroes;

	[DataBundleSchemaFilter(typeof(HelperListSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordTable possibleHelpers;

	public bool disabled;

	public float maxTime;

	public WaveSchema CachedWaveSchema { get; set; }
}
