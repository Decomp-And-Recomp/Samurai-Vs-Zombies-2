public class Config : Singleton<Config>
{
	private TextDBSchema[] mData;

	public TextDBSchema[] data
	{
		get
		{
			return mData;
		}
	}

	public Config()
	{
		ResetCachedData();
	}

	public void ResetCachedData()
	{
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			mData = DataBundleUtils.InitializeRecords<TextDBSchema>("Config");
		}
	}
}
