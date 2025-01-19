using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TapjoyPlugin : MonoBehaviour
{
	[method: MethodImpl(32)]
	public static event Action connectCallSucceeded;

	[method: MethodImpl(32)]
	public static event Action connectCallFailed;

	[method: MethodImpl(32)]
	public static event Action<int> getTapPointsSucceeded;

	[method: MethodImpl(32)]
	public static event Action getTapPointsFailed;

	[method: MethodImpl(32)]
	public static event Action<int> spendTapPointsSucceeded;

	[method: MethodImpl(32)]
	public static event Action spendTapPointsFailed;

	[method: MethodImpl(32)]
	public static event Action awardTapPointsSucceeded;

	[method: MethodImpl(32)]
	public static event Action awardTapPointsFailed;

	[method: MethodImpl(32)]
	public static event Action<int> tapPointsEarned;

	[method: MethodImpl(32)]
	public static event Action getFullScreenAdSucceeded;

	[method: MethodImpl(32)]
	public static event Action getFullScreenAdFailed;

	[method: MethodImpl(32)]
	public static event Action getDailyRewardAdSucceeded;

	[method: MethodImpl(32)]
	public static event Action getDailyRewardAdFailed;

	[method: MethodImpl(32)]
	public static event Action getDisplayAdSucceeded;

	[method: MethodImpl(32)]
	public static event Action getDisplayAdFailed;

	[method: MethodImpl(32)]
	public static event Action videoAdStarted;

	[method: MethodImpl(32)]
	public static event Action videoAdFailed;

	[method: MethodImpl(32)]
	public static event Action videoAdCompleted;

	[method: MethodImpl(32)]
	public static event Action showOffersFailed;

	[method: MethodImpl(32)]
	public static event Action<TapjoyViewType> viewOpened;

	[method: MethodImpl(32)]
	public static event Action<TapjoyViewType> viewClosed;

	private void Awake()
	{
		base.gameObject.name = GetType().ToString();
		SetCallbackHandler(base.gameObject.name);
		MonoBehaviour.print("UnitySendMessage directs to " + base.gameObject.name);
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void TapjoyConnectSuccess(string message)
	{
		if (TapjoyPlugin.connectCallSucceeded != null)
		{
			TapjoyPlugin.connectCallSucceeded();
		}
	}

	public void TapjoyConnectFail(string message)
	{
		if (TapjoyPlugin.connectCallFailed != null)
		{
			TapjoyPlugin.connectCallFailed();
		}
	}

	public void TapPointsLoaded(string message)
	{
		if (TapjoyPlugin.getTapPointsSucceeded != null)
		{
			TapjoyPlugin.getTapPointsSucceeded(int.Parse(message));
		}
	}

	public void TapPointsLoadedError(string message)
	{
		if (TapjoyPlugin.getTapPointsFailed != null)
		{
			TapjoyPlugin.getTapPointsFailed();
		}
	}

	public void TapPointsSpent(string message)
	{
		if (TapjoyPlugin.spendTapPointsSucceeded != null)
		{
			TapjoyPlugin.spendTapPointsSucceeded(int.Parse(message));
		}
	}

	public void TapPointsSpendError(string message)
	{
		if (TapjoyPlugin.spendTapPointsFailed != null)
		{
			TapjoyPlugin.spendTapPointsFailed();
		}
	}

	public void TapPointsAwarded(string message)
	{
		if (TapjoyPlugin.awardTapPointsSucceeded != null)
		{
			TapjoyPlugin.awardTapPointsSucceeded();
		}
	}

	public void TapPointsAwardError(string message)
	{
		if (TapjoyPlugin.awardTapPointsFailed != null)
		{
			TapjoyPlugin.awardTapPointsFailed();
		}
	}

	public void CurrencyEarned(string message)
	{
		if (TapjoyPlugin.tapPointsEarned != null)
		{
			TapjoyPlugin.tapPointsEarned(int.Parse(message));
		}
	}

	public void FullScreenAdLoaded(string message)
	{
		if (TapjoyPlugin.getFullScreenAdSucceeded != null)
		{
			TapjoyPlugin.getFullScreenAdSucceeded();
		}
	}

	public void FullScreenAdError(string message)
	{
		if (TapjoyPlugin.getFullScreenAdFailed != null)
		{
			TapjoyPlugin.getFullScreenAdFailed();
		}
	}

	public void DailyRewardAdLoaded(string message)
	{
		if (TapjoyPlugin.getDailyRewardAdSucceeded != null)
		{
			TapjoyPlugin.getDailyRewardAdSucceeded();
		}
	}

	public void DailyRewardAdError(string message)
	{
		if (TapjoyPlugin.getDailyRewardAdFailed != null)
		{
			TapjoyPlugin.getDailyRewardAdFailed();
		}
	}

	public void DisplayAdLoaded(string message)
	{
		if (TapjoyPlugin.getDisplayAdSucceeded != null)
		{
			TapjoyPlugin.getDisplayAdSucceeded();
		}
	}

	public void DisplayAdError(string message)
	{
		if (TapjoyPlugin.getDisplayAdFailed != null)
		{
			TapjoyPlugin.getDisplayAdFailed();
		}
	}

	public void VideoAdStart(string message)
	{
		if (TapjoyPlugin.videoAdStarted != null)
		{
			TapjoyPlugin.videoAdStarted();
		}
	}

	public void VideoAdError(string message)
	{
		if (TapjoyPlugin.videoAdFailed != null)
		{
			TapjoyPlugin.videoAdFailed();
		}
	}

	public void VideoAdComplete(string message)
	{
		if (TapjoyPlugin.videoAdCompleted != null)
		{
			TapjoyPlugin.videoAdCompleted();
		}
	}

	public void ShowOffersError(string message)
	{
		if (TapjoyPlugin.showOffersFailed != null)
		{
			TapjoyPlugin.showOffersFailed();
		}
	}

	public void ViewOpened(string message)
	{
		if (TapjoyPlugin.viewOpened != null)
		{
			int obj = int.Parse(message);
			TapjoyPlugin.viewOpened((TapjoyViewType)obj);
		}
	}

	public void ViewClosed(string message)
	{
		if (TapjoyPlugin.viewClosed != null)
		{
			int obj = int.Parse(message);
			TapjoyPlugin.viewClosed((TapjoyViewType)obj);
		}
	}

	public static void SetCallbackHandler(string handlerName)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SetCallbackHandler(handlerName);
		}
	}

	public static void RequestTapjoyConnect(string appID, string secretKey)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.RequestTapjoyConnect(appID, secretKey);
		}
	}

	public static void RequestTapjoyConnect(string appID, string secretKey, Dictionary<string, string> flags)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.RequestTapjoyConnect(appID, secretKey, flags);
		}
	}

	public static void EnableLogging(bool enable)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.EnableLogging(enable);
		}
	}

	public static void ActionComplete(string actionID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ActionComplete(actionID);
		}
	}

	public static void SetUserID(string userID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SetUserID(userID);
		}
	}

	public static void ShowOffers()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowOffers();
		}
	}

	public static void GetTapPoints()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetTapPoints();
		}
	}

	public static void SpendTapPoints(int points)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SpendTapPoints(points);
		}
	}

	public static void AwardTapPoints(int points)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.AwardTapPoints(points);
		}
	}

	public static int QueryTapPoints()
	{
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			return 0;
		}
		return TapjoyPluginAndroid.QueryTapPoints();
	}

	public static void ShowDefaultEarnedCurrencyAlert()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowDefaultEarnedCurrencyAlert();
		}
	}

	public static void GetDisplayAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDisplayAd();
		}
	}

	public static void ShowDisplayAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowDisplayAd();
		}
	}

	public static void HideDisplayAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.HideDisplayAd();
		}
	}

	[Obsolete("SetDisplayAdContentSize is deprecated. Please use SetDisplayAdSize.")]
	public static void SetDisplayAdContentSize(int size)
	{
		if (Application.platform != 0)
		{
			SetDisplayAdSize((TapjoyDisplayAdSize)size);
		}
	}

	public static void SetDisplayAdSize(TapjoyDisplayAdSize size)
	{
		if (Application.platform != 0)
		{
			string displayAdSize = "320x50";
			if (size == TapjoyDisplayAdSize.SIZE_640X100)
			{
				displayAdSize = "640x100";
			}
			if (size == TapjoyDisplayAdSize.SIZE_768X90)
			{
				displayAdSize = "768x90";
			}
			TapjoyPluginAndroid.SetDisplayAdSize(displayAdSize);
		}
	}

	[Obsolete("RefreshDisplayAd(bool enable) is deprecated. Please use EnableDisplayAdAutoRefresh.")]
	public static void RefreshDisplayAd(bool enable)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.EnableDisplayAdAutoRefresh(enable);
		}
	}

	public static void EnableDisplayAdAutoRefresh(bool enable)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.EnableDisplayAdAutoRefresh(enable);
		}
	}

	[Obsolete("RefreshDisplayAd() is deprecated. Please use GetDisplayAd.")]
	public static void RefreshDisplayAd()
	{
		TapjoyPluginAndroid.GetDisplayAd();
	}

	public static void MoveDisplayAd(int x, int y)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.MoveDisplayAd(x, y);
		}
	}

	[Obsolete("SetUserDefinedColorWithIntValue is deprecated. There is no navigation bar for iOS in v9.x.x+.")]
	public static void SetUserDefinedColorWithIntValue(int color)
	{
	}

	public static void SetTransitionEffect(int transition)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SetTransitionEffect(transition);
		}
	}

	[Obsolete("GetFeaturedApp is deprecated, please use GetFullScreenAd instead.")]
	public static void GetFeaturedApp()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetFullScreenAd();
		}
	}

	[Obsolete("SetFeaturedAppDisplayCount is deprecated.")]
	public static void SetFeaturedAppDisplayCount(int displayCount)
	{
	}

	[Obsolete("ShowFeaturedAppFullScreenAd is deprecated, please use ShowFullScreenAd instead.")]
	public static void ShowFeaturedAppFullScreenAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowFullScreenAd();
		}
	}

	public static void GetFullScreenAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetFullScreenAd();
		}
	}

	public static void ShowFullScreenAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowFullScreenAd();
		}
	}

	[Obsolete("GetReEngagementAd is deprecated, please use GetDailyRewardAd instead.")]
	public static void GetReEngagementAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDailyRewardAd();
		}
	}

	[Obsolete("ShowReEngagementAd is deprecated, please use ShowDailyRewardAd instead.")]
	public static void ShowReEngagementAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowDailyRewardAd();
		}
	}

	public static void GetDailyRewardAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDailyRewardAd();
		}
	}

	public static void ShowDailyRewardAd()
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowDailyRewardAd();
		}
	}

	[Obsolete("InitVideoAd is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void InitVideoAd()
	{
	}

	[Obsolete("SetVideoCacheCount is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void SetVideoCacheCount(int cacheCount)
	{
	}

	[Obsolete("EnableVideoCache is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void EnableVideoCache(bool enable)
	{
	}

	public static void SendShutDownEvent()
	{
		TapjoyPluginAndroid.SendShutDownEvent();
	}

	public static void SendIAPEvent(string name, float price, int quantity, string currencyCode)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SendIAPEvent(name, price, quantity, currencyCode);
		}
	}

	public static void ShowOffersWithCurrencyID(string currencyID, bool selector)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.ShowOffersWithCurrencyID(currencyID, selector);
		}
	}

	public static void GetDisplayAdWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDisplayAdWithCurrencyID(currencyID);
		}
	}

	[Obsolete("RefreshDisplayAdWithCurrencyID is deprecated, please use GetDisplayAdWithCurrencyID instead.")]
	public static void RefreshDisplayAdWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDisplayAdWithCurrencyID(currencyID);
		}
	}

	[Obsolete("GetFeaturedAppWithCurrencyID is deprecated, please use GetFullScreenAdWithCurrencyID instead.")]
	public static void GetFeaturedAppWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetFullScreenAdWithCurrencyID(currencyID);
		}
	}

	public static void GetFullScreenAdWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetFullScreenAdWithCurrencyID(currencyID);
		}
	}

	public static void SetCurrencyMultiplier(float multiplier)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.SetCurrencyMultiplier(multiplier);
		}
	}

	[Obsolete("GetReEngagementAdWithCurrencyID is deprecated, please use GetDailyRewardAdWithCurrencyID instead.")]
	public static void GetReEngagementAdWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDailyRewardAdWithCurrencyID(currencyID);
		}
	}

	public static void GetDailyRewardAdWithCurrencyID(string currencyID)
	{
		if (Application.platform != 0)
		{
			TapjoyPluginAndroid.GetDailyRewardAdWithCurrencyID(currencyID);
		}
	}
}
