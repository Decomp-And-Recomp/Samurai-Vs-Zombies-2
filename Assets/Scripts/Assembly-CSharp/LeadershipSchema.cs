[DataBundleClass(Category = "Design")]
public class LeadershipSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey purchaseText;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey descText;

	public int maxLevel;

	public int hideInStoreLevel;

	public string storeCost0;

	[DataBundleField(ColumnWidth = 140)]
	public float resourcesPerSeconds0;

	public int maxResource0;

	[DataBundleField(ColumnWidth = 140)]
	public int levelUpThreshold0;

	public string storeCost1;

	[DataBundleField(ColumnWidth = 140)]
	public float resourcesPerSeconds1;

	public int maxResource1;

	[DataBundleField(ColumnWidth = 140)]
	public int levelUpThreshold1;

	public string storeCost2;

	[DataBundleField(ColumnWidth = 140)]
	public float resourcesPerSeconds2;

	public int maxResource2;

	[DataBundleField(ColumnWidth = 140)]
	public int levelUpThreshold2;

	public string storeCost3;

	[DataBundleField(ColumnWidth = 140)]
	public float resourcesPerSeconds3;

	public int maxResource3;

	[DataBundleField(ColumnWidth = 140)]
	public int levelUpThreshold3;
}
