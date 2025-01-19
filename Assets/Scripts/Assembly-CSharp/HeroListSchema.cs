[DataBundleClass(Category = "Design")]
public class HeroListSchema
{
	[DataBundleKey(Schema = typeof(HeroSchema))]
	public DataBundleRecordKey hero;

	public static HeroListSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<HeroListSchema>(record);
	}
}
