using UnityEngine;

public class HUDSharedHeroPortrait : WeakGlobalMonoBehavior<HUDSharedHeroPortrait>
{
	private string mIconPath;

	public Texture2D Texture { get; private set; }

	public HUDSharedHeroPortrait()
	{
		SetUniqueInstance(this);
	}

	private void Start()
	{
		HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID];
		if (heroSchema != null)
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(heroSchema.IconPath, 1);
			if (cachedResource != null)
			{
				Texture = cachedResource.Resource as Texture2D;
				mIconPath = heroSchema.IconPath;
			}
		}
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (mIconPath != null)
		{
			mIconPath = null;
			Texture = null;
		}
	}
}
