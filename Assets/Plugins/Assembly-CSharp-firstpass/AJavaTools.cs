using System;
using System.Collections.Generic;
using System.IO;
using Glu.Plugins.AJavaTools_Private;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

public class AJavaTools : MonoBehaviour
{
	public static class Backup
	{
		public static void DataChanged()
		{
			AJTB_DataChanged();
		}

		public static void RequestRestore()
		{
			AJTB_RequestRestore();
		}

		public static void RequestRestoreIfNoData()
		{
			if (Util.IsFirstLaunch() && !Util.IsDataRestored())
			{
				RequestRestore();
			}
		}
	}

	public static class DeviceInfo
	{
		public const int SCREENLAYOUT_SIZE_SMALL = 1;

		public const int SCREENLAYOUT_SIZE_NORMAL = 2;

		public const int SCREENLAYOUT_SIZE_LARGE = 3;

		public const int SCREENLAYOUT_SIZE_XLARGE = 4;

		private static int GetScreenWidth_value;

		private static int GetScreenHeight_value;

		private static int GetScreenLayout_value;

		private static double GetScreenDiagonalInches_value;

		private static string GetDeviceLanguage_value;

		private static string GetDeviceCountry_value;

		private static int GetDeviceSDKVersion_value;

		private static bool IsDeviceRooted_checked;

		private static bool IsDeviceRooted_value;

		private static string GetAndroidID_value;

		private static string GetExternalStorageDirectory_value;

		private static bool IsGluDebug_checked;

		private static bool IsGluDebug_value;

		public static int GetScreenWidth()
		{
			if (GetScreenWidth_value == 0)
			{
				GetScreenWidth_value = AJTDI_GetScreenWidth();
			}
			return GetScreenWidth_value;
		}

		public static int GetScreenHeight()
		{
			if (GetScreenHeight_value == 0)
			{
				GetScreenHeight_value = AJTDI_GetScreenHeight();
			}
			return GetScreenHeight_value;
		}

		public static int GetScreenLayout()
		{
			if (GetScreenLayout_value == 0)
			{
				GetScreenLayout_value = AJTDI_GetScreenLayout();
			}
			return GetScreenLayout_value;
		}

		public static double GetScreenDiagonalInches()
		{
			if (GetScreenDiagonalInches_value == 0.0)
			{
				GetScreenDiagonalInches_value = AJTDI_GetScreenDiagonalInches();
			}
			return GetScreenDiagonalInches_value;
		}

		public static bool IsTablet()
		{
			return GetScreenDiagonalInches() >= 6.0;
		}

		public static string GetDeviceLanguage()
		{
			if (GetDeviceLanguage_value == null)
			{
				GetDeviceLanguage_value = AJTDI_GetDeviceLanguage();
			}
			return GetDeviceLanguage_value;
		}

		public static string GetDeviceCountry()
		{
			if (GetDeviceCountry_value == null)
			{
				GetDeviceCountry_value = AJTDI_GetDeviceCountry();
			}
			return GetDeviceCountry_value;
		}

		public static string GetDeviceLocale()
		{
			string text = GetDeviceLanguage();
			if (text.Equals("zh"))
			{
				text += GetDeviceCountry().ToLower();
			}
			else if (text.Equals("pt"))
			{
				text += GetDeviceCountry().ToLower();
			}
			return text;
		}

		public static void SetDeviceLocale(string language, string country = "")
		{
			AJTDI_SetDeviceLocale(language, country);
		}

		public static int GetDeviceSDKVersion()
		{
			if (GetDeviceSDKVersion_value == 0)
			{
				GetDeviceSDKVersion_value = AJTDI_GetDeviceSDKVersion();
			}
			return GetDeviceSDKVersion_value;
		}

		public static bool IsExternalStorageMounted()
		{
			return AJTDI_IsExternalStorageMounted();
		}

		public static bool IsDeviceRooted()
		{
			if (!IsDeviceRooted_checked)
			{
				IsDeviceRooted_checked = true;
				IsDeviceRooted_value = AJTDI_IsDeviceRooted();
			}
			return IsDeviceRooted_value;
		}

		public static string GetAndroidID()
		{
			if (GetAndroidID_value == null)
			{
				GetAndroidID_value = AJTDI_GetAndroidID();
			}
			return GetAndroidID_value;
		}

		public static string GetExternalStorageDirectory()
		{
			if (GetExternalStorageDirectory_value == null)
			{
				GetExternalStorageDirectory_value = AJTDI_GetExternalStorageDirectory();
			}
			return GetExternalStorageDirectory_value;
		}

		public static bool IsGluDebug()
		{
			if (!IsGluDebug_checked)
			{
				IsGluDebug_checked = true;
				IsGluDebug_value = AJTDI_IsGluDebug();
			}
			return IsGluDebug_value;
		}
	}

	public static class GameInfo
	{
		private static string GetPackageName_value;

		private static string GetVersionName_value;

		private static int GetVersionCode_value;

		private static string GetFilesPath_value;

		private static string GetExternalFilesPath_value;

		public static string GetPackageName()
		{
			if (GetPackageName_value == null)
			{
				GetPackageName_value = AJTGI_GetPackageName();
			}
			return GetPackageName_value;
		}

		public static string GetVersionName()
		{
			if (GetVersionName_value == null)
			{
				GetVersionName_value = AJTGI_GetVersionName();
			}
			return GetVersionName_value;
		}

		public static int GetVersionCode()
		{
			if (GetVersionCode_value == 0)
			{
				GetVersionCode_value = AJTGI_GetVersionCode();
			}
			return GetVersionCode_value;
		}

		public static int GetVersionMajor()
		{
			try
			{
				return Convert.ToInt32(GetVersionName().Split('.')[0]);
			}
			catch (Exception)
			{
				return 1;
			}
		}

		public static int GetVersionMinor()
		{
			try
			{
				return Convert.ToInt32(GetVersionName().Split('.')[1]);
			}
			catch (Exception)
			{
				return 0;
			}
		}

		public static int GetVersionMicro()
		{
			try
			{
				return Convert.ToInt32(GetVersionName().Split('.')[2]);
			}
			catch (Exception)
			{
				return 0;
			}
		}

		public static string GetFilesPath()
		{
			if (GetFilesPath_value == null)
			{
				GetFilesPath_value = AJTGI_GetFilesPath();
			}
			return GetFilesPath_value;
		}

		public static string GetExternalFilesPath()
		{
			if (GetExternalFilesPath_value == null)
			{
				GetExternalFilesPath_value = AJTGI_GetExternalFilesPath();
			}
			return GetExternalFilesPath_value;
		}
	}

	public static class Internet
	{
		public static void LoadWebView(string url, string gameObjectName = "")
		{
			AJTI_LoadWebView(url, gameObjectName);
		}

		public static string GetGameURL()
		{
			if (Properties.GetBuildType().Equals("google") || Properties.GetBuildType().Equals("facebook"))
			{
				return "market://details?id=" + GameInfo.GetPackageName();
			}
			if (Properties.GetBuildType().Equals("amazon"))
			{
				return "amzn://apps/android?p=" + GameInfo.GetPackageName();
			}
			return "http://m.glu.com";
		}

		public static string GetMoreGamesURL()
		{
			if (Properties.GetBuildType().Equals("google"))
			{
				return "market://search?q=pub:%22Glu%20Mobile%22";
			}
			if (Properties.GetBuildType().Equals("amazon"))
			{
				return "amzn://apps/android?p=" + GameInfo.GetPackageName() + "&showAll=1";
			}
			return "http://m.glu.com";
		}
	}

	public static class UI
	{
		public const int GRAVITY_BOTTOM = 80;

		public const int GRAVITY_CENTER = 17;

		public const int GRAVITY_CENTER_HORIZONTAL = 1;

		public const int GRAVITY_CENTER_VERTICAL = 16;

		public const int GRAVITY_LEFT = 3;

		public const int GRAVITY_RIGHT = 5;

		public const int GRAVITY_TOP = 48;

		public const int BUTTON_POSITIVE = -1;

		public const int BUTTON_NEGATIVE = -2;

		public const int BUTTON_NEUTRAL = -3;

		public static void ShowToast(string message)
		{
			AJTUI_ShowToast(message);
		}

		public static void StartIndeterminateProgress(int gravity)
		{
			AJTUI_StartIndeterminateProgress(gravity);
		}

		public static void StopIndeterminateProgress()
		{
			AJTUI_StopIndeterminateProgress();
		}

		public static void ShowAlert(string title, string message, string button)
		{
			AJTUI_ShowAlert(string.Empty, string.Empty, title, message, button, string.Empty, string.Empty);
		}

		public static void ShowAlert(string gameObjectName, string callbackName, string title, string message, string buttonPositive, string buttonNegative = "", string buttonNeutral = "")
		{
			AJTUI_ShowAlert(gameObjectName, callbackName, title, message, buttonPositive, buttonNegative, buttonNeutral);
		}

		public static void SetAlertGravity(int gravity, int xOffset, int yOffset)
		{
			AJTUI_SetAlertGravity(gravity, xOffset, yOffset);
		}

		public static void SetAlertDimBehind(bool flag)
		{
			AJTUI_SetAlertDimBehind(flag);
		}

		public static void SetAlertModeless(bool flag)
		{
			AJTUI_SetAlertModeless(flag);
		}

		public static void CancelAlert()
		{
			AJTUI_CancelAlert();
		}

		public static void ShowNotificationPrompt(string gameObjectName, string callbackName)
		{
			AJTUI_ShowNotificationPrompt(gameObjectName, callbackName);
		}

		public static void ShowExitPrompt(string gameObjectName = "", string callbackName = "")
		{
			AJTUI_ShowExitPrompt(gameObjectName, callbackName);
		}

		public static string GetString(string key, params string[] replace)
		{
			return AJTUI_GetString(key, replace);
		}
	}

	public static class Util
	{
		private static string GetOBBDownloadPlan_value;

		private static bool IsDataRestored_checked;

		private static bool IsDataRestored_value;

		private static int GetRunCount_value = -1;

		private static int GetRunCountThisVersion_value = -1;

		public static bool LaunchGame(string packageName, string altURL = "")
		{
			return AJTU_LaunchGame(packageName, altURL);
		}

		public static void RelaunchGame()
		{
			AJTU_RelaunchGame();
		}

		public static string GetOBBDownloadPlan()
		{
			if (GetOBBDownloadPlan_value == null)
			{
				GetOBBDownloadPlan_value = AJTU_GetOBBDownloadPlan();
			}
			return GetOBBDownloadPlan_value;
		}

		public static void LogEventOBB()
		{
			string oBBDownloadPlan = GetOBBDownloadPlan();
			if (!oBBDownloadPlan.Equals("old"))
			{
				if (IsFirstLaunch())
				{
					Debug.Log("OBB Flurry: ANDROID_OBB_FIRST_TIME_INSTALL_SUCCEEDED: " + oBBDownloadPlan);
					AStats.Flurry.LogEvent("ANDROID_OBB_FIRST_TIME_INSTALL_SUCCEEDED", oBBDownloadPlan);
				}
				else
				{
					Debug.Log("OBB Flurry: ANDROID_OBB_UPGRADE_SUCCEEDED: " + oBBDownloadPlan);
					AStats.Flurry.LogEvent("ANDROID_OBB_UPGRADE_SUCCEEDED", oBBDownloadPlan);
				}
			}
		}

		public static bool IsDataRestored()
		{
			if (!IsDataRestored_checked)
			{
				IsDataRestored_checked = true;
				IsDataRestored_value = AJTU_IsDataRestored();
			}
			return IsDataRestored_value;
		}

		public static void LogEventDataRestored()
		{
			if (IsDataRestored())
			{
				Debug.Log("OBB Flurry: ANDROID_DATA_RESTORED");
				AStats.Flurry.LogEvent("ANDROID_DATA_RESTORED");
			}
		}

		public static int GetRunCount()
		{
			if (GetRunCount_value == -1)
			{
				GetRunCount_value = AJTU_GetRunCount();
			}
			return GetRunCount_value;
		}

		public static bool IsFirstLaunch()
		{
			return GetRunCount() == 0;
		}

		public static int GetRunCountThisVersion()
		{
			if (GetRunCountThisVersion_value == -1)
			{
				GetRunCountThisVersion_value = AJTU_GetRunCountThisVersion();
			}
			return GetRunCountThisVersion_value;
		}

		public static bool IsFirstLaunchThisVersion()
		{
			return GetRunCountThisVersion() == 0;
		}

		public static void SendBroadcast(string action, string uri = "")
		{
			AJTU_SendBroadcast(action, uri);
		}

		public static void VerifySignature()
		{
			AJTU_VerifySignature();
		}
	}

	public static class Properties
	{
		private static string GetBuildType_value;

		private static string GetBuildTag_value;

		private static string GetBuildLocale_value;

		private static string GetAppPublicKey_value;

		private static string GetTapjoyAppID_value;

		private static string GetTapjoySecretKey_value;

		private static string GetTapjoyPPASubscription_value;

		private static string GetPlayHavenToken_value;

		private static string GetPlayHavenSecret_value;

		private static string GetAmazonAdAppID_value;

		private static string GetDefaultAdProvider_value;

		private static string GetFacebookAppID_value;

		private static string GetFlurryKey_value;

		private static string GetKontagentKey_value;

		private static string GetMobileAppTrackingPackage_value;

		private static string GetMobileAppTrackingKey_value;

		private static string GetGWalletSKU_value;

		private static string GetGGNSRC_value;

		private static string GetIAPOrdering_value;

		private static string GetGATrackingID_value;

		private static string GetChartBoostAppID_value;

		private static string GetChartBoostAppSignature_value;

		public static string GetProperty(string key, string defaultValue = null)
		{
			string text = AJTU_GetProperty(key);
			return (text != null) ? text : defaultValue;
		}

		public static int GetPropertyInt(string key, int defaultValue = -1)
		{
			string text = AJTU_GetProperty(key);
			return (text != null) ? Convert.ToInt32(text) : defaultValue;
		}

		public static bool GetPropertyBool(string key, bool defaultValue = false)
		{
			string text = AJTU_GetProperty(key);
			return (text != null) ? Convert.ToBoolean(text) : defaultValue;
		}

		public static string GetBuildType()
		{
			if (GetBuildType_value == null)
			{
				GetBuildType_value = GetProperty("BUILD_TYPE", "google");
			}
			return GetBuildType_value;
		}

		public static bool IsBuildGoogle()
		{
			return GetBuildType().Equals("google");
		}

		public static bool IsBuildAmazon()
		{
			return GetBuildType().Equals("amazon");
		}

		public static bool IsBuildTStore()
		{
			return GetBuildType().Equals("tstore");
		}

		public static string GetBuildTag()
		{
			if (GetBuildTag_value == null)
			{
				GetBuildTag_value = GetProperty("BUILD_TAG");
			}
			return GetBuildTag_value;
		}

		public static string GetBuildLocale()
		{
			if (GetBuildLocale_value == null)
			{
				GetBuildLocale_value = GetProperty("BUILD_LOCALE", "default");
			}
			return GetBuildLocale_value;
		}

		public static bool IsLocaleDefault()
		{
			return GetBuildLocale().Equals("default");
		}

		public static string GetAppPublicKey()
		{
			if (GetAppPublicKey_value == null)
			{
				GetAppPublicKey_value = GetProperty("APP_PUBLIC_KEY", string.Empty);
			}
			return GetAppPublicKey_value;
		}

		public static string GetTapjoyAppID()
		{
			if (GetTapjoyAppID_value == null)
			{
				GetTapjoyAppID_value = GetProperty("TAPJOY_APPID", string.Empty);
			}
			return GetTapjoyAppID_value;
		}

		public static string GetTapjoySecretKey()
		{
			if (GetTapjoySecretKey_value == null)
			{
				GetTapjoySecretKey_value = GetProperty("TAPJOY_SECRETKEY", string.Empty);
			}
			return GetTapjoySecretKey_value;
		}

		public static string GetTapjoyPPASubscription()
		{
			if (GetTapjoyPPASubscription_value == null)
			{
				GetTapjoyPPASubscription_value = GetProperty("TAPJOY_PPA_SUBSCRIPTION", string.Empty);
			}
			return GetTapjoyPPASubscription_value;
		}

		public static string GetPlayHavenToken()
		{
			if (GetPlayHavenToken_value == null)
			{
				GetPlayHavenToken_value = GetProperty("PLAYHAVEN_TOKEN", string.Empty);
			}
			return GetPlayHavenToken_value;
		}

		public static string GetPlayHavenSecret()
		{
			if (GetPlayHavenSecret_value == null)
			{
				GetPlayHavenSecret_value = GetProperty("PLAYHAVEN_SECRET", string.Empty);
			}
			return GetPlayHavenSecret_value;
		}

		public static string GetAmazonAdAppID()
		{
			if (GetAmazonAdAppID_value == null)
			{
				GetAmazonAdAppID_value = GetProperty("AMAZON_AD_APPID", string.Empty);
			}
			return GetAmazonAdAppID_value;
		}

		public static string GetDefaultAdProvider()
		{
			if (GetDefaultAdProvider_value == null)
			{
				GetDefaultAdProvider_value = GetProperty("DEFAULT_AD_PROVIDER", string.Empty);
			}
			return GetDefaultAdProvider_value;
		}

		public static string GetFacebookAppID()
		{
			if (GetFacebookAppID_value == null)
			{
				GetFacebookAppID_value = GetProperty("FACEBOOK_APPID", string.Empty);
			}
			return GetFacebookAppID_value;
		}

		public static string GetFlurryKey()
		{
			if (GetFlurryKey_value == null)
			{
				GetFlurryKey_value = GetProperty((!Debug.isDebugBuild) ? "FLURRY_KEY_LIVE" : "FLURRY_KEY_STAGE", string.Empty);
			}
			return GetFlurryKey_value;
		}

		public static string GetKontagentKey()
		{
			if (GetKontagentKey_value == null)
			{
				string text = ((!Debug.isDebugBuild) ? "KONTAGENT_KEY_LIVE" : "KONTAGENT_KEY_STAGE");
				if (DeviceInfo.IsDeviceRooted())
				{
					string property = GetProperty(text + "_JAILBROKEN");
					if (!string.IsNullOrEmpty(property))
					{
						GetKontagentKey_value = property;
					}
				}
				if (GetKontagentKey_value == null)
				{
					GetKontagentKey_value = GetProperty(text, string.Empty);
				}
			}
			return GetKontagentKey_value;
		}

		public static string GetMobileAppTrackingPackage()
		{
			if (GetMobileAppTrackingPackage_value == null)
			{
				GetMobileAppTrackingPackage_value = GetProperty("MAT_PACKAGE", string.Empty);
			}
			return GetMobileAppTrackingPackage_value;
		}

		public static string GetMobileAppTrackingKey()
		{
			if (GetMobileAppTrackingKey_value == null)
			{
				GetMobileAppTrackingKey_value = GetProperty("MAT_KEY", string.Empty);
			}
			return GetMobileAppTrackingKey_value;
		}

		public static string GetGWalletSKU()
		{
			if (GetGWalletSKU_value == null)
			{
				GetGWalletSKU_value = GetProperty("GWALLET_SKU", string.Empty);
			}
			return GetGWalletSKU_value;
		}

		public static string GetGGNSRC()
		{
			if (GetGGNSRC_value == null)
			{
				GetGGNSRC_value = GetProperty("GGN_SRC", string.Empty);
			}
			return GetGGNSRC_value;
		}

		public static string GetIAPOrdering()
		{
			if (GetIAPOrdering_value == null)
			{
				GetIAPOrdering_value = GetProperty("IAP_ORDERING", "ssssssssiiiiiiiiiiii");
			}
			return GetIAPOrdering_value;
		}

		public static string GetGATrackingID()
		{
			if (GetGATrackingID_value == null)
			{
				GetGATrackingID_value = GetProperty("GOOGLE_ANALYTICS_TRACKING_ID", string.Empty);
			}
			return GetGATrackingID_value;
		}

		public static string GetChartBoostAppID()
		{
			if (GetChartBoostAppID_value == null)
			{
				GetChartBoostAppID_value = GetProperty("CHARTBOOST_APP_ID", string.Empty);
			}
			return GetChartBoostAppID_value;
		}

		public static string GetChartBoostAppSignature()
		{
			if (GetChartBoostAppSignature_value == null)
			{
				GetChartBoostAppSignature_value = GetProperty("CHARTBOOST_APP_SIGNATURE", string.Empty);
			}
			return GetChartBoostAppSignature_value;
		}
	}

	public static class DebugUtil
	{
		private static IDictionary<string, string> debugProperties;

		private static IDictionary<string, string> DebugProperties
		{
			get
			{
				if (debugProperties == null)
				{
					string buildTag = Properties.GetBuildTag();
					AJTDU_DeleteInvalidDebugProperties(buildTag);
					string path = AJTDU_GetDebugPropertyPath(buildTag);
					debugProperties = ReadDebugProperties(path);
				}
				return debugProperties;
			}
		}

		public static void ListAllSignatures()
		{
			AJTDU_ListAllSignatures();
		}

		public static void PullInternalData()
		{
			AJTDU_PullInternalData();
		}

		public static void PushInternalData()
		{
			AJTDU_PushInternalData();
		}

		public static string GetDebugProperty(string key, string defaultValue = null)
		{
			if (key.IsEmpty())
			{
				return null;
			}
			string value;
			if (DebugProperties.TryGetValue(key, out value))
			{
				return value;
			}
			return defaultValue;
		}

		public static void SetDebugProperty(string key, string value)
		{
			if (!Debug.isDebugBuild)
			{
				Debug.LogWarning("You can't set debug properties in release build");
			}
			else if (!key.IsEmpty())
			{
				IDictionary<string, string> dictionary = DebugProperties;
				dictionary[key] = value;
				string buildTag = Properties.GetBuildTag();
				string path = AJTDU_GetDebugPropertyPath(buildTag);
				string directoryName = Path.GetDirectoryName(path);
				try
				{
					Directory.CreateDirectory(directoryName);
					JavaProperties.Save(path, dictionary);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.ToString());
				}
			}
		}

		private static IDictionary<string, string> ReadDebugProperties(string path)
		{
			if (!File.Exists(path))
			{
				return new Dictionary<string, string>();
			}
			IDictionary<string, string> dictionary = null;
			try
			{
				dictionary = JavaProperties.Load(path);
			}
			catch (Exception)
			{
				dictionary = new Dictionary<string, string>();
			}
			if (Debug.isDebugBuild)
			{
				return dictionary;
			}
			string property = Properties.GetProperty("DEBUG_PROPERTIES_IN_RELEASE", string.Empty);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			string[] array = property.Split('|');
			foreach (string key in array)
			{
				string value;
				if (dictionary.TryGetValue(key, out value))
				{
					dictionary2[key] = value;
				}
			}
			return dictionary2;
		}
	}

	private static GameObject pluginsGameObject;

	private static AndroidJavaClass _ajt;

	private static AndroidJavaClass _ajtb;

	private static AndroidJavaClass _ajtdu;

	private static AndroidJavaClass _ajtdi;

	private static AndroidJavaClass _ajtgi;

	private static AndroidJavaClass _ajti;

	private static AndroidJavaClass _ajtui;

	private static AndroidJavaClass _ajtu;

	public static AndroidJavaClass ajt
	{
		get
		{
			if (_ajt == null)
			{
				_ajt = new AndroidJavaClass("com.glu.plugins.AJavaTools");
			}
			return _ajt;
		}
	}

	public static AndroidJavaClass ajtb
	{
		get
		{
			if (_ajtb == null)
			{
				_ajtb = new AndroidJavaClass("com.glu.plugins.AJTBackup");
			}
			return _ajtb;
		}
	}

	public static AndroidJavaClass ajtdu
	{
		get
		{
			if (_ajtdu == null)
			{
				_ajtdu = new AndroidJavaClass("com.glu.plugins.AJTDebugUtil");
			}
			return _ajtdu;
		}
	}

	public static AndroidJavaClass ajtdi
	{
		get
		{
			if (_ajtdi == null)
			{
				_ajtdi = new AndroidJavaClass("com.glu.plugins.AJTDeviceInfo");
			}
			return _ajtdi;
		}
	}

	public static AndroidJavaClass ajtgi
	{
		get
		{
			if (_ajtgi == null)
			{
				_ajtgi = new AndroidJavaClass("com.glu.plugins.AJTGameInfo");
			}
			return _ajtgi;
		}
	}

	public static AndroidJavaClass ajti
	{
		get
		{
			if (_ajti == null)
			{
				_ajti = new AndroidJavaClass("com.glu.plugins.AJTInternet");
			}
			return _ajti;
		}
	}

	public static AndroidJavaClass ajtui
	{
		get
		{
			if (_ajtui == null)
			{
				_ajtui = new AndroidJavaClass("com.glu.plugins.AJTUI");
			}
			return _ajtui;
		}
	}

	public static AndroidJavaClass ajtu
	{
		get
		{
			if (_ajtu == null)
			{
				_ajtu = new AndroidJavaClass("com.glu.plugins.AJTUtil");
			}
			return _ajtu;
		}
	}

	public static void Init(GameObject gameObject = null)
	{
		AJT_Init(Debug.isDebugBuild);
		if (gameObject != null)
		{
			gameObject.AddComponent<AJavaTools>();
		}
	}

	public static GameObject GetPluginsGameObject()
	{
		if (pluginsGameObject == null)
		{
			GameObject gameObject = GameObject.Find("/AndroidPlugins");
			if (gameObject == null)
			{
				gameObject = new GameObject("AndroidPlugins");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			pluginsGameObject = gameObject;
		}
		return pluginsGameObject;
	}

	private static void AJT_Init(bool debug)
	{
		ajt.CallStatic("Init", debug);
	}

	private static void AJTB_DataChanged()
	{
		ajtb.CallStatic("DataChanged");
	}

	private static void AJTB_RequestRestore()
	{
		ajtb.CallStatic("RequestRestore");
	}

	private static void AJTDU_ListAllSignatures()
	{
		ajtdu.CallStatic("ListAllSignatures");
	}

	private static void AJTDU_PullInternalData()
	{
		ajtdu.CallStatic("PullInternalData");
	}

	private static void AJTDU_PushInternalData()
	{
		ajtdu.CallStatic("PushInternalData");
	}

	private static int AJTDI_GetScreenWidth()
	{
		return ajtdi.CallStatic<int>("GetScreenWidth", new object[0]);
	}

	private static int AJTDI_GetScreenHeight()
	{
		return ajtdi.CallStatic<int>("GetScreenHeight", new object[0]);
	}

	private static int AJTDI_GetScreenLayout()
	{
		return ajtdi.CallStatic<int>("GetScreenLayout", new object[0]);
	}

	private static double AJTDI_GetScreenDiagonalInches()
	{
		return ajtdi.CallStatic<double>("GetScreenDiagonalInches", new object[0]);
	}

	private static string AJTDI_GetDeviceLanguage()
	{
		return ajtdi.CallStatic<string>("GetDeviceLanguage", new object[0]);
	}

	private static string AJTDI_GetDeviceCountry()
	{
		return ajtdi.CallStatic<string>("GetDeviceCountry", new object[0]);
	}

	private static void AJTDI_SetDeviceLocale(string language, string country)
	{
		ajtdi.CallStatic("SetDeviceLocale", language, country);
	}

	private static int AJTDI_GetDeviceSDKVersion()
	{
		return ajtdi.CallStatic<int>("GetDeviceSDKVersion", new object[0]);
	}

	private static bool AJTDI_IsExternalStorageMounted()
	{
		return ajtdi.CallStatic<bool>("IsExternalStorageMounted", new object[0]);
	}

	private static bool AJTDI_IsDeviceRooted()
	{
		return ajtdi.CallStatic<bool>("IsDeviceRooted", new object[0]);
	}

	private static string AJTDI_GetAndroidID()
	{
		return ajtdi.CallStatic<string>("GetAndroidID", new object[0]);
	}

	private static string AJTDI_GetExternalStorageDirectory()
	{
		return ajtdi.CallStatic<string>("GetExternalStorageDirectory", new object[0]);
	}

	private static bool AJTDI_IsGluDebug()
	{
		return ajtdi.CallStatic<bool>("IsGluDebug", new object[0]);
	}

	private static string AJTGI_GetPackageName()
	{
		return ajtgi.CallStatic<string>("GetPackageName", new object[0]);
	}

	private static string AJTGI_GetVersionName()
	{
		return ajtgi.CallStatic<string>("GetVersionName", new object[0]);
	}

	private static int AJTGI_GetVersionCode()
	{
		return ajtgi.CallStatic<int>("GetVersionCode", new object[0]);
	}

	private static string AJTGI_GetFilesPath()
	{
		return ajtgi.CallStatic<string>("GetFilesPath", new object[0]);
	}

	private static string AJTGI_GetExternalFilesPath()
	{
		return ajtgi.CallStatic<string>("GetExternalFilesPath", new object[0]);
	}

	private static void AJTI_LoadWebView(string url, string gameObjectName)
	{
		ajti.CallStatic("LoadWebView", url, gameObjectName);
	}

	private static void AJTUI_ShowToast(string message)
	{
		ajtui.CallStatic("ShowToast", message);
	}

	private static void AJTUI_StartIndeterminateProgress(int gravity)
	{
		ajtui.CallStatic("StartIndeterminateProgress", gravity);
	}

	private static void AJTUI_StopIndeterminateProgress()
	{
		ajtui.CallStatic("StopIndeterminateProgress");
	}

	private static void AJTUI_ShowAlert(string gameObjectName, string callbackName, string title, string message, string buttonPositive, string buttonNegative, string buttonNeutral)
	{
		if (gameObjectName == null)
		{
			gameObjectName = string.Empty;
		}
		if (callbackName == null)
		{
			callbackName = string.Empty;
		}
		if (title == null)
		{
			title = string.Empty;
		}
		if (message == null)
		{
			message = string.Empty;
		}
		if (buttonPositive == null)
		{
			buttonPositive = string.Empty;
		}
		if (buttonNegative == null)
		{
			buttonNegative = string.Empty;
		}
		if (buttonNeutral == null)
		{
			buttonNeutral = string.Empty;
		}
		ajtui.CallStatic("ShowAlert", gameObjectName, callbackName, title, message, buttonPositive, buttonNegative, buttonNeutral);
	}

	private static void AJTUI_SetAlertGravity(int gravity, int xOffset, int yOffset)
	{
		ajtui.CallStatic("SetAlertGravity", gravity, xOffset, yOffset);
	}

	private static void AJTUI_SetAlertDimBehind(bool flag)
	{
		ajtui.CallStatic("SetAlertDimBehind", flag);
	}

	private static void AJTUI_SetAlertModeless(bool flag)
	{
		ajtui.CallStatic("SetAlertModeless", flag);
	}

	private static void AJTUI_CancelAlert()
	{
		ajtui.CallStatic("CancelAlert");
	}

	private static void AJTUI_ShowNotificationPrompt(string gameObjectName, string callbackName)
	{
		ajtui.CallStatic("ShowNotificationPrompt", gameObjectName, callbackName);
	}

	private static void AJTUI_ShowExitPrompt(string gameObjectName, string callbackName)
	{
		ajtui.CallStatic("ShowExitPrompt", gameObjectName, callbackName);
	}

	private static string AJTUI_GetString(string key, string[] replace)
	{
		return ajtui.CallStatic<string>("GetString", new object[2] { key, replace });
	}

	private static bool AJTU_LaunchGame(string packageName, string altURL)
	{
		return ajtu.CallStatic<bool>("LaunchGame", new object[2] { packageName, altURL });
	}

	private static void AJTU_RelaunchGame()
	{
		ajtu.CallStatic("RelaunchGame");
	}

	private static string AJTU_GetProperty(string key)
	{
		return ajtu.CallStatic<string>("GetProperty", new object[1] { key });
	}

	private static string AJTU_GetOBBDownloadPlan()
	{
		return ajtu.CallStatic<string>("GetOBBDownloadPlan", new object[0]);
	}

	private static bool AJTU_IsDataRestored()
	{
		return ajtu.CallStatic<bool>("IsDataRestored", new object[0]);
	}

	private static int AJTU_GetRunCount()
	{
		return ajtu.CallStatic<int>("GetRunCount", new object[0]);
	}

	private static int AJTU_GetRunCountThisVersion()
	{
		return ajtu.CallStatic<int>("GetRunCountThisVersion", new object[0]);
	}

	private static void AJTU_SendBroadcast(string action, string uri)
	{
		ajtu.CallStatic("SendBroadcast", action, uri);
	}

	private static void AJTU_VerifySignature()
	{
		ajtu.CallStatic("VerifySignature");
	}

	private static string AJTDU_GetDebugPropertyPath(string buildTag)
	{
		string path = AJTDU_GetDebugPropertyFilename(buildTag);
		return Path.Combine(GameInfo.GetExternalFilesPath(), path);
	}

	private static void AJTDU_DeleteInvalidDebugProperties(string buildTag)
	{
		string text = AJTDU_GetDebugPropertyFilename(buildTag);
		string externalFilesPath = GameInfo.GetExternalFilesPath();
		if (!Directory.Exists(externalFilesPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(externalFilesPath, "ajt_debug_properties_*.dat");
		foreach (string path in files)
		{
			string fileName = Path.GetFileName(path);
			if (text != fileName)
			{
				try
				{
					File.Delete(path);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
		}
	}

	private static string AJTDU_GetDebugPropertyFilename(string buildTag)
	{
		return "ajt_debug_properties_{0}.dat".Fmt(buildTag);
	}
}
