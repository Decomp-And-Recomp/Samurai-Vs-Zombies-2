using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CFlurry
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.Flurry");
		}
	}

	private static class Impl
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.Flurry.Impl");
			}
		}

		private const string kEventEventTime = "Event time";

		private const string kEventLanguage = "Language";

		private const string kEventCountry = "Country";

		private const string kEventOSVersion = "OS version";

		private static bool m_isDebugLogEnabled;

		private static AndroidJavaClass m_flurryAgent;

		private static GameObject m_goFlurry;

		private static string m_userID;

		private static AndroidJavaClass m_system;

		private static bool m_isAppStarted;

		private static bool m_isSessionStarted;

		private static int m_sessionStartTime;

		static Impl()
		{
			m_isDebugLogEnabled = Debug.isDebugBuild;
			m_flurryAgent = new AndroidJavaClass("com.flurry.android.FlurryAgent");
			m_goFlurry = null;
			m_userID = null;
			m_system = null;
			m_isAppStarted = false;
			m_isSessionStarted = false;
			m_sessionStartTime = 0;
			CreateGameObject();
		}

		public static void SetAppVersion(string version)
		{
			m_flurryAgent.CallStatic("setVersionName", version);
		}

		public static string GetFlurryAgentVersion()
		{
			return m_flurryAgent.CallStatic<int>("getAgentVersion", new object[0]).ToString();
		}

		public static void SetShowErrorInLogEnabled(bool value)
		{
		}

		public static void SetDebugLogEnabled(bool value)
		{
			m_isDebugLogEnabled = value;
		}

		public static void SetSessionContinueSeconds(int seconds)
		{
			m_flurryAgent.CallStatic("setContinueSessionMillis", (long)seconds * 1000L);
		}

		public static void SetSecureTransportEnabled(bool value)
		{
			m_flurryAgent.CallStatic("setUseHttps", value);
		}

		public static string GetUserID()
		{
			if (m_userID == null)
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				if (m_userID == null)
				{
					if (m_isDebugLogEnabled)
					{
					}
					AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getSystemService", new object[1] { "phone" });
					if (androidJavaObject != null)
					{
						m_userID = androidJavaObject.Call<string>("getDeviceId", new object[0]);
					}
				}
				if (m_userID == null)
				{
					if (m_isDebugLogEnabled)
					{
					}
					AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.os.Build");
					if (androidJavaClass2 != null)
					{
						try
						{
							m_userID = androidJavaClass2.GetStatic<string>("SERIAL");
						}
						catch (Exception)
						{
							if (m_isDebugLogEnabled)
							{
							}
							m_userID = null;
						}
					}
				}
			}
			return m_userID;
		}

		public static void StartSession(string apiKey)
		{
			m_system = new AndroidJavaClass("java.lang.System");
			m_userID = null;
			m_userID = GetUserID();
			if (m_userID != null)
			{
				if (!m_isDebugLogEnabled)
				{
				}
			}
			else if (!m_isDebugLogEnabled)
			{
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			m_flurryAgent.CallStatic("onStartSession", @static, apiKey);
			if (m_userID != null)
			{
				m_flurryAgent.CallStatic("setUserId", m_userID);
			}
			if (!m_isAppStarted)
			{
				int num = PlayerPrefs.GetInt(apiKey, 0) + 1;
				LogEvent("AppStart", new Dictionary<string, object> { { "eventValue", num } });
				PlayerPrefs.SetInt(apiKey, num);
				PlayerPrefs.Save();
				m_isAppStarted = true;
			}
			m_sessionStartTime = UNIXTime();
			LogEvent("SessionStart", GetSystemParameters());
			m_isSessionStarted = true;
		}

		public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
		{
			if (eventParams.ContainsKey("Event time"))
			{
				eventParams["Event time"] = m_system.CallStatic<long>("currentTimeMillis", new object[0]).ToString();
			}
			else
			{
				eventParams.Add("Event time", m_system.CallStatic<long>("currentTimeMillis", new object[0]).ToString());
			}
			using (AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.util.HashMap"))
			{
				IntPtr methodID = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
				object[] array = new object[2];
				foreach (KeyValuePair<string, object> eventParam in eventParams)
				{
					array[0] = eventParam.Key;
					array[1] = ((eventParam.Value != null) ? eventParam.Value.ToString() : string.Empty);
					AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
				}
				m_flurryAgent.CallStatic("logEvent", eventTypeId, androidJavaObject);
			}
		}

		public static void SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
		{
		}

		public static void SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
		{
		}

		public static void SetEventLoggingEnabled(bool value)
		{
			m_flurryAgent.CallStatic("setLogEnabled", value);
		}

		public static void StopSession()
		{
			if (m_isSessionStarted)
			{
				LogEvent("SessionStop", new Dictionary<string, object> { 
				{
					"eventValue",
					UNIXTime() - m_sessionStartTime
				} });
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						m_flurryAgent.CallStatic("onEndSession", androidJavaObject);
					}
				}
				m_isSessionStarted = false;
			}
			m_userID = null;
			m_system = null;
		}

		private static void CreateGameObject()
		{
			if (m_goFlurry == null)
			{
				GameObject gameObject = new GameObject("FlurryGameObject");
				gameObject.AddComponent<FlurryBehaviourScript>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				m_goFlurry = gameObject;
			}
		}

		private static int UNIXTime()
		{
			return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}

		private static Dictionary<string, object> GetSystemParameters()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("java.util.Locale");
			AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("getDefault", new object[0]);
			AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.os.Build$VERSION");
			dictionary.Add("Language", androidJavaObject.Call<string>("getLanguage", new object[0]));
			dictionary.Add("Country", androidJavaObject.Call<string>("getCountry", new object[0]));
			dictionary.Add("OS version", androidJavaClass2.GetStatic<string>("RELEASE"));
			return dictionary;
		}
	}

	public static void SetAppVersion(string version)
	{
		Impl.SetAppVersion(version);
	}

	public static string GetFlurryAgentVersion()
	{
		return Impl.GetFlurryAgentVersion();
	}

	public static void SetShowErrorInLogEnabled(bool value)
	{
		Impl.SetShowErrorInLogEnabled(value);
	}

	public static void SetDebugLogEnabled(bool value)
	{
		Impl.SetDebugLogEnabled(value);
	}

	public static void SetSessionContinueSeconds(int seconds)
	{
		Impl.SetSessionContinueSeconds(seconds);
	}

	public static void SetSecureTransportEnabled(bool value)
	{
		Impl.SetSecureTransportEnabled(value);
	}

	public static string GetUserID()
	{
		return Impl.GetUserID() ?? "Undefined";
	}

	public static void StartSession(string apiKey)
	{
		Impl.StartSession(apiKey);
	}

	public static void StopSession()
	{
		Impl.StopSession();
	}

	public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
	{
		if (LoggerSingleton<Logger>.IsEnabledFor(10))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("LogEvent - Started, event \"{0}\"", eventTypeId ?? "null");
			if (eventParams != null && eventParams.Count > 0)
			{
				foreach (KeyValuePair<string, object> eventParam in eventParams)
				{
					stringBuilder.AppendFormat("\t{0}={1}\n", eventParam.Key, (eventParam.Value == null) ? "NULL" : eventParam.Value);
				}
			}
		}
		Impl.LogEvent(eventTypeId, eventParams);
	}

	public static void SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
	{
		Impl.SetSessionReportsOnCloseEnabled(sendSessionReportsOnClose);
	}

	public static void SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
	{
		Impl.SetSessionReportsOnPauseEnabled(setSessionReportsOnPauseEnabled);
	}

	public static void SetEventLoggingEnabled(bool value)
	{
		Impl.SetEventLoggingEnabled(value);
	}
}
