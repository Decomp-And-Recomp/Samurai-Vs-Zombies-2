using System;
using UnityEngine;

public class ModeSelectImpl : MonoBehaviour
{
	private const string kDailyChallengeButtonName = "Button_DailyChallenge";

	public GluiStandardButtonContainer[] modeButtons;

	public GameObject DailyChallengeCollectedParent;

	public GameObject MPShieldParent;

	public GluiText MPShieldTimerText;

	private static ModeSelectImpl smInstance;

	public static ModeSelectImpl Instance
	{
		get
		{
			return smInstance;
		}
	}

	private void Start()
	{
		bool flag = Singleton<Profile>.Instance.JustUnlockedDailyChallenge();
		smInstance = this;
		if (flag && !Singleton<Profile>.Instance.DisplayedDailyChallengeUnlock)
		{
			Singleton<Profile>.Instance.DisplayedDailyChallengeUnlock = true;
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("GAME_MODE", "Button_DailyChallenge");
			GameObject gameObject = base.gameObject.FindChild("Locator_UnlockFX");
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/SelectSuite/FX_UnlockDailyChallenge.prefab", 1);
			GameObject gameObject2 = GameObjectPool.DefaultObjectPool.Acquire(cachedResource.Resource as GameObject, gameObject.transform.position, Quaternion.identity);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			EffectKiller.AddKiller(gameObject2, GameObjectPool.DefaultObjectPool);
		}
		if (DailyChallengeCollectedParent != null)
		{
			DailyChallengeCollectedParent.SetActive(Singleton<Profile>.Instance.CompletedTodaysDailyChallenge);
		}
		UpdateModeButtons();
		/*if (Singleton<Profile>.Instance.souls == Singleton<Profile>.Instance.GetMaxSouls() && !Singleton<Profile>.Instance.HasSeenSoulJarFullPopup && !flag)
		{
			GluiActionSender.SendGluiAction("POPUP_SOUL_JAR_FULL", base.gameObject, null);
			Singleton<Profile>.Instance.HasSeenSoulJarFullPopup = true;
		}*/
		UpdateShield();
	}

	public void UpdateModeButtons()
	{
		string value = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("GAME_MODE") as string;
		bool flag = Singleton<Profile>.Instance.UnlockedDailyChallenge();
		GluiStandardButtonContainer[] array = modeButtons;
		foreach (GluiStandardButtonContainer gluiStandardButtonContainer in array)
		{
			if (gluiStandardButtonContainer.name.Equals("Button_DailyChallenge") && !flag)
			{
				gluiStandardButtonContainer.Enabled = false;
				gluiStandardButtonContainer.Locked = true;
			}
			GluiWidget[] componentsInChildren = gluiStandardButtonContainer.GetComponentsInChildren<GluiWidget>(false);
			foreach (GluiWidget gluiWidget in componentsInChildren)
			{
				gluiWidget.Start();
			}
			gluiStandardButtonContainer.Selected = gluiStandardButtonContainer.name.Equals(value);
		}
	}

	public void UpdateShield()
	{
		TimeSpan timeSpan = Singleton<Profile>.Instance.MultiplayerShieldExpireTime - SntpTime.UniversalTime;
		if (timeSpan.TotalMinutes > 0.0)
		{
			MPShieldParent.SetActive(true);
			int num = (int)timeSpan.TotalDays;
			int num2 = (int)timeSpan.TotalHours % 24;
			string text = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings.mpShieldTime"), num, num2);
			MPShieldTimerText.Text = text;
		}
		else
		{
			MPShieldParent.SetActive(false);
			base.enabled = false;
		}
	}
}
