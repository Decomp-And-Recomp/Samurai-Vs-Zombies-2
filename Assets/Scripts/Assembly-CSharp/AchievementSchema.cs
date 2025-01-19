using UnityEngine;

[DataBundleClass]
public class AchievementSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey displayName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey description;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey completedDescription;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.FrontEnd)]
	public Texture2D Icon;

	[DataBundleDefaultValue(1)]
	public int CompletionCount;

	public int AchievementPoints;

	public string GameCenterID;

	public Achievements.AchievementType achievementType;
}
