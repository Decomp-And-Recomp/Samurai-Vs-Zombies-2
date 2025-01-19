using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

public static class Assertion
{
	private sealed class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Assertion");
		}
	}

	[Serializable]
	public class Exception : System.Exception
	{
		public Exception()
		{
		}

		public Exception(string message)
			: base(message)
		{
		}

		public Exception(string message, System.Exception innerException)
			: base(message, innerException)
		{
		}

		protected Exception(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	private enum AssertionMessageButton
	{
		Break = 0,
		Ignore = 1,
		AlwaysIgnoreThis = 2
	}

	private const string DefineEnabled = "ASSERTION_ENABLED";

	private static HashSet<string> ignoredAssertions;

	private static bool failedAssertionDelegateSet;

	private static Action<string, IEnumerable<StackFrame>> failedAssertionDelegate;

	public static bool Enabled
	{
		get
		{
			return false;
		}
	}

	public static Action<string, IEnumerable<StackFrame>> FailedAssertionDelegate
	{
		get
		{
			if (!failedAssertionDelegateSet)
			{
				Action<string, IEnumerable<StackFrame>> action = delegate(string message, IEnumerable<StackFrame> stackTrace)
				{
					Application.Quit();
					throw new Exception(message);
				};
				FailedAssertionDelegate = action;
			}
			return failedAssertionDelegate;
		}
		set
		{
			failedAssertionDelegateSet = true;
			failedAssertionDelegate = value;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ASSERTION_ENABLED")]
	public static void Check(bool condition)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ASSERTION_ENABLED")]
	public static void Check(bool condition, string format, params object[] args)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ASSERTION_ENABLED")]
	public static void Fail()
	{
	}

	[Conditional("ASSERTION_ENABLED")]
	[Conditional("UNITY_EDITOR")]
	public static void Fail(string format, params object[] args)
	{
	}

	[Conditional("ASSERTION_ENABLED")]
	[Conditional("UNITY_EDITOR")]
	public static void Check(bool condition, UnityEngine.Object context)
	{
	}

	[Conditional("ASSERTION_ENABLED")]
	[Conditional("UNITY_EDITOR")]
	public static void Check(bool condition, UnityEngine.Object context, string format, params object[] args)
	{
	}

	[Conditional("ASSERTION_ENABLED")]
	[Conditional("UNITY_EDITOR")]
	public static void Fail(UnityEngine.Object context)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ASSERTION_ENABLED")]
	public static void Fail(UnityEngine.Object context, string format, params object[] args)
	{
	}

	private static void FailImpl(UnityEngine.Object context, string format, params object[] args)
	{
		IEnumerable<StackFrame> enumerable = new StackTrace(true).GetFrames().Skip(2);
		StringBuilder stringBuilder = new StringBuilder(format);
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append("\n\n");
		}
		stringBuilder.Append(PrintStackTrace(enumerable));
		if (!object.ReferenceEquals(context, null))
		{
		}
		Action<string, IEnumerable<StackFrame>> action = FailedAssertionDelegate;
		if (action != null)
		{
			string arg = Logging.SimpleFormatter.FormatMessage(format, args);
			action(arg, enumerable);
		}
	}

	private static string PrintStackTrace(IEnumerable<StackFrame> frames)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (StackFrame frame in frames)
		{
			MethodBase method = frame.GetMethod();
			stringBuilder.AppendFormat("{0}.{1} (at {2}:{3})\n", method.DeclaringType, method.Name, frame.GetFileName(), frame.GetFileLineNumber());
		}
		return stringBuilder.ToString();
	}
}
