[DataBundleClass]
public class AchievementListSchema
{
	[DataBundleKey(Schema = typeof(AchievementSchema))]
	public DataBundleRecordKey achievement;

	public static AchievementListSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<AchievementListSchema>(record);
	}
}
