using UnityEngine;

public class BuffIconManager : WeakGlobalInstance<BuffIconManager>
{
	public BuffIconManager()
	{
		SetUniqueInstance(this);
	}

	public GameObject GetPrefab(string iconFile)
	{
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(iconFile, 1);
		return cachedResource.Resource as GameObject;
	}
}
