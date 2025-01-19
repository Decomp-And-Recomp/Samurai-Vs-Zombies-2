using System;
using UnityEngine;

internal static class JniHelper
{
	public static void CallStaticSafe(this AndroidJavaClass clazz, string methodName, params object[] args)
	{
		PushLocalFrame();
		try
		{
			clazz.CallStatic(methodName, args);
		}
		finally
		{
			PopLocalFrame();
		}
	}

	public static void PushLocalFrame()
	{
		PushLocalFrame(128);
	}

	public static void PushLocalFrame(int capacity)
	{
		if (AndroidJNI.PushLocalFrame(capacity) != 0)
		{
			AndroidJNI.ExceptionClear();
			if (AndroidJNI.PushLocalFrame(0) != 0)
			{
				string message = string.Format("Failed to allocate memory for {0} local JNI references", capacity);
				throw new InsufficientMemoryException(message);
			}
		}
	}

	public static void PopLocalFrame()
	{
		AndroidJNI.PopLocalFrame(IntPtr.Zero);
	}

	public static IntPtr PopLocalFrame(IntPtr result)
	{
		return AndroidJNI.PopLocalFrame(result);
	}

	public static int GetFreeLocalReferenceCount()
	{
		int num = 0;
		int num2 = 1;
		while (DoesHaveLocalReferenceCount(num2) && num2 < 1073741824)
		{
			num2 *= 2;
		}
		while (num2 > 0)
		{
			int num3 = num + num2;
			if (DoesHaveLocalReferenceCount(num3))
			{
				num = num3;
			}
			num2 /= 2;
		}
		return num;
	}

	private static bool DoesHaveLocalReferenceCount(int count)
	{
		if (AndroidJNI.PushLocalFrame(count) == 0)
		{
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
			return true;
		}
		AndroidJNI.ExceptionClear();
		return false;
	}
}
