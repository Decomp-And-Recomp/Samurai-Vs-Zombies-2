using System.Collections.Generic;
using UnityEngine;

public class AchievementHandler : MonoBehaviour, IGluiActionHandler
{
	private AchievementTracker mSharingAchievement;

	private GameObject mFacebookButton;

	private void Start()
	{
		List<HeroSchema> list = new List<HeroSchema>();
		string[] allIDs = Singleton<HeroesDatabase>.Instance.AllIDs;
		foreach (string id in allIDs)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[id];
			if (Singleton<Profile>.Instance.GetHeroPurchased(id) && heroSchema.unlockAchievement != null)
			{
				Singleton<Achievements>.Instance.IncrementAchievement(heroSchema.unlockAchievement.Key, 1);
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action.Contains("FACEBOOK_SHARE_"))
		{
			string id = action.Substring(15, action.Length - 15);
			AchievementTracker achievement = Singleton<Achievements>.Instance.GetAchievement(id);
			if (achievement != null)
			{
				mSharingAchievement = achievement;
				mFacebookButton = sender;
				string description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookCollectionMessage"), StringUtils.GetStringFromStringRef(achievement.achievement.Data.displayName));
				SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookCollectionTitle"), description, mFacebookButton, string.Empty, string.Empty, mSharingAchievement);
			}
			return true;
		}
		return false;
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			if (mFacebookButton != null)
			{
				mFacebookButton.SetActive(false);
			}
			if (mSharingAchievement != null)
			{
				mSharingAchievement.shared = true;
			}
		}
	}
}
