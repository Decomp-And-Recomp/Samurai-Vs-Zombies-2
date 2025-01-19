using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DataBundleTableHandle<T> : IDisposable where T : class
{
	private T[] recordData;

	private string tableKey;

	private DataBundleResourceGroup loadedGroup;

	private bool loadRecordLinks;

	private Action<T[]> onLoaded;

	private List<DataBundleRuntime.DataBundleResourceInfo> resources = new List<DataBundleRuntime.DataBundleResourceInfo>();

	private List<DataBundleRuntime.DataBundleResourceInfo> bundleAssets = new List<DataBundleRuntime.DataBundleResourceInfo>();

	private bool alreadyDisposed;

	public string TableKey
	{
		get
		{
			return tableKey;
		}
	}

	public T[] Data
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

	public DataBundleTableHandle(string tableKey)
	{
		this.tableKey = tableKey;
		recordData = DataBundleRuntime.Instance.InitializeRecords<T>(this.tableKey);
	}

	public void Load(DataBundleResourceGroup groupToLoad, bool loadRecordLinks, Action<T[]> onLoaded)
	{
		LoadWithFilter(groupToLoad, loadRecordLinks, onLoaded, null);
	}

	public void LoadWithFilter(DataBundleResourceGroup groupToLoad, bool loadRecordLinks, Action<T[]> onLoaded, Predicate<T> loadFilter)
	{
		this.onLoaded = onLoaded;
		this.loadRecordLinks = loadRecordLinks;
		if (recordData != null && recordData.Length > 0 && groupToLoad != 0)
		{
			LoadUnityObjectsAsync(groupToLoad, loadFilter);
		}
		else
		{
			LoadComplete(DataBundleResourceGroup.None);
		}
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

	public T FindFirstRecordByKey<V>(string keyFieldName, V value)
	{
		T[] array = FindRecordsByKey(keyFieldName, value);
		if (array == null || array.Length < 1)
		{
			return (T)null;
		}
		return array[0];
	}

	public T[] FindRecordsByKey<V>(string keyFieldName, V value)
	{
		FieldInfo[] fields = typeof(T).GetFields();
		FieldInfo fieldInfo = null;
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo2 in array)
		{
			if (fieldInfo2.Name == keyFieldName && fieldInfo2.FieldType == value.GetType())
			{
				fieldInfo = fieldInfo2;
				break;
			}
		}
		if (fieldInfo == null)
		{
			return null;
		}
		List<T> list = new List<T>();
		T[] array2 = recordData;
		foreach (T val in array2)
		{
			if (val != null && object.Equals(fieldInfo.GetValue(val), value))
			{
				list.Add(val);
			}
		}
		return list.ToArray();
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

	private void LoadUnityObjectsAsync(DataBundleResourceGroup groupToLoad, Predicate<T> loadFilter)
	{
		int recordIndex = 0;
		List<DataBundleRuntime.DataBundleResourceInfo> list = new List<DataBundleRuntime.DataBundleResourceInfo>();
		List<DataBundleRuntime.DataBundleResourceInfo>[] array = new List<DataBundleRuntime.DataBundleResourceInfo>[recordData.Length];
		bool flag = false;
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys(typeof(T), tableKey))
		{
			if (loadFilter == null || loadFilter(recordData[recordIndex]))
			{
				foreach (DataBundleRuntime.DataBundleResourceInfo item2 in DataBundleRuntime.Instance.EnumerateUnityObjectPaths(typeof(T), tableKey, item, loadRecordLinks))
				{
					if (item2 != null && !string.IsNullOrEmpty(item2.path) && (groupToLoad & item2.data.group) != 0)
					{
						if (item2.data.staticResource)
						{
							list.Add(item2);
							continue;
						}
						array[recordIndex].Add(item2);
						flag = true;
					}
				}
				T obj = recordData[recordIndex];
				foreach (DataBundleRuntime.DataBundleResourceInfo item3 in list)
				{
					SharedResourceLoader.SharedResource sharedResource = null;
					if (!resources.Contains(item3))
					{
						sharedResource = ResourceCache.Cache(item3.path);
						resources.Add(item3);
					}
					else
					{
						sharedResource = ResourceCache.GetCachedResource(item3.path);
					}
					if (item3.data.fInfo.DeclaringType == typeof(T) && string.Equals(item3.table, tableKey) && string.Equals(item3.recordKey, item))
					{
						item3.data.fInfo.SetValue(obj, sharedResource.Resource);
					}
				}
				list.Clear();
			}
			recordIndex++;
		}
		if (!flag)
		{
			LoadComplete(groupToLoad);
			return;
		}
		int recordLoadCount = array.Length;
		List<string> recordKeys = DataBundleRuntime.Instance.GetRecordKeys(typeof(T), tableKey, false);
		for (recordIndex = 0; recordIndex < array.Length; recordIndex++)
		{
			T data = recordData[recordIndex];
			int resourceLoadCount = array[recordIndex].Count;
			foreach (DataBundleRuntime.DataBundleResourceInfo r in array[recordIndex])
			{
				if (!bundleAssets.Contains(r))
				{
					bundleAssets.Add(r);
				}
				BundleLoader.LoadAssetAsync(r.path, delegate
				{
					if (r.data.fInfo.DeclaringType == typeof(T) && string.Equals(r.table, tableKey) && string.Equals(r.recordKey, recordKeys[recordIndex]))
					{
						UnityEngine.Object loadedAsset = BundleLoader.GetLoadedAsset(r.path);
						r.data.fInfo.SetValue(data, loadedAsset);
					}
					if (--resourceLoadCount == 0)
					{
						recordLoadCount--;
						if (recordLoadCount == 0)
						{
							LoadComplete(groupToLoad);
						}
					}
				});
			}
		}
	}

	private void UnloadUnityObjects()
	{
		List<string> recordKeys = DataBundleRuntime.Instance.GetRecordKeys(typeof(T), tableKey, false);
		foreach (DataBundleRuntime.DataBundleResourceInfo resource in resources)
		{
			for (int i = 0; i < recordData.Length; i++)
			{
				if (resource.data.fInfo.DeclaringType == typeof(T) && string.Equals(resource.table, tableKey) && string.Equals(resource.recordKey, recordKeys[i]))
				{
					resource.data.fInfo.SetValue(recordData[i], null);
				}
			}
			ResourceCache.UnCache(resource.path);
		}
		resources.Clear();
		foreach (DataBundleRuntime.DataBundleResourceInfo bundleAsset in bundleAssets)
		{
			for (int j = 0; j < recordData.Length; j++)
			{
				if (bundleAsset.data.fInfo.DeclaringType == typeof(T) && string.Equals(bundleAsset.table, tableKey) && string.Equals(bundleAsset.recordKey, recordKeys[j]))
				{
					bundleAsset.data.fInfo.SetValue(recordData[j], null);
				}
			}
			BundleLoader.UnloadAsset(bundleAsset.path);
		}
		bundleAssets.Clear();
	}

	private void UnloadUnityObjects(DataBundleResourceGroup groupsToIgnore)
	{
		List<string> list = null;
		bool flag = !alreadyDisposed && !string.IsNullOrEmpty(tableKey) && recordData != null;
		if (flag)
		{
			list = DataBundleRuntime.Instance.GetRecordKeys(typeof(T), tableKey, false);
		}
		int num = 0;
		while (num < resources.Count)
		{
			DataBundleRuntime.DataBundleResourceInfo dataBundleResourceInfo = resources[num];
			if ((dataBundleResourceInfo.data.group & groupsToIgnore) == 0)
			{
				if (flag)
				{
					for (int i = 0; i < recordData.Length; i++)
					{
						if (dataBundleResourceInfo.data.fInfo.DeclaringType == typeof(T) && string.Equals(dataBundleResourceInfo.table, tableKey) && string.Equals(dataBundleResourceInfo.recordKey, list[i]))
						{
							dataBundleResourceInfo.data.fInfo.SetValue(recordData[i], null);
						}
					}
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
				if (flag)
				{
					for (int j = 0; j < recordData.Length; j++)
					{
						if (dataBundleResourceInfo2.data.fInfo.DeclaringType == typeof(T) && string.Equals(dataBundleResourceInfo2.table, tableKey) && string.Equals(dataBundleResourceInfo2.recordKey, list[j]))
						{
							dataBundleResourceInfo2.data.fInfo.SetValue(recordData[j], null);
						}
					}
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
				recordData = null;
				onLoaded = null;
			}
			Unload();
		}
	}

	~DataBundleTableHandle()
	{
		Dispose(false);
	}
}
