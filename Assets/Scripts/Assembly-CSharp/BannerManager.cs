public class BannerManager : UIHandler<BannerManager>
{
	private Banner mCurrentUIBanner;

	public void Init()
	{
	}

	public void OpenBanner(Banner bannerToAdd)
	{
		if (mCurrentUIBanner != null)
		{
			mCurrentUIBanner.Destroy();
		}
		bannerToAdd.Init();
		mCurrentUIBanner = bannerToAdd;
	}

	public void CloseBanner()
	{
		if (mCurrentUIBanner != null)
		{
			mCurrentUIBanner.Destroy();
			mCurrentUIBanner = null;
		}
	}

	public override void Update()
	{
		if (mCurrentUIBanner != null && !mCurrentUIBanner.Update())
		{
			mCurrentUIBanner.Destroy();
		}
	}
}
