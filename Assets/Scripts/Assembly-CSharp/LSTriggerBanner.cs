public class LSTriggerBanner : Banner
{
	protected override string uiPrefabPath
	{
		get
		{
			return "UI/Prefabs/HUD/Banner_LegendaryStrike_Incoming";
		}
	}

	public LSTriggerBanner(float fTimeBeforeFade)
		: base(fTimeBeforeFade)
	{
	}

	protected override void InitText()
	{
	}
}
