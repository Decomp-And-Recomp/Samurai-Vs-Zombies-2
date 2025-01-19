using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.Plugins.ASocial;
using UnityEngine;

public class ApplicationUtilities : SingletonSpawningMonoBehaviour<ApplicationUtilities>
{
	private bool mIsInitialized;

	private static int localNotificationDelay = 259200;

	private static string localNotificationMessage = "Placeholder! Set using ApplicationUtilities.LocalNotificationMessage";

	private float timeScaleBackup = Time.timeScale;

	public static bool _autoSave;

	public static bool _allowAutoSave = false;

	public bool canAwardGems;

	public bool isFacebookFriendsRead;

	public bool isDelaySetFacebookID;

	public bool isFacebookLoggedIn;

	public Action<string, string, string, string> facebookOnDoneLogin;

	public string post_title = string.Empty;

	public string post_description = string.Empty;

	public string post_imageLink = string.Empty;

	public string post_link = string.Empty;

	public AchievementTracker post_achievement;

	public GameObject post_facebookButton;

	public static bool _gameAtStartUp = true;

	public static bool _restoreCloudSaveButtonPressed = false;

	private SntpTime mSNTPTime;

	private static Action mOnPlayHavenError;

	public Action<bool> OnPause;

	public static Hashtable iapPrices = new Hashtable();

	public static bool HasShutdown { get; private set; }

	public static bool InitializationComplete { get; private set; }

	public static bool ConnectionAlertIsShown { get; private set; }

	public static bool ShowConnectionErrors { get; set; }

	public static bool IsPlaying { get; private set; }

	public static string UserID
	{
		get
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
	}

	public static DateTime Now
	{
		get
		{
			return SntpTime.UniversalTime;
		}
	}

	public SntpTime SNTPTime
	{
		get
		{
			return mSNTPTime;
		}
	}

	public static string LocalNotificationMessage
	{
		get
		{
			return localNotificationMessage;
		}
		set
		{
			localNotificationMessage = value;
		}
	}

	public static int LocalNotificationDelay
	{
		get
		{
			return localNotificationDelay;
		}
		set
		{
			localNotificationDelay = value;
		}
	}

	public static bool PlayHavenGameLaunchQueued { get; set; }

	public static bool PausedForIAP { get; set; }

	public static bool OnPlatform()
	{
		return Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android;
	}

	public static void ShowInternetErrorAlert()
	{
	}

	public static float DeviceYSize()
	{
		return 1024f;
	}

	public static float DeviceXSize()
	{
		return (float)Screen.width / (float)Screen.height * DeviceYSize();
	}

	protected override void Awake()
	{
		Glu.Plugins.ASocial.Facebook.FacebookLoginHandler += onFacebookLoginComplete;
		Glu.Plugins.ASocial.Facebook.FacebookFeedPostCompleteHandler += onFacebookFeedPostComplete;
		base.Awake();
	}

	public void Init()
	{
		if (mIsInitialized)
		{
			return;
		}
		mIsInitialized = true;
		IsPlaying = Application.isPlaying;
		mSNTPTime = new SntpTime();
		ANotificationManager.Init(string.Empty);
		AStats.Init();
		if (PlayerPrefs.GetInt("autoSave", -1) == -1)
		{
			PlayerPrefs.SetInt("autoSave", 1);
		}
		Glu.Plugins.ASocial.Facebook.Init();
		PlayGameServices.Init();
		if (AJavaTools.Properties.IsBuildAmazon())
		{
			Amazon.Init((Amazon.GameCircleFeatures)6);
		}
		StartCoroutine(Init3rdPartyPlugins());
		isDelaySetFacebookID = PlayerPrefs.GetInt("DELAY_SET_FACEBOOK_ID") == 1;
		isFacebookLoggedIn = Glu.Plugins.ASocial.Facebook.IsLoggedIn();
	}

	private IEnumerator Init3rdPartyPlugins()
	{
		if (!GeneralConfig.IsLive)
		{
		}
		Singleton<Analytics>.Instance.ApplicationStart();
		PlayHavenGameLaunchQueued = true;
		if (Amazon.IsSignedIn())
		{
			Singleton<Achievements>.Instance.OnLogin();
		}
		AStats.MobileAppTracking.Init();
		mSNTPTime.UpdateTimeOffset();
		InitializationComplete = true;
		yield break;
	}

	public static void StoreOpened()
	{
		MakePlayHavenContentRequest("bank_launch");
	}

	public static void MakePlayHavenContentRequest(string requestType)
	{
		MakePlayHavenContentRequest(requestType, null);
	}

	public static void MakePlayHavenContentRequest(string requestType, Action onError)
	{
		if (GeneralConfig.PlayHavenEnabled)
		{
			if (requestType == "game_launch")
			{
				PlayHavenGameLaunchQueued = false;
			}
		}
	}

	private void OnApplicationPause(bool state)
	{
		if (!state)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				if (GeneralConfig.PlayHavenEnabled && !PausedForIAP)
				{
					PlayHavenGameLaunchQueued = true;
				}
				if (SingletonSpawningMonoBehaviour<GluIap>.Exists && !PausedForIAP)
				{
					SingletonSpawningMonoBehaviour<GluIap>.Instance.UpdateProductData();
				}
				Singleton<Analytics>.Instance.ApplicationResume();
				ANotificationManager.ClearActiveNotifications();
				StringUtils.ClearCultureInfo();
			}
		}
		else
		{
			Singleton<Analytics>.Instance.ApplicationPause();
			if (mSNTPTime != null)
			{
				mSNTPTime.UpdateTimeOffset();
			}
			if (AJavaTools.Properties.IsBuildAmazon())
			{
				Amazon.SyncOnExit();
			}
		}
		if (OnPause != null)
		{
			OnPause(state);
		}
	}

	private void FixedUpdate()
	{
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized && isDelaySetFacebookID && GripNetwork.Ready)
		{
			Singleton<Profile>.Instance.MultiplayerData.DelayedGamespyUpdate();
			PlayerPrefs.SetInt("DELAY_SET_FACEBOOK_ID", 0);
			isDelaySetFacebookID = false;
		}
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized && !isFacebookFriendsRead && isFacebookLoggedIn && GripNetwork.Ready)
		{
			AndroidFacebookFriendsRead();
		}
	}

	protected override void OnDestroy()
	{
		Glu.Plugins.ASocial.Facebook.FacebookLoginHandler -= onFacebookLoginComplete;
		Glu.Plugins.ASocial.Facebook.FacebookFeedPostCompleteHandler -= onFacebookFeedPostComplete;
		PlayGameServices.onCloudLoadSuccessHandler -= onCloudLoadSuccess;
		PlayGameServices.onCloudConflictHandler -= onCloudConflict;
		PlayGameServices.onCloudLoadNoDataHandler -= onCloudLoadNoData;
		PlayGameServices.onConnectedToPGSHandler -= onGPGS_SignInSuccess;
		PlayGameServices.onDisconnectedFromPGSHandler -= onGPGS_SignOutSuccess;
		PlayGameServices.onConnectionToPGSFailed -= onGPGS_SignOutSuccess;
		PlayGameServices.onLoadedFriendsListHandler -= onGPGS_FriendsLoaded;
		PlayGameServices.LoadFriends();
		PlayGameServices.onLoadedFriendsListHandler -= onGPGS_FriendsLoaded;
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		OnApplicationPause(true);
		HasShutdown = true;
		if (Singleton<Profile>.Instance != null)
		{
			Singleton<Profile>.Instance.FlurrySession.ReportSessionEnd();
		}
		Singleton<Analytics>.Instance.ApplicationExit();
		GluIap instance = SingletonSpawningMonoBehaviour<GluIap>.Instance;
		instance.OnStateChange = (Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>)Delegate.Remove(instance.OnStateChange, new Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>(OnIAPStateChange));
		Logging.Destroy();
		base.OnApplicationQuit();
	}

	private void OnIAPStateChange(ICInAppPurchase.TRANSACTION_STATE state, string productID, bool fromVGP)
	{
		if (OnPlatform())
		{
		}
	}

	private void onGWalletBillingRecommendationsChanged()
	{
		if (SingletonSpawningMonoBehaviour<GluIap>.Instance != null)
		{
			SingletonSpawningMonoBehaviour<GluIap>.Instance.refreshRecommendations();
		}
	}

	private void onGWalletBalanceChanged()
	{
	}

	private void onFacebookLoginComplete(object sender, FacebookEventArgs args)
	{
		if (args.GetStatus() == FacebookEventArgs.FaceBookLoginStatus.Success)
		{
			GameObject gameObject = GameObject.Find("Button_FaceBook");
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			StartCoroutine(onFacebookLoginTasks());
		}
		else if (args.GetStatus() != FacebookEventArgs.FaceBookLoginStatus.Cancelled && args.GetStatus() != FacebookEventArgs.FaceBookLoginStatus.Failed)
		{
		}
	}

	private void onFacebookFeedPostComplete(object sender, FacebookFeedEventArgs args)
	{
		if (args.GetStatus() != FacebookFeedEventArgs.FacebookFeedStatus.Success && args.GetStatus() != FacebookFeedEventArgs.FacebookFeedStatus.Cancelled && args.GetStatus() != FacebookFeedEventArgs.FacebookFeedStatus.Failed)
		{
		}
	}

	public void AndroidFacebookFriendsRead()
	{
		try
		{
			string text = "facebookid IN (";
			int num = 0;
			List<Dictionary<string, string>> friends = Glu.Plugins.ASocial.Facebook.GetFriendsWithApp();
			foreach (Dictionary<string, string> item in friends)
			{
				if (num > 0)
				{
					text += ", ";
				}
				text = text + "'" + item["uid"] + "'";
				num++;
			}
			text += ")";
			string[] fieldNames = new string[3] { "ownerid", "gamecenterid", "facebookid" };
			GripNetwork.SearchRecords("UserData", fieldNames, text, string.Empty, null, num, 1, delegate(GripNetwork.Result result, GripField[,] data)
			{
				if (result == GripNetwork.Result.Success && data != null)
				{
					for (int i = 0; i < data.GetLength(0); i++)
					{
						if (data[i, 0].mInt.HasValue)
						{
							string friendName = string.Empty;
							foreach (Dictionary<string, string> item2 in friends)
							{
								if (item2["uid"] == data[i, 2].mString)
								{
									friendName = item2["name"];
									break;
								}
							}
							Singleton<Profile>.Instance.MultiplayerData.AddFriend(data[i, 0].mInt.Value, data[i, 1].mString, data[i, 2].mString, friendName, true);
						}
					}
				}
				else if (data != null)
				{
				}
			});
			isFacebookFriendsRead = true;
		}
		catch (Exception)
		{
		}
	}

	public void AndroidFacebookFeed(string title, string description, GameObject facebookButton, string link = "", string imageLink = "", AchievementTracker sharingAchievement = null)
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			MultiplayerData.NetworkRequiredDialog();
		}
		if (link == string.Empty)
		{
			link = MultiplayerData.FacebookLink;
		}
		if (imageLink == string.Empty)
		{
			imageLink = MultiplayerData.FacebookImageLink;
		}
		if (Glu.Plugins.ASocial.Facebook.IsLoggedIn())
		{
			Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string> dictionary = new Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string>();
			dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Name, title);
			dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Description, description);
			dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Picture, imageLink);
			dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Link, link);
			Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string> parameters = dictionary;
			Glu.Plugins.ASocial.Facebook.Post(parameters);
			if (facebookButton != null)
			{
				facebookButton.SetActive(false);
			}
			if (sharingAchievement != null)
			{
				sharingAchievement.shared = true;
			}
		}
		else
		{
			post_title = title;
			post_description = description;
			post_imageLink = imageLink;
			post_link = link;
			post_facebookButton = facebookButton;
			post_achievement = sharingAchievement;
			AndroidFacebookLogIn(PostAfterLoginSuccess);
		}
	}

	public void AndroidGoogleFriendsRead()
	{
		string text = "androidid IN (";
		int num = 0;
		List<PlayGameServices.Friend> friends = PlayGameServices.FriendsList;
		if (friends == null)
		{
			return;
		}
		foreach (PlayGameServices.Friend item in friends)
		{
			if (num > 0)
			{
				text += ", ";
			}
			text = text + "'" + item.ID + "'";
			num++;
		}
		text += ")";
		string[] fieldNames = new string[3] { "ownerid", "gamecenterid", "androidid" };
		GripNetwork.SearchRecords("UserData", fieldNames, text, string.Empty, null, num, 1, delegate(GripNetwork.Result result, GripField[,] data)
		{
			if (result == GripNetwork.Result.Success && data != null)
			{
				for (int i = 0; i < data.GetLength(0); i++)
				{
					if (data[i, 0].mInt.HasValue)
					{
						string friendName = string.Empty;
						foreach (PlayGameServices.Friend item2 in friends)
						{
							if (item2.ID == data[i, 2].mString)
							{
								friendName = item2.DisplayName;
								break;
							}
						}
						Singleton<Profile>.Instance.MultiplayerData.AddFriend(data[i, 0].mInt.Value, data[i, 2].mString, null, friendName, true);
					}
				}
			}
			else if (data != null)
			{
			}
		});
	}

	public void AndroidFacebookLogIn(Action<string, string, string, string> onDone = null)
	{
		if (onDone != null)
		{
			facebookOnDoneLogin = onDone;
		}
		Glu.Plugins.ASocial.Facebook.Login();
	}

	private void PostAfterLoginSuccess(string title, string description, string link, string imageLink)
	{
		Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string> dictionary = new Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string>();
		dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Name, title);
		dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Description, description);
		dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Picture, imageLink);
		dictionary.Add(Glu.Plugins.ASocial.Facebook.FeedType.Link, link);
		Dictionary<Glu.Plugins.ASocial.Facebook.FeedType, string> parameters = dictionary;
		Glu.Plugins.ASocial.Facebook.Post(parameters);
		if ((bool)post_facebookButton)
		{
			post_facebookButton.SetActive(false);
		}
		if (post_achievement != null)
		{
			post_achievement.shared = true;
		}
	}


	public void awardTapjoy(int amount)
	{
	}

	public void performAutoSave()
	{
		if (_allowAutoSave && PlayGameServices.IsSignedIn)
		{
			PlayGameServices.SaveToCloud(readSaveFile(AJavaTools.GameInfo.GetFilesPath() + "/local"));
		}
		_allowAutoSave = false;
	}

	public byte[] readSaveFile(string path)
	{
		using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using (BinaryReader binaryReader = new BinaryReader(fileStream))
			{
				return binaryReader.ReadBytes((int)fileStream.Length);
			}
		}
	}

	public void writeSaveFile(string path, byte[] data)
	{
		using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(data);
			}
		}
	}

	private void onCloudLoadSuccess(object sender, EventArgs args)
	{
		_restoreCloudSaveButtonPressed = false;
		byte[] array = PlayGameServices.GetcloudSave();
		AJavaTools.UI.ShowAlert(base.gameObject.name, "onGameResetPopup", StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_RESTORE_GAME_TITLE"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_SAVE_FILE_SUCCESSFULLY_LOADED_ANDROID") + "\n\n" + StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_GAME_RESTARTED"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_RESTART"), StringUtils.GetStringFromStringRef("LocalizedStrings", "cancel"), string.Empty);
	}

	private void onCloudConflict(object sender, EventArgs args)
	{
		AJavaTools.UI.ShowAlert(base.gameObject.name, "onCloudLoadConflictPopup", StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_SAVE_GAME_CONFLICT_TITLE"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_SAVE_GAME_CONFLICT_DESC"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_ANDROID"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_LOCAL"), string.Empty);
	}

	private void onCloudLoadNoData(object sender, EventArgs args)
	{
		if (_restoreCloudSaveButtonPressed)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.icloud_file_not_found_notification_message_text");
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
			GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
		}
		_restoreCloudSaveButtonPressed = false;
	}

	private void onCloudLoadConflictPopup(string info)
	{
		int num = Convert.ToInt32(info);
		_restoreCloudSaveButtonPressed = false;
		switch (num)
		{
		case -1:
			AJavaTools.UI.ShowAlert(base.gameObject.name, "onGameResetPopup", StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_RESTORE_GAME_TITLE"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_SAVE_FILE_SUCCESSFULLY_LOADED_ANDROID") + "\n\n" + StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_GAME_RESTARTED"), StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_RESTART"), StringUtils.GetStringFromStringRef("LocalizedStrings", "cancel"), string.Empty);
			break;
		}
	}

	private void onGameResetPopup(string info)
	{
		int num = Convert.ToInt32(info);
		_restoreCloudSaveButtonPressed = false;
		switch (num)
		{
		case -1:
		{
			PlayGameServices.ResolveConflict(true);
			byte[] data = PlayGameServices.GetcloudSave();
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.writeSaveFile(AJavaTools.GameInfo.GetFilesPath() + "/local.restore", data);
			AJavaTools.Util.RelaunchGame();
			break;
		}
		}
	}

	private void onGPGS_SignInSuccess(object sender, EventArgs args)
	{
		if (StartScreenImpl._leaderboardsButton != null && StartScreenImpl._achievementsButton != null && StartScreenImpl._googlePlusButton != null)
		{
			StartScreenImpl._leaderboardsButton.gameObject.SetActive(true);
			StartScreenImpl._achievementsButton.gameObject.SetActive(true);
			StartScreenImpl._iCloudButton.gameObject.SetActive(true);
			StartScreenImpl._googlePlusButton.gameObject.SetActive(false);
		}
		if (Singleton<Profile>.Instance != null)
		{
			Singleton<Profile>.Instance.MultiplayerData.SetAndroidID(PlayGameServices.GetPlayerID());
		}
		PlayGameServices.LoadFriends();
		if (PlayerPrefs.GetInt("loadCloudSaveOnFirstLaunch", 0) == 0)
		{
			PlayerPrefs.SetInt("loadCloudSaveOnFirstLaunch", 1);
			PlayGameServices.LoadFromCloud();
		}
		Singleton<Achievements>.Instance.ReportOfflineProgress();
	}

	private void onGPGS_SignOutSuccess(object sender, EventArgs args)
	{
		if (StartScreenImpl._leaderboardsButton != null && StartScreenImpl._achievementsButton != null && StartScreenImpl._googlePlusButton != null)
		{
			StartScreenImpl._leaderboardsButton.gameObject.SetActive(false);
			StartScreenImpl._achievementsButton.gameObject.SetActive(false);
			StartScreenImpl._iCloudButton.gameObject.SetActive(false);
			StartScreenImpl._googlePlusButton.gameObject.SetActive(true);
		}
	}

	private void onGPGS_FriendsLoaded(object sender, EventArgs args)
	{
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidGoogleFriendsRead();
	}

	private void onGPGS_ConnectionToPGSFailed(object sender, EventArgs args)
	{
		if (PlayGameServices.IsSignedIn)
		{
			if (StartScreenImpl._leaderboardsButton != null && StartScreenImpl._achievementsButton != null && StartScreenImpl._googlePlusButton != null)
			{
				StartScreenImpl._leaderboardsButton.gameObject.SetActive(true);
				StartScreenImpl._achievementsButton.gameObject.SetActive(true);
				StartScreenImpl._iCloudButton.gameObject.SetActive(true);
				StartScreenImpl._googlePlusButton.gameObject.SetActive(false);
				Singleton<Profile>.Instance.MultiplayerData.SetAndroidID(PlayGameServices.GetPlayerID());
			}
		}
		else if (StartScreenImpl._leaderboardsButton != null && StartScreenImpl._achievementsButton != null && StartScreenImpl._googlePlusButton != null)
		{
			StartScreenImpl._leaderboardsButton.gameObject.SetActive(false);
			StartScreenImpl._achievementsButton.gameObject.SetActive(false);
			StartScreenImpl._iCloudButton.gameObject.SetActive(false);
			StartScreenImpl._googlePlusButton.gameObject.SetActive(true);
		}
	}

	private IEnumerator onFacebookLoginTasks()
	{
		float seconds = 5f;
		yield return new WaitForSeconds(seconds);
		try
		{
			if (Singleton<Profile>.Instance.MultiplayerData.FacebookID != Glu.Plugins.ASocial.Facebook.GetID())
			{
				Singleton<Profile>.Instance.MultiplayerData.SetFacebookID(Glu.Plugins.ASocial.Facebook.GetID());
			}
		}
		catch (Exception)
		{
		}
		if (facebookOnDoneLogin != null)
		{
			try
			{
				facebookOnDoneLogin(post_title, post_description, post_link, post_imageLink);
			}
			catch (Exception ex)
			{
				Exception e = ex;
			}
			facebookOnDoneLogin = null;
		}
		yield return null;
	}
}
