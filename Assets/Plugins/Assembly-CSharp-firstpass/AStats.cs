using System;
using System.Collections.Generic;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

public class AStats
{
	public class Flurry
	{
		public static void SetExtras(params string[] parameters)
		{
		}

		public static void StartSession()
		{
		}

		public static void LogEvent(string eventId)
		{
		}

		public static void LogEventTimed(string eventId)
		{
		}

		public static void LogEvent(string eventId, string info)
		{
			LogEvent(eventId, "info", info);
		}

		public static void LogEventTimed(string eventId, string info)
		{
			LogEventTimed(eventId, "info", info);
		}

		public static void LogEvent(string eventId, params string[] parameters)
		{
		}

		public static void LogEventTimed(string eventId, params string[] parameters)
		{
		}

		public static void LogEvent(string eventId, Dictionary<string, string> dict)
		{
		}

		public static void LogEventTimed(string eventId, Dictionary<string, string> dict)
		{
		}

		public static void EndTimedEvent(string eventId)
		{
		}

		public static void EndSession()
		{
		}
	}

	public class Kontagent
	{
		private const string JailbrokenPrefix = "ZJB_";

		public static void StartSession()
		{
		}

		public static void LogEvent(string eventId)
		{
		}

		public static void LogEvent(string eventId, string info)
		{
			LogEvent(eventId, "info", info);
		}

		public static void LogEvent(string eventId, params string[] parameters)
		{
		}

		public static void LogEvent(string eventId, Dictionary<string, string> dict)
		{
		}

		public static void RevenueTracking(int val)
		{
		}

		public static void RevenueTracking(int val, string info)
		{
			RevenueTracking(val, "info", info);
		}

		public static void RevenueTracking(int val, params string[] parameters)
		{
		}

		public static void RevenueTracking(int val, Dictionary<string, string> dict)
		{
		}

		public static void EndSession()
		{
		}

		private static string[] AdjustArguments(string[] args)
		{
			if (args == null)
			{
				return args;
			}
			if (!AJavaTools.DeviceInfo.IsDeviceRooted())
			{
				return args;
			}
			int num = args.Length;
			string[] array = new string[num];
			for (int i = 0; i < num; i += 2)
			{
				string key = (array[i] = args[i]);
				if (i + 1 >= num)
				{
					break;
				}
				array[i + 1] = AdjustRootedValue(key, args[i + 1]);
			}
			return array;
		}

		private static Dictionary<string, string> AdjustArguments(Dictionary<string, string> dict)
		{
			if (dict == null)
			{
				return dict;
			}
			if (!AJavaTools.DeviceInfo.IsDeviceRooted())
			{
				return dict;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in dict)
			{
				string key = item.Key;
				dictionary[key] = AdjustRootedValue(key, item.Value);
			}
			return dictionary;
		}

		private static string AdjustRootedValue(string key, string val)
		{
			return (!(key == "st1")) ? val : ("ZJB_" + val);
		}
	}

	public class MobileAppTracking
	{
		public static void Init()
		{
		}

		public static void TrackAction(string eventName)
		{
		}

		public static void RevenueTracking(string eventId, float price, string currency)
		{
		}
	}

	public class GoogleAnalytics
	{
		public static void StartSession()
		{
		}

		public static void LogEvent(string category, string action, string label, long val = 0)
		{
			GA_SendEvent(category, action, label, val);
		}
	}

	public static void Init()
	{
	}

	private static void GA_StartSession(string trackingID)
	{
	}

	private static void GA_SendEvent(string category, string action, string label, long value)
	{
	}
}
