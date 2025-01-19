using System.Diagnostics;
using UnityEngine;

public class LoggerSingleton<T> where T : new()
{
	private const string DefineLevelAtLeastOne = "LOGGING_LEVEL_AT_LEAST_ONE";

	private static Logging.ILogger instance;

	public static bool Propagate
	{
		get
		{
			return Instance.Propagate;
		}
		set
		{
			Instance.Propagate = value;
		}
	}

	public static Logging.ILogger Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			new T();
			if (instance == null)
			{
				SetLoggerName("App." + typeof(T).Name);
			}
			return instance;
		}
	}

	public static int StaticLevel
	{
		get
		{
			return -1;
		}
	}

	[Conditional("LOGGING_LEVEL_AT_LEAST_ONE")]
	[Conditional("UNITY_EDITOR")]
	public static void Log(int level, string format, params object[] args)
	{
		LogImpl(level, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void Debug(string format, params object[] args)
	{
		LogImpl(10, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_INFO")]
	public static void Info(string format, params object[] args)
	{
		LogImpl(20, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_WARN")]
	public static void Warn(string format, params object[] args)
	{
		LogImpl(30, format, args);
	}

	[Conditional("LOGGING_LEVEL_ERROR")]
	[Conditional("UNITY_EDITOR")]
	public static void Error(string format, params object[] args)
	{
		LogImpl(40, format, args);
	}

	[Conditional("LOGGING_LEVEL_CRITICAL")]
	[Conditional("UNITY_EDITOR")]
	public static void Critical(string format, params object[] args)
	{
		LogImpl(50, format, args);
	}

	[Conditional("LOGGING_LEVEL_AT_LEAST_ONE")]
	[Conditional("UNITY_EDITOR")]
	public static void Log(Object context, int level, string format, params object[] args)
	{
		LogWithContextImpl(context, level, format, args);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void Debug(Object context, string format, params object[] args)
	{
		LogWithContextImpl(context, 10, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_INFO")]
	public static void Info(Object context, string format, params object[] args)
	{
		LogWithContextImpl(context, 20, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_WARN")]
	public static void Warn(Object context, string format, params object[] args)
	{
		LogWithContextImpl(context, 30, format, args);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("LOGGING_LEVEL_ERROR")]
	public static void Error(Object context, string format, params object[] args)
	{
		LogWithContextImpl(context, 40, format, args);
	}

	[Conditional("LOGGING_LEVEL_CRITICAL")]
	[Conditional("UNITY_EDITOR")]
	public static void Critical(Object context, string format, params object[] args)
	{
		LogWithContextImpl(context, 50, format, args);
	}

	public static bool IsEnabledFor(int level)
	{
		if (!IsEnabledForDefine(level))
		{
			return false;
		}
		return Instance.IsEnabledFor(level);
	}

	public static void SetLevel(int level)
	{
		Instance.SetLevel(level);
	}

	public static int GetLevel()
	{
		return Instance.GetLevel();
	}

	public static int GetEffectiveLevel()
	{
		return Instance.GetEffectiveLevel();
	}

	public static void AddFilter(Logging.IFilter filter)
	{
		Instance.AddFilter(filter);
	}

	public static void RemoveFilter(Logging.IFilter filter)
	{
		Instance.RemoveFilter(filter);
	}

	public static bool Filter(ref Logging.LogRecord record)
	{
		return Instance.Filter(ref record);
	}

	public static int GetFilterCount()
	{
		return Instance.GetFilterCount();
	}

	public static Logging.IFilter GetFilter(int i)
	{
		return Instance.GetFilter(i);
	}

	public static void AddHandler(Logging.IHandler handler)
	{
		Instance.AddHandler(handler);
	}

	public static void RemoveHandler(Logging.IHandler handler)
	{
		Instance.RemoveHandler(handler);
	}

	public static int GetHandlerCount()
	{
		return Instance.GetHandlerCount();
	}

	public static Logging.IHandler GetHandler(int i)
	{
		return Instance.GetHandler(i);
	}

	private static bool IsEnabledForDefine(int level)
	{
		return false;
	}

	private static void LogImpl(int level, string format, object[] args)
	{
		Instance.Log(level, format, args);
	}

	private static void LogWithContextImpl(Object context, int level, string format, object[] args)
	{
		Instance.Log(context, level, format, args);
	}

	protected static void SetLoggerName(string name)
	{
		instance = Logging.GetLogger(name);
		Logging.RegisterDestructor(delegate
		{
			instance = null;
		});
	}
}
