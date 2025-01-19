public enum UpdateCheckState
{
	DisconnectAll = 1,
	DownloadPointers = 2,
	InitAppBundles = 3,
	CheckBundlesExist = 4,
	EncodePointers = 5,
	DownloadUpdates = 6,
	VerifyDBValidity = 7,
	ReconnectAll = 8,
	Complete = 9
}
