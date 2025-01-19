using System;
using System.Collections.Generic;
using Glu.Kontagent;

public class Analytics : Singleton<Analytics>
{
	private enum SessionStatus
	{
		Started = 0,
		Stopped = 1
	}

	public Action OnNewSession;

	private SessionStatus status = SessionStatus.Stopped;

	private DateTime? pauseTime;

	private int sessionContinueSeconds = 10;

	public static KeyValuePair<string, object> Param(string key, object obj)
	{
		return new KeyValuePair<string, object>(key, obj);
	}

	public static KeyValuePair<string, string> KParam(string key, string val)
	{
		return new KeyValuePair<string, string>(key, val);
	}

	public void LogEvent(string eventTypeId)
	{
		LogEvent(eventTypeId, new Dictionary<string, object>());
	}

	public void LogEvent(string eventTypeId, string info, int? eventValue = null, int? eventReference = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (info != string.Empty)
		{
			dictionary["info"] = info;
		}
		if (eventValue.HasValue)
		{
			dictionary["eventValue"] = eventValue.Value;
		}
		if (eventReference.HasValue)
		{
			dictionary["eventReference"] = eventReference.Value;
		}
		LogEvent(eventTypeId, dictionary);
	}

	public void LogEvent(string eventTypeId, params KeyValuePair<string, object>[] p)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(p.Length);
		for (int i = 0; i < p.Length; i++)
		{
			KeyValuePair<string, object> keyValuePair = p[i];
			dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		}
		LogEvent(eventTypeId, dictionary);
	}

	public void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
	{
		if (eventParams.Count > 8)
		{
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, object> eventParam in eventParams)
		{
			dictionary.Add(eventParam.Key, eventParam.Value.ToString());
		}
		AStats.Flurry.LogEvent(eventTypeId, dictionary);
	}

	public void ApplicationStart()
	{
		if (status == SessionStatus.Stopped)
		{
			StartSession();
		}
	}

	public void ApplicationPause()
	{
		AStats.Flurry.EndSession();
		AStats.Kontagent.EndSession();
	}

	public void ApplicationResume()
	{
		AStats.Flurry.StartSession();
		AStats.Kontagent.StartSession();
	}

	public void ApplicationExit()
	{
		StopSession();
	}

	public void KontagentEvent(string name, string st1, string st2, string st3, int level, int eventValue)
	{
		Kontagent.LogEvent(name, st1, st2, st3, level, eventValue);
	}

	public void KontagentEvent(string name, string st1, string st2, int level, int eventValue)
	{
		Kontagent.LogEvent(name, st1, st2, string.Empty, level, eventValue);
	}

	public void KontagentEvent(string name, string st1, int level, int eventValue)
	{
		Kontagent.LogEvent(name, st1, string.Empty, string.Empty, level, eventValue);
	}

	public void KontagentEvent(string name, string st1, string st2, string st3, int level, int eventValue, params KeyValuePair<string, string>[] p)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(p.Length);
		for (int i = 0; i < p.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = p[i];
			dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		}
		Kontagent.LogEvent(name, st1, st2, st3, level, eventValue, dictionary);
	}

	public void KontagentEvent(string name, string st1, string st2, int level, int eventValue, params KeyValuePair<string, string>[] p)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(p.Length);
		for (int i = 0; i < p.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = p[i];
			dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		}
		Kontagent.LogEvent(name, st1, st2, string.Empty, level, eventValue, dictionary);
	}

	public void KontagentEvent(string name, string st1, int level, int eventValue, params KeyValuePair<string, string>[] p)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(p.Length);
		for (int i = 0; i < p.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = p[i];
			dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		}
		Kontagent.LogEvent(name, st1, string.Empty, string.Empty, level, eventValue, dictionary);
	}

	private void StartSession()
	{
		AStats.Flurry.StartSession();
		AStats.Kontagent.StartSession();
		status = SessionStatus.Started;
		if (OnNewSession != null)
		{
			OnNewSession();
		}
	}

	private void StopSession()
	{
		AStats.Flurry.EndSession();
		AStats.Kontagent.EndSession();
		status = SessionStatus.Stopped;
	}
}
