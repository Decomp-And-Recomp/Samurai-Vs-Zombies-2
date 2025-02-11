using UnityEngine;

public class ANotificationManager
{
	private const string GWALLET_SERVER_STAGE = "http://gwallet-stage.glu.com/wallet-server/";

	private const string GWALLET_SERVER_CERTIFICATION = "http://gwallet-cert.glu.com/wallet-server/";

	private const string GWALLET_SERVER_PREPROD = "http://gwallet-pp.glu.com/wallet-server/";

	private const string GWALLET_SERVER_LIVE = "http://gwallet.glu.com/wallet-server/";

	public static void Init(string legacyServerURL = "")
	{
	}

	public static void ScheduleNotificationSecFromNow(int time, string message, string uri = "")
	{
	}

	public static void ScheduleNotificationMillisFromEpoch(long time, string message, string uri = "")
	{
	}

	public static void ClearActiveNotifications()
	{
	}

	public static void ClearScheduledNotifications()
	{
	}

	public static bool IsEnabled()
	{
		return false;
	}

	public static void SetEnabled(bool enable)
	{
	}

	private static void ANM_Init(bool debug, string buildType, string gwStore, string gwSKU, string gwURL, string legacyServerURL)
	{
	}

	private static void ANM_ScheduleNotificationSecFromNow(int time, string message, string uri)
	{
	}

	private static void ANM_ScheduleNotificationMillisFromEpoch(long time, string message, string uri)
	{
	}

	private static void ANM_ClearActiveNotifications()
	{
	}

	private static void ANM_ClearScheduledNotifications()
	{
	}

	private static bool ANM_IsEnabled()
	{
		return false;
	}

	private static void ANM_SetEnabled(bool enable)
	{
	}
}
