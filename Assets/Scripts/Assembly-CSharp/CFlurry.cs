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

		private static GameObject m_goFlurry;

		private static string m_userID;

		private static bool m_isAppStarted;

		private static bool m_isSessionStarted;

		private static int m_sessionStartTime;

		static Impl()
		{
			m_isDebugLogEnabled = Debug.isDebugBuild;
			m_goFlurry = null;
			m_userID = null;
			m_isAppStarted = false;
			m_isSessionStarted = false;
			m_sessionStartTime = 0;
			CreateGameObject();
		}

		public static void SetAppVersion(string version)
		{
		}

		public static string GetFlurryAgentVersion()
		{
			return "";
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
		}

		public static void SetSecureTransportEnabled(bool value)
		{
		}

		public static string GetUserID()
		{
			return "";
		}

		public static void StartSession(string apiKey)
		{
		}

		public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
		{
		}

		public static void SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
		{
		}

		public static void SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
		{
		}

		public static void SetEventLoggingEnabled(bool value)
		{
		}

		public static void StopSession()
		{
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
