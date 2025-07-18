using UnityEngine;

public class DailyChallengeUnlockPopupImpl : MonoBehaviour, IGluiActionHandler
{
	public GluiStandardButtonContainer FacebookButton;

	private void Start()
	{
		/*if (FacebookButton != null)
		{
			FacebookButton.gameObject.SetActive(true);
		}*/
		Draw();
	}

	private void Update()
	{
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "FACEBOOK_DAILY_CHALLENGE_UNLOCKED")
		{
			string description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookUnlockMessage"), StringUtils.GetStringFromStringRef("LocalizedStrings", "GameMode_Daily_name"));
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookUnlockTitle"), description, FacebookButton.gameObject, string.Empty, string.Empty);
			return true;
		}
		return false;
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			FacebookButton.gameObject.SetActive(false);
		}
	}

	private void Draw()
	{
	}
}
