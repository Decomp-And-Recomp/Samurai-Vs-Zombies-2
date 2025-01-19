using UnityEngine;

public class AppControllerGriptonite
{
	private static bool _IsAppEnteringForeground()
	{
		return false;
	}

	private static bool _IsAppEnteringBackground()
	{
		return false;
	}

	public static bool IsAppEnteringBackground()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _IsAppEnteringBackground();
		}
		return false;
	}

	public static bool IsAppEnteringForeground()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _IsAppEnteringForeground();
		}
		return false;
	}
}
