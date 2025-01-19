using System;

public static class FBBDebug
{
	public static void Trace(string message)
	{
		Console.Out.WriteLine("[Unity|Facebook|Trace] " + message);
	}

	public static void Log(string message)
	{
		Console.Out.WriteLine("[Unity|Facebook|Log] " + message);
	}

	public static void LogWarning(string message)
	{
		Console.Out.WriteLine("[Unity|Facebook|Warning] " + message);
	}

	public static void LogError(string message)
	{
		Console.Error.WriteLine("[Unity|Facebook|ERROR] " + message);
	}
}
