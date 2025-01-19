[DataBundleClass(Category = "Design")]
public class HelperSwapSchema
{
	[DataBundleKey(Schema = typeof(HelperSchema))]
	public DataBundleRecordKey swapFrom;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey swapTo;

	public static HelperSwapSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<HelperSwapSchema>(record);
	}
}
