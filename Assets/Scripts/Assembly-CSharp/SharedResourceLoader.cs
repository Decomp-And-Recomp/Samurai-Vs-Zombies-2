using System;
using System.Collections.Generic;
using GripTech;
using UnityEngine;

public static class SharedResourceLoader
{
	public class SharedResource
	{
		public int refCount;

		public int level;

		public string Path { get; private set; }

		public UnityEngine.Object Resource { get; private set; }

		private SharedResource(string path)
		{
			Path = path;
			if (!string.IsNullOrEmpty(path))
			{
				Resource = ResourceLoader.Load(path);
			}
		}

		public static void ShareConstructor()
		{
			createSharedResouce = (string path) => new SharedResource(path);
		}
	}

	private static Func<string, SharedResource> createSharedResouce;

	private static Dictionary<string, WeakReference> mLoadedAssets;

	static SharedResourceLoader()
	{
		mLoadedAssets = new Dictionary<string, WeakReference>();
		SharedResource.ShareConstructor();
	}

	public static SharedResource GetLoadedAsset(string path)
	{
		WeakReference value = null;
		if (!mLoadedAssets.TryGetValue(path, out value))
		{
			return null;
		}
		return value.Target as SharedResource;
	}

	public static SharedResource LoadAsset(string path)
	{
		SharedResource sharedResource = GetLoadedAsset(path);
		if (sharedResource == null || object.ReferenceEquals(sharedResource.Resource, null))
		{
			sharedResource = createSharedResouce(path);
			mLoadedAssets[path] = new WeakReference(sharedResource);
		}
		return sharedResource;
	}

	public static void UnloadAsset(string path)
	{
		SharedResource loadedAsset = GetLoadedAsset(path);
		if (loadedAsset != null)
		{
			ResourceLoader.Unload(loadedAsset.Resource);
			mLoadedAssets.Remove(path);
		}
	}
}
