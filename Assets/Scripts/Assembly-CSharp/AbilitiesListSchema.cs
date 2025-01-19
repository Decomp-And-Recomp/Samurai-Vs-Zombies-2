[DataBundleClass(Category = "Design")]
public class AbilitiesListSchema
{
	[DataBundleKey(Schema = typeof(AbilitySchema))]
	public DataBundleRecordKey ability;

	public static AbilitiesListSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<AbilitiesListSchema>(record);
	}
}
