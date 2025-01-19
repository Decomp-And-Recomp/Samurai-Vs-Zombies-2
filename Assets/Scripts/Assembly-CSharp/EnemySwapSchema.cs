[DataBundleClass(Category = "Design")]
public class EnemySwapSchema
{
	[DataBundleKey]
	public int key;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	public DataBundleRecordKey swapFrom;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	public DataBundleRecordKey swapTo;

	public static EnemySwapSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<EnemySwapSchema>(record);
	}
}
