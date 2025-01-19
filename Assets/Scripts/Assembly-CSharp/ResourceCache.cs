using System.Collections.Generic;
using UnityEngine;

public static class ResourceCache
{
	private static int defaultCacheLevel = 0;

	private static Dictionary<string, SharedResourceLoader.SharedResource> mLoadedAssets = new Dictionary<string, SharedResourceLoader.SharedResource>();

	public static int DefaultCacheLevel
	{
		get
		{
			return defaultCacheLevel;
		}
		set
		{
			defaultCacheLevel = value;
		}
	}

	public static bool IsCached(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		return mLoadedAssets.ContainsKey(path);
	}

	public static SharedResourceLoader.SharedResource GetCachedResource(string path)
	{
		return GetCachedResource(path, -1);
	}

	public static SharedResourceLoader.SharedResource GetCachedResource(string path, int loadLevelIfNotCached)
	{
		SharedResourceLoader.SharedResource value = null;
		if (mLoadedAssets.TryGetValue(path, out value))
		{
			return value;
		}
		if (loadLevelIfNotCached >= 0)
		{
			return Cache(path, loadLevelIfNotCached);
		}
		return null;
	}

	public static SharedResourceLoader.SharedResource Cache(string path)
	{
		return Cache(path, DefaultCacheLevel);
	}

	public static SharedResourceLoader.SharedResource Cache(string path, int level)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		SharedResourceLoader.SharedResource value = null;
		if (!mLoadedAssets.TryGetValue(path, out value))
		{
			value = SharedResourceLoader.LoadAsset(path);
			mLoadedAssets[path] = value;
			value.level = level;
		}
		else
		{
			value.level = Mathf.Min(value.level, level);
		}
		value.refCount++;
		return value;
	}

	public static void UnCache(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		SharedResourceLoader.SharedResource value;
		if (mLoadedAssets.TryGetValue(path, out value))
		{
			value.refCount--;
			if (value.refCount == 0)
			{
				mLoadedAssets.Remove(path);
				SharedResourceLoader.UnloadAsset(path);
			}
		}
		else
		{
			SharedResourceLoader.UnloadAsset(path);
		}
	}

	public static void UnloadAllAboveLevel(int minLevel)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, SharedResourceLoader.SharedResource> mLoadedAsset in mLoadedAssets)
		{
			if (mLoadedAsset.Value.level >= minLevel)
			{
				mLoadedAsset.Value.refCount = 0;
				SharedResourceLoader.UnloadAsset(mLoadedAsset.Value.Path);
				list.Add(mLoadedAsset.Key);
			}
		}
		foreach (string item in list)
		{
			mLoadedAssets.Remove(item);
		}
	}

	public static void LoadCachedResources<T>(T obj, string tableRecordKey)
	{
		foreach (DataBundleRuntime.DataBundleResourceInfo item in DataBundleRuntime.Instance.EnumerateUnityObjectPathsAtKey(typeof(T), tableRecordKey, false))
		{
			if (item != null && !string.IsNullOrEmpty(item.path))
			{
				SharedResourceLoader.SharedResource cachedResource = GetCachedResource(item.path);
				if (cachedResource != null)
				{
					item.data.fInfo.SetValue(obj, cachedResource.Resource);
				}
			}
		}
	}

	public static void UnloadCachedResources<T>(T obj, string tableRecordKey)
	{
		foreach (DataBundleRuntime.DataBundleResourceInfo item in DataBundleRuntime.Instance.EnumerateUnityObjectPathsAtKey(typeof(T), tableRecordKey, false))
		{
			if (item != null && !string.IsNullOrEmpty(item.path))
			{
				item.data.fInfo.SetValue(obj, null);
			}
		}
	}
}
