public class AWSServerConfig
{
	public static string Bucket
	{
		get
		{
			return "samuzombie2";
		}
	}

	public static string GameFolderName
	{
		get
		{
			return AJavaTools.Properties.GetBuildType();
		}
	}

	public static string S3FolderVersion
	{
		get
		{
			return GeneralConfig.Version;
		}
	}

	public static string S3Url
	{
		get
		{
			return "https://" + Bucket + ".s3.amazonaws.com/";
		}
	}

	public static string CloudFrontUrl
	{
		get
		{
			return "https://d2lk18ssnvgdhj.cloudfront.net/";
		}
	}

	public static string ToggleFileName
	{
		get
		{
			return "Toggle.txt";
		}
	}

	public static string ForcedUpdateFileName
	{
		get
		{
			return "ForceUpdate.txt";
		}
	}

	public static string ForceUpdateVerificationText
	{
		get
		{
			return "Force Update";
		}
	}

	public static string StagingUploadDirectoryPath
	{
		get
		{
			return "ServerUploadStaging/";
		}
	}

	public static string DevStagingDirectoryName
	{
		get
		{
			return "DevStaging";
		}
	}

	public static string LiveStagingDirectoryName
	{
		get
		{
			return "LiveStaging";
		}
	}

	public static string LiveDirectoryName
	{
		get
		{
			return "Live";
		}
	}

	public static string UdaliteFolder
	{
		get
		{
			return string.Empty;
		}
	}

	public static string DownloadUrl
	{
		get
		{
			string s3Url = S3Url;
			if (!GeneralConfig.IsLive)
			{
				return s3Url + GameFolderName + "/" + S3FolderVersion + "/" + LiveStagingDirectoryName + "/";
			}
			return s3Url + GameFolderName + "/" + S3FolderVersion + "/" + LiveDirectoryName + "/";
		}
	}

	public static string PointerFile
	{
		get
		{
			return UpdateCheckData.LiveStagingPtrFile.TrimStart(UpdateCheckData.DirectorySeparator);
		}
	}

	public static string DownloadSubDirectory
	{
		get
		{
			if (!GeneralConfig.IsLive)
			{
				return LiveStagingDirectoryName + "/";
			}
			return LiveDirectoryName + "/";
		}
	}

	public static string ArchiveGameVersionUrl
	{
		get
		{
			return S3Url + GameFolderName + "/" + S3FolderVersion + "/Archive/";
		}
	}

	public static string LiveGameVersionUrl
	{
		get
		{
			return S3Url + GameFolderName + "/" + S3FolderVersion + "/" + LiveDirectoryName + "/";
		}
	}

	public static string DevStagingGameVersionUrl
	{
		get
		{
			return S3Url + GameFolderName + "/" + S3FolderVersion + "/" + DevStagingDirectoryName + "/";
		}
	}

	public static string LiveStagingGameVersionUrl
	{
		get
		{
			return S3Url + GameFolderName + "/" + S3FolderVersion + "/" + LiveStagingDirectoryName + "/";
		}
	}

	public static int MaxDownloadAttemptsPerFile
	{
		get
		{
			return 3;
		}
	}

	public static int MaxDownloadIdleTime
	{
		get
		{
			return 30;
		}
	}

	public static bool CheckForUpdates
	{
		get
		{
			return true;
		}
	}
}
