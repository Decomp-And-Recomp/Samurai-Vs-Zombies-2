public class BannerBoss : Banner
{
	protected override string uiPrefabPath
	{
		get
		{
			return "UI/Prefabs/HUD/Banner_BossAnnounce";
		}
	}

	public BannerBoss(float fTimeBeforeFade)
		: base(fTimeBeforeFade)
	{
	}

	protected override void InitText()
	{
	}
}
