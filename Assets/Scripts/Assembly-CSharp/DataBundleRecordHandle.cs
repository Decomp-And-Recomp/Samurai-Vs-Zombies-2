using System;
using System.Collections.Generic;
using UnityEngine;

public class DataBundleRecordHandle<T> : IDisposable
{
	private T recordData;

	private string tableRecordKey;

	private bool loadRecordLinks;

	private DataBundleResourceGroup loadedGroup;

	private Action<T> onLoaded;

	private List<DataBundleRuntime.DataBundleResourceInfo> resources = new List<DataBundleRuntime.DataBundleResourceInfo>();

	private List<DataBundleRuntime.DataBundleResourceInfo> bundleAssets = new List<DataBundleRuntime.DataBundleResourceInfo>();

	private bool alreadyDisposed;

	public string TableRecordKey
	{
		get
		{
			return tableRecordKey;
		}
	}

	public T Data
	{
		get
		{
			return recordData;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return resources.Count > 0 || bundleAssets.Count > 0;
		}
	}

	public DataBundleResourceGroup LoadedGroup
	{
		get
		{
			return loadedGroup;
		}
	}

	public DataBundleRecordHandle(string table, string recordKey)
		: this(DataBundleRuntime.TableRecordKey(table, recordKey))
	{
	}

	public DataBundleRecordHandle(string tableRecordKey)
	{
		this.tableRecordKey = tableRecordKey;
		recordData = DataBundleRuntime.Instance.InitializeRecord<T>(this.tableRecordKey);
	}

	public void Load(Action<T> onLoaded)
	{
		Load(DataBundleResourceGroup.All, false, onLoaded);
	}

	public void Load(DataBundleResourceGroup groupToLoad, bool loadRecordLinks, Action<T> onLoaded)
	{
		this.onLoaded = onLoaded;
		this.loadRecordLinks = loadRecordLinks;
		if (groupToLoad != 0 && recordData != null)
		{
			LoadUnityObjectsAsync(groupToLoad);
		}
		else
		{
			LoadComplete(DataBundleResourceGroup.None);
		}
	}

	public void LoadTableAgnostic(DataBundleResourceGroup groupToLoad, bool loadRecordLinks, Action<T> onLoaded)
	{
		IEnumerable<string> enumerable = DataBundleRuntime.Instance.EnumerateTables<T>();
		foreach (string item in enumerable)
		{
			string key = item + "." + tableRecordKey;
			recordData = DataBundleRuntime.Instance.InitializeRecord<T>(key);
			if (recordData != null)
			{
				tableRecordKey = key;
				Load(groupToLoad, loadRecordLinks, onLoaded);
				return;
			}
		}
		LoadComplete(DataBundleResourceGroup.None);
	}

	public void Unload()
	{
		if (IsLoaded && !ApplicationUtilities.HasShutdown)
		{
			UnityThreadHelper.CallOnMainThread(UnloadUnityObjects);
		}
		loadedGroup = DataBundleResourceGroup.None;
	}

	public void UnloadExcept(DataBundleResourceGroup groupsToIgnore)
	{
		if (IsLoaded)
		{
			UnloadUnityObjects(groupsToIgnore);
		}
		if (resources.Count > 0 || bundleAssets.Count > 0)
		{
			loadedGroup &= groupsToIgnore;
		}
		else
		{
			loadedGroup = DataBundleResourceGroup.None;
		}
	}

	private void LoadComplete(DataBundleResourceGroup loaded)
	{
		if (!alreadyDisposed)
		{
			if (IsLoaded)
			{
				loadedGroup |= loaded;
			}
			if (onLoaded != null)
			{
				onLoaded(recordData);
				onLoaded = null;
			}
		}
		else
		{
			Unload();
		}
	}

	private void LoadUnityObjectsAsync(DataBundleResourceGroup groupToLoad)
	{
		List<DataBundleRuntime.DataBundleResourceInfo> list = new List<DataBundleRuntime.DataBundleResourceInfo>();
		string[] tableKey = tableRecordKey.Split(DataBundleRuntime.separator);
		foreach (DataBundleRuntime.DataBundleResourceInfo item in DataBundleRuntime.Instance.EnumerateUnityObjectPaths(typeof(T), tableKey[0], tableKey[1], loadRecordLinks))
		{
			if (item != null && !string.IsNullOrEmpty(item.path) && (groupToLoad & item.data.group) != 0)
			{
				if (item.data.staticResource)
				{
					list.Add(item);
				}
				else if (!bundleAssets.Contains(item))
				{
					bundleAssets.Add(item);
				}
			}
		}
		foreach (DataBundleRuntime.DataBundleResourceInfo item2 in list)
		{
			SharedResourceLoader.SharedResource sharedResource = null;
			if (!resources.Contains(item2))
			{
				sharedResource = ResourceCache.Cache(item2.path);
				resources.Add(item2);
			}
			else
			{
				sharedResource = ResourceCache.GetCachedResource(item2.path);
			}
			if (item2.data.fInfo.DeclaringType == typeof(T) && string.Equals(item2.table, tableKey[0]) && string.Equals(item2.recordKey, tableKey[1]))
			{
				item2.data.fInfo.SetValue(recordData, sharedResource.Resource);
			}
		}
		if (bundleAssets.Count == 0)
		{
			LoadComplete(groupToLoad);
			return;
		}
		int resourceLoadCount = 0;
		foreach (DataBundleRuntime.DataBundleResourceInfo r in bundleAssets)
		{
			BundleLoader.LoadAssetAsync(r.path, delegate
			{
				if (r.data.fInfo.DeclaringType == typeof(T) && string.Equals(r.table, tableKey[0]) && string.Equals(r.recordKey, tableKey[1]))
				{
					UnityEngine.Object loadedAsset = BundleLoader.GetLoadedAsset(r.path);
					r.data.fInfo.SetValue(recordData, loadedAsset);
				}
				if (++resourceLoadCount == bundleAssets.Count)
				{
					LoadComplete(groupToLoad);
				}
			});
		}
	}

	private void UnloadUnityObjects()
	{
		string[] array = null;
		bool flag = !alreadyDisposed && !string.IsNullOrEmpty(tableRecordKey) && recordData != null;
		if (flag)
		{
			array = tableRecordKey.Split(DataBundleRuntime.separator);
		}
		foreach (DataBundleRuntime.DataBundleResourceInfo resource in resources)
		{
			if (flag && resource.data.fInfo.DeclaringType == typeof(T) && string.Equals(resource.table, array[0]) && string.Equals(resource.recordKey, array[1]))
			{
				resource.data.fInfo.SetValue(recordData, null);
			}
			ResourceCache.UnCache(resource.path);
		}
		resources.Clear();
		foreach (DataBundleRuntime.DataBundleResourceInfo bundleAsset in bundleAssets)
		{
			if (flag && bundleAsset.data.fInfo.DeclaringType == typeof(T) && string.Equals(bundleAsset.table, array[0]) && string.Equals(bundleAsset.recordKey, array[1]))
			{
				bundleAsset.data.fInfo.SetValue(recordData, null);
			}
			BundleLoader.UnloadAsset(bundleAsset.path);
		}
		bundleAssets.Clear();
	}

	private void UnloadUnityObjects(DataBundleResourceGroup groupsToIgnore)
	{
		string[] array = null;
		bool flag = !alreadyDisposed && !string.IsNullOrEmpty(tableRecordKey) && recordData != null;
		if (flag)
		{
			array = tableRecordKey.Split(DataBundleRuntime.separator);
		}
		int num = 0;
		while (num < resources.Count)
		{
			DataBundleRuntime.DataBundleResourceInfo dataBundleResourceInfo = resources[num];
			if ((dataBundleResourceInfo.data.group & groupsToIgnore) == 0)
			{
				if (flag && dataBundleResourceInfo.data.fInfo.DeclaringType == typeof(T) && string.Equals(dataBundleResourceInfo.table, array[0]) && string.Equals(dataBundleResourceInfo.recordKey, array[1]))
				{
					dataBundleResourceInfo.data.fInfo.SetValue(recordData, null);
				}
				ResourceCache.UnCache(dataBundleResourceInfo.path);
				resources.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		num = 0;
		while (num < bundleAssets.Count)
		{
			DataBundleRuntime.DataBundleResourceInfo dataBundleResourceInfo2 = bundleAssets[num];
			if ((dataBundleResourceInfo2.data.group & groupsToIgnore) == 0)
			{
				if (flag && dataBundleResourceInfo2.data.fInfo.DeclaringType == typeof(T) && string.Equals(dataBundleResourceInfo2.table, array[0]) && string.Equals(dataBundleResourceInfo2.recordKey, array[1]))
				{
					dataBundleResourceInfo2.data.fInfo.SetValue(recordData, null);
				}
				BundleLoader.UnloadAsset(dataBundleResourceInfo2.path);
				bundleAssets.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (!alreadyDisposed)
		{
			alreadyDisposed = true;
			if (isDisposing)
			{
				recordData = default(T);
				onLoaded = null;
			}
			Unload();
			tableRecordKey = null;
		}
	}

	~DataBundleRecordHandle()
	{
		Dispose(false);
	}
}
