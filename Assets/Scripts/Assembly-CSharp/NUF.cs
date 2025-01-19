using System;
using System.Globalization;
using UnityEngine;

public class NUF
{
	private delegate void PopUpAlertDelegate(int accepted);

	public delegate void RateMeDelegate(int buttonIndex);

	private static Action<int> popUpAlertCallback;

	private static string systemLanguage;

	private static string systemCountry;

	private static int _ScheduleNotification(int tD, string mB, string bT, string cSF)
	{
		return 0;
	}

	private static int _CancelAllLocalNotification()
	{
		return 0;
	}

	private static void _SetIconBadgeNumber(int count)
	{
	}

	private static int _PresentLocalNotificationNow(string mB, string bT, string cSF)
	{
		return 0;
	}

	private static ulong _FileSystemFreeSpace(string path)
	{
		return 0uL;
	}

	private static void _AddSkipBackupAttributeToFile(string path)
	{
	}

	private static void _PresentRateMeAlert(string title, string messageBody, string acceptButton, string cancelButton, string neverButton, string url, RateMeDelegate rateMeCallback)
	{
	}

	private static bool _IsIpodMusicPlaying()
	{
		return false;
	}

	private static bool _PauseIpodMusic(bool pause)
	{
		return true;
	}

	private static float _GetHardwareVolume()
	{
		return 1f;
	}

	private static void _PopUpAlert(string title, string messageBody, string cancelButton, int numOtherButtons, string[] otherButtons, PopUpAlertDelegate callback)
	{
	}

	private static void _StartMemoryWarningHandler(string gameObjectToWarn, string functionInGameObject)
	{
	}

	private static uint _CurrentUsedMemory()
	{
		return 0u;
	}

	private static void _StartSpinner(float x, float y)
	{
	}

	private static void _StopSpinner()
	{
	}

	private static string _GetDeviceToken()
	{
		return null;
	}

	private static void _RegisterPushNotificationCallback(string gameObjectName, string methodName)
	{
	}

	private static void _HTTPRequest(string url, string method, string content)
	{
	}

	private static void _SetupReachabilityHandler(string hostName, string gameObjectToWarn, string functionInGameObject)
	{
	}

	private static string _GetLanguage()
	{
		return string.Empty;
	}

	private static string _GetCountry()
	{
		return string.Empty;
	}

	public static void HTTPRequest(string url, string method, string content)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_HTTPRequest(url, method, content);
		}
	}

	public static string GetDeviceToken()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _GetDeviceToken();
		}
		return null;
	}

	public static void RegisterPushNotificationCallback(string gameObjectName, string methodName)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_RegisterPushNotificationCallback(gameObjectName, methodName);
		}
	}

	public static int ScheduleNotification(int timeDelay, string messageBody, string buttonText, string customSoundFilename)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _ScheduleNotification(timeDelay, messageBody, buttonText, customSoundFilename);
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			ANotificationManager.ScheduleNotificationSecFromNow(timeDelay, messageBody, string.Empty);
			return 1;
		}
		return 1;
	}

	public static int CancelAllLocalNotification()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _CancelAllLocalNotification();
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			ANotificationManager.ClearActiveNotifications();
			ANotificationManager.ClearScheduledNotifications();
			return 1;
		}
		return 1;
	}

	public static int PresentLocalNotificationNow(string messageBody, string buttonText, string customSoundFilename)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _PresentLocalNotificationNow(messageBody, buttonText, customSoundFilename);
		}
		return 1;
	}

	public static void SetIconBadgeNumber(int count)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_SetIconBadgeNumber(count);
		}
	}

	public static ulong FileSystemFreeSpace(string path)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _FileSystemFreeSpace(path);
		}
		return 4294967295uL;
	}

	public static void AddSkipBackupAttributeToFile(string path)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_AddSkipBackupAttributeToFile(path);
		}
	}

	public static void PresentRateMeAlert(string title, string messageBody, string acceptButton, string cancelButton, string neverButton, string url, RateMeDelegate rateMeCallback)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_PresentRateMeAlert(title, messageBody, acceptButton, cancelButton, neverButton, url, rateMeCallback);
		}
	}

	public static bool IsIpodMusicPlaying()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _IsIpodMusicPlaying();
		}
		return false;
	}

	public static void PauseIpodMusic(bool pause)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_PauseIpodMusic(pause);
		}
	}

	public static float GetHardwareVolume()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _GetHardwareVolume();
		}
		return 1f;
	}

	public static void PopUpAlert(string title, string messageBody, string cancelButton, string[] otherButtons, Action<int> onComplete)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			popUpAlertCallback = onComplete;
			_PopUpAlert(title, messageBody, cancelButton, (otherButtons != null) ? otherButtons.Length : 0, otherButtons, PopUpAlertComplete);
		}
		else if (onComplete != null)
		{
			onComplete(0);
		}
	}

	[MonoPInvokeCallback(typeof(PopUpAlertDelegate))]
	private static void PopUpAlertComplete(int buttonIndex)
	{
		if (popUpAlertCallback != null)
		{
			Action<int> action = popUpAlertCallback;
			popUpAlertCallback = null;
			action(buttonIndex);
		}
	}

	public static void StartMemoryWarningHandler(string gameObjectToWarn, string functionInGameObject)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_StartMemoryWarningHandler(gameObjectToWarn, functionInGameObject);
		}
	}

	public static uint CurrentUsedMemory()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _CurrentUsedMemory();
		}
		return 0u;
	}

	public static void SetupReachabilityHandler(string hostName, string gameObjectToWarn, string functionInGameObject)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_SetupReachabilityHandler(hostName, gameObjectToWarn, functionInGameObject);
		}
	}

	public static void StartSpinner(Vector2 screenPos)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_StartSpinner(screenPos.x, screenPos.y);
		}
	}

	public static void StopSpinner()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_StopSpinner();
		}
		AJavaTools.UI.StopIndeterminateProgress();
	}

	private static string ConvertIOSLanguageCodeToUnityLanguageName(string iosLangCode)
	{
		switch (iosLangCode)
		{
		case "de":
			return SystemLanguage.German.ToString();
		case "es":
			return SystemLanguage.Spanish.ToString();
		case "fr":
			return SystemLanguage.French.ToString();
		case "it":
			return SystemLanguage.Italian.ToString();
		case "ja":
			return SystemLanguage.Japanese.ToString();
		case "ko":
			return SystemLanguage.Korean.ToString();
		case "pt":
			return SystemLanguage.Portuguese.ToString();
		case "ru":
			return SystemLanguage.Russian.ToString();
		case "zh-Hans":
			return "Chinese (Simplified)";
		case "zh-Hant":
			return "Chinese (Traditional)";
		case "en":
			return SystemLanguage.English.ToString();
		default:
			return SystemLanguage.English.ToString();
		}
	}

	private static string ConvertAndroidLanguage()
	{
		switch (AJavaTools.DeviceInfo.GetDeviceLocale())
		{
		case "de":
			return SystemLanguage.German.ToString();
		case "es":
			return SystemLanguage.Spanish.ToString();
		case "fr":
			return SystemLanguage.French.ToString();
		case "it":
			return SystemLanguage.Italian.ToString();
		case "ja":
			return SystemLanguage.Japanese.ToString();
		case "ko":
			return SystemLanguage.Korean.ToString();
		case "ptbr":
			return SystemLanguage.Portuguese.ToString();
		case "ru":
			return SystemLanguage.Russian.ToString();
		case "zhcn":
			return "Chinese (Simplified)";
		case "zhtw":
			return "Chinese (Traditional)";
		default:
			AJavaTools.DeviceInfo.SetDeviceLocale("en", string.Empty);
			return SystemLanguage.English.ToString();
		}
	}

	public static string GetLanguage()
	{
		if (systemLanguage == null)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				systemLanguage = ConvertIOSLanguageCodeToUnityLanguageName(_GetLanguage());
			}
			else
			{
				systemLanguage = ConvertAndroidLanguage();
			}
		}
		return systemLanguage;
	}

	public static string GetCountry()
	{
		if (systemCountry == null)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				systemCountry = _GetCountry();
			}
			else
			{
				systemCountry = RegionInfo.CurrentRegion.TwoLetterISORegionName;
			}
		}
		return systemCountry;
	}
}
