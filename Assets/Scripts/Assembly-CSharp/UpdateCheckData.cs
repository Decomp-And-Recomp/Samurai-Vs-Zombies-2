public class UpdateCheckData
{
	public static readonly char DirectorySeparator = '/';

	public static readonly char FilenameSeparator = '-';

	public static readonly string PtrFileExt = ".ptr";

	public static readonly string LivePtr = "Live";

	public static readonly string LiveStagingPtr = "LiveStaging";

	public static readonly string LivePtrFile = DirectorySeparator + "{0}" + FilenameSeparator + LivePtr + PtrFileExt;

	public static readonly string LiveStagingPtrFile = DirectorySeparator + "{0}" + FilenameSeparator + LiveStagingPtr + PtrFileExt;

	public static readonly string TempFolder = "/UploadServerTemp";

	public static readonly string MainDB = "Main";

	public static readonly string MainLivePtrFile = string.Format(LivePtrFile, MainDB);

	public static readonly string MainLiveStagingPtrFile = string.Format(LiveStagingPtrFile, MainDB);

	public static readonly string ReplaceUpdate = "Replace";

	public static readonly string ForcedUpdateFile = "ForcedUpdate.txt";

	public static readonly string DBMD5 = "{0}DBSum";

	public static readonly string DBVer = "{0}DBVersion";

	public static readonly string BaseVersion = "BaseAppVersion";

	public static readonly string IncrBuild = "{0}IncrBuild";

	public static readonly string BaseIncrBuild = "000";

	public UpdateSystem.UpdateError error;

	public bool ForceUpdate { get; set; }

	public string DeviceLanguage { get; set; }

	public string MainPtrContents { get; set; }

	public string LangPtrContents { get; set; }

	public string MainDatabasePointer { get; set; }

	public string LangDatabasePointer { get; set; }

	public string MainDatabaseBuild { get; set; }

	public string LangDatabaseBuild { get; set; }

	public int FileIndex { get; set; }

	public int FileCount { get; set; }
}
