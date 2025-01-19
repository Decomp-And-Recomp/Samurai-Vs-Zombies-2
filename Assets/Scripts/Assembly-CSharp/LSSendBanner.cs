public class LSSendBanner : Banner
{
	protected override string uiPrefabPath
	{
		get
		{
			return "UI/Prefabs/HUD/Banner_LegendaryStrike_Sent";
		}
	}

	public LSSendBanner(float fTimeBeforeFade)
		: base(fTimeBeforeFade)
	{
	}

	protected override void InitText()
	{
	}
}
