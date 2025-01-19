[DataBundleClass(Category = "Design")]
public class MultiplayerAIOpponentSchema
{
	[DataBundleKey]
	public int index;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleField(ColumnWidth = 300)]
	public DataBundleRecordKey displayName;

	public int buffLevelEnemy;

	public int buffLevelAmulet;

	public static int Count(string tableName)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordTableLength(typeof(MultiplayerAIOpponentSchema), tableName);
		}
		return 0;
	}

	public static string FromIndex(string tableName, int index)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.TableRecordKey(tableName, DataBundleRuntime.Instance.GetRecordKeys(typeof(MultiplayerAIOpponentSchema), tableName, false)[index]);
		}
		return string.Empty;
	}

	public static MultiplayerAIOpponentSchema GetRecord(string table, string key)
	{
		return GetRecord(DataBundleRuntime.TableRecordKey(table, key));
	}

	public static MultiplayerAIOpponentSchema GetRecord(string tableRecordKey)
	{
		MultiplayerAIOpponentSchema multiplayerAIOpponentSchema = DataBundleRuntime.Instance.InitializeRecord<MultiplayerAIOpponentSchema>(tableRecordKey);
		if (multiplayerAIOpponentSchema == null)
		{
		}
		return multiplayerAIOpponentSchema;
	}
}
