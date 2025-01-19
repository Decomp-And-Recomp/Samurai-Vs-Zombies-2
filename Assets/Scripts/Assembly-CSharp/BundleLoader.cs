using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class BundleLoader : MonoBehaviour
{
	private class BundleData
	{
		public AssetBundle bundle;

		public int useCount = 1;

		public BundleData()
		{
		}

		public BundleData(AssetBundle b)
		{
			bundle = b;
		}
	}

	private class AssetData
	{
		public enum UnloadType
		{
			Default = 0,
			Asset = 1,
			Instance = 2
		}

		public string bundleLoadedFrom;

		public UnityEngine.Object loadedAsset;

		public Type loadedAssetType;

		public int useCount = 1;

		public UnloadType unloadAs;

		public AssetData(UnityEngine.Object asset, string bundleLoadedFrom, Type assetType)
		{
			loadedAsset = asset;
			this.bundleLoadedFrom = bundleLoadedFrom;
			loadedAssetType = assetType;
			unloadAs = UnloadType.Default;
		}

		public AssetData(UnityEngine.Object asset, string bundleLoadedFrom, Type assetType, UnloadType unloadAs)
		{
			loadedAsset = asset;
			this.bundleLoadedFrom = bundleLoadedFrom;
			loadedAssetType = assetType;
			this.unloadAs = unloadAs;
		}
	}

	private static BundleAssetInfo assetInfo = null;

	private static BundleManifest manifest = null;

	private static Dictionary<string, BundleData> bundleData = new Dictionary<string, BundleData>();

	private static Dictionary<string, AssetData> assetData = new Dictionary<string, AssetData>();

	private static bool assetInfoBundleLoading = false;

	private static readonly Dictionary<string, Type> extentionToTypeDictionary = new Dictionary<string, Type>
	{
		{
			".mat",
			typeof(Material)
		},
		{
			".tif",
			typeof(Texture2D)
		},
		{
			".tga",
			typeof(Texture2D)
		},
		{
			".png",
			typeof(Texture2D)
		},
		{
			".exr",
			typeof(Texture2D)
		},
		{
			".fbx",
			typeof(GameObject)
		},
		{
			".prefab",
			typeof(GameObject)
		}
	};

	private static List<string> bundleDataLoading = new List<string>();

	private static List<string> assetDataLoading = new List<string>();

	private static BundleLoader instance = null;

	private static GameObject containerObject = null;

	public static BundleLoader Instance
	{
		get
		{
			if (instance == null)
			{
				if (containerObject == null && !ApplicationUtilities.HasShutdown)
				{
					containerObject = new GameObject("BundleLoader");
					UnityEngine.Object.DontDestroyOnLoad(containerObject);
				}
				instance = containerObject.AddComponent<BundleLoader>();
			}
			return instance;
		}
	}

	public static int PendingUnloadCount { get; private set; }

	public static bool UnloadAllLoadedObjectsOnNextUnload { get; set; }

	public static bool LoadFromBundleAsync { get; set; }

	public static bool UseAssetCache()
	{
		return true;
	}

	public static bool UseLocalPrefabs()
	{
		return false;
	}

	private void LateUpdate()
	{
		if (PendingUnloadCount > 0 && bundleDataLoading.Count == 0 && assetDataLoading.Count == 0)
		{
			UnloadUnusedBundles(UnloadAllLoadedObjectsOnNextUnload);
			UnloadAllLoadedObjectsOnNextUnload = false;
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			bundleData.Clear();
			UnityEngine.Object.Destroy(containerObject);
			instance = null;
			containerObject = null;
			PendingUnloadCount = 0;
		}
	}

	public static void LoadAssetInfo()
	{
		if (!assetInfoBundleLoading)
		{
			assetInfoBundleLoading = true;
			BundleAssetInfo.Instance.DeserializeBundleAssetInfo();
			assetInfo = BundleAssetInfo.Instance;
			assetInfoBundleLoading = false;
		}
	}

	public static int LoadedAssetCount()
	{
		return assetData.Count;
	}

	public static List<KeyValuePair<int, string>> LoadedAssets()
	{
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		foreach (KeyValuePair<string, AssetData> assetDatum in assetData)
		{
			list.Add(new KeyValuePair<int, string>(assetDatum.Value.useCount, assetDatum.Key));
		}
		list.Sort(SortIntStringPair);
		return list;
	}

	public static int LoadedBundleCount()
	{
		return bundleData.Count;
	}

	public static List<KeyValuePair<int, string>> LoadedBundles()
	{
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		foreach (KeyValuePair<string, BundleData> bundleDatum in bundleData)
		{
			list.Add(new KeyValuePair<int, string>(bundleDatum.Value.useCount, bundleDatum.Key));
		}
		list.Sort(SortIntStringPair);
		return list;
	}

	private static int SortIntStringPair(KeyValuePair<int, string> a, KeyValuePair<int, string> b)
	{
		return a.Key.CompareTo(b.Key);
	}

	public static IEnumerator UnloadAllAssets()
	{
		while (assetDataLoading.Count > 0)
		{
			yield return null;
		}
		while (assetData.Count > 0)
		{
			IEnumerator enumerator = assetData.Keys.GetEnumerator();
			enumerator.MoveNext();
			UnloadAsset((string)enumerator.Current, true);
		}
	}

	public static IEnumerator UnloadAllBundles()
	{
		while (bundleDataLoading.Count > 0)
		{
			yield return null;
		}
		while (assetDataLoading.Count > 0)
		{
			yield return null;
		}
		while (bundleData.Count > 0)
		{
			IEnumerator enumerator = bundleData.Keys.GetEnumerator();
			enumerator.MoveNext();
			UnloadBundle((string)enumerator.Current, true);
			UnloadUnusedBundles(true);
			yield return null;
		}
	}

	public static void UnloadAssetInfo()
	{
		assetInfo = null;
		manifest = null;
	}

	public static void LoadBundleAsync(string bundleName, Action onComplete)
	{
		if (!bundleData.ContainsKey(bundleName))
		{
			Instance.StartCoroutine(LoadBundle(bundleName, onComplete));
			return;
		}
		bundleData[bundleName].useCount++;
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public static IEnumerator LoadBundle(string bundleName)
	{
		return LoadBundle(bundleName, null);
	}

	public static IEnumerator LoadBundle(string bundleName, Action onComplete)
	{
		if (manifest == null)
		{
			manifest = BundleManifest.ManifestFromFile(AssetBundleConfig.BundleDataPath + '/' + AssetBundleConfig.ManifestFileName);
		}
		while (bundleDataLoading.Contains(bundleName))
		{
			yield return null;
		}
		if (bundleData.ContainsKey(bundleName))
		{
			GenericUtils.TryInvoke(onComplete);
			yield break;
		}
		bundleDataLoading.Add(bundleName);
		string overrideFolder = manifest.GetBundleOverrideFolder(bundleName, PortableQualitySettings.GetQuality());
		int bundleVersion = manifest.GetBundleVersion(bundleName);
		AssetBundle loadedBundle = null;
		WWW www = null;
		if (AssetBundleConfig.UncompressedLocalAssetBundles && bundleVersion == 0)
		{
			string bundlePath2 = BundleUtils.GetBundlePath(bundleName, overrideFolder, false);
			loadedBundle = AssetBundle.LoadFromFile(bundlePath2);
		}
		else
		{
			string bundlePath = BundleUtils.GetBundlePath(bundleName, overrideFolder, true);
			www = WWW.LoadFromCacheOrDownload(bundlePath, bundleVersion);
			yield return www;
		}
		bundleDataLoading.Remove(bundleName);
		if (www != null)
		{
			if (www.error != null)
			{
				GenericUtils.TryInvoke(onComplete);
				yield break;
			}
			loadedBundle = www.assetBundle;
		}
		if (loadedBundle == null)
		{
			GenericUtils.TryInvoke(onComplete);
			yield break;
		}
		BundleData newBundle = new BundleData(loadedBundle);
		bundleData.Add(bundleName, newBundle);
		GenericUtils.TryInvoke(onComplete);
	}

	public static void UnloadBundle(string bundleName)
	{
		UnloadBundle(bundleName, false);
	}

	public static void UnloadBundle(string bundleName, bool force)
	{
		if (!BundleLoader.bundleData.ContainsKey(bundleName))
		{
			return;
		}
		BundleData bundleData = BundleLoader.bundleData[bundleName];
		if (bundleData.useCount > 0)
		{
			if (force)
			{
				bundleData.useCount = 0;
			}
			else
			{
				bundleData.useCount = Mathf.Max(bundleData.useCount - 1, 0);
			}
			if (bundleData.useCount <= 0)
			{
				PendingUnloadCount++;
			}
		}
	}

	public static void UnloadUnusedBundles(bool unloadAllLoadedObjects)
	{
		if (PendingUnloadCount <= 0)
		{
			return;
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (KeyValuePair<string, BundleData> bundleDatum in bundleData)
		{
			string key = bundleDatum.Key;
			if (bundleDatum.Value.useCount > 0)
			{
				continue;
			}
			bundleDatum.Value.bundle.Unload(unloadAllLoadedObjects);
			list.Add(key);
			foreach (string key2 in assetData.Keys)
			{
				if (assetData[key2].bundleLoadedFrom.Equals(key))
				{
					list2.Add(key2);
				}
			}
		}
		foreach (string item in list)
		{
			bundleData.Remove(item);
		}
		foreach (string item2 in list2)
		{
			UnloadAsset(item2, true);
		}
		PendingUnloadCount = 0;
	}

	public static bool IsBundleLoaded(string bundleName)
	{
		return bundleData.ContainsKey(bundleName);
	}

	public static void LoadAssetAsync(string assetPath, Action onComplete)
	{
		if (!IsAssetLoaded(assetPath))
		{
			Instance.StartCoroutine(LoadAsset(assetPath, onComplete));
			return;
		}
		assetData[assetPath].useCount++;
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public static IEnumerator LoadAsset(string assetPath)
	{
		return LoadAsset(assetPath, null, null);
	}

	public static IEnumerator LoadAsset(string assetPath, Action onComplete)
	{
		return LoadAsset(assetPath, null, onComplete);
	}

	public static IEnumerator LoadAsset(string assetPath, string bundleName, Action onComplete)
	{
		while (IsAssetLoading(assetPath))
		{
			yield return null;
		}
		if (IsAssetLoaded(assetPath))
		{
			assetData[assetPath].useCount++;
		}
		else
		{
			assetDataLoading.Add(assetPath);
			if (manifest == null)
			{
				manifest = BundleManifest.ManifestFromFile(AssetBundleConfig.BundleDataPath + '/' + AssetBundleConfig.ManifestFileName);
			}
			if (assetInfo == null)
			{
				LoadAssetInfo();
			}
			if (string.IsNullOrEmpty(bundleName))
			{
				List<string> bundlesAssetIsIn = assetInfo.BundlesAssetIsIn(assetPath);
				foreach (string bundle2 in bundlesAssetIsIn)
				{
					if (IsBundleLoaded(bundle2))
					{
						bundleName = bundle2;
						break;
					}
					if (bundleDataLoading.Contains(bundle2))
					{
						while (bundleDataLoading.Contains(bundle2))
						{
							yield return null;
						}
						bundleName = bundle2;
						break;
					}
				}
				if (string.IsNullOrEmpty(bundleName) && bundlesAssetIsIn.Count > 0)
				{
					bundleName = bundlesAssetIsIn[0];
				}
			}
			if (!string.IsNullOrEmpty(bundleName))
			{
				List<string> bundlesAssetIsDependentOn = assetInfo.BundlesAssetIsDependentOn(assetPath, bundleName);
				foreach (string bundle in bundlesAssetIsDependentOn)
				{
					if (!bundleData.ContainsKey(bundle))
					{
						IEnumerator coroutineIEnumerator = LoadBundle(bundle);
						while (coroutineIEnumerator.MoveNext())
						{
							yield return coroutineIEnumerator.Current;
						}
					}
					else
					{
						bundleData[bundle].useCount++;
					}
				}
				string extension = Path.GetExtension(assetPath);
				Type type = ((!extentionToTypeDictionary.ContainsKey(extension)) ? typeof(UnityEngine.Object) : extentionToTypeDictionary[extension]);
				UnityEngine.Object loadedObject2 = null;
				BundleData bData = null;
				if (bundleData.TryGetValue(bundleName, out bData))
				{
					if (LoadFromBundleAsync)
					{
						AssetBundleRequest request = bData.bundle.LoadAssetAsync(assetPath, type);
						if (!request.isDone)
						{
							yield return request;
						}
						loadedObject2 = request.asset;
					}
					else
					{
						loadedObject2 = bData.bundle.LoadAsset(assetPath, type);
					}
					if (loadedObject2 == null)
					{
					}
					if (type.Equals(typeof(Material)))
					{
						Material loadedMaterial = UnityEngine.Object.Instantiate(loadedObject2) as Material;
						loadedMaterial.name = loadedObject2.name;
						if (loadedMaterial.shader != null)
						{
							Shader compiledShader = Shader.Find(loadedMaterial.shader.name);
							if (compiledShader != null && compiledShader.GetInstanceID() != loadedMaterial.shader.GetInstanceID())
							{
								loadedMaterial.shader = compiledShader;
							}
						}
						Material cachedMaterial = BundleUtils.ValidateMaterial(loadedMaterial);
						AssetData newAssetData = new AssetData(cachedMaterial, bundleName, type);
						if (!object.ReferenceEquals(cachedMaterial, loadedMaterial))
						{
							UnityEngine.Object.DestroyImmediate(loadedMaterial);
							newAssetData.unloadAs = AssetData.UnloadType.Default;
						}
						else
						{
							newAssetData.unloadAs = AssetData.UnloadType.Instance;
						}
						assetData.Add(assetPath, newAssetData);
						Resources.UnloadAsset(loadedObject2);
					}
					else if (type.Equals(typeof(Texture2D)))
					{
						if (UseAssetCache())
						{
							AssetCache.Cache(loadedObject2, type);
						}
						assetData.Add(assetPath, new AssetData(loadedObject2, bundleName, type, AssetData.UnloadType.Asset));
					}
					else if (type == typeof(AudioClip) || type == typeof(Mesh) || type == typeof(AnimationClip))
					{
						assetData.Add(assetPath, new AssetData(loadedObject2, bundleName, type, AssetData.UnloadType.Asset));
					}
					else
					{
						assetData.Add(assetPath, new AssetData(loadedObject2, bundleName, type));
					}
				}
			}
			assetDataLoading.Remove(assetPath);
		}
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public static UnityEngine.Object GetLoadedAsset(string assetPath)
	{
		if (IsAssetLoaded(assetPath))
		{
			return assetData[assetPath].loadedAsset;
		}
		return null;
	}

	public static void UnloadAsset(string assetPath)
	{
		UnloadAsset(assetPath, false);
	}

	public static void UnloadAsset(string assetPath, bool force)
	{
		if (!IsAssetLoaded(assetPath))
		{
			return;
		}
		AssetData assetData = BundleLoader.assetData[assetPath];
		if (force || assetData.useCount <= 1)
		{
			List<string> list = assetInfo.BundlesAssetIsDependentOn(assetPath, assetData.bundleLoadedFrom);
			if (assetData.loadedAsset != null)
			{
				Type loadedAssetType = assetData.loadedAssetType;
				if (UseAssetCache())
				{
					AssetCache.Remove(assetData.loadedAsset, loadedAssetType);
				}
				if (assetData.unloadAs == AssetData.UnloadType.Instance)
				{
					UnityEngine.Object.DestroyImmediate(assetData.loadedAsset);
				}
				else if (assetData.unloadAs == AssetData.UnloadType.Asset)
				{
					Resources.UnloadAsset(assetData.loadedAsset);
				}
			}
			BundleLoader.assetData.Remove(assetPath);
			{
				foreach (string item in list)
				{
					if (bundleData.ContainsKey(item))
					{
						UnloadBundle(item, false);
					}
				}
				return;
			}
		}
		assetData.useCount--;
	}

	public static bool IsAssetLoaded(string assetPath)
	{
		return assetData.ContainsKey(assetPath);
	}

	public static bool IsAssetLoading(string assetPath)
	{
		return assetDataLoading.Contains(assetPath);
	}

	public static T FindLoadedAsset<T>(Predicate<T> condition) where T : UnityEngine.Object
	{
		if (condition != null)
		{
			foreach (KeyValuePair<string, AssetData> assetDatum in assetData)
			{
				if (assetDatum.Value.loadedAssetType == typeof(T) && condition(assetDatum.Value.loadedAsset as T))
				{
					return assetDatum.Value.loadedAsset as T;
				}
			}
		}
		return (T)null;
	}

	public static string DebugStats()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("**** ASSET BUNDLES ****");
		foreach (KeyValuePair<string, BundleData> bundleDatum in bundleData)
		{
			stringBuilder.AppendFormat("{0}, useCount={1}\n", bundleDatum.Key, bundleDatum.Value.useCount);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("**** ASSETS LOADED FROM BUNDLES ****");
		foreach (KeyValuePair<string, AssetData> assetDatum in assetData)
		{
			stringBuilder.AppendFormat("{0}, useCount={1}, bundle={2}\n", assetDatum.Key, assetDatum.Value.useCount, assetDatum.Value.bundleLoadedFrom);
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	public static string GetAssetPath(UnityEngine.Object asset)
	{
		if (asset != null)
		{
			foreach (KeyValuePair<string, AssetData> assetDatum in assetData)
			{
				if (object.ReferenceEquals(assetDatum.Value.loadedAsset, asset))
				{
					return assetDatum.Key;
				}
			}
		}
		return string.Empty;
	}

	public static bool BundleExists(string bundleName)
	{
		if (manifest == null)
		{
			manifest = BundleManifest.ManifestFromFile(AssetBundleConfig.BundleDataPath + '/' + AssetBundleConfig.ManifestFileName);
		}
		return manifest.GetBundleVersion(bundleName) != -1;
	}
}
