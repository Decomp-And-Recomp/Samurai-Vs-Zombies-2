using System;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

public class AndroidQualitySettings : MonoBehaviour
{
	private const string QualityDebugProperty = "quality";

	private string[] qualityDescription = new string[6] { "iPod4", "iPod4/iPhone4", "iPhone4", "iPad2", "iPad3/iPhone5", "Unknown" };

	private static AndroidQuality internalQuality = AndroidQuality.Unknown;

	private float lastInterval;

	private int frames;

	private float fps;

	private bool showDisplay;

	public static AndroidQuality Quality
	{
		get
		{
			if (internalQuality == AndroidQuality.Unknown)
			{
				GameObject target = new GameObject("AndroidQualitySettings", typeof(AndroidQualitySettings));
				UnityEngine.Object.DontDestroyOnLoad(target);
				Setup();
			}
			return internalQuality;
		}
	}

	public static void OverrideQualitySetting(AndroidQuality quality)
	{
		if (!Debug.isDebugBuild)
		{
			Debug.LogWarning("You can't override quality setting in release from code");
			return;
		}
		string value = null;
		if (quality != AndroidQuality.Unknown)
		{
			int num = (int)quality;
			value = num.ToString();
		}
		OverrideQualitySettingImpl(quality);
	}

	private static void Setup()
	{
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		int processorCount = SystemInfo.processorCount;
		Debug.Log("******* AndroidQualitySettings **********");
		Debug.Log("*** gpu name: " + graphicsDeviceName);
		Debug.Log("*** gpu memory: " + SystemInfo.graphicsMemorySize);
		Debug.Log("*** shader level: " + SystemInfo.graphicsShaderLevel);
		Debug.Log("*** cpu cores: " + processorCount);
		Debug.Log("*** sys memory: " + SystemInfo.systemMemorySize);
		Debug.Log("*****************************************");
		AndroidQuality androidQuality = ReadOverrideQuality();
		if (androidQuality != AndroidQuality.Unknown)
		{
			OverrideQualitySettingImpl(androidQuality);
		}
		else if (graphicsDeviceName.StartsWith("PowerVR"))
		{
			if (processorCount == 1)
			{
				internalQuality = AndroidQuality.Tier_0;
			}
			else if (graphicsDeviceName.Contains("540") || graphicsDeviceName.Contains("544") || graphicsDeviceName.Contains("531"))
			{
				if (Screen.width > 1024)
				{
					internalQuality = AndroidQuality.Tier_3;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_2;
				}
			}
			else
			{
				internalQuality = AndroidQuality.Tier_2;
			}
		}
		else if (graphicsDeviceName.StartsWith("Adreno"))
		{
			Debug.Log("Adreno Found");
			if (processorCount == 1)
			{
				internalQuality = AndroidQuality.Tier_1;
			}
			else if (graphicsDeviceName.Contains("225") || graphicsDeviceName.Contains("220"))
			{
				internalQuality = AndroidQuality.Tier_2;
			}
			else if (graphicsDeviceName.Contains("320") && processorCount >= 4)
			{
				internalQuality = AndroidQuality.Tier_4;
			}
			else
			{
				internalQuality = AndroidQuality.Tier_2;
			}
		}
		else if (graphicsDeviceName.StartsWith("Mali"))
		{
			if (graphicsDeviceName.Contains("400"))
			{
				if (processorCount == 2)
				{
					if (Screen.width > 800)
					{
						internalQuality = AndroidQuality.Tier_2;
					}
					else
					{
						internalQuality = AndroidQuality.Tier_3;
					}
				}
				if (processorCount >= 4)
				{
					internalQuality = AndroidQuality.Tier_4;
				}
			}
			else if (graphicsDeviceName.Contains("604"))
			{
				if (processorCount >= 2)
				{
					internalQuality = AndroidQuality.Tier_4;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_2;
				}
			}
			else
			{
				internalQuality = AndroidQuality.Tier_2;
			}
		}
		else if (graphicsDeviceName.StartsWith("NVIDIA Tegra") || graphicsDeviceName.StartsWith("ULP GeForce"))
		{
			switch (processorCount)
			{
			case 4:
				internalQuality = AndroidQuality.Tier_4;
				break;
			case 2:
				if (Screen.width > 1024)
				{
					internalQuality = AndroidQuality.Tier_1;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_2;
				}
				break;
			default:
				internalQuality = AndroidQuality.Tier_2;
				break;
			}
		}
		else
		{
			internalQuality = AndroidQuality.Tier_1;
		}
	}

	private static AndroidQuality ReadOverrideQuality()
	{
		return AndroidQuality.Unknown;
	}

	private static void OverrideQualitySettingImpl(AndroidQuality quality)
	{
		Debug.LogWarning("Overriding AndroidQuality setting {0}".Fmt(quality));
		internalQuality = quality;
	}

	private void OnGUI()
	{
		if (Debug.isDebugBuild && showDisplay)
		{
			GUILayout.Label(string.Concat("\nGPU name: ", SystemInfo.graphicsDeviceName, "\nGPU memory: ", SystemInfo.graphicsMemorySize, "\nShader level: ", SystemInfo.graphicsShaderLevel, "\nCPU cores: ", SystemInfo.processorCount, "\nSys memory: ", SystemInfo.systemMemorySize, "\nSceenSize: ", Screen.width, "x", Screen.height, "\nQuality: ", internalQuality, " - ", qualityDescription[(int)internalQuality], "\nFPS: ", fps.ToString("f2")));
		}
	}

	private void Update()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + 0.5f)
		{
			if (Input.touchCount > 2)
			{
				showDisplay = !showDisplay;
			}
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
		}
	}
}
