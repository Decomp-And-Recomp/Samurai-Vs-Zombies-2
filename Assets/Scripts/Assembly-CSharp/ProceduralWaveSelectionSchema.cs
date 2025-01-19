[DataBundleClass(Category = "Design")]
public class ProceduralWaveSelectionSchema
{
	[DataBundleKey]
	public int key;

	[DataBundleRecordTableFilter("Months")]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordKey startMonth;

	public int startDay;

	[DataBundleField(ColumnWidth = 200)]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("Months")]
	public DataBundleRecordKey endMonth;

	public int endDay;

	[DataBundleSchemaFilter(typeof(ProceduralWaveListSchema), false)]
	public DataBundleRecordTable proceduralWavePool;
}
