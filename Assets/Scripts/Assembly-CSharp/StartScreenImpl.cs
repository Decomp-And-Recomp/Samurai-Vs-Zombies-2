using System;
using System.Collections;
using Glu.Plugins.ASocial;
using UnityEngine;

public class StartScreenImpl : MonoBehaviour, IGluiActionHandler
{
	public GluiStandardButtonContainer FacebookButton;

	public GluiSprite spriteGameTitle;

	private string titleLogoFileName = "Assets/Game/Resources/UI/textures/StartScreenTemp/Title_English.png";

	private float timeScaleBackup = Time.timeScale;

	public static GameObject _leaderboardsButton;

	public static GameObject _achievementsButton;

	public static GameObject _gameCenterButton;

	public static GameObject _googlePlusButton;

	public static GameObject _playerButton;

	public static GameObject _facebookButton;

	public static GameObject _iCloudButton;

	private void Awake()
	{
		if (!Singleton<Profile>.Exists)
		{
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
	}

	private IEnumerator Start()
	{
		string language = BundleUtils.GetSystemLanguage();
		if (language != "English")
		{
			string titleOverrideName = null;
			switch (language)
			{
			case "Chinese (Simplified)":
				titleOverrideName = "Title_ChineseSimplified.png";
				break;
			case "Chinese (Traditional)":
				titleOverrideName = "Title_ChineseTraditional.png";
				break;
			case "Japanese":
				titleOverrideName = "Title_Japanese.png";
				break;
			case "Korean":
				titleOverrideName = "Title_Korean.png";
				break;
			}
			if (titleOverrideName != null)
			{
				ResourceCache.UnCache(titleLogoFileName);
				titleLogoFileName = "Assets/Game/Resources/UI/textures/StartScreenTemp/" + titleOverrideName;
				SharedResourceLoader.SharedResource res = ResourceCache.GetCachedResource(titleLogoFileName, 1);
				spriteGameTitle.Start();
				spriteGameTitle.Texture = res.Resource as Texture2D;
			}
		}
		while (!Singleton<Profile>.Instance.Initialized)
		{
			yield return null;
		}
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep2_StartScreen");
		if (!GeneralConfig.IsLive)
		{
			bool iCloudEnabled = FileManager.CheckCloudStorageAvailability();
			string iCloudPath = FileManager.GetCloudContainerDirectoryPath();
		}
		if (Facebook.IsInitialized() && Facebook.IsLoggedIn() && FacebookButton != null)
		{
			FacebookButton.Visible = false;
			UnityEngine.Object.Destroy(FacebookButton.gameObject);
		}
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.canAwardGems = true;
		_leaderboardsButton = GameObject.Find("Button_Leaderboard");
		_achievementsButton = GameObject.Find("Button_Achievement");
		_gameCenterButton = GameObject.Find("Button_GameCenter");
		_iCloudButton = GameObject.Find("Button_iCloud");
		_facebookButton = GameObject.Find("Button_FaceBook");
		Vector3 _currentPosition2 = _facebookButton.transform.localPosition;
		_leaderboardsButton.transform.localPosition = new Vector3(_currentPosition2.x, _currentPosition2.y, _currentPosition2.z);
		_achievementsButton.transform.localPosition = new Vector3(_currentPosition2.x + 140f, _currentPosition2.y, _currentPosition2.z);
		_iCloudButton.transform.localPosition = new Vector3(_currentPosition2.x + 280f, _currentPosition2.y, _currentPosition2.z);
		_facebookButton.transform.localPosition = new Vector3(_currentPosition2.x, _currentPosition2.y + 280f, _currentPosition2.z);
		_leaderboardsButton.gameObject.SetActive(false);
		_achievementsButton.gameObject.SetActive(false);
		_iCloudButton.gameObject.SetActive(false);
		_gameCenterButton.gameObject.SetActive(false);
		GameObject _anchor = GameObject.Find("Anchor_LeftBottom");
		_googlePlusButton = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("UI/Prefabs/StartScreen/Button_GooglePlus"));
		_googlePlusButton.transform.parent = _anchor.transform;
		_googlePlusButton.transform.localPosition = new Vector3(_currentPosition2.x + 200f, _googlePlusButton.transform.localPosition.y, _googlePlusButton.transform.localPosition.z);
		_playerButton = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("UI/Prefabs/StartScreen/Button_PGS"));
		_playerButton.transform.parent = _anchor.transform;
		_playerButton.transform.localPosition = new Vector3(_currentPosition2.x, _currentPosition2.y + 140f, _currentPosition2.z);
		_playerButton.SetActive(false);
		if (AJavaTools.Properties.IsBuildAmazon())
		{
			_gameCenterButton.gameObject.SetActive(true);
			if (_gameCenterButton != null)
			{
				_gameCenterButton.FindChildComponent<GluiSprite>("Art_GameCenter").Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Button_GameCircle", 1).Resource as Texture2D;
				_currentPosition2 = _gameCenterButton.transform.localPosition;
				_gameCenterButton.transform.localPosition = new Vector3(_currentPosition2.x - 420f, _currentPosition2.y, _currentPosition2.z);
			}
		}
		else if (!AJavaTools.Properties.IsBuildAmazon())
		{
			_leaderboardsButton.FindChildComponent<GluiSprite>("Art_Leaderboard").Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Button_Leaderboards_PGS", 1).Resource as Texture2D;
			_achievementsButton.FindChildComponent<GluiSprite>("Art_Achievement").Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Button_Achievements_PGS", 1).Resource as Texture2D;
			_googlePlusButton.gameObject.SetActive(true);
		}
		PlayerPrefs.SetInt("gameLoadedCorrectly", 1);
		PlayerPrefs.SetString("gameTag", AJavaTools.Properties.GetBuildTag());
		if (PlayerPrefs.GetInt("pgsSignIn", 0) == 0)
		{
			AJavaTools.UI.ShowAlert(base.gameObject.name, "pgsSignInCallback", StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_G_SI_REQUIRED"), StringUtils.GetStringFromStringRef("LocalizedStrings", "SI_GOOGLE_CLOUD"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_SIGN_IN"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_SKIP"), string.Empty);
			Time.timeScale = 0f;
			PlayerPrefs.SetInt("pgsSignIn", 1);
		}
		if (AJavaTools.Properties.GetBuildType() == "tstore" && Singleton<Profile>.Instance != null)
		{
		}
	}

	private void Update()
	{
		if (ApplicationUtilities.PlayHavenGameLaunchQueued)
		{
			ApplicationUtilities.MakePlayHavenContentRequest("game_launch");
		}
	}

	private void OnDestroy()
	{
		ResourceCache.UnCache(titleLogoFileName);
	}

	private void OnFBLogin(bool success)
	{
		if (FacebookButton != null)
		{
			FacebookButton.Visible = !success;
			if (success)
			{
				UnityEngine.Object.Destroy(FacebookButton.gameObject);
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (!Singleton<Profile>.Instance.Initialized)
		{
			return true;
		}
		switch (action)
		{
		case "START_GAME":
			if (Singleton<Profile>.Instance.GetWaveLevel(1) == 1)
			{
				Singleton<Profile>.Instance.wave_SinglePlayerGame = 1;
				string value = DataBundleRuntime.Instance.GetValue<string>(typeof(WaveSchema), Singleton<PlayModesManager>.Instance.selectedModeData.waves.RecordTable, Singleton<Profile>.Instance.wave_SinglePlayerGame.ToString(), "scene", false);
				LoadingScreen.LoadLevel(value);
			}
			else
			{
				GluiActionSender.SendGluiAction("MENU_MAIN_STORE", sender, data);
			}
			return true;
		case "FACEBOOK_LOGIN":
			FacebookButton.Visible = false;
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				AJavaTools.UI.ShowToast(StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_REQUIRE_INTERNET_ANDROID"));
			}
			else
			{
				Glu.Plugins.ASocial.Facebook.Login();
			}
			return true;
		case "GAMECENTER_LOGIN":
			if (AJavaTools.Properties.IsBuildAmazon())
			{
				Amazon.ShowLeaderBoards();
			}
			else if (!AJavaTools.Properties.IsBuildAmazon())
			{
				if (Application.internetReachability == NetworkReachability.NotReachable)
				{
					AJavaTools.UI.ShowToast(StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_REQUIRE_INTERNET_ANDROID"));
				}
			}
			return true;
		case "POPUP_ACHIEVEMENTS":
			return true;
		case "POPUP_LEADERBOARDS":
			return true;
		default:
			return false;
		}
	}

	private void pgsSignInCallback(string info)
	{
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				AJavaTools.UI.ShowToast(StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_REQUIRE_INTERNET_ANDROID"));
			}
		}
		AJavaTools.UI.ShowNotificationPrompt(base.gameObject.name, "setNotification");
	}

	private void setNotification(string info)
	{
		switch (Convert.ToInt32(info))
		{
		case -1:
			ANotificationManager.SetEnabled(true);
			PlayerPrefs.SetInt("PUSH_NOTIFICATIONS_DISABLED", 0);
			AStats.Flurry.LogEvent("NOTIFICATION_SETTING_CHANGED_TO", "true");
			break;
		case -2:
			ANotificationManager.SetEnabled(false);
			PlayerPrefs.SetInt("PUSH_NOTIFICATIONS_DISABLED", 1);
			AStats.Flurry.LogEvent("NOTIFICATION_SETTING_CHANGED_TO", "false");
			break;
		}
		PlayerPrefs.Save();
		Time.timeScale = timeScaleBackup;
	}

	private void OnApplicationPause(bool state)
	{
		if (!state && !AJavaTools.Properties.IsBuildAmazon())
		{
			StartCoroutine(checkButtons());
		}
	}

	private IEnumerator checkButtons()
	{
		yield return new WaitForSeconds(0.25f);
		if (_leaderboardsButton != null && _achievementsButton != null && _googlePlusButton != null && _iCloudButton != null)
		{
			_leaderboardsButton.gameObject.SetActive(false);
			_achievementsButton.gameObject.SetActive(false);
			_iCloudButton.gameObject.SetActive(false);
			_playerButton.gameObject.SetActive(false);
			_googlePlusButton.gameObject.SetActive(true);
		}
	}
}
