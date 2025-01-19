using UnityEngine;

[DataBundleClass(Category = "Design")]
public class CollectionSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayName;

	public bool disabled;

	[DataBundleSchemaFilter(typeof(CollectionItemSchema), false)]
	public DataBundleRecordTable items;

	[DataBundleSchemaFilter(typeof(CollectionDummyRewardsSchema), false)]
	public DataBundleRecordTable dummyRewards;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture rewardIconLevel1;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey rewardDesc1;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey shortDescription1;

	[DataBundleSchemaFilter(typeof(CollectionDummyRewardsSchema), false)]
	public DataBundleRecordKey rewardXtra1;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture rewardIconLevel2;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey rewardDesc2;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey shortDescription2;

	[DataBundleSchemaFilter(typeof(CollectionDummyRewardsSchema), false)]
	public DataBundleRecordKey rewardXtra2;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture rewardIconLevel3;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey rewardDesc3;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey shortDescription3;

	[DataBundleSchemaFilter(typeof(CollectionDummyRewardsSchema), false)]
	public DataBundleRecordKey rewardXtra3;

	public CollectionItemSchema[] Items { get; private set; }

	public void Initialize()
	{
		Items = items.InitializeRecords<CollectionItemSchema>();
		CollectionItemSchema[] array = Items;
		foreach (CollectionItemSchema collectionItemSchema in array)
		{
			collectionItemSchema.Initialize(items.RecordTable);
		}
	}

	public Texture GetRewardIcon(int level, out string rewardText)
	{
		switch (level)
		{
		case 0:
			rewardText = StringUtils.GetStringFromStringRef(rewardDesc1);
			return rewardIconLevel1;
		case 1:
			rewardText = StringUtils.GetStringFromStringRef(rewardDesc2);
			return rewardIconLevel2;
		case 2:
			rewardText = StringUtils.GetStringFromStringRef(rewardDesc3);
			return rewardIconLevel3;
		default:
			rewardText = StringUtils.GetStringFromStringRef(rewardDesc3);
			return rewardIconLevel3;
		}
	}

	public CollectionDummyRewardsSchema GetXtraReward(int level)
	{
		switch (level)
		{
		case 0:
			if (rewardXtra1 != null && !string.IsNullOrEmpty(rewardXtra1.Key))
			{
				return CollectionDummyRewardsSchema.GetRecord(rewardXtra1.Table, rewardXtra1.Key);
			}
			break;
		case 1:
			if (rewardXtra2 != null && !string.IsNullOrEmpty(rewardXtra2.Key))
			{
				return CollectionDummyRewardsSchema.GetRecord(rewardXtra2.Table, rewardXtra2.Key);
			}
			break;
		case 2:
			if (rewardXtra3 != null && !string.IsNullOrEmpty(rewardXtra3.Key))
			{
				return CollectionDummyRewardsSchema.GetRecord(rewardXtra3.Table, rewardXtra3.Key);
			}
			break;
		}
		return null;
	}

	public static int ToIndex(string table, string key)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordKeys(typeof(CollectionSchema), table, false).IndexOf(key);
		}
		return 0;
	}
}
