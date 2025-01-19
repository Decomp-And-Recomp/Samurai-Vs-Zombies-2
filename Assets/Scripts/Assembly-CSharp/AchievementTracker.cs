using UnityEngine.SocialPlatforms;

public class AchievementTracker
{
	public IAchievement unityAchievement;

	public IAchievementDescription description;

	public DataBundleRecordHandle<AchievementSchema> achievement;

	public int completedCount;

	public float progress;

	public bool dirty;

	public bool shared = true;
}
