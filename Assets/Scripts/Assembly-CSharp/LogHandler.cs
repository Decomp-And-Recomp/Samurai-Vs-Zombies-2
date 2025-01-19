using System;
using UnityEngine;

public static class LogHandler
{
	private static bool registered;

	private static Application.LogCallback logHandlers;

	public static void RegisterLogCallback(Application.LogCallback callback)
	{
		if (!registered)
		{
			Application.RegisterLogCallback(HandleLog);
			registered = true;
		}
		logHandlers = (Application.LogCallback)Delegate.Combine(logHandlers, callback);
	}

	private static void HandleLog(string logString, string stackTrace, LogType type)
	{
		GenericUtils.TryInvoke(logHandlers, logString, stackTrace, type);
	}
}
