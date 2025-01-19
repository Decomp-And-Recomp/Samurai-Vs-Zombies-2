[DataBundleClass(Category = "Design")]
public class HelperEnemySwapSchema
{
	[DataBundleKey(Schema = typeof(HelperSchema))]
	public DataBundleRecordKey helperSwapFrom;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	public DataBundleRecordKey enemySwapTo;

	public static HelperEnemySwapSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<HelperEnemySwapSchema>(record);
	}
}
