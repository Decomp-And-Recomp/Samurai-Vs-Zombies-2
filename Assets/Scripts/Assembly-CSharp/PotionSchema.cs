using UnityEngine;

[DataBundleClass(Category = "Design", Comment = "Consumables")]
public class PotionSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true)]
	public AudioClip pressedSound;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("PlayMode")]
	public DataBundleRecordKey playmode;

	public bool gameHud;

	public int amount;

	public string cost;

	public int storePack;

	public string storePackCost;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey storeDescBasic;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayNameBasic;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey storeDescUpgraded;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayNameUpgraded;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D iconBasic;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D iconUpgraded;

	public string heal;

	public float leadership;

	public string iconType
	{
		get
		{
			string result = "iconBasic";
			switch (id)
			{
			case "healthPotion":
				if (Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Sushi") > 0)
				{
					result = "iconUpgraded";
				}
				break;
			case "leadershipPotion":
				if (Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Tea") > 0)
				{
					result = "iconUpgraded";
				}
				break;
			}
			return result;
		}
	}

	public Texture2D icon
	{
		get
		{
			if (iconType == "iconBasic")
			{
				return iconBasic;
			}
			return iconUpgraded;
		}
	}

	public DataBundleRecordKey storeDesc
	{
		get
		{
			if (iconType == "iconBasic")
			{
				return storeDescBasic;
			}
			return storeDescUpgraded;
		}
	}

	public DataBundleRecordKey displayName
	{
		get
		{
			if (iconType == "iconBasic")
			{
				return displayNameBasic;
			}
			return displayNameUpgraded;
		}
	}

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(PotionSchema), tableName, id, iconType, true);
	}
}
