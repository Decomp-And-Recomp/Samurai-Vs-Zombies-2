using System;
using UnityEngine;

public class TapjoyInterface
{
	private enum TJScreenOrientation
	{
		DEVICE_ORIENTATION_UNKNOWN = 0,
		DEVICE_ORIENTATION_0 = 1,
		DEVICE_ORIENTATION_90 = 2,
		DEVICE_ORIENTATION_180 = 3,
		DEVICE_ORIENTATION_270 = 4,
		DEVICE_ORIENTATION_DEFAULT = 5
	}

	public enum AdsPosition
	{
		Center = 0,
		VTop = 1,
		VBottom = 3,
		HRight = 8,
		HLeft = 12,
		Fullscreen = 16
	}

	private const bool SIMULATION_MODE_ON = false;

	private const uint DEFAULT_POINTS_EXCHANGE_RATE = 1u;

	private static bool _isInitialized;

	private static bool simulationMode = true;

	private static uint simulationPoints;

	private static uint prevConsumedSimulationPoints;

	private static float lastTime;

	private static IntPtr m_tapjoyConnectInstance;

	private static IntPtr m_getAppIDMethod;

	private static IntPtr m_showOffersMethod;

	private static IntPtr m_getTapPointsMethod;

	private static IntPtr m_getTapPointsTotalMethod;

	private static IntPtr m_spendTapPointsMethod;

	private static IntPtr m_awardTapPointsMethod;

	private static IntPtr m_getFeaturedAppMethod;

	private static IntPtr m_didReceiveFeaturedAppDataMethod;

	private static IntPtr m_didReceiveFeaturedAppDataFailMethod;

	private static IntPtr m_getFeaturedAppObjectMethod;

	private static IntPtr m_showFeaturedAppFullScreenAdMethod;

	private static IntPtr m_showBannerAdMethod;

	private static IntPtr m_hideBannerAdMethod;

	private static IntPtr m_tickMethod;

	private static IntPtr m_getUserIDMethod;

	private static IntPtr m_setUserIDMethod;

	private static TapjoyFeaturedAppInfo m_featuredAppInfo;

	private static bool m_queryFeaturedApp;

	public static string TJC_AD_BANNERSIZE_320X50 = "320X50";

	public static string TJC_AD_BANNERSIZE_640X100 = "640X100";

	public static string TJC_AD_BANNERSIZE_768X90 = "768X90";

	private static void TJ_initialize(string appId, string secretKey, bool showVideoAds)
	{
		/*if (Debug.isDebugBuild)
		{
			IntPtr intPtr = AndroidJNI.FindClass("com/tapjoy/TapjoyLog");
			IntPtr staticMethodID = AndroidJNI.GetStaticMethodID(intPtr, "enableLogging", "(Z)V");
			jvalue[] array = new jvalue[1];
			array[0].z = true;
			AndroidJNI.CallStaticVoidMethod(intPtr, staticMethodID, array);
			AndroidJNI.DeleteLocalRef(intPtr);
		}
		IntPtr intPtr2 = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
		IntPtr staticFieldID = AndroidJNI.GetStaticFieldID(intPtr2, "currentActivity", "Landroid/app/Activity;");
		IntPtr staticObjectField = AndroidJNI.GetStaticObjectField(intPtr2, staticFieldID);
		IntPtr intPtr3 = AndroidJNI.FindClass("com/tapjoy/TapjoyConnect");
		IntPtr staticMethodID2 = AndroidJNI.GetStaticMethodID(intPtr3, "requestTapjoyConnect", "(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;)V");
		IntPtr intPtr4 = AndroidJNI.NewStringUTF(appId);
		IntPtr intPtr5 = AndroidJNI.NewStringUTF(secretKey);
		jvalue[] array2 = new jvalue[3];
		array2[0].l = staticObjectField;
		array2[1].l = intPtr4;
		array2[2].l = intPtr5;
		AndroidJNI.CallStaticVoidMethod(intPtr3, staticMethodID2, array2);
		IntPtr staticMethodID3 = AndroidJNI.GetStaticMethodID(intPtr3, "getTapjoyConnectInstance", "()Lcom/tapjoy/TapjoyConnect;");
		IntPtr obj = AndroidJNI.CallStaticObjectMethod(intPtr3, staticMethodID3, new jvalue[0]);
		m_tapjoyConnectInstance = AndroidJNI.NewGlobalRef(obj);
		m_queryFeaturedApp = false;
		if (showVideoAds)
		{
			IntPtr methodID = AndroidJNI.GetMethodID(intPtr3, "initVideoAd", "()V");
			AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, methodID, new jvalue[0]);
		}
		m_getTapPointsMethod = AndroidJNI.GetMethodID(intPtr3, "getTapPoints", "()V");
		m_didReceiveFeaturedAppDataMethod = AndroidJNI.GetMethodID(intPtr3, "didReceiveFeaturedAppData", "()Z");
		m_didReceiveFeaturedAppDataFailMethod = AndroidJNI.GetMethodID(intPtr3, "didReceiveFeaturedAppDataFail", "()Z");
		m_getAppIDMethod = AndroidJNI.GetMethodID(intPtr3, "getAppID", "()Ljava/lang/String;");
		m_showOffersMethod = AndroidJNI.GetMethodID(intPtr3, "showOffers", "()V");
		m_getTapPointsTotalMethod = AndroidJNI.GetMethodID(intPtr3, "getTapPointsTotal", "()I");
		m_spendTapPointsMethod = AndroidJNI.GetMethodID(intPtr3, "spendTapPoints", "(I)V");
		m_awardTapPointsMethod = AndroidJNI.GetMethodID(intPtr3, "awardTapPoints", "(I)V");
		m_getFeaturedAppObjectMethod = AndroidJNI.GetMethodID(intPtr3, "getFeaturedAppObject", "()Lcom/tapjoy/TapjoyFeaturedAppObject;");
		m_getFeaturedAppMethod = AndroidJNI.GetMethodID(intPtr3, "getFeaturedApp", "()V");
		m_showFeaturedAppFullScreenAdMethod = AndroidJNI.GetMethodID(intPtr3, "showFeaturedAppFullScreenAd", "()V");
		m_showBannerAdMethod = AndroidJNI.GetMethodID(intPtr3, "showBannerAd", "(I)V");
		m_hideBannerAdMethod = AndroidJNI.GetMethodID(intPtr3, "hideBannerAd", "()V");
		m_tickMethod = AndroidJNI.GetMethodID(intPtr3, "tick", "(I)V");
		m_setUserIDMethod = AndroidJNI.GetMethodID(intPtr3, "setUserID", "(Ljava/lang/String;)V");
		m_getUserIDMethod = AndroidJNI.GetMethodID(intPtr3, "getUserID", "()Ljava/lang/String;");
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_getTapPointsMethod, new jvalue[0]);
		AndroidJNI.DeleteLocalRef(intPtr2);
		AndroidJNI.DeleteLocalRef(staticObjectField);
		AndroidJNI.DeleteLocalRef(intPtr3);
		AndroidJNI.DeleteLocalRef(intPtr5);
		AndroidJNI.DeleteLocalRef(intPtr4);
		AndroidJNI.DeleteLocalRef(obj);*/
	}

	private static void TJ_destroy()
	{
		//AndroidJNI.DeleteGlobalRef(m_tapjoyConnectInstance);
		m_tapjoyConnectInstance = IntPtr.Zero;
	}

	private static void TJ_onResume()
	{
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_getTapPointsMethod, new jvalue[0]);
	}

	private static void TJ_tick(int deltaMS)
	{
		/*jvalue[] array = new jvalue[1];
		array[0].i = deltaMS;
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_tickMethod, array);
		if (m_queryFeaturedApp)
		{
			if (AndroidJNI.CallBooleanMethod(m_tapjoyConnectInstance, m_didReceiveFeaturedAppDataMethod, new jvalue[0]))
			{
				m_queryFeaturedApp = false;
				IntPtr obj = AndroidJNI.CallObjectMethod(m_tapjoyConnectInstance, m_getFeaturedAppObjectMethod, new jvalue[0]);
				IntPtr intPtr = AndroidJNI.FindClass("com/tapjoy/TapjoyFeaturedAppObject");
				IntPtr fieldID = AndroidJNI.GetFieldID(intPtr, "amount", "I");
				m_featuredAppInfo.amount = AndroidJNI.GetIntField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "maxTimesToDisplayThisApp", "I");
				m_featuredAppInfo.maxTimesToDisplayThisApp = AndroidJNI.GetIntField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "cost", "Ljava/lang/String;");
				m_featuredAppInfo.cost = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "storeID", "Ljava/lang/String;");
				m_featuredAppInfo.storeId = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "name", "Ljava/lang/String;");
				m_featuredAppInfo.name = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "description", "Ljava/lang/String;");
				m_featuredAppInfo.description = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "iconURL", "Ljava/lang/String;");
				m_featuredAppInfo.iconURL = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "redirectURL", "Ljava/lang/String;");
				m_featuredAppInfo.redirectURL = AndroidJNI.GetStringField(obj, fieldID);
				fieldID = AndroidJNI.GetFieldID(intPtr, "fullScreenAdURL", "Ljava/lang/String;");
				m_featuredAppInfo.fullScreenAdURL = AndroidJNI.GetStringField(obj, fieldID);
				AndroidJNI.DeleteLocalRef(intPtr);
				AndroidJNI.DeleteLocalRef(obj);
			}
			else if (AndroidJNI.CallBooleanMethod(m_tapjoyConnectInstance, m_didReceiveFeaturedAppDataFailMethod, new jvalue[0]))
			{
				m_queryFeaturedApp = false;
			}
		}*/
	}

	private static string TJ_GetTapjoyID()
	{
		return "";//AndroidJNI.CallStringMethod(m_tapjoyConnectInstance, m_getAppIDMethod, new jvalue[0]);
	}

	private static string TJ_getUserID()
	{
		return "";//AndroidJNI.CallStringMethod(m_tapjoyConnectInstance, m_getUserIDMethod, new jvalue[0]);
	}

	private static void TJ_setUserID(string userId)
	{
		/*IntPtr intPtr = AndroidJNI.NewStringUTF(userId);
		jvalue[] array = new jvalue[1];
		array[0].l = intPtr;
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_setUserIDMethod, array);
		AndroidJNI.DeleteLocalRef(intPtr);*/
	}

	private static void TJ_openTapjoyInterface()
	{
		//AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_showOffersMethod, new jvalue[0]);
	}

	private static void TJ_closeTapjoyInterface()
	{
	}

	private static bool TJ_canDisplayInterface()
	{
		return true;
	}

	private static bool TJ_interfaceIsOpen()
	{
		return false;
	}

	private static uint TJ_getRemainingTapjoyPoints()
	{
		return 0u;
	}

	private static bool TJ_consumeTapjoyPoints(uint points)
	{
		return false;
	}

	private static uint TJ_getPreviousConsumedThisSession()
	{
		return 0u;
	}

	private static uint TJ_getServerTapjoyPoints()
	{
		return 0;//(uint)AndroidJNI.CallIntMethod(m_tapjoyConnectInstance, m_getTapPointsTotalMethod, new jvalue[0]);
	}

	private static bool TJ_consumeServerTapjoyPoints(uint points)
	{
		/*int num = AndroidJNI.CallIntMethod(m_tapjoyConnectInstance, m_getTapPointsTotalMethod, new jvalue[0]);
		if (num >= points)
		{
			jvalue[] array = new jvalue[1];
			array[0].i = (int)points;
			AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_spendTapPointsMethod, array);
			return true;
		}*/
		return false;
	}

	private static uint TJ_convertPointsToGameCurrency(uint points)
	{
		return 0u;
	}

	private static uint TJ_getExchangeRate()
	{
		return 0u;
	}

	private static void TJ_debugGivePoints(uint points)
	{
		jvalue[] array = new jvalue[1];
		array[0].i = (int)points;
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_awardTapPointsMethod, array);
	}

	private static void TJ_queryFeaturedApp()
	{
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_getFeaturedAppMethod, new jvalue[0]);
		m_queryFeaturedApp = true;
	}

	private static bool TJ_getFeaturedAppQueryState()
	{
		return false; //m_queryFeaturedApp && !AndroidJNI.CallBooleanMethod(m_tapjoyConnectInstance, m_didReceiveFeaturedAppDataMethod, new jvalue[0]);
	}

	private static TapjoyFeaturedAppInfo TJ_getFeaturedApp()
	{
		return m_featuredAppInfo;
	}

	private static void TJ_showFeaturedAppFullScreenAd()
	{
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_showFeaturedAppFullScreenAdMethod, new jvalue[0]);
	}

	private static void TJ_resetFeaturedApp()
	{
		m_queryFeaturedApp = false;
		m_featuredAppInfo.amount = 0;
		m_featuredAppInfo.maxTimesToDisplayThisApp = 0;
		m_featuredAppInfo.cost = string.Empty;
		m_featuredAppInfo.storeId = string.Empty;
		m_featuredAppInfo.name = string.Empty;
		m_featuredAppInfo.description = string.Empty;
		m_featuredAppInfo.iconURL = string.Empty;
		m_featuredAppInfo.redirectURL = string.Empty;
		m_featuredAppInfo.fullScreenAdURL = string.Empty;
	}

	private static void TJ_showAd(uint pos)
	{
		jvalue[] array = new jvalue[1];
		array[0].i = (int)pos;
		AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_showBannerAdMethod, array);
	}

	private static void TJ_hideAd()
	{
		//AndroidJNI.CallVoidMethod(m_tapjoyConnectInstance, m_hideBannerAdMethod, new jvalue[0]);
	}

	private static void TJ_setVideoCacheCount(uint count)
	{
	}

	private static bool TJ_isVisible()
	{
		return false;
	}

	private static void TJ_screenOrientation(int orientation)
	{
	}

	private static string getJNIObjectClassName(IntPtr obj)
	{
		/*IntPtr intPtr = AndroidJNI.FindClass("java/lang/Class");
		IntPtr intPtr2 = AndroidJNI.FindClass("java/lang/Object");
		IntPtr methodID = AndroidJNI.GetMethodID(intPtr, "getName", "()Ljava/lang/String;");
		IntPtr methodID2 = AndroidJNI.GetMethodID(intPtr2, "getClass", "()Ljava/lang/Class;");
		IntPtr obj2 = AndroidJNI.CallObjectMethod(obj, methodID2, new jvalue[0]);
		string result = AndroidJNI.CallStringMethod(obj2, methodID, new jvalue[0]);
		AndroidJNI.DeleteLocalRef(obj2);
		AndroidJNI.DeleteLocalRef(intPtr2);
		AndroidJNI.DeleteLocalRef(intPtr);
		return result;*/
		return "";
	}

	public static void Initialize(string resId, string secretKey)
	{
		Initialize(resId, secretKey, false);
	}

	public static void Initialize(string resId, string secretKey, bool showVideoAds)
	{
		if (!_isInitialized)
		{
			_isInitialized = true;
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				simulationMode = false;
			}
			TapjoyEventHandler.Init();
			if (!simulationMode)
			{
				TJ_initialize(resId, secretKey, showVideoAds);
			}
		}
	}

	[Obsolete]
	public static void Initialize(string resId, string loResId, string secretKey)
	{
		Initialize(resId, secretKey, false);
	}

	[Obsolete]
	public static void Initialize(string resId, string loResId, string secretKey, bool showVideoAds)
	{
		Initialize(resId, secretKey, showVideoAds);
	}

	public static void OnResume()
	{
		if (!simulationMode)
		{
			TJ_onResume();
		}
	}

	public static void Update()
	{
		if (!simulationMode)
		{
			int deltaMS = (int)((Time.realtimeSinceStartup - lastTime) * 1000f);
			TJ_tick(deltaMS);
		}
		lastTime = Time.realtimeSinceStartup;
	}

	public static void Destroy()
	{
		if (!simulationMode)
		{
			TJ_destroy();
		}
	}

	public static string GetTapjoyID()
	{
		if (!simulationMode)
		{
			return TJ_GetTapjoyID();
		}
		return "TestTapjoyID";
	}

	public static string GetUserID()
	{
		if (!simulationMode)
		{
			return TJ_getUserID();
		}
		return "TestUserID";
	}

	public static void SetUserID(string userId)
	{
		if (!simulationMode)
		{
			TJ_setUserID(userId);
		}
	}

	public static void OpenTapjoyInterface()
	{
		if (!simulationMode)
		{
			TJ_openTapjoyInterface();
		}
	}

	[Obsolete("Doesn't work")]
	public static void CloseTapjoyInterface()
	{
		if (!simulationMode)
		{
			TJ_closeTapjoyInterface();
		}
	}

	public static bool CanDisplayInterface()
	{
		if (simulationMode)
		{
			return false;
		}
		return TJ_canDisplayInterface();
	}

	public static bool InterfaceIsOpen()
	{
		if (simulationMode)
		{
			return false;
		}
		return TJ_interfaceIsOpen();
	}

	[Obsolete("Use GetServerTapjoyPoints")]
	public static uint GetRemainingTapjoyPoints()
	{
		if (simulationMode)
		{
			return simulationPoints;
		}
		return TJ_getRemainingTapjoyPoints();
	}

	[Obsolete("Use ConsumeServerTapjoyPoints")]
	public static bool ConsumeTapjoyPoints(uint points)
	{
		if (simulationMode)
		{
			if (simulationPoints >= points)
			{
				simulationPoints -= points;
				prevConsumedSimulationPoints = points;
				return true;
			}
			return false;
		}
		return TJ_consumeTapjoyPoints(points);
	}

	[Obsolete("Locally saved points API is unsafe and should not be used")]
	public static uint GetPreviousConsumedThisSession()
	{
		if (simulationMode)
		{
			return prevConsumedSimulationPoints;
		}
		return TJ_getPreviousConsumedThisSession();
	}

	public static uint GetServerTapjoyPoints()
	{
		return TJ_getServerTapjoyPoints();
	}

	public static bool ConsumeServerTapjoyPoints(uint points)
	{
		return TJ_consumeServerTapjoyPoints(points);
	}

	public static uint ConvertPointsToGameCurrency(uint points)
	{
		if (simulationMode)
		{
			return 1 * points;
		}
		return TJ_convertPointsToGameCurrency(points);
	}

	public static uint GetExchangeRate()
	{
		if (simulationMode)
		{
			return 1u;
		}
		return TJ_getExchangeRate();
	}

	public static void DebugGivePoints(uint points)
	{
		if (simulationMode)
		{
			simulationPoints += points;
		}
		else
		{
			TJ_debugGivePoints(points);
		}
	}

	public static void queryFeaturedApp()
	{
		if (!simulationMode)
		{
			TJ_queryFeaturedApp();
		}
	}

	public static bool getFeaturedAppQueryState()
	{
		if (!simulationMode)
		{
			return TJ_getFeaturedAppQueryState();
		}
		return false;
	}

	public static TapjoyFeaturedAppInfo getFeaturedApp()
	{
		if (!simulationMode)
		{
			return TJ_getFeaturedApp();
		}
		return default(TapjoyFeaturedAppInfo);
	}

	public static void showFeaturedAppFullScreenAd()
	{
		TJ_showFeaturedAppFullScreenAd();
	}

	public static void resetFeaturedApp()
	{
		if (!simulationMode)
		{
			TJ_resetFeaturedApp();
		}
	}

	public static void showAd(uint pos)
	{
		if (!simulationMode)
		{
			TJ_showAd(pos);
		}
	}

	public static void hideAd()
	{
		if (!simulationMode)
		{
			TJ_hideAd();
		}
	}

	public static void SetVideoCacheCount(uint count)
	{
		if (!simulationMode)
		{
			TJ_setVideoCacheCount(count);
		}
	}

	public static bool isVisible()
	{
		if (!simulationMode)
		{
			return TJ_isVisible();
		}
		return false;
	}

	public static void RegisterVideoCallback(TapjoyEventHandler.VideoStateHandler handler)
	{
		TapjoyEventHandler.GetHandlerInstance().videoStateHandler = handler;
	}
}
