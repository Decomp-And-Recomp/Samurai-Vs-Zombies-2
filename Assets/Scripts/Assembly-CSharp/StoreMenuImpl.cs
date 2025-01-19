using System;
using System.Collections;
using System.Collections.Generic;
using Glu;
using UnityEngine;

public class StoreMenuImpl : SingletonMonoBehaviour<StoreMenuImpl>, IGluiActionHandler, IGluiDataSource
{
	public GameObject badge_newHeroes;

	public GameObject badge_newAllies;

	public GameObject bage_newExtras;

	public static bool ReturnedFromGame;

	public static string queuedPlayhavenRequest = string.Empty;

	private Dictionary<string, bool> hasViewedNewItem = new Dictionary<string, bool>();

	private bool tapjoyOpen;

	public bool HasViewedNewItem(string id)
	{
		bool value = false;
		hasViewedNewItem.TryGetValue(id, out value);
		return value;
	}

	public void SetHasViewedNewItem(string id)
	{
		hasViewedNewItem[id] = true;
	}

	private IEnumerator Start()
	{
		ApplicationUtilities._allowAutoSave = false;
		UpdateNewBadge(badge_newHeroes, StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), Singleton<HeroesDatabase>.Instance.AllIDs);
		UpdateNewBadge(badge_newAllies, "Helpers");
		UpdateNewBadge(bage_newExtras, "Consumables");
		yield return StartCoroutine(Profile.CheckForUpdates());
		if (SingletonSpawningMonoBehaviour<UpdateSystem>.Instance.DataBundlesModified)
		{
			Profile.ShowRestartPopup();
		}
		if (FrontEnd_HUD.Instance != null)
		{
			FrontEnd_HUD.SetDefenseRatingMode(false);
		}
		if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 2 && Singleton<Profile>.Instance.GetWaveLevel(2) == 1)
		{
			Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep8_StoreTutorial");
		}
		else if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 3 && Singleton<Profile>.Instance.GetWaveLevel(3) == 1 && !Singleton<Profile>.Instance.IsOnboardingStageComplete("OnboardingStep20_StoreMain2"))
		{
			Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep20_StoreMain2");
			if (PortableQualitySettings.GetQuality() != EPortableQualitySetting.Low)
			{
				AStats.MobileAppTracking.TrackAction("tutorial_complete");
			}
			ApplicationUtilities.MakePlayHavenContentRequest("tutorial_end");
		}
		bool customPlayHavenContentRequest = false;
		if (ReturnedFromGame)
		{
			ReturnedFromGame = false;
			if (Singleton<PlayStatistics>.Instance.data.waveTypePlayed == WaveManager.WaveType.Wave_SinglePlayer)
			{
				if (!Singleton<PlayStatistics>.Instance.data.victory)
				{
					int wave = Singleton<PlayStatistics>.Instance.data.wavePlayed;
					if (wave % 10 == 0)
					{
						string playHavenRequest2 = "wave_lost_" + wave;
						ApplicationUtilities.MakePlayHavenContentRequest(playHavenRequest2);
						customPlayHavenContentRequest = true;
					}
					else if (wave == 15 || wave == 25 || wave == 29 || wave == 49)
					{
						string playHavenRequest = "wave_lost_" + wave + "_paywall";
						ApplicationUtilities.MakePlayHavenContentRequest(playHavenRequest);
						customPlayHavenContentRequest = true;
					}
					else
					{
						ApplicationUtilities.MakePlayHavenContentRequest("wave_lost");
						customPlayHavenContentRequest = true;
					}
					WaveSchema wavePlayed = WaveManager.GetWaveData(Singleton<PlayStatistics>.Instance.data.wavePlayed, Singleton<Profile>.Instance.waveTypeToPlay);
					if (wavePlayed != null && wavePlayed.recommendedHeroIsRequired)
					{
						if (wavePlayed.recommendedHero.Key == "HeroBalanced")
						{
							ApplicationUtilities.MakePlayHavenContentRequest("required_samurai_lost");
							customPlayHavenContentRequest = true;
						}
						else if (wavePlayed.recommendedHero.Key == "HeroAttack")
						{
							ApplicationUtilities.MakePlayHavenContentRequest("required_kunoichi_lost");
							customPlayHavenContentRequest = true;
						}
						else if (wavePlayed.recommendedHero.Key == "HeroDefense")
						{
							ApplicationUtilities.MakePlayHavenContentRequest("required_ronin_lost");
							customPlayHavenContentRequest = true;
						}
					}
					if (Singleton<Profile>.Instance.GetHeroPurchased("HeroAbility") && !Singleton<Profile>.Instance.GetHeroPurchased("HeroLeadership"))
					{
						ApplicationUtilities.MakePlayHavenContentRequest("conditional_sorceress_wave_lost");
					}
					else if (!Singleton<Profile>.Instance.GetHeroPurchased("HeroAbility") && Singleton<Profile>.Instance.GetHeroPurchased("HeroLeadership"))
					{
						ApplicationUtilities.MakePlayHavenContentRequest("conditional_daimyo_wave_lost");
					}
				}
				if (string.IsNullOrEmpty(Singleton<Profile>.Instance.MultiplayerData.UserName))
				{
					ApplicationUtilities.MakePlayHavenContentRequest("wave_end_no_multiplayer");
					customPlayHavenContentRequest = true;
				}
			}
			else if (Singleton<PlayStatistics>.Instance.data.waveTypePlayed == WaveManager.WaveType.Wave_DailyChallenge)
			{
				if (!Singleton<PlayStatistics>.Instance.data.victory)
				{
					ApplicationUtilities.MakePlayHavenContentRequest("wave_lost_daily");
					customPlayHavenContentRequest = true;
				}
				else
				{
					int dailyWins = Singleton<Profile>.Instance.dailyChallengesCompleted;
					switch (dailyWins)
					{
					case 1:
						ApplicationUtilities.MakePlayHavenContentRequest("first_daily_win");
						break;
					case 5:
					case 10:
					case 15:
					case 25:
					case 50:
						ApplicationUtilities.MakePlayHavenContentRequest(dailyWins + "_daily_win");
						break;
					}
				}
			}
			else if (Singleton<PlayStatistics>.Instance.data.waveTypePlayed == WaveManager.WaveType.Wave_Multiplayer && !Singleton<PlayStatistics>.Instance.data.victory)
			{
				ApplicationUtilities.MakePlayHavenContentRequest("wave_lost_multiplayer");
				customPlayHavenContentRequest = true;
				if (!Singleton<Profile>.Instance.GetHeroPurchased("HeroLeadership"))
				{
					ApplicationUtilities.MakePlayHavenContentRequest("wave_lost_multiplayer_no_damiyo");
				}
				if (!Singleton<Profile>.Instance.GetHeroPurchased("HeroAbility"))
				{
					ApplicationUtilities.MakePlayHavenContentRequest("wave_lost_multiplayer_no_sorceress");
				}
			}
		}
		if (!string.IsNullOrEmpty(queuedPlayhavenRequest))
		{
			ApplicationUtilities.MakePlayHavenContentRequest(queuedPlayhavenRequest);
			queuedPlayhavenRequest = string.Empty;
			customPlayHavenContentRequest = true;
		}
		if (!customPlayHavenContentRequest)
		{
			if (ApplicationUtilities.PlayHavenGameLaunchQueued)
			{
				ApplicationUtilities.MakePlayHavenContentRequest("game_launch");
			}
			else
			{
				ApplicationUtilities.MakePlayHavenContentRequest("store_launch");
			}
		}
		GluiState_NarrativePanel.ShowIfSet();
		ApplicationUtilities._gameAtStartUp = false;
	}

	private void UpdateNewBadge(GameObject go, string firstKey, params string[] moreKeys)
	{
		if (go == null)
		{
			return;
		}
		object[] records;
		Get_GluiData(firstKey, null, null, out records);
		object[] array = records;
		for (int i = 0; i < array.Length; i++)
		{
			StoreData.Item item = (StoreData.Item)array[i];
			if (item.isNew)
			{
				go.SetActive(true);
				return;
			}
		}
		if (moreKeys != null)
		{
			foreach (string dataFilterKey in moreKeys)
			{
				Get_GluiData(dataFilterKey, null, null, out records);
				object[] array2 = records;
				for (int k = 0; k < array2.Length; k++)
				{
					StoreData.Item item2 = (StoreData.Item)array2[k];
					if (item2.isNew)
					{
						go.SetActive(true);
						return;
					}
				}
			}
		}
		go.SetActive(false);
	}

	public static void UpdateTapJoyPoints(GameObject sender)
	{
	}

	private void Update()
	{
		UpdateTapJoyPoints(base.gameObject);
		if (SingletonSpawningMonoBehaviour<GluIap>.Instance._storeNeedsRefresh)
		{
			SingletonSpawningMonoBehaviour<GluIap>.Instance.refreshRecommendations();
		}
		if (!tapjoyOpen && TapjoyInterface.InterfaceIsOpen())
		{
			tapjoyOpen = true;
		}
		else if (tapjoyOpen && !Tapjoy.isOfferwallOpen)
		{
			tapjoyOpen = false;
			ApplicationUtilities.MakePlayHavenContentRequest("tj_closed");
		}
		if (ApplicationUtilities.PlayHavenGameLaunchQueued)
		{
			ApplicationUtilities.MakePlayHavenContentRequest("game_launch");
		}
		Singleton<Achievements>.Instance.Update();
	}

	protected override void OnDestroy()
	{
		if (!ApplicationUtilities.HasShutdown)
		{
			Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep10_LeaveStore");
		}
		base.OnDestroy();
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "TAB_HEROES":
			badge_newHeroes.SetActive(false);
			break;
		case "TAB_ALLIES":
			badge_newAllies.SetActive(false);
			break;
		case "TAB_BOOSTS":
			bage_newExtras.SetActive(false);
			break;
		}
		return false;
	}

	public void Get_GluiData(string dataFilterKey, string dataFilterKeySecondary, GluiDataScan_AdditionalParameters additionalParameters, out object[] records)
	{
		List<object> list = new List<object>();
		switch (dataFilterKey)
		{
		case "Heroes":
			GetData_Heroes(list);
			break;
		case "Allies":
			GetData_Allies(list);
			break;
		case "PowerUps":
			GetData_PowerUps(list);
			break;
		default:
			list.AddRange(StoreAvailability.GetList(dataFilterKey).ConvertAll((Converter<StoreData.Item, object>)((StoreData.Item i) => i)));
			break;
		}
		records = list.ToArray();
	}

	private void GetData_Allies(List<object> results)
	{
		results.Add("Allies");
		if (StoreAvailability_Helpers.IsAnyChampionForSale())
		{
			results.Add("Champions");
		}
	}

	private void GetData_PowerUps(List<object> results)
	{
		results.Add("Consumables");
		results.Add("Upgrades");
		results.Add("Charms");
	}

	private void GetData_Heroes(List<object> results)
	{
		List<DataBundleRecordHandle<HeroSchema>> list = new List<DataBundleRecordHandle<HeroSchema>>(Singleton<HeroesDatabase>.Instance.AllHeroes);
		if (SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("HideUnpurchasedHeroes", false))
		{
			list.RemoveAll((DataBundleRecordHandle<HeroSchema> h) => h.Data.disabled || (h.Data.hideUntilUnlocked && h.Data.Locked));
		}
		else
		{
			list.RemoveAll((DataBundleRecordHandle<HeroSchema> h) => h.Data.disabled);
		}
		list.Sort(delegate(DataBundleRecordHandle<HeroSchema> x, DataBundleRecordHandle<HeroSchema> y)
		{
			bool flag = !(x.Data.purchaseToUnlock ^ y.Data.purchaseToUnlock);
			if (x.Data.Locked && y.Data.Locked)
			{
				if (flag)
				{
					return x.Data.waveToUnlock - y.Data.waveToUnlock;
				}
				if (y.Data.purchaseToUnlock)
				{
					return 1;
				}
				return -1;
			}
			if (x.Data.Locked)
			{
				return 1;
			}
			if (y.Data.Locked)
			{
				return -1;
			}
			if (flag)
			{
				return y.Data.waveToUnlock - x.Data.waveToUnlock;
			}
			return y.Data.purchaseToUnlock ? 1 : (-1);
		});
		int num = list.FindIndex((DataBundleRecordHandle<HeroSchema> s) => s.Data.Locked);
		results.AddRange(list.ConvertAll((Converter<DataBundleRecordHandle<HeroSchema>, object>)((DataBundleRecordHandle<HeroSchema> s) => s)));
		if (num > 0)
		{
			results.Insert(num, StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"));
		}
		else
		{
			results.Add(StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"));
		}
	}
}
