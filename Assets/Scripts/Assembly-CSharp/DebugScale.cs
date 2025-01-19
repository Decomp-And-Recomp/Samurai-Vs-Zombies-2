using UnityEngine;

public class DebugScale : MonoBehaviour
{
	private static float kIncreaseScaleForReadability = 1f;

	public static bool isIos()
	{
		return Application.platform == RuntimePlatform.IPhonePlayer;
	}

	public static bool isEditor()
	{
		return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor;
	}

	public static bool isIpadFamily()
	{
		if (!isIos())
		{
			return false;
		}
		string deviceModel = SystemInfo.deviceModel;
		return deviceModel.Substring(0, 4) == "iPad";
	}

	public static float Scale()
	{
		float num = 1f;
		if (!isIos())
		{
			if (Screen.width < 512)
			{
				num = 2f;
			}
			else if (Screen.width < 960)
			{
				num = 1.5f;
			}
		}
		return num * kIncreaseScaleForReadability;
	}
}
