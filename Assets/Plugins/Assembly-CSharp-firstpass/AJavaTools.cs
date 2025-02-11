using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Glu.Plugins.AJavaTools_Private;
using Glu.Plugins.AMiscUtils;
using UnityEditor;
using UnityEngine;

public class AJavaTools : MonoBehaviour
{
	public static class Backup
	{
		public static void DataChanged()
		{
		}

		public static void RequestRestore()
		{
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

		public static string GetDeviceLocale()
		{
			return CultureInfo.CurrentUICulture.Name.Split('-')[0];
		}

		public static void SetDeviceLocale(string language, string country = "")
		{
		}

		public static bool IsDeviceRooted()
		{
			return false;
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
			return "com.zweronz.svz2";
		}

		public static string GetVersionName()
		{
			return Application.version;
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
			return Application.persistentDataPath;
		}

		public static string GetExternalFilesPath()
		{
			return Application.persistentDataPath;
		}
	}

	public static class Internet
	{
		public static void LoadWebView(string url, string gameObjectName = "")
		{
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
		}

		public static void StartIndeterminateProgress(int gravity)
		{
		}

		public static void StopIndeterminateProgress()
		{
		}

		public static void ShowAlert(string title, string message, string button)
		{
		}

		public static void ShowAlert(string gameObjectName, string callbackName, string title, string message, string buttonPositive, string buttonNegative = "", string buttonNeutral = "")
		{
		}

		public static void SetAlertGravity(int gravity, int xOffset, int yOffset)
		{
		}

		public static void SetAlertDimBehind(bool flag)
		{
		}

		public static void SetAlertModeless(bool flag)
		{
		}

		public static void CancelAlert()
		{
		}

		public static void ShowNotificationPrompt(string gameObjectName, string callbackName)
		{
		}

		public static void ShowExitPrompt(string gameObjectName = "", string callbackName = "")
		{
		}

		public static string GetString(string key, params string[] replace)
		{
			return "";
		}
	}

	public static class Util
	{
		private static string GetOBBDownloadPlan_value;

		private static bool IsDataRestored_checked;

		private static bool IsDataRestored_value;

		private static int GetRunCount_value = -1;

		private static int GetRunCountThisVersion_value = -1;

		public static void RelaunchGame()
		{
		}

		public static void LogEventOBB()
		{
		}

		public static bool IsDataRestored()
		{
			return false;
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
			return 0;
		}

		public static bool IsFirstLaunch()
		{
			return GetRunCount() == 0;
		}

		public static int GetRunCountThisVersion()
		{
			return 0;
		}

		public static bool IsFirstLaunchThisVersion()
		{
			return GetRunCountThisVersion() == 0;
		}

		public static void VerifySignature()
		{
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

		public static string GetBuildType()
		{
			return "";
		}

		public static bool IsBuildGoogle()
		{
			return false;
		}

		public static bool IsBuildAmazon()
		{
			return false;
		}

		public static bool IsBuildTStore()
		{
			return GetBuildType().Equals("tstore");
		}

		public static string GetBuildTag()
		{
			return "";
		}

		public static string GetBuildLocale()
		{
			return "";
		}
	}

	public static class DebugUtil
	{
	}

	private static GameObject pluginsGameObject;

	public static void Init(GameObject gameObject = null)
	{
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
