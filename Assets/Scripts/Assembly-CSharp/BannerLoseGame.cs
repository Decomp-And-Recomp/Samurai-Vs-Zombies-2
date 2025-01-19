public class BannerLoseGame : Banner
{
	protected override string uiPrefabPath
	{
		get
		{
			return "UI/Prefabs/HUD/Banner_Lose";
		}
	}

	public BannerLoseGame(float fTimeBeforeFade)
		: base(fTimeBeforeFade)
	{
	}

	protected override void InitText()
	{
	}
}
