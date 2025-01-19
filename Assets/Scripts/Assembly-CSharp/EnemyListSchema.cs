[DataBundleClass(Category = "Design")]
public class EnemyListSchema
{
	[DataBundleKey(Schema = typeof(EnemySchema))]
	public DataBundleRecordKey enemy;

	public static EnemyListSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<EnemyListSchema>(record);
	}
}
