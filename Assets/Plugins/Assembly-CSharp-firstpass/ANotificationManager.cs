using UnityEngine;

public class ANotificationManager
{
	private const string GWALLET_SERVER_STAGE = "http://gwallet-stage.glu.com/wallet-server/";

	private const string GWALLET_SERVER_CERTIFICATION = "http://gwallet-cert.glu.com/wallet-server/";

	private const string GWALLET_SERVER_PREPROD = "http://gwallet-pp.glu.com/wallet-server/";

	private const string GWALLET_SERVER_LIVE = "http://gwallet.glu.com/wallet-server/";

	private static AndroidJavaClass _anm;

	public static AndroidJavaClass anm
	{
		get
		{
			if (_anm == null)
			{
				_anm = new AndroidJavaClass("com.glu.plugins.ANotificationManager");
			}
			return _anm;
		}
	}

	public static void Init(string legacyServerURL = "")
	{
		string gwStore = (AJavaTools.Properties.IsBuildAmazon() ? "AMAZON_APPSTORE_FOR_ANDROID" : "ANDROID_MARKET");
		ANM_Init(Debug.isDebugBuild, AJavaTools.Properties.GetBuildType(), gwStore, AJavaTools.Properties.GetGWalletSKU(), (!Debug.isDebugBuild) ? "http://gwallet.glu.com/wallet-server/" : "http://gwallet-pp.glu.com/wallet-server/", legacyServerURL);
	}

	public static void ScheduleNotificationSecFromNow(int time, string message, string uri = "")
	{
		ANM_ScheduleNotificationSecFromNow(time, message, uri);
	}

	public static void ScheduleNotificationMillisFromEpoch(long time, string message, string uri = "")
	{
		ANM_ScheduleNotificationMillisFromEpoch(time, message, uri);
	}

	public static void ClearActiveNotifications()
	{
		ANM_ClearActiveNotifications();
	}

	public static void ClearScheduledNotifications()
	{
		ANM_ClearScheduledNotifications();
	}

	public static bool IsEnabled()
	{
		return ANM_IsEnabled();
	}

	public static void SetEnabled(bool enable)
	{
		ANM_SetEnabled(enable);
	}

	private static void ANM_Init(bool debug, string buildType, string gwStore, string gwSKU, string gwURL, string legacyServerURL)
	{
		anm.CallStatic("Init", debug, buildType, gwStore, gwSKU, gwURL, legacyServerURL);
	}

	private static void ANM_ScheduleNotificationSecFromNow(int time, string message, string uri)
	{
		anm.CallStatic("ScheduleNotificationSecFromNow", time, message, uri);
	}

	private static void ANM_ScheduleNotificationMillisFromEpoch(long time, string message, string uri)
	{
		anm.CallStatic("ScheduleNotificationMillisFromEpoch", time, message, uri);
	}

	private static void ANM_ClearActiveNotifications()
	{
		anm.CallStatic("ClearActiveNotifications");
	}

	private static void ANM_ClearScheduledNotifications()
	{
		anm.CallStatic("ClearScheduledNotifications");
	}

	private static bool ANM_IsEnabled()
	{
		return anm.CallStatic<bool>("IsEnabled", new object[0]);
	}

	private static void ANM_SetEnabled(bool enable)
	{
		anm.CallStatic("SetEnabled", enable);
	}
}
