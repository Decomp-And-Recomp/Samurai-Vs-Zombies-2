[DataBundleClass(Category = "Design")]
public class WaveCommandSchema
{
	[DataBundleKey]
	public int index;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	[DataBundleRecordTableFilter("Enemies")]
	public DataBundleRecordKey enemy;

	[DataBundleField(ColumnWidth = 200)]
	public string command;

	[DataBundleSchemaFilter(typeof(EnemyGroupSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordTable enemyGroup;
}
