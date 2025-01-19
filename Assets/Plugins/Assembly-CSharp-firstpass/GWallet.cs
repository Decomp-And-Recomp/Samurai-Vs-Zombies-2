using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GWallet
{
	private const string m_ExternalFileName = "gwallet";

	private const string m_ExternalUnitTestFileName = "gwut";

	private static string m_storeProvider;

	private static string m_sku;

	private static string m_serverURL;

	private static bool m_debugEnabled;

	private static bool m_init;

	private static AndroidJavaClass jc;

	private static AndroidJavaObject jo;

	static GWallet()
	{
		m_storeProvider = string.Empty;
		m_sku = string.Empty;
		m_serverURL = string.Empty;
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		GWallet_init(null, null, null, jo.GetRawObject());
	}

	[DllImport("gwallet")]
	private static extern void GWallet_init(string storeProvider, string sku, string serverURL, IntPtr context);

	[DllImport("gwallet")]
	private static extern void GWallet_onSuspend();

	[DllImport("gwallet")]
	private static extern void GWallet_onDestroy();

	[DllImport("gwallet")]
	private static extern void GWallet_onResume();

	[DllImport("gwallet")]
	private static extern void GWallet_update(int updateTimeInMS);

	[DllImport("gwallet")]
	private static extern int GWallet_subscribeWithReceipt(string storeReceipt, string subscriberUserId);

	[DllImport("gwallet")]
	private static extern bool GWallet_isSubscriber();

	[DllImport("gwallet")]
	private static extern int GWallet_getBalance(ref int balance);

	[DllImport("gwallet")]
	private static extern int GWallet_addCurrency(uint amount, string description, string eventType, ref int balance);

	[DllImport("gwallet")]
	private static extern int GWallet_subtractCurrency(uint amount, string description, string eventType, ref int balance);

	[DllImport("gwallet")]
	private static extern int GWallet_getUserId();

	[DllImport("gwallet")]
	private static extern int GWallet_getAccountId();

	[DllImport("gwallet")]
	private static extern string GWallet_getAccountEmail();

	[DllImport("gwallet")]
	private static extern string GWallet_getUserStatus();

	[DllImport("gwallet")]
	private static extern int GWallet_getDeviceStatus();

	[DllImport("gwallet")]
	private static extern string GWallet_getVersion();

	[DllImport("gwut")]
	private static extern void GWUT_Unity_RunTests();

	[DllImport("gwallet")]
	private static extern string GWallet_getStringResult(int value);

	[DllImport("gwallet")]
	private static extern void GWallet_setCallBackHandler(string objectName, string methodName);

	[DllImport("gwallet")]
	private static extern int GWallet_getNumNotifications();

	[DllImport("gwallet")]
	private static extern bool GWallet_getNotificationAtIndex(int index, ref GWNotification_Unity notification);

	[DllImport("gwallet")]
	private static extern bool GWallet_setNotificationConsumed(int index);

	[DllImport("gwallet")]
	private static extern int GWallet_getNumAdvertisements();

	[DllImport("gwallet")]
	private static extern bool GWallet_getAdvertisementAtIndex(int index, ref _GWAdvertisement_Unity advertisement);

	[DllImport("gwallet")]
	private static extern bool GWallet_disposeOfAdvertisement(ref _GWAdvertisement_Unity advertisement);

	[DllImport("gwallet")]
	private static extern int GWallet_getNumSubscriptionPlans();

	[DllImport("gwallet")]
	private static extern bool GWallet_getSubscriptionPlanAtIndex(int index, ref GWSubscriptionPlan_Unity subscriptionPlan);

	[DllImport("gwallet")]
	private static extern int GWallet_getNumSubscriptionRecommendations();

	[DllImport("gwallet")]
	private static extern bool GWallet_getSubscriptionRecommendationAtIndex(int index, ref GWSubscriptionRecommendation_Unity subscriptionRecommendation);

	[DllImport("gwallet")]
	private static extern int GWallet_getNumIAPRecommendations();

	[DllImport("gwallet")]
	private static extern bool GWallet_getIAPRecommendationAtIndex(int index, ref GWIAPRecommendation_Unity iapRecommendation);

	[DllImport("gwallet")]
	private static extern bool GWallet_isDebugEnabled();

	[DllImport("gwallet")]
	private static extern int GWallet_LogEvent(string description, string eventType);

	[DllImport("gwallet")]
	private static extern int GWallet_addSoftCurrency(int amount, string description, string eventType);

	[DllImport("gwallet")]
	private static extern int GWallet_subtractSoftCurrency(int amount, string description, string eventType);

	private static void Log(string message)
	{
		if (m_debugEnabled)
		{
			Debug.Log(message);
		}
	}

	public static void Init(string storeProvider, string sku, string serverURL, string objectName, string methodName)
	{
		Log("GWallet.Init entered");
		m_storeProvider = storeProvider;
		m_sku = sku;
		m_serverURL = serverURL;
		GWallet_init(m_storeProvider, m_sku, m_serverURL, jo.GetRawObject());
		m_debugEnabled = GWallet_isDebugEnabled();
		if (objectName != null && objectName.Length > 0 && methodName != null && methodName.Length > 0)
		{
			Log("GWallet.SetCallbackHandler entered");
			GWallet_setCallBackHandler(objectName, methodName);
			Log("GWallet.SetCallbackHandler exited");
		}
		m_init = true;
		Log("GWallet.Init exited");
	}

	public static void Update(int updateTimeInMS)
	{
		if (m_init)
		{
			GWallet_update(updateTimeInMS);
		}
	}

	public static void Pause()
	{
		Log("GWallet.Pause entered");
		GWallet_onSuspend();
		Log("GWallet.Pause exited");
	}

	public static void Resume()
	{
		Log("GWallet.Resume entered");
		GWallet_onResume();
		Log("GWallet.Resume exited");
	}

	public static void Destroy()
	{
		Log("GWallet.Destroy entered");
		GWallet_onDestroy();
		Log("GWallet.Destroy exited");
	}

	public static eGWalletCompletionStatus Subscribe(string storeReceipt, string subscriberUserId)
	{
		Log("GWallet.Subscribe entered");
		eGWalletCompletionStatus result = (eGWalletCompletionStatus)GWallet_subscribeWithReceipt(storeReceipt, subscriberUserId);
		Log("GWallet.Subscribe exited");
		return result;
	}

	public static eGWalletCompletionStatus GetBalance(ref int pReturnBalance)
	{
		eGWalletCompletionStatus eGWalletCompletionStatus2 = eGWalletCompletionStatus.GWALLET_SUCCESS;
		if (m_init)
		{
			return (eGWalletCompletionStatus)GWallet_getBalance(ref pReturnBalance);
		}
		pReturnBalance = 0;
		return eGWalletCompletionStatus.GWALLET_ERROR_NOT_INITIALIZED;
	}

	public static eGWalletCompletionStatus AddCurrency(uint amount, string description, string eventType, ref int pReturnBalance)
	{
		Log("GWallet.AddCurrency called");
		return (eGWalletCompletionStatus)GWallet_addCurrency(amount, description, eventType, ref pReturnBalance);
	}

	public static eGWalletCompletionStatus SubtractCurrency(uint amount, string description, string eventType, ref int pReturnBalance)
	{
		Log("GWallet.SubtractCurrency called");
		return (eGWalletCompletionStatus)GWallet_subtractCurrency(amount, description, eventType, ref pReturnBalance);
	}

	public static eGWalletCompletionStatus LogEvent(int id, bool impression)
	{
		Log("GWallet.LogEvent called");
		int balance = 0;
		string description = string.Format("{0:G}|{1}", id, (!impression) ? "click" : "impression");
		return (eGWalletCompletionStatus)GWallet_addCurrency(0u, description, "EVENT", ref balance);
	}

	public static string GetVersion()
	{
		Log("GWallet.GetVersion called");
		return GWallet_getVersion();
	}

	public static bool IsSubscriber()
	{
		Log("GWallet.IsSubscriber called");
		return GWallet_isSubscriber();
	}

	public static int GetUserId()
	{
		Log("GWallet.GetUserId called");
		return GWallet_getUserId();
	}

	public static int GetAccountId()
	{
		Log("GWallet.GetAccountId called");
		return GWallet_getAccountId();
	}

	public static string GetAccountEmail()
	{
		Log("GWallet.GetAccountEmail called");
		return GWallet_getAccountEmail();
	}

	public static eGWalletCompletionStatus GetDeviceStatus()
	{
		return (eGWalletCompletionStatus)GWallet_getDeviceStatus();
	}

	public static bool IsDeviceSupported()
	{
		return GetDeviceStatus() == eGWalletCompletionStatus.GWALLET_SUCCESS;
	}

	private static string GetStringResult(eGWalletCompletionStatus value)
	{
		Log("GWallet.GetStringResult called");
		return GWallet_getStringResult((int)value);
	}

	public static void ParseCallbackResponse(ref GWCallbackResponse_Unity callbackResponseData, string rawCallbackString)
	{
		Log("GWallet.ParseCallbackResponse called");
		string[] array = rawCallbackString.Split('|');
		callbackResponseData.m_completionStatus = eGWalletCompletionStatus.GWALLET_ERROR_UNKNOWN;
		callbackResponseData.m_completedCallType = eGWalletCallType.GW_CALLTYPE_UNKNOWN;
		callbackResponseData.m_completionMessage = "UNKNOWN ERROR";
		if (array.Length == 3)
		{
			try
			{
				callbackResponseData.m_completionStatus = (eGWalletCompletionStatus)Convert.ToInt32(array[0]);
			}
			catch (FormatException ex)
			{
				Console.WriteLine("Input string is not a sequence of digits.\n" + ex.ToString());
			}
			catch (OverflowException ex2)
			{
				Console.WriteLine("The number cannot fit in an Int32.\n" + ex2.ToString());
			}
			try
			{
				callbackResponseData.m_completedCallType = (eGWalletCallType)Convert.ToInt32(array[1]);
			}
			catch (FormatException ex3)
			{
				Console.WriteLine("Input string is not a sequence of digits.\n" + ex3.ToString());
			}
			catch (OverflowException ex4)
			{
				Console.WriteLine("The number cannot fit in an Int32.\n" + ex4.ToString());
			}
			callbackResponseData.m_completionMessage = array[2];
			callbackResponseData.m_completetionStatusAsString = GetStringResult(callbackResponseData.m_completionStatus);
		}
	}

	public static string GetUserStatus()
	{
		return GWallet_getUserStatus();
	}

	public static int GetNumNotifications()
	{
		Log("GWallet.GetNumNotifications called");
		return GWallet_getNumNotifications();
	}

	public static bool GetNotificationAtIndex(int index, ref GWNotification_Unity notification)
	{
		Log("GWallet.GetNumNotifications called");
		return GWallet_getNotificationAtIndex(index, ref notification);
	}

	public static bool SetNotificationConsumed(int notificationId)
	{
		return GWallet_setNotificationConsumed(notificationId);
	}

	public static bool IsNotificationConsumed(int notificationId)
	{
		bool result = false;
		GWNotification_Unity notification = default(GWNotification_Unity);
		int numNotifications = GetNumNotifications();
		for (int i = 0; i < numNotifications; i++)
		{
			if (GetNotificationAtIndex(i, ref notification) && notification.m_id == notificationId)
			{
				result = notification.m_consumed;
				break;
			}
		}
		return result;
	}

	public static int GetNumAdvertisements()
	{
		Log("GWallet.GetNumAdvertisements called");
		return GWallet_getNumAdvertisements();
	}

	public static bool GetAdvertisementAtIndex(int index, ref GWAdvertisement_Unity advertisement)
	{
		Log("GWallet.GetAdvertisementAtIndex called");
		_GWAdvertisement_Unity advertisement2 = default(_GWAdvertisement_Unity);
		bool flag = GWallet_getAdvertisementAtIndex(index, ref advertisement2);
		if (flag)
		{
			advertisement.m_attributeCount = advertisement2.m_attributeCount;
			advertisement.m_expiryDate = advertisement2.m_expiryDate;
			advertisement.m_resourceUrl = advertisement2.m_resourceUrl;
			advertisement.m_type = advertisement2.m_type;
			advertisement.m_displayLocation = advertisement2.m_displayLocation;
			advertisement.m_key = new string[advertisement2.m_attributeCount];
			advertisement.m_value = new string[advertisement2.m_attributeCount];
			for (int i = 0; i < advertisement.m_attributeCount; i++)
			{
				int ofs = i * IntPtr.Size;
				IntPtr ptr = Marshal.ReadIntPtr(advertisement2.m_keyPtr, ofs);
				string text = Marshal.PtrToStringAnsi(ptr);
				advertisement.m_key[i] = text;
				IntPtr ptr2 = Marshal.ReadIntPtr(advertisement2.m_valuePtr, ofs);
				string text2 = Marshal.PtrToStringAnsi(ptr2);
				advertisement.m_value[i] = text2;
			}
			GWallet_disposeOfAdvertisement(ref advertisement2);
		}
		return flag;
	}

	public static int GetNumSubscriptionPlans()
	{
		return GWallet_getNumSubscriptionPlans();
	}

	public static bool GetSubscriptionPlanAtIndex(int index, ref GWSubscriptionPlan_Unity subscriptionPlan)
	{
		return GWallet_getSubscriptionPlanAtIndex(index, ref subscriptionPlan);
	}

	public static int GetNumSubscriptionRecommendations()
	{
		return GWallet_getNumSubscriptionRecommendations();
	}

	public static bool GetSubscriptionRecommendationAtIndex(int index, ref GWSubscriptionRecommendation_Unity subscriptionRecommendation)
	{
		return GWallet_getSubscriptionRecommendationAtIndex(index, ref subscriptionRecommendation);
	}

	public static int GetNumIAPRecommendations()
	{
		return GWallet_getNumIAPRecommendations();
	}

	public static bool GetIAPRecommendationAtIndex(int index, ref GWIAPRecommendation_Unity iapRecommendation)
	{
		return GWallet_getIAPRecommendationAtIndex(index, ref iapRecommendation);
	}

	public static eGWalletCompletionStatus LogEvent(string description, string eventType)
	{
		return (eGWalletCompletionStatus)GWallet_LogEvent(description, eventType);
	}

	public static eGWalletCompletionStatus AddSoftCurrency(int amount, string description, string eventType)
	{
		return (eGWalletCompletionStatus)GWallet_addSoftCurrency(amount, description, eventType);
	}

	public static eGWalletCompletionStatus SubtractSoftCurrency(int amount, string description, string eventType)
	{
		return (eGWalletCompletionStatus)GWallet_subtractSoftCurrency(amount, description, eventType);
	}
}
