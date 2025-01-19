using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Asset Cache List")]
public class AssetCacheList : MonoBehaviour
{
	public List<Object> assetsToCache;

	private void Awake()
	{
		if (assetsToCache == null)
		{
			return;
		}
		foreach (Object item in assetsToCache)
		{
			if (item is Material)
			{
				AssetCache.Cache(BundleUtils.ValidateMaterial(item as Material));
			}
			else
			{
				AssetCache.Cache(item, item.GetType());
			}
		}
	}
}
