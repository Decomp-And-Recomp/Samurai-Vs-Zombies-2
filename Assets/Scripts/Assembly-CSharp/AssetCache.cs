using System;
using System.Collections.Generic;
using UnityEngine;

public static class AssetCache
{
	private static Dictionary<Type, List<TypedWeakReference<UnityEngine.Object>>> assets = new Dictionary<Type, List<TypedWeakReference<UnityEngine.Object>>>();

	private static bool enabled = true;

	public static bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	public static void Cache<T>(T uncachedAsset) where T : UnityEngine.Object
	{
		Cache(uncachedAsset, typeof(T));
	}

	public static void Cache(UnityEngine.Object asset, Type assetType)
	{
		if (!Enabled || object.ReferenceEquals(asset, null) || string.IsNullOrEmpty(asset.name))
		{
			return;
		}
		List<TypedWeakReference<UnityEngine.Object>> list;
		if (!assets.ContainsKey(assetType))
		{
			list = new List<TypedWeakReference<UnityEngine.Object>>();
			assets[assetType] = list;
		}
		else
		{
			list = assets[assetType];
		}
		int num = 0;
		while (num < list.Count)
		{
			UnityEngine.Object ptr = list[num].ptr;
			if (ptr == null)
			{
				list.RemoveAt(num);
				continue;
			}
			if (ptr.name.Equals(asset.name))
			{
				list[num] = new TypedWeakReference<UnityEngine.Object>(asset);
				return;
			}
			num++;
		}
		list.Add(new TypedWeakReference<UnityEngine.Object>(asset));
	}

	public static T GetCached<T>(T asset) where T : UnityEngine.Object
	{
		if (object.ReferenceEquals(asset, null))
		{
			return (T)null;
		}
		return GetCached(asset.name, typeof(T)) as T;
	}

	public static T GetCached<T>(string assetName) where T : UnityEngine.Object
	{
		return GetCached(assetName, typeof(T)) as T;
	}

	public static UnityEngine.Object GetCached(string assetName, Type assetType)
	{
		if (!Enabled || string.IsNullOrEmpty(assetName) || !assets.ContainsKey(assetType))
		{
			return null;
		}
		List<TypedWeakReference<UnityEngine.Object>> list = assets[assetType];
		int num = 0;
		while (num < list.Count)
		{
			UnityEngine.Object ptr = list[num].ptr;
			if (ptr == null)
			{
				list.RemoveAt(num);
				continue;
			}
			if (ptr.name.Equals(assetName))
			{
				return ptr;
			}
			num++;
		}
		return null;
	}

	public static bool Contains<T>(T asset) where T : UnityEngine.Object
	{
		return Contains(asset, typeof(T));
	}

	public static bool Contains(UnityEngine.Object asset, Type assetType)
	{
		if (object.ReferenceEquals(asset, null))
		{
			return false;
		}
		return GetCached(asset.name, assetType) != null;
	}

	public static bool Remove<T>(T asset) where T : UnityEngine.Object
	{
		return Remove(asset, typeof(T));
	}

	public static bool Remove(UnityEngine.Object asset, Type assetType)
	{
		if (!Enabled || object.ReferenceEquals(asset, null) || string.IsNullOrEmpty(asset.name) || !assets.ContainsKey(assetType))
		{
			return false;
		}
		List<TypedWeakReference<UnityEngine.Object>> list = assets[assetType];
		int num = 0;
		while (num < list.Count)
		{
			UnityEngine.Object ptr = list[num].ptr;
			if (ptr == null)
			{
				list.RemoveAt(num);
				continue;
			}
			if (ptr.name.Equals(asset.name))
			{
				list.RemoveAt(num);
				return true;
			}
			num++;
		}
		return false;
	}
}
