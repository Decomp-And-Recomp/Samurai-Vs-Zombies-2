[DataBundleClass(Category = "Design")]
public class HelperListSchema
{
	[DataBundleKey(Schema = typeof(HelperSchema))]
	public DataBundleRecordKey ability;

	public static HelperListSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<HelperListSchema>(record);
	}
}
