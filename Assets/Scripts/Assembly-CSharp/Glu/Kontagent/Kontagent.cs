using System.Collections.Generic;

namespace Glu.Kontagent
{
	public static class Kontagent
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.Kontagent");
			}
		}

		private static void _kontagentStartSession(string apiKey, bool testMode, bool trusted, string integrityDetails, string appVersion)
		{
		}

		private static void _kontagentCustomEvent(string name, string st1, string st2, string st3, string level, string val, string data)
		{
		}

		private static void _kontagentRevenueTracking(int val)
		{
		}

		private static string dictionaryToJSON(Dictionary<string, string> dict)
		{
			if (dict == null)
			{
				return null;
			}
			string text = "{";
			foreach (KeyValuePair<string, string> item in dict)
			{
				if (text != "{")
				{
					text += ",";
				}
				text += "\"";
				text += item.Key;
				text += "\":\"";
				text += item.Value;
				text += "\"";
			}
			text += "}";
			if (text == "{}")
			{
				text = null;
			}
			return text;
		}

		public static void StartSession(string apiKey)
		{
			AStats.Kontagent.StartSession();
		}

		public static void StartSession(string apiKey, bool testMode)
		{
			AStats.Kontagent.EndSession();
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val)
		{
			if (name.Contains("com.glu.samuzombie2."))
			{
				name = name.Replace("com.glu.samuzombie2.", string.Empty).ToLower();
			}
			AStats.Kontagent.LogEvent(name, st1, st2, st3, level.ToString(), val.ToString(), null);
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val, Dictionary<string, string> data)
		{
			string text = ((!level.HasValue) ? null : level.Value.ToString());
			string text2 = ((!val.HasValue) ? null : val.Value.ToString());
			if (st2 != null)
			{
			}
			if (st3 != null)
			{
			}
			if (text != null)
			{
			}
			if (text2 != null)
			{
			}
			if (name.Contains("com.glu.samuzombie2."))
			{
				name = name.Replace("com.glu.samuzombie2.", string.Empty).ToLower();
			}
			data.Add("st1", st1);
			data.Add("st2", st2);
			data.Add("st3", st3);
			data.Add("l", level.ToString());
			data.Add("v", val.ToString());
			AStats.Kontagent.LogEvent(name, data);
		}

		public static void RevenueTracking(int val)
		{
			AStats.Kontagent.RevenueTracking(val);
		}
	}
}
