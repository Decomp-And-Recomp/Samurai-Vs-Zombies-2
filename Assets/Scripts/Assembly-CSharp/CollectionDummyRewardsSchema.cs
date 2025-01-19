using UnityEngine;

[DataBundleClass(Category = "Design")]
public class CollectionDummyRewardsSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordKey displayName;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture rewardIcon;

	[DataBundleField(ColumnWidth = 100, TooltipInfo = "Save data field (int) to be modified by the reward.")]
	public string rewardSaveInt;

	public int rewardAmount;

	private static int Count(string tableName)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordTableLength(typeof(CollectionDummyRewardsSchema), tableName);
		}
		return 0;
	}

	private static string FromIndex(string tableName, int index)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.TableRecordKey(tableName, DataBundleRuntime.Instance.GetRecordKeys(typeof(CollectionDummyRewardsSchema), tableName, false)[index]);
		}
		return string.Empty;
	}

	public static CollectionDummyRewardsSchema GetRecord(string table, string key)
	{
		return GetRecord(DataBundleRuntime.TableRecordKey(table, key));
	}

	private static CollectionDummyRewardsSchema GetRecord(string tableRecordKey)
	{
		CollectionDummyRewardsSchema collectionDummyRewardsSchema = DataBundleRuntime.Instance.InitializeRecord<CollectionDummyRewardsSchema>(tableRecordKey);
		if (collectionDummyRewardsSchema == null)
		{
		}
		return collectionDummyRewardsSchema;
	}

	public static CollectionDummyRewardsSchema GetRandomRecord(string tableName)
	{
		int index = Random.Range(0, Count(tableName));
		string tableRecordKey = FromIndex(tableName, index);
		return GetRecord(tableRecordKey);
	}
}
