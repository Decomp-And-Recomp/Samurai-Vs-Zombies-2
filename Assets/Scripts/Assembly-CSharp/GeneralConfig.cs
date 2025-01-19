public class GeneralConfig
{
	private static bool? iCloudSupportEnabled;

	private static bool? PlayHavenSupportEnabled;

	public static string[] SupportedLanguages = new string[11]
	{
		"English", "Chinese (Simplified)", "Chinese (Traditional)", "French", "German", "Italian", "Japanese", "Korean", "Portuguese", "Russian",
		"Spanish"
	};

	public static string GameName
	{
		get
		{
			return "samuzombie2";
		}
	}

	public static string Version
	{
		get
		{
			return "2.1.0";
		}
	}

	public static string DataVersion
	{
		get
		{
			return ConfigSchema.Entry("DataVersion");
		}
	}

	public static bool iCloudEnabled
	{
		get
		{
			return false;
		}
	}

	public static string TapJoyAppID
	{
		get
		{
			return ConfigSchema.Entry("TapJoyAppID");
		}
	}

	public static string TapJoySecret
	{
		get
		{
			return ConfigSchema.Entry("TapJoySecret");
		}
	}

	public static string PlayHavenToken
	{
		get
		{
			return ConfigSchema.Entry("PlayHavenToken");
		}
	}

	public static string PlayHavenSecret
	{
		get
		{
			return ConfigSchema.Entry("PlayHavenSecret");
		}
	}

	public static bool PlayHavenEnabled
	{
		get
		{
			if (DataBundleRuntime.Instance == null)
			{
				return false;
			}
			if (!PlayHavenSupportEnabled.HasValue)
			{
				bool result = false;
				string text = ConfigSchema.Entry("PlayHavenEnabled");
				bool flag = false;
				if (text != null)
				{
					bool.TryParse(text, out result);
					flag = true;
				}
				if (!flag)
				{
					return false;
				}
				PlayHavenSupportEnabled = result;
			}
			return PlayHavenSupportEnabled.Value;
		}
	}

	public static string ChartBoostApplicationID
	{
		get
		{
			return ConfigSchema.Entry("ChartBoostApplicationID");
		}
	}

	public static string ChartBoostApplicationSignature
	{
		get
		{
			return ConfigSchema.Entry("ChartBoostApplicationSignature");
		}
	}

	public static string MobileAppTrackingAdvertiserId
	{
		get
		{
			return ConfigSchema.Entry("MobileAppTrackingAdvertiserId");
		}
	}

	public static string MobileAppTrackingAppKey
	{
		get
		{
			return ConfigSchema.Entry("MobileAppTrackingAppKey");
		}
	}

	public static string FlurryApiKey
	{
		get
		{
			if (!Debug.isDebugBuild && IsLive)
			{
				return ConfigSchema.Entry("FlurryAndroidLive");
			}
			return ConfigSchema.Entry("FlurryAndroidStaging");
		}
	}

	public static string FlurryAppVersion
	{
		get
		{
			return string.Format("{0}.{1}", Version, DataVersion);
		}
	}

	public static string KontagentApiKey
	{
		get
		{
			if (!Debug.isDebugBuild && IsLive)
			{
				return ConfigSchema.Entry("KontagentAndroidLive");
			}
			return ConfigSchema.Entry("KontagentAndroidStaging");
		}
	}

	public static string RateMeURL
	{
		get
		{
			return ConfigSchema.Entry("RateMeURL");
		}
	}

	public static string ForcedUpdateURL
	{
		get
		{
			return ConfigSchema.Entry("ForcedUpdateURL");
		}
	}

	public static int ScreenSleepTimeout
	{
		get
		{
			int result;
			if (int.TryParse(ConfigSchema.Entry("ScreenSleepTimeout"), out result))
			{
				return result;
			}
			return -1;
		}
	}

	public static int GameSpyID
	{
		get
		{
			int result = 0;
			if ((!Debug.isDebugBuild && IsLive) || EmulateLive)
			{
				int.TryParse(ConfigSchema.Entry("GameSpyID_Android"), out result);
			}
			else
			{
				int.TryParse(ConfigSchema.Entry("GameSpyID_Test"), out result);
			}
			return result;
		}
	}

	public static string GameSpyName
	{
		get
		{
			if ((!Debug.isDebugBuild && IsLive) || EmulateLive)
			{
				return ConfigSchema.Entry("GameSpyName_Android");
			}
			return ConfigSchema.Entry("GameSpyName_Test");
		}
	}

	public static string GameSpyAccessKey
	{
		get
		{
			if ((!Debug.isDebugBuild && IsLive) || EmulateLive)
			{
				return ConfigSchema.Entry("GameSpyAccessKey_Android");
			}
			return ConfigSchema.Entry("GameSpyAccessKey_Test");
		}
	}

	public static string GameSpySecretKey
	{
		get
		{
			if ((!Debug.isDebugBuild && IsLive) || EmulateLive)
			{
				return ConfigSchema.Entry("GameSpySecretKey_Android");
			}
			return ConfigSchema.Entry("GameSpySecretKey_Test");
		}
	}

	public static int GameSpyPartnerCode
	{
		get
		{
			int result;
			if (int.TryParse(ConfigSchema.Entry("GameSpyPartnerCode"), out result))
			{
				return result;
			}
			return 99;
		}
	}

	public static int GameSpyNamespaceID
	{
		get
		{
			int result;
			if (int.TryParse(ConfigSchema.Entry("GameSpyNamespaceID"), out result))
			{
				return result;
			}
			return 102;
		}
	}

	public static string UrbanAirshipKey
	{
		get
		{
			if (IsLive)
			{
				return ConfigSchema.Entry("UrbanAirshipKey_Live");
			}
			return ConfigSchema.Entry("UrbanAirshipKey_Test");
		}
	}

	public static string UrbanAirshipSecret
	{
		get
		{
			if (IsLive)
			{
				return ConfigSchema.Entry("UrbanAirshipSecret_Live");
			}
			return ConfigSchema.Entry("UrbanAirshipSecret_Test");
		}
	}

	public static string S3Bucket
	{
		get
		{
			return ConfigSchema.Entry("S3Bucket");
		}
	}

	public static bool IsLive
	{
		get
		{
			return true;
		}
	}

	public static bool EmulateLive { get; set; }
}
