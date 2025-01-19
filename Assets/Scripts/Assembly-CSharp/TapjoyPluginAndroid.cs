using System.Collections.Generic;
using UnityEngine;

public class TapjoyPluginAndroid : MonoBehaviour
{
	private static AndroidJavaObject currentActivity;

	private static AndroidJavaClass tapjoyConnect;

	private static AndroidJavaObject tapjoyConnectInstance;

	private static AndroidJavaClass TapjoyConnect
	{
		get
		{
			if (tapjoyConnect == null)
			{
				MonoBehaviour.print("Loading TapjoyPlugin");
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				currentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				tapjoyConnect = new AndroidJavaClass("com.tapjoy.TapjoyConnectUnity");
			}
			return tapjoyConnect;
		}
	}

	private static AndroidJavaObject TapjoyConnectInstance
	{
		get
		{
			if (tapjoyConnectInstance == null)
			{
				tapjoyConnectInstance = TapjoyConnect.CallStatic<AndroidJavaObject>("getTapjoyConnectInstance", new object[0]);
			}
			return tapjoyConnectInstance;
		}
	}

	public static void SetCallbackHandler(string handlerName)
	{
		TapjoyConnect.CallStatic("setHandlerClass", handlerName);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey)
	{
		RequestTapjoyConnect(appID, secretKey, null);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey, Dictionary<string, string> flags)
	{
		if (flags != null)
		{
			foreach (KeyValuePair<string, string> flag in flags)
			{
				TapjoyConnect.CallStatic("setFlagKeyValue", flag.Key, flag.Value);
			}
		}
		TapjoyConnect.CallStatic("requestTapjoyConnect", currentActivity, appID, secretKey);
	}

	public static void EnableLogging(bool enable)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.tapjoy.TapjoyLog");
		androidJavaClass.CallStatic("enableLogging", enable);
	}

	public static void ActionComplete(string actionID)
	{
		TapjoyConnectInstance.Call("actionComplete", actionID);
	}

	public static void SetUserID(string userID)
	{
		TapjoyConnectInstance.Call("setUserID", userID);
	}

	public static void ShowOffers()
	{
		TapjoyConnectInstance.Call("showOffers");
	}

	public static void GetTapPoints()
	{
		TapjoyConnectInstance.Call("getTapPoints");
	}

	public static void SpendTapPoints(int points)
	{
		TapjoyConnectInstance.Call("spendTapPoints", points);
	}

	public static void AwardTapPoints(int points)
	{
		TapjoyConnectInstance.Call("awardTapPoints", points);
	}

	public static int QueryTapPoints()
	{
		return TapjoyConnectInstance.Call<int>("getTapPointsTotal", new object[0]);
	}

	public static void SetEarnedPointsNotifier()
	{
		TapjoyConnectInstance.Call("setEarnedPointsNotifier");
	}

	public static void ShowDefaultEarnedCurrencyAlert()
	{
		TapjoyConnectInstance.Call("showDefaultEarnedCurrencyAlert");
	}

	public static void GetDisplayAd()
	{
		TapjoyConnectInstance.Call("getDisplayAd");
	}

	public static void ShowDisplayAd()
	{
		TapjoyConnectInstance.Call("showDisplayAd");
	}

	public static void HideDisplayAd()
	{
		TapjoyConnectInstance.Call("hideDisplayAd");
	}

	public static void SetDisplayAdSize(string size)
	{
		TapjoyConnectInstance.Call("setDisplayAdSize", size);
	}

	public static void EnableDisplayAdAutoRefresh(bool enable)
	{
		TapjoyConnectInstance.Call("enableDisplayAdAutoRefresh", enable);
	}

	public static void RefreshDisplayAd()
	{
		TapjoyConnectInstance.Call("getDisplayAd");
	}

	public static void MoveDisplayAd(int x, int y)
	{
		TapjoyConnectInstance.Call("setDisplayAdPosition", x, y);
	}

	public static void SetTransitionEffect(int transition)
	{
	}

	public static void GetFullScreenAd()
	{
		TapjoyConnectInstance.Call("getFullScreenAd");
	}

	public static void ShowFullScreenAd()
	{
		TapjoyConnectInstance.Call("showFullScreenAd");
	}

	public static void GetDailyRewardAd()
	{
		TapjoyConnectInstance.Call("getDailyRewardAd");
	}

	public static void ShowDailyRewardAd()
	{
		TapjoyConnectInstance.Call("showDailyRewardAd");
	}

	public static void SendShutDownEvent()
	{
		TapjoyConnectInstance.Call("sendShutDownEvent");
	}

	public static void SendIAPEvent(string name, float price, int quantity, string currencyCode)
	{
		TapjoyConnectInstance.Call("sendIAPEvent", name, price, quantity, currencyCode);
	}

	public static void ShowOffersWithCurrencyID(string currencyID, bool selector)
	{
		TapjoyConnectInstance.Call("showOffersWithCurrencyID", currencyID, selector);
	}

	public static void GetDisplayAdWithCurrencyID(string currencyID)
	{
		TapjoyConnectInstance.Call("getDisplayAdWithCurrencyID", currencyID);
	}

	public static void GetFullScreenAdWithCurrencyID(string currencyID)
	{
		TapjoyConnectInstance.Call("getFullScreenAdWithCurrencyID", currencyID);
	}

	public static void SetCurrencyMultiplier(float multiplier)
	{
		TapjoyConnectInstance.Call("setCurrencyMultiplier", multiplier);
	}

	public static void GetDailyRewardAdWithCurrencyID(string currencyID)
	{
		TapjoyConnectInstance.Call("getDailyRewardAdWithCurrencyID", currencyID);
	}
}
