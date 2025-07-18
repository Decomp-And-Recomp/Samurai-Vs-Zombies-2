using System;
using UnityEngine;

public class CharacterUnlockPopupImpl : MonoBehaviour, IGluiActionHandler
{
	public GluiStandardButtonContainer FacebookButton;

	private void Start()
	{
		ResultsMenuImpl.UnlockedFeature unlockedHero = ResultsMenuImpl.GetUnlockedHero();
		if (unlockedHero == null)
		{
			/*if (FacebookButton != null)
			{
				FacebookButton.gameObject.SetActive(false);
			}*/
			throw new Exception("Invalid Hero Unlock Data.");
		}
		/*if (FacebookButton != null)
		{
			FacebookButton.gameObject.SetActive(true);
		}*/
		Draw(unlockedHero);
	}

	private void Update()
	{
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "FACEBOOK_HERO_UNLOCKED")
		{
			string description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookUnlockMessage"), ResultsMenuImpl.GetUnlockedHero().text);
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

	private void Draw(ResultsMenuImpl.UnlockedFeature unlockEntry)
	{
		GluiSprite gluiSprite = base.gameObject.FindChildComponent<GluiSprite>("Swap_Icon_Hero");
		gluiSprite.Texture = unlockEntry.icon;
		GluiText gluiText = base.gameObject.FindChildComponent<GluiText>("SwapText_HeroName");
		gluiText.Text = unlockEntry.text;
		GluiText gluiText2 = base.gameObject.FindChildComponent<GluiText>("SwapText_HeroDescription");
		gluiText2.Text = unlockEntry.description;
	}
}
