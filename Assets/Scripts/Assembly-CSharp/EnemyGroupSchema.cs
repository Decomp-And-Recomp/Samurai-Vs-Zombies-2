[DataBundleClass(Category = "Design")]
public class EnemyGroupSchema
{
	[DataBundleKey]
	public int index;

	public float minWaitTime;

	public float maxWaitTime;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	[DataBundleRecordTableFilter("Enemies")]
	public DataBundleRecordKey enemy_1;

	public int quantity_1;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	[DataBundleRecordTableFilter("Enemies")]
	public DataBundleRecordKey enemy_2;

	public int quantity_2;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	[DataBundleRecordTableFilter("Enemies")]
	public DataBundleRecordKey enemy_3;

	public int quantity_3;
}
