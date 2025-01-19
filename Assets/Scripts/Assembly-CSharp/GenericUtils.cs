using System;
using System.Diagnostics;

public class GenericUtils
{
	public static string StackTrace()
	{
		string text = string.Empty;
		StackTrace stackTrace = new StackTrace(true);
		StackFrame[] frames = stackTrace.GetFrames();
		bool flag = true;
		StackFrame[] array = frames;
		foreach (StackFrame stackFrame in array)
		{
			if (flag)
			{
				flag = false;
				continue;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			else
			{
				text += "###STACK###\n";
				text += "###########\n";
			}
			text += stackFrame.ToString();
		}
		return text + "\n###########";
	}

	public static bool TryInvoke(Delegate del, params object[] args)
	{
		object returnValue;
		return TryInvoke(del, out returnValue, args);
	}

	public static bool TryInvoke(Delegate del, out object returnValue, params object[] args)
	{
		if (del == null)
		{
			returnValue = null;
			return false;
		}
		returnValue = del.DynamicInvoke(args);
		return true;
	}

	public static void InvokeInSequence(Action onComplete, params Action<Action>[] dels)
	{
		InvokeInSequenceAt(0, onComplete, dels);
	}

	private static void InvokeInSequenceAt(int index, Action onComplete, params Action<Action>[] dels)
	{
		if (dels == null || index >= dels.Length)
		{
			if (onComplete != null)
			{
				onComplete();
			}
		}
		else
		{
			dels[index](delegate
			{
				InvokeInSequenceAt(++index, onComplete, dels);
			});
		}
	}

	public static void InvokeInParallel(Action onComplete, params Action<Action>[] dels)
	{
		if (dels == null || dels.Length == 0)
		{
			if (onComplete != null)
			{
				onComplete();
			}
			return;
		}
		int delsToInvoke = dels.Length;
		foreach (Action<Action> action in dels)
		{
			action(delegate
			{
				if (--delsToInvoke == 0 && onComplete != null)
				{
					onComplete();
				}
			});
		}
	}
}
