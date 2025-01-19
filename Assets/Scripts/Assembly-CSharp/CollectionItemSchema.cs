using UnityEngine;

[DataBundleClass(Category = "Design")]
public class CollectionItemSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "CollectionID")]
	public DataBundleRecordKey index;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleField(ColumnWidth = 300)]
	public DataBundleRecordKey displayName;

	public int soulsToAttack;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture icon;

	[DataBundleField(ColumnWidth = 200)]
	[DataBundleSchemaFilter(typeof(PlayModeSchema), false, DontFollowRecordLink = true)]
	public DataBundleRecordKey playMode;

	public int CollectionID { get; private set; }

	public string IconPath { get; private set; }

	public static CollectionItemSchema Initialize(DataBundleRecordKey record)
	{
		CollectionItemSchema collectionItemSchema = DataBundleUtils.InitializeRecord<CollectionItemSchema>(record);
		if (collectionItemSchema != null)
		{
			collectionItemSchema.Initialize(record.Table);
		}
		return collectionItemSchema;
	}

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(CollectionItemSchema), tableName, index.Key, "icon", true);
		CollectionID = DynamicEnum.ToIndex(index);
	}

	public static CollectionItemSchema GetRecord(string tableName, string key)
	{
		CollectionItemSchema collectionItemSchema = DataBundleRuntime.Instance.InitializeRecord<CollectionItemSchema>(tableName, key);
		if (collectionItemSchema == null)
		{
		}
		return collectionItemSchema;
	}
}
