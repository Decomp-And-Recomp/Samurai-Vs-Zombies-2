using System;
using System.Collections.Generic;
using Glu.Plugins.ASocial;
using UnityEngine;

public class ResultsMenuImpl : UIHandler<ResultsMenuImpl>, IGluiActionHandler
{
	public class UnlockedFeature
	{
		public string id;

		public Texture2D icon;

		public string text;

		public string description;
	}

	public GluiStandardButtonContainer FacebookButton;

	public GluiText FacebookButtonText;

	private float kVictoryAnimSpeed = 1.2f;

	public static bool CheckedForCompletedCollections;

	private GluiText mVictoryGoldLabel;

	private float mVictoryGoldAwarded;

	private float mVictoryGoldAnimTimer;

	private int mLastDisplayedVictoryGold = -1;

	private GluiText mVictorySoulsLabel;

	private float mVictorySoulsAwarded;

	private float mVictorySoulsAnimTimer;

	private int mLastDisplayedVictorySouls = -1;

	private bool mCheckForUnlocks = true;

	private bool mCheckForRateMe = true;

	private bool showSoulTutorial = true;

	private string mFacebookInfoString;

	public override void Awake()
	{
		base.Awake();
		if (!Singleton<Profile>.Exists)
		{
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
	}

	public override void Start()
	{
		CheckedForCompletedCollections = false;
		base.Start();
		if (FacebookButton != null)
		{
			FacebookButton.gameObject.SetActive(false);
		}
		DrawPage();
		Singleton<Achievements>.Instance.SuppressPartialReporting(false);
		mCheckForRateMe = !Singleton<Profile>.Instance.neverShowRateMeAlertAgain;
		if (!Singleton<PlayStatistics>.Instance.data.victory)
		{
			SingletonSpawningMonoBehaviour<UMusicManager>.Instance.PlayByKey("MusicEvents.Sting_Game_Lose_01");
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateVictoryGoldAnimation();
		UpdateVictorySoulsAnimation();
		if (!Singleton<Profile>.Exists || !Singleton<Profile>.Instance.Initialized || Singleton<Profile>.Instance.MultiplayerData == null || Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			return;
		}
		if (mCheckForUnlocks)
		{
			mCheckForUnlocks = false;
			if (IsGettingMysteryBoxFromThisWave())
			{
				MysteryBoxImpl.BoxID = "MysteryBox";
				GluiActionSender.SendGluiAction("POPUP_MYSTERY_BOX", base.gameObject, null);
				if (Singleton<Profile>.Instance.inDailyChallenge)
				{
					Singleton<Profile>.Instance.lastCompletedDailyChallenge = Profile.GetDailyChallengeDaysSinceStart();
					int dailyChallengesCompleted = Singleton<Profile>.Instance.dailyChallengesCompleted + 1;
					Singleton<Profile>.Instance.dailyChallengesCompleted = dailyChallengesCompleted;
					if (!AJavaTools.Properties.IsBuildAmazon() && PlayGameServices.IsSignedIn)
					{
						PlayGameServices.SubmitScore(Profile.kDailyChallengeLeaderboard, Singleton<Profile>.Instance.dailyChallengesCompleted);
					}
				}
			}
			if (!Singleton<Profile>.Instance.inDailyChallenge)
			{
				if (GetUpgradeUnlockedFeatures(true) != null)
				{
					GluiActionSender.SendGluiAction("POPUP_UPGRADE_UNLOCKED", base.gameObject, null);
				}
				if (GetUnlockedHero() != null)
				{
					GluiActionSender.SendGluiAction("POPUP_CHARACTER_UNLOCKED", base.gameObject, null);
				}
				if (JustUnlockedDailyChallenge())
				{
					GluiActionSender.SendGluiAction("POPUP_DAILYCHALLENGE_UNLOCKED", base.gameObject, null);
				}
			}
		}
		else if (mCheckForRateMe)
		{
			mCheckForRateMe = false;
			if (Singleton<PlayStatistics>.Instance.data.victory && Singleton<PlayStatistics>.Instance.data.shouldShowRateMeDialog && AJavaTools.Properties.GetBuildType() != "tstore")
			{
				string text = StringUtils.GetStringFromStringRef("LocalizedStrings", "like_our_game");
				if (AJavaTools.Properties.IsBuildGoogle())
				{
					text = text + " " + AJavaTools.UI.GetString("you_will_also_receive_5_glu_credits_ex");
				}
				AJavaTools.UI.ShowAlert(base.gameObject.name, "onRateMeClicked", StringUtils.GetStringFromStringRef("LocalizedStrings", "rate_me_title"), text, StringUtils.GetStringFromStringRef("LocalizedStrings", "rate_it"), StringUtils.GetStringFromStringRef("LocalizedStrings", "cancel_rating"), string.Empty);
			}
		}
		else if (showSoulTutorial)
		{
			GluiPopupQueueMachine component = GameObject.Find("Machine_Popups").GetComponent<GluiPopupQueueMachine>();
			if (component == null || component.IsCurrentDefaultState)
			{
				showSoulTutorial = false;
				GluiActionSender.SendGluiAction("TUTORIAL_RESULT_SOUL_TRIGGER", base.gameObject, null);
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "FACEBOOK_STOLEN":
		{
			string description2 = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookStealMessage"), StringUtils.GetStringFromStringRef(MultiplayerGlobalHelpers.GetSelectedCard().displayName));
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookStealTitle"), description2, FacebookButton.gameObject, string.Empty, string.Empty);
			return true;
		}
		case "FACEBOOK_BOSS_WAVE":
		{
			string description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookBossMessage"), mFacebookInfoString);
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookBossTitle"), description, FacebookButton.gameObject, string.Empty, string.Empty);
			return true;
		}
		case "FACEBOOK_DAILY_CHALLENGE":
		{
			string stringFromStringRef = StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookDailyChallengeMessage");
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookDailyChallengeTitle"), stringFromStringRef, FacebookButton.gameObject, string.Empty, string.Empty);
			return true;
		}
		default:
			return false;
		}
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			FacebookButton.gameObject.SetActive(false);
		}
	}

	public static List<UnlockedFeature> GetUpgradeUnlockedFeatures(bool doPlayHavenRequest)
	{
		int wavePlayed = Singleton<PlayStatistics>.Instance.data.wavePlayed;
		if (!Singleton<PlayStatistics>.Instance.data.victory || Singleton<Profile>.Instance.GetWaveLevel(wavePlayed) != 2)
		{
			return null;
		}
		List<UnlockedFeature> list = new List<UnlockedFeature>();
		string[] allIDs = Singleton<AbilitiesDatabase>.Instance.allIDs;
		foreach (string id in allIDs)
		{
			AbilitySchema abilitySchema = Singleton<AbilitiesDatabase>.Instance[id];
			if (abilitySchema.levelToUnlock == (float)(wavePlayed + 1))
			{
				UnlockedFeature unlockedFeature = new UnlockedFeature();
				unlockedFeature.id = id;
				unlockedFeature.icon = abilitySchema.icon;
				unlockedFeature.text = StringUtils.GetStringFromStringRef(abilitySchema.displayName);
				list.Add(unlockedFeature);
			}
		}
		string[] allIDs2 = Singleton<HelpersDatabase>.Instance.allIDs;
		foreach (string id2 in allIDs2)
		{
			HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[id2];
			if (helperSchema.waveToUnlock == wavePlayed + 1)
			{
				UnlockedFeature unlockedFeature2 = new UnlockedFeature();
				unlockedFeature2.id = id2;
				unlockedFeature2.icon = helperSchema.HUDIcon;
				unlockedFeature2.text = StringUtils.GetStringFromStringRef(helperSchema.displayName);
				list.Add(unlockedFeature2);
				if (doPlayHavenRequest && !string.IsNullOrEmpty(helperSchema.unlockPlayhavenRequest))
				{
					ApplicationUtilities.MakePlayHavenContentRequest(helperSchema.unlockPlayhavenRequest);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public static bool JustUnlockedDailyChallenge()
	{
		return Singleton<Profile>.Instance.JustUnlockedDailyChallenge();
	}

	public static UnlockedFeature GetUnlockedHero()
	{
		int wavePlayed = Singleton<PlayStatistics>.Instance.data.wavePlayed;
		if (!Singleton<PlayStatistics>.Instance.data.victory || Singleton<Profile>.Instance.GetWaveLevel(wavePlayed) != 2)
		{
			return null;
		}
		string[] allIDs = Singleton<HeroesDatabase>.Instance.AllIDs;
		foreach (string id in allIDs)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[id];
			if (heroSchema.waveToUnlock == wavePlayed + 1)
			{
				UnlockedFeature unlockedFeature = new UnlockedFeature();
				unlockedFeature.id = id;
				unlockedFeature.icon = heroSchema.storePortrait;
				unlockedFeature.text = StringUtils.GetStringFromStringRef(heroSchema.displayName);
				unlockedFeature.description = StringUtils.GetStringFromStringRef(heroSchema.desc);
				if (heroSchema.unlockAchievement != null)
				{
					Singleton<Achievements>.Instance.IncrementAchievement(heroSchema.unlockAchievement.Key, 1);
				}
				if (!string.IsNullOrEmpty(heroSchema.unlockPlayhavenRequest))
				{
					ApplicationUtilities.MakePlayHavenContentRequest(heroSchema.unlockPlayhavenRequest);
				}
				return unlockedFeature;
			}
		}
		return null;
	}

	private void DrawPage()
	{
		if (!Singleton<PlayStatistics>.Instance.data.victory)
		{
			GameObject gameObject = base.gameObject.FindChild("ScrollList_Loot");
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		try
		{
			if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				base.gameObject.FindChild("Header_SinglePlayer").SetActive(false);
			}
			else
			{
				base.gameObject.FindChild("Header_Multiplayer").SetActive(false);
				GluiText gluiText = base.gameObject.FindChildComponent<GluiText>("Text_Wave");
				if (Singleton<Profile>.Instance.inDailyChallenge)
				{
					gluiText.Text = StringUtils.GetStringFromStringRef(Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.waveDisplayName);
				}
				else
				{
					gluiText.Text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "debrief_title"), Singleton<PlayStatistics>.Instance.data.wavePlayed);
				}
				if (Singleton<PlayStatistics>.Instance.data.victory && FacebookButton != null)
				{
					if (Singleton<Profile>.Instance.inDailyChallenge)
					{
						FacebookButton.gameObject.SetActive(true);
						FacebookButton.onReleaseActions = new string[1] { "FACEBOOK_DAILY_CHALLENGE" };
					}
					else
					{
						WaveSchema waveData = WaveManager.GetWaveData(Singleton<PlayStatistics>.Instance.data.wavePlayed, WaveManager.WaveType.Wave_SinglePlayer);
						if (waveData != null && !string.IsNullOrEmpty(waveData.BossEnemy.Key))
						{
							FacebookButton.onReleaseActions = new string[1] { "FACEBOOK_BOSS_WAVE" };
							EnemySchema enemySchema = Singleton<EnemiesDatabase>.Instance[waveData.BossEnemy.Key];
							if (enemySchema != null)
							{
								mFacebookInfoString = StringUtils.GetStringFromStringRef(enemySchema.displayName);
								FacebookButton.gameObject.SetActive(true);
							}
						}
					}
				}
			}
		}
		catch (Exception)
		{
		}
		try
		{
			if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				GameObject parent = base.gameObject.FindChild("Header_Multiplayer");
				GameObject gameObject2 = parent.FindChild("SwapText_ArtifactStatus_Negative");
				GameObject gameObject3 = parent.FindChild("Text_ArtifactFailed2");
				GameObject gameObject4 = parent.FindChild("SwapText_ArtifactStatus_Defended");
				GameObject gameObject5 = parent.FindChild("SwapText_ArtifactStatus_Collected");
				gameObject5.SetActive(Singleton<PlayStatistics>.Instance.data.victory && Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending);
				gameObject4.SetActive(Singleton<PlayStatistics>.Instance.data.victory && Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.gameMode == EMultiplayerMode.kDefending);
				gameObject2.SetActive(!Singleton<PlayStatistics>.Instance.data.victory && Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.gameMode == EMultiplayerMode.kDefending);
				gameObject3.SetActive(!Singleton<PlayStatistics>.Instance.data.victory && Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending);
				if (FacebookButton != null && Singleton<PlayStatistics>.Instance.data.victory && Singleton<Profile>.Instance.MultiplayerData.LastOpponentFriendData != null && !string.IsNullOrEmpty(Singleton<Profile>.Instance.MultiplayerData.LastOpponentFriendData.facebookID))
				{
					FacebookButton.gameObject.SetActive(true);
					FacebookButton.onReleaseActions = new string[1] { "FACEBOOK_STOLEN" };
					if (FacebookButtonText != null)
					{
						FacebookButtonText.TaggedStringReference = "LocalizedStrings.FacebokTauntButton";
					}
				}
			}
			else
			{
				GluiText gluiText2 = base.gameObject.FindChildComponent<GluiText>("SwapText_Victory");
				GluiText gluiText3 = base.gameObject.FindChildComponent<GluiText>("SwapText_Loss");
				gluiText2.gameObject.SetActive(Singleton<PlayStatistics>.Instance.data.victory);
				gluiText3.gameObject.SetActive(!Singleton<PlayStatistics>.Instance.data.victory);
			}
		}
		catch (Exception)
		{
		}
		try
		{
			GameObject gameObject6 = base.gameObject.FindChild("Text_Awarded");
			mVictoryGoldLabel = gameObject6.FindChildComponent<GluiText>("SwapText_AwardedQty");
			mVictoryGoldAwarded = Singleton<PlayStatistics>.Instance.data.inGameGoldGained;
			UpdateVictoryGoldAnimation();
			if (!Singleton<PlayStatistics>.Instance.data.victory)
			{
				gameObject6.GetComponent<GluiText>().Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "debrief_collected");
			}
			Singleton<Profile>.Instance.coins += Singleton<PlayStatistics>.Instance.data.inGameGoldGained;
		}
		catch (Exception)
		{
		}
		try
		{
			GameObject gameObject7 = base.gameObject.FindChild("Text_Souls");
			GameObject gameObject8 = base.gameObject.FindChild("Text_SoulsFull");
			mVictorySoulsLabel = gameObject7.FindChildComponent<GluiText>("SwapText_SoulsQty");
			if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				gameObject7.SetActive(false);
				gameObject8.SetActive(false);
			}
			else if (Singleton<PlayStatistics>.Instance.data.victory)
			{
				mVictorySoulsAwarded = 0f;
				if (Singleton<PlayStatistics>.Instance.data.inGameSoulsAwarded > 0)
				{
					if (Singleton<PlayStatistics>.Instance.data.inGameSoulsAwarded == Singleton<PlayStatistics>.Instance.data.inGameSoulsGained)
					{
						mVictorySoulsAwarded = Singleton<PlayStatistics>.Instance.data.inGameSoulsGained;
						gameObject8.SetActive(false);
					}
					else
					{
						gameObject7.SetActive(false);
						mVictorySoulsAwarded = -1f;
					}
				}
				else
				{
					gameObject8.SetActive(false);
				}
				UpdateVictorySoulsAnimation();
			}
			else
			{
				mVictorySoulsAwarded = 0f;
				UpdateVictorySoulsAnimation();
				gameObject7.SetActive(false);
				gameObject8.SetActive(false);
			}
		}
		catch (Exception)
		{
		}
	}

	private void UpdateVictoryGoldAnimation()
	{
		if ((mVictoryGoldAwarded != 0f && mVictoryGoldAnimTimer != 1f) || mLastDisplayedVictoryGold < 0)
		{
			mVictoryGoldAnimTimer = Mathf.Min(1f, mVictoryGoldAnimTimer + Time.deltaTime / kVictoryAnimSpeed);
			int num = (int)(mVictoryGoldAnimTimer * mVictoryGoldAwarded);
			if (num != mLastDisplayedVictoryGold)
			{
				mLastDisplayedVictoryGold = num;
				mVictoryGoldLabel.Text = num.ToString();
			}
		}
	}

	private void UpdateVictorySoulsAnimation()
	{
		if (mVictorySoulsAwarded != -1f && ((mVictorySoulsAwarded != 0f && mVictorySoulsAnimTimer != 1f) || mLastDisplayedVictorySouls < 0))
		{
			mVictorySoulsAnimTimer = Mathf.Min(1f, mVictorySoulsAnimTimer + Time.deltaTime / kVictoryAnimSpeed);
			int num = (int)(mVictorySoulsAnimTimer * mVictorySoulsAwarded);
			if (num != mLastDisplayedVictorySouls)
			{
				mLastDisplayedVictorySouls = num;
				mVictorySoulsLabel.Text = num.ToString();
			}
		}
	}

	[MonoPInvokeCallback(typeof(NUF.RateMeDelegate))]
	private static void OnNeverShowAlertAgain(int buttonIndex)
	{
		if (buttonIndex == 1 || buttonIndex == 2)
		{
			Singleton<Profile>.Instance.neverShowRateMeAlertAgain = true;
			Singleton<Profile>.Instance.Save();
		}
	}

	private static bool IsGettingMysteryBoxFromThisWave()
	{
		if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			return false;
		}
		if (!Singleton<PlayStatistics>.Instance.data.victory)
		{
			return false;
		}
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			return Singleton<Profile>.Instance.lastCompletedDailyChallenge != Profile.GetDailyChallengeDaysSinceStart();
		}
		if (Singleton<PlayStatistics>.Instance.data.shouldAwardMysteryBox)
		{
			return Singleton<PlayStatistics>.Instance.data.wavePlayedLevel == 1;
		}
		return false;
	}

	private void onRateMeClicked(string button)
	{
		int num = Convert.ToInt32(button);
		if (num == -1)
		{
			if (!AJavaTools.Properties.IsBuildGoogle() || AJavaTools.Properties.GetBuildType().Equals("facebook"))
			{
				Singleton<Profile>.Instance.AddGems(5, "IncentivizedRateMe");
				Singleton<Profile>.Instance.purchasedGems += 5;
			}
			Application.OpenURL(AJavaTools.Internet.GetGameURL());
			AStats.Flurry.LogEvent("GameRated");
			Singleton<Profile>.Instance.neverShowRateMeAlertAgain = true;
			Singleton<Profile>.Instance.Save();
		}
	}
}
