using UnityEngine;

[DataBundleClass(Category = "Design")]
public class CollectionStarterSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(CollectionSchema), false)]
	public DataBundleRecordKey item1Set;

	public int item1Index;

	[DataBundleSchemaFilter(typeof(CollectionSchema), false)]
	public DataBundleRecordKey item2Set;

	public int item2Index;

	[DataBundleSchemaFilter(typeof(CollectionSchema), false)]
	public DataBundleRecordKey item3Set;

	public int item3Index;

	private static int Count(string tableName)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordTableLength(typeof(CollectionStarterSchema), tableName);
		}
		return 0;
	}

	private static string FromIndex(string tableName, int index)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.TableRecordKey(tableName, DataBundleRuntime.Instance.GetRecordKeys(typeof(CollectionStarterSchema), tableName, false)[index]);
		}
		return string.Empty;
	}

	private static CollectionStarterSchema GetRecord(string tableRecordKey)
	{
		CollectionStarterSchema collectionStarterSchema = DataBundleRuntime.Instance.InitializeRecord<CollectionStarterSchema>(tableRecordKey);
		if (collectionStarterSchema == null)
		{
		}
		return collectionStarterSchema;
	}

	public static CollectionStarterSchema GetRandomRecord(string tableName)
	{
		int index = Random.Range(0, Count(tableName));
		string tableRecordKey = FromIndex(tableName, index);
		return GetRecord(tableRecordKey);
	}
}
