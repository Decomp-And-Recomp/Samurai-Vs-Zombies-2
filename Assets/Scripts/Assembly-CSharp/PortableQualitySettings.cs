using System;
using UnityEngine;

public static class PortableQualitySettings
{
	private static string CurrentLevel
	{
		get
		{
			return QualitySettings.names[QualitySettings.GetQualityLevel()];
		}
		set
		{
			int num = Array.FindIndex(QualitySettings.names, (string n) => n.Equals(value));
			if (num != -1)
			{
				QualitySettings.SetQualityLevel(num);
			}
		}
	}

	public static void SetQualityLevelForDevice()
	{
		string currentLevel = CurrentLevel;
		switch (GetQuality())
		{
		case EPortableQualitySetting.Low:
			currentLevel = "Fastest";
			break;
		case EPortableQualitySetting.Medium:
			currentLevel = "Fast";
			break;
		case EPortableQualitySetting.High:
			currentLevel = "Beautiful";
			break;
		case EPortableQualitySetting.VeryHigh:
			currentLevel = "Fantastic";
			break;
		default:
			currentLevel = "Fastest";
			break;
		}
		if (currentLevel != CurrentLevel)
		{
			CurrentLevel = currentLevel;
		}
		PrintQualitySettings();
	}

	public static EPortableQualitySetting GetQuality()
	{
		return GetQualityOfAndroidDevice();
	}

	//public static EPortableQualitySetting GetQualityOfIOSDevice(UnityEngine.iOS.DeviceGeneration generation, bool warnOnUnknownDevice)
	//{
	//	switch (generation)
	//	{
	//	case UnityEngine.iOS.DeviceGeneration.iPhone:
	//	case UnityEngine.iOS.DeviceGeneration.iPhone3G:
	//	case UnityEngine.iOS.DeviceGeneration.iPodTouch1Gen:
	//	case UnityEngine.iOS.DeviceGeneration.iPodTouch2Gen:
	//	case UnityEngine.iOS.DeviceGeneration.iPodTouch3Gen:
	//	case UnityEngine.iOS.DeviceGeneration.iPad1Gen:
	//		return EPortableQualitySetting.Low;
	//	case UnityEngine.iOS.DeviceGeneration.iPhone3GS:
	//	case UnityEngine.iOS.DeviceGeneration.iPhone4:
	//	case UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen:
	//		return EPortableQualitySetting.Medium;
	//	case UnityEngine.iOS.DeviceGeneration.iPad2Gen:
	//	case UnityEngine.iOS.DeviceGeneration.iPhone4S:
	//	case UnityEngine.iOS.DeviceGeneration.iPad3Gen:
	//		return EPortableQualitySetting.High;
	//	default:
	//		if (warnOnUnknownDevice)
	//		{
	//		}
	//		return EPortableQualitySetting.High;
	//	}
	//}

	public static EPortableQualitySetting GetQualityOfAndroidDevice()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return EPortableQualitySetting.High;
#endif
		switch (AndroidQualitySettings.Quality)
		{
			case AndroidQuality.Tier_0:
				return EPortableQualitySetting.Low;
			case AndroidQuality.Tier_1:
				return EPortableQualitySetting.Medium;
			case AndroidQuality.Tier_2:
				return EPortableQualitySetting.Medium;
			case AndroidQuality.Tier_3:
			case AndroidQuality.Tier_4:
				return EPortableQualitySetting.High;
			default:
				return EPortableQualitySetting.High;
		}
	}

	public static void PrintQualitySettings()
	{
	}
}
