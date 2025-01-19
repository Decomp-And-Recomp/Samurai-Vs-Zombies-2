using System.Diagnostics;
using UnityEngine;

public sealed class Debug
{
	public static bool isDebugBuild
	{
		get
		{
			return UnityEngine.Debug.isDebugBuild;
		}
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		UnityEngine.Debug.DrawLine(start, end, color);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawLine(Vector3 start, Vector3 end)
	{
		UnityEngine.Debug.DrawLine(start, end);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
		UnityEngine.Debug.DrawRay(start, dir, color);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DrawRay(Vector3 start, Vector3 dir)
	{
		UnityEngine.Debug.DrawRay(start, dir);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void Break()
	{
		UnityEngine.Debug.Break();
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	public static void DebugBreak()
	{
		UnityEngine.Debug.DebugBreak();
	}

	[Conditional("LOGGING_LEVEL_INFO")]
	public static void Log(object message)
	{
	}

	[Conditional("LOGGING_LEVEL_INFO")]
	public static void Log(object message, Object context)
	{
	}

	[Conditional("LOGGING_LEVEL_ERROR")]
	public static void LogError(object message)
	{
	}

	[Conditional("LOGGING_LEVEL_ERROR")]
	public static void LogError(object message, Object context)
	{
	}

	[Conditional("LOGGING_LEVEL_WARN")]
	public static void LogWarning(object message)
	{
	}

	[Conditional("LOGGING_LEVEL_WARN")]
	public static void LogWarning(object message, Object context)
	{
	}
}
