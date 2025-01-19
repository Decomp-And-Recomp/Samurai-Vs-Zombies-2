using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateSystem : SingletonSpawningMonoBehaviour<UpdateSystem>
{
	public class UpdateError
	{
		public enum ErrorType
		{
			AppBundleAssetMissing = 0,
			CouldNotConnect = 1,
			DownloadFailed = 2,
			CacheMissingFile = 3,
			AppUpdateRequired = 4,
			DataBundleCorrupted = 5
		}

		public ErrorType type;

		public WWW failedDownload;

		public string bundleName;

		public string bundleVersion;

		public UpdateError(ErrorType et, WWW fd, string bn, string bv)
		{
			type = et;
			failedDownload = fd;
			bundleName = bn;
			bundleVersion = bv;
		}

		public override string ToString()
		{
			return string.Format("[UpdateError: type={0}, failedDownload={1}, bundleName={2}, bundleVersion={3}]", type, (failedDownload == null) ? string.Empty : failedDownload.url, bundleName, bundleVersion);
		}
	}

	private class BundleWWW
	{
		public BundleManifest.BundleData bundleData;

		public WWW www;

		public int attemptsMade;

		public BundleWWW(BundleManifest.BundleData bundleData, WWW www)
		{
			this.bundleData = bundleData;
			this.www = www;
			attemptsMade = 0;
		}
	}

	private int maxConcurrentWWW = 10;

	private int maxAttempts = 3;

	private string downloadUrl;

	private BundleManifest appBundleManifest;

	private BundleManifest localManifest;

	private BundleManifest s3Manifest;

	private List<BundleManifest.BundleData> bundleDownloadList;

	private List<BundleWWW> queuedBundles;

	private List<BundleWWW> nonCacheQueuedAssets;

	private List<string> loadedBundles;

	private WWW www;

	private UpdateError error;

	public static bool HasOptionalQualityUpdates;

	public UpdateError Error
	{
		get
		{
			return error;
		}
	}

	public bool CacheInitialized { get; set; }

	public bool Complete { get; set; }

	public int FileIndex { get; set; }

	public int FileCount { get; set; }

	public bool DataBundlesModified { get; set; }

	public IEnumerator BeginUpdate(bool getQualityUpdates)
	{
		Complete = false;
		error = null;
		HasOptionalQualityUpdates = false;
		DataBundlesModified = false;
		while (!Caching.ready)
		{
			yield return null;
		}
		downloadUrl = AWSServerConfig.DownloadUrl;
		www = new WWW(AppendRandomValueToUrl(downloadUrl + AWSServerConfig.ToggleFileName));
		DateTime waitStartTime = DateTime.Now;
		do
		{
			if ((DateTime.Now - waitStartTime).TotalSeconds >= 15.0)
			{
				error = new UpdateError(UpdateError.ErrorType.CouldNotConnect, www, string.Empty, string.Empty);
				Complete = true;
				yield break;
			}
		}
		while (!www.isDone);
		if (www.error != null)
		{
			error = new UpdateError(UpdateError.ErrorType.CouldNotConnect, www, string.Empty, string.Empty);
			Complete = true;
			yield break;
		}
		string toggle = www.text;
		www.Dispose();
		downloadUrl = downloadUrl + toggle + '/';
		string localManifestPath = AssetBundleConfig.BundleDataPath + '/' + AssetBundleConfig.ManifestFileName;
		localManifest = BundleManifest.ManifestFromFile(localManifestPath);
		www = new WWW(AppendRandomValueToUrl(downloadUrl + AssetBundleConfig.ManifestFileName));
		if (!www.isDone)
		{
			yield return www;
		}
		if (www.error != null)
		{
			error = new UpdateError(UpdateError.ErrorType.CouldNotConnect, www, string.Empty, string.Empty);
			Complete = true;
			yield break;
		}
		s3Manifest = BundleManifest.ManifestFromString(www.text.ToString());
		www.Dispose();
		if (!(localManifest.ManifestHash == s3Manifest.ManifestHash))
		{
			List<BundleManifest.BundleData> bundlesToDownload = s3Manifest.BundlesForQuality(PortableQualitySettings.GetQuality(), getQualityUpdates);
			List<BundleManifest.BundleData> qualityBundles = null;
			if (!getQualityUpdates)
			{
				qualityBundles = s3Manifest.BundlesForQuality(PortableQualitySettings.GetQuality(), true);
				int i = 0;
				while (i < qualityBundles.Count)
				{
					bool hasBundle = false;
					BundleManifest.BundleData foundBundle3 = localManifest.FindBundleByNameAndQualityAndLoc(qualityBundles[i].name, qualityBundles[i].minQualitySetting, qualityBundles[i].maxQualitySetting, DataBundleRuntime.notLocalized);
					if (foundBundle3 != null)
					{
						if (foundBundle3.HasSameHash(qualityBundles[i]))
						{
							qualityBundles[i].version = foundBundle3.version;
							if (Caching.IsVersionCached(qualityBundles[i].name, qualityBundles[i].version))
							{
								hasBundle = true;
							}
						}
						else
						{
							bool isDownloading = false;
							foreach (BundleManifest.BundleData dlBundle in bundlesToDownload)
							{
								if (dlBundle.name == foundBundle3.name)
								{
									isDownloading = true;
								}
							}
							if (!isDownloading)
							{
								qualityBundles[i].md5Hash = foundBundle3.md5Hash;
							}
						}
					}
					if (hasBundle)
					{
						qualityBundles.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}
			FileCount = bundlesToDownload.Count - 1;
			FileIndex = 0;
			queuedBundles = new List<BundleWWW>();
			nonCacheQueuedAssets = new List<BundleWWW>();
			string language = BundleUtils.GetSystemLanguage();
			foreach (BundleManifest.BundleData bundle2 in bundlesToDownload)
			{
				BundleManifest.BundleData foundBundle2 = null;
				foundBundle2 = localManifest.FindBundleByNameAndQualityAndLoc(bundle2.name, bundle2.minQualitySetting, bundle2.maxQualitySetting, bundle2.locLanguage);
				bool download = true;
				if (foundBundle2 != null)
				{
					if (!foundBundle2.HasSameHash(bundle2))
					{
						bundle2.version = foundBundle2.version + 1;
					}
					else
					{
						bundle2.version = foundBundle2.version;
						download = false;
					}
				}
				if (download)
				{
					string bundleToDownload = bundle2.name;
					string overrideFolder = bundle2.FolderName();
					if (overrideFolder != string.Empty)
					{
						bundleToDownload = overrideFolder + "/" + bundleToDownload;
					}
					if (bundleToDownload.EndsWith(AssetBundleConfig.DataBundleName) || bundleToDownload.EndsWith(AssetBundleConfig.DataBundleStringList) || bundleToDownload.EndsWith(AssetBundleConfig.BundleAssetInfoName) || bundleToDownload.EndsWith(AssetBundleConfig.BundleAssetList))
					{
						WWW downloadWWW = new WWW(Uri.EscapeUriString(downloadUrl + bundleToDownload));
						nonCacheQueuedAssets.Add(new BundleWWW(bundle2, downloadWWW));
						DataBundlesModified = true;
					}
					else
					{
						WWW downloadWWW2 = WWW.LoadFromCacheOrDownload(Uri.EscapeUriString(downloadUrl + bundleToDownload), bundle2.version);
						queuedBundles.Add(new BundleWWW(bundle2, downloadWWW2));
					}
				}
				FileIndex++;
				IEnumerator coroutineIEnumerator2 = CacheData(UpdateError.ErrorType.DownloadFailed, maxConcurrentWWW);
				while (coroutineIEnumerator2.MoveNext())
				{
					yield return coroutineIEnumerator2.Current;
				}
				if (qualityBundles != null)
				{
					HasOptionalQualityUpdates = queuedBundles.Count < qualityBundles.Count;
				}
				if (error != null)
				{
					Complete = true;
					yield break;
				}
			}
			IEnumerator coroutineIEnumerator = CacheData(UpdateError.ErrorType.DownloadFailed, 0);
			while (coroutineIEnumerator.MoveNext())
			{
				yield return coroutineIEnumerator.Current;
			}
			if (error != null)
			{
				Complete = true;
				yield break;
			}
			s3Manifest.ReplaceOrCreateManifestFile(localManifestPath, language);
			foreach (BundleManifest.BundleData bundle in s3Manifest.bundleList)
			{
				if ((bundle.name.EndsWith(AssetBundleConfig.DataBundleName) || bundle.name.EndsWith(AssetBundleConfig.DataBundleStringList) || bundle.name.EndsWith(AssetBundleConfig.BundleAssetInfoName) || bundle.name.EndsWith(AssetBundleConfig.BundleAssetList)) && !File.Exists(AssetBundleConfig.BundleDataPath + "/" + language + "/" + bundle.name))
				{
					error = new UpdateError(UpdateError.ErrorType.CacheMissingFile, null, bundle.name, bundle.version.ToString());
					Complete = true;
					yield break;
				}
			}
		}
		Complete = true;
	}

	public IEnumerator CheckForForcedUpdate()
	{
		Complete = false;
		error = null;
		WWW www = new WWW(AppendRandomValueToUrl(AWSServerConfig.DownloadUrl + AWSServerConfig.ForcedUpdateFileName));
		if (!www.isDone)
		{
			yield return www;
		}
		if (www.error == null && www.text == AWSServerConfig.ForceUpdateVerificationText)
		{
			error = new UpdateError(UpdateError.ErrorType.AppUpdateRequired, www, string.Empty, string.Empty);
			Complete = true;
		}
		www.Dispose();
	}

	public IEnumerator InitializeCacheWithAppBundleAssets(bool initializeAllBundles)
	{
		error = null;
		CacheInitialized = false;
		while (!Caching.ready)
		{
			yield return null;
		}
		Caching.ClearCache();
		appBundleManifest = BundleManifest.ManifestFromFile(BundleUtils.LocalAssetBundlePath() + AssetBundleConfig.ManifestFileName);
		nonCacheQueuedAssets = new List<BundleWWW>();
		queuedBundles = new List<BundleWWW>();
		if (initializeAllBundles)
		{
			List<BundleManifest.BundleData> bundlesToCache = appBundleManifest.BundlesForQuality(PortableQualitySettings.GetQuality(), true);
			FileCount = bundlesToCache.Count - 1;
			FileIndex = 0;
			foreach (BundleManifest.BundleData bundle in bundlesToCache)
			{
				string bundlePath = BundleUtils.GetBundlePath(bundle.name, bundle.FolderName());
				if (!bundlePath.EndsWith(AssetBundleConfig.DataBundleName) && !bundlePath.EndsWith(AssetBundleConfig.DataBundleStringList) && !bundlePath.EndsWith(AssetBundleConfig.BundleAssetInfoName) && !bundlePath.EndsWith(AssetBundleConfig.BundleAssetList))
				{
					WWW cacheWWW = WWW.LoadFromCacheOrDownload(bundlePath, bundle.version);
					queuedBundles.Add(new BundleWWW(bundle, cacheWWW));
					IEnumerator coroutineIEnumerator2 = CacheData(UpdateError.ErrorType.AppBundleAssetMissing, maxConcurrentWWW);
					while (coroutineIEnumerator2.MoveNext())
					{
						yield return coroutineIEnumerator2.Current;
					}
					if (error != null)
					{
						CacheInitialized = true;
						yield break;
					}
					FileIndex++;
				}
			}
			IEnumerator coroutineIEnumerator = CacheData(UpdateError.ErrorType.AppBundleAssetMissing, 0);
			while (coroutineIEnumerator.MoveNext())
			{
				yield return coroutineIEnumerator.Current;
			}
			if (error != null)
			{
				CacheInitialized = true;
				yield break;
			}
		}
		string localManifestPath = AssetBundleConfig.BundleDataPath + '/' + AssetBundleConfig.ManifestFileName;
		appBundleManifest.ReplaceOrCreateManifestFile(localManifestPath, BundleUtils.GetSystemLanguage());
		CacheInitialized = true;
	}

	public static IEnumerator CacheBundleFile(string name)
	{
		string language = BundleUtils.GetSystemLanguage();
		string srcPath = BundleUtils.LocalAssetBundlePath() + language + "/" + name;
		string dstPath = AssetBundleConfig.BundleDataPath + "/" + language + "/" + name;
		WWW www = new WWW(srcPath);
		using (FileStream fs = new FileStream(dstPath, FileMode.Create))
		{
			fs.Write(www.bytes, 0, www.bytes.Length);
			fs.Close();
		}
		www.Dispose();
		yield return www;
	}

	public static IEnumerator CopyDataBundleFiles()
	{
		string dstFolder = string.Concat(str2: BundleUtils.GetSystemLanguage(), str0: AssetBundleConfig.BundleDataPath, str1: "/");
		if (!Directory.Exists(dstFolder))
		{
			Directory.CreateDirectory(dstFolder);
		}
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.StartCoroutine(CacheBundleFile(AssetBundleConfig.DataBundleName));
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.StartCoroutine(CacheBundleFile(AssetBundleConfig.DataBundleStringList));
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.StartCoroutine(CacheBundleFile(AssetBundleConfig.BundleAssetInfoName));
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.StartCoroutine(CacheBundleFile(AssetBundleConfig.BundleAssetList));
		yield return null;
	}

	public IEnumerator CacheData(UpdateError.ErrorType failErrorType, int maxConcurrent)
	{
		int totalAssetsToBeCached = queuedBundles.Count + nonCacheQueuedAssets.Count;
		while (totalAssetsToBeCached > 0 && totalAssetsToBeCached >= maxConcurrent)
		{
			IEnumerator coroutineIEnumerator2 = SaveAssetToPersistentData(failErrorType);
			while (coroutineIEnumerator2.MoveNext())
			{
				yield return coroutineIEnumerator2.Current;
			}
			IEnumerator coroutineIEnumerator = LoadQueuedBundlesIntoCache(failErrorType);
			while (coroutineIEnumerator.MoveNext())
			{
				yield return coroutineIEnumerator.Current;
			}
			totalAssetsToBeCached = queuedBundles.Count + nonCacheQueuedAssets.Count;
		}
	}

	public IEnumerator SaveAssetToPersistentData(UpdateError.ErrorType failErrorType)
	{
		bool yieldForDownloads = false;
		for (int i = nonCacheQueuedAssets.Count - 1; i >= 0; i--)
		{
			BundleWWW bundle = nonCacheQueuedAssets[i];
			if (bundle.www.isDone)
			{
				if (bundle.www.error != null)
				{
					if (bundle.attemptsMade >= maxAttempts)
					{
						error = new UpdateError(failErrorType, bundle.www, bundle.bundleData.name, bundle.bundleData.version.ToString());
						nonCacheQueuedAssets.Clear();
						yield break;
					}
					string url = bundle.www.url;
					bundle.www.Dispose();
					bundle.www = new WWW(url);
					bundle.attemptsMade++;
				}
				else
				{
					string assetPath2 = AssetBundleConfig.BundleDataPath + "/";
					if (bundle.bundleData.locLanguage != DataBundleRuntime.notLocalized)
					{
						assetPath2 = assetPath2 + bundle.bundleData.locLanguage + "/";
						if (!Directory.Exists(assetPath2))
						{
							Directory.CreateDirectory(assetPath2);
						}
					}
					assetPath2 += bundle.bundleData.name;
					BundleUtils.DeleteFileIfExists(assetPath2);
					using (FileStream fs = new FileStream(assetPath2, FileMode.Create))
					{
						fs.Write(bundle.www.bytes, 0, bundle.www.bytes.Length);
						fs.Close();
					}
					bundle.www.Dispose();
					string bundleName = bundle.bundleData.name;
					nonCacheQueuedAssets.RemoveAt(i);
				}
			}
			else
			{
				yieldForDownloads = true;
			}
		}
		if (yieldForDownloads)
		{
			yield return null;
		}
	}

	public IEnumerator LoadQueuedBundlesIntoCache(UpdateError.ErrorType failErrorType)
	{
		bool yieldForDownloads = false;
		for (int i = queuedBundles.Count - 1; i >= 0; i--)
		{
			if (queuedBundles[i].www.isDone || queuedBundles[i].www.error != null)
			{
				if (queuedBundles[i].www.error != null)
				{
					if (queuedBundles[i].www.error.Contains("file of the same name is already loaded"))
					{
						string url2 = queuedBundles[i].www.url;
						queuedBundles[i].www.Dispose();
						queuedBundles[i].www = WWW.LoadFromCacheOrDownload(url2, queuedBundles[i].bundleData.version);
					}
					else
					{
						if (queuedBundles[i].attemptsMade >= maxAttempts)
						{
							error = new UpdateError(failErrorType, queuedBundles[i].www, queuedBundles[i].bundleData.name, queuedBundles[i].bundleData.version.ToString());
							queuedBundles.Clear();
							yield break;
						}
						string url = queuedBundles[i].www.url;
						queuedBundles[i].www.Dispose();
						queuedBundles[i].www = WWW.LoadFromCacheOrDownload(url, queuedBundles[i].bundleData.version);
						queuedBundles[i].attemptsMade++;
					}
				}
				else
				{
					queuedBundles[i].www.assetBundle.Unload(true);
					queuedBundles[i].www.Dispose();
					string bundleName2 = queuedBundles[i].bundleData.name;
					string overrideFolder = queuedBundles[i].bundleData.FolderName();
					if (overrideFolder != string.Empty)
					{
						bundleName2 = overrideFolder + "/" + bundleName2;
					}
					queuedBundles.RemoveAt(i);
				}
			}
			else
			{
				yieldForDownloads = true;
			}
		}
		if (yieldForDownloads)
		{
			yield return null;
		}
	}

	public static string AppendRandomValueToUrl(string url)
	{
		return url + "?random=" + UnityEngine.Random.Range(0, 1000000);
	}

	public static void CopyDataBundleFile(string srcFile, string dstFile, bool force)
	{
		if (File.Exists(srcFile) && (force || !File.Exists(dstFile)))
		{
			BundleUtils.CopyFile(srcFile, dstFile);
		}
	}
}
