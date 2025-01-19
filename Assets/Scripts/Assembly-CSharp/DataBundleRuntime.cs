using System;
using System.Collections.Generic;
using UnityEngine;

public class DataBundleRuntime
{
	public class DataBundleResourceInfo : IEquatable<DataBundleResourceInfo>
	{
		public string path;

		public string table;

		public string recordKey;

		public DataBundleRuntimeCacheData.DataBundleFieldData data;

		public bool Equals(DataBundleResourceInfo other)
		{
			return other != null && string.Equals(path, other.path) && string.Equals(data.fieldID, other.data.fieldID);
		}
	}

	public static readonly string resourceFolder = "/Resources/";

	public static readonly string defaultLanguage = "English";

	public static readonly string notLocalized = "None";

	public static readonly string dataBaseName = "udamanRuntimeDatabase.db";

	public static readonly char separator = '.';

	private bool initialized;

	private static DataBundleRuntime instance;

	private static IUdamanRuntimeDataProvider instDataProvider;

	private static Dictionary<string, int> mMapSchemaNames = new Dictionary<string, int>();

	private static List<Dictionary<string, int>> mListOfTableNames = new List<Dictionary<string, int>>();

	private static List<object[]> mLstItemRecords = new List<object[]>();

	private static List<List<string>> mListItemRecordKeys = new List<List<string>>();

	public static string DataBundleAppVersionPrefsKey
	{
		get
		{
			return "DATA_BUNDLE_APP_VERSION";
		}
	}

	public bool TableCacheDirty { get; set; }

	public bool Initialized
	{
		get
		{
			return initialized;
		}
	}

	public static DataBundleRuntime Instance
	{
		get
		{
			if (instance != null && !instance.initialized)
			{
				return null;
			}
			return instance;
		}
	}

	public static void Clear()
	{
		instance = null;
	}

	public static string TableRecordKey(string table, string key)
	{
		return string.Format("{0}{1}{2}", table, separator, key);
	}

	public static string TableKey(string tableRecordKey)
	{
		if (!string.IsNullOrEmpty(tableRecordKey))
		{
			int num = tableRecordKey.IndexOf(separator);
			if (num != -1)
			{
				return tableRecordKey.Substring(0, num);
			}
		}
		return string.Empty;
	}

	public static string RecordKey(string tableRecordKey)
	{
		if (!string.IsNullOrEmpty(tableRecordKey))
		{
			int num = tableRecordKey.IndexOf(separator);
			if (num != -1)
			{
				return tableRecordKey.Substring(num + 1);
			}
		}
		return tableRecordKey;
	}

	public T GetValue<T>(Type type, string table, string recordKey, string fieldId, bool queryAssetPath, string language = null)
	{
		T defaultValue = DataBundleUtils.GetDefaultValue<T>(type, fieldId);
		return instDataProvider.GetFieldValue(type, table, recordKey, fieldId, defaultValue, queryAssetPath, language);
	}

	public T GetValue<T>(Type type, string recordKey, string fieldId, bool queryAssetPath, string language = null)
	{
		T defaultValue = DataBundleUtils.GetDefaultValue<T>(type, fieldId);
		string[] array = recordKey.Split(separator);
		if (array != null && array.Length > 1)
		{
			return instDataProvider.GetFieldValue(type, array[0], array[1], fieldId, defaultValue, queryAssetPath, language);
		}
		return defaultValue;
	}

	public IEnumerable<DataBundleResourceInfo> EnumerateUnityObjectPaths(Type type, string table, bool followRecordLinks)
	{
		foreach (string recordKey in EnumerateRecordKeys(type, table))
		{
			foreach (DataBundleResourceInfo item in EnumerateUnityObjectPaths(type, table, recordKey, followRecordLinks))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<DataBundleResourceInfo> EnumerateUnityObjectPathsAtKey(Type type, string tableRecordKey, bool followRecordLinks)
	{
		string[] tableKey = tableRecordKey.Split(separator);
		if (tableKey == null || tableKey.Length <= 1)
		{
			yield break;
		}
		foreach (DataBundleResourceInfo item in EnumerateUnityObjectPaths(type, tableKey[0], tableKey[1], followRecordLinks))
		{
			yield return item;
		}
	}

	public IEnumerable<DataBundleResourceInfo> EnumerateUnityObjectPaths(Type type, string table, string recordKey, bool followRecordLinks)
	{
		foreach (DataBundleResourceInfo item in instDataProvider.EnumerateUnityObjectPaths(type, table, recordKey, followRecordLinks))
		{
			yield return item;
		}
	}

	public T InitializeRecord<T>(string table, string key)
	{
		if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
		{
			return default(T);
		}
		int schemaToTableNamesIndex = GetSchemaToTableNamesIndex<T>();
		if (schemaToTableNamesIndex >= 0)
		{
			return GetRecordFromPreloadedTable<T>(schemaToTableNamesIndex, table, key);
		}
		return instDataProvider.InitializeRecord<T>(table, key);
	}

	public T InitializeRecord<T>(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return default(T);
		}
		string[] array = key.Split(separator);
		if (array.Length > 1)
		{
			return InitializeRecord<T>(array[0], array[1]);
		}
		return default(T);
	}

	public T[] InitializeRecords<T>(string table)
	{
		int schemaToTableNamesIndex = GetSchemaToTableNamesIndex<T>();
		if (schemaToTableNamesIndex >= 0)
		{
			return GetRecordsFromPreloadedTable<T>(schemaToTableNamesIndex, table);
		}
		return instDataProvider.InitializeRecords<T>(table);
	}

	public IEnumerable<T> EnumerateRecords<T>(string table)
	{
		T[] array = InitializeRecords<T>(table);
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
	}

	public IEnumerable<string> EnumerateTables<T>()
	{
		foreach (string tableName in instDataProvider.GetTableNames(typeof(T)))
		{
			yield return tableName;
		}
	}

	public int GetRecordTableLength(Type schema, string table)
	{
		return instDataProvider.GetTableLength(schema, table);
	}

	public IEnumerable<string> EnumerateRecordKeys<T>(string table)
	{
		foreach (string item in EnumerateRecordKeys(typeof(T), table))
		{
			yield return item;
		}
	}

	public IEnumerable<string> EnumerateRecordKeys(Type type, string table)
	{
		foreach (string item in instDataProvider.EnumerateRecordKeys(type, table))
		{
			yield return item;
		}
	}

	public List<string> GetRecordKeys(Type type, string table, bool bFullKey)
	{
		return instDataProvider.GetRecordKeys(type, table, bFullKey);
	}

	public static void Initialize()
	{
		if (instance == null || !instance.initialized)
		{
			instance = new DataBundleRuntime();
			bool forceUpdateDataBundle = AssetBundleConfig.ForceUpdateDataBundle;
			instDataProvider = new DataBundleRuntimeLegacyUdaman();
			instDataProvider.Initialize(forceUpdateDataBundle);
			instance.initialized = true;
			if (forceUpdateDataBundle)
			{
				PlayerPrefs.SetString(DataBundleAppVersionPrefsKey, GeneralConfig.Version);
			}
			if (false)
			{
				((DataBundleRuntimeLegacyUdaman)instDataProvider).GetFieldValue(null, null, null, null, 0, false);
				((DataBundleRuntimeLegacyUdaman)instDataProvider).GetFieldValue(null, null, null, null, 0f, false);
				((DataBundleRuntimeLegacyUdaman)instDataProvider).GetFieldValue(null, null, null, null, false, false);
			}
		}
	}

	public void Reinitialize()
	{
		if (instance != null)
		{
			instance.initialized = false;
		}
		Initialize();
	}

	public static void PreloadItemRecords<T>() where T : class
	{
		string name = typeof(T).Name;
		mListOfTableNames.Add(new Dictionary<string, int>());
		int num = mListOfTableNames.Count - 1;
		IEnumerable<string> enumerable = Instance.EnumerateTables<T>();
		foreach (string item2 in enumerable)
		{
			T[] item = Instance.InitializeRecords<T>(item2);
			mLstItemRecords.Add(item);
			mListItemRecordKeys.Add(Instance.GetRecordKeys(typeof(T), item2, false));
			Dictionary<string, int> dictionary = mListOfTableNames[num];
			dictionary[item2] = mLstItemRecords.Count - 1;
		}
		mMapSchemaNames[name] = num;
	}

	public static int GetSchemaToTableNamesIndex<T>()
	{
		string name = typeof(T).Name;
		if (mMapSchemaNames != null && mMapSchemaNames.Count > 0 && mMapSchemaNames.ContainsKey(name))
		{
			return mMapSchemaNames[name];
		}
		return -1;
	}

	public static Dictionary<string, int> GetTableNamesFromPreloadedSchema<T>()
	{
		int schemaToTableNamesIndex = GetSchemaToTableNamesIndex<T>();
		if (mListOfTableNames != null && mListOfTableNames.Count > schemaToTableNamesIndex)
		{
			return mListOfTableNames[schemaToTableNamesIndex];
		}
		return null;
	}

	public static T[] GetRecordsFromPreloadedTable<T>(int SchemaIndex, string strTableName)
	{
		int index = mListOfTableNames[SchemaIndex][strTableName];
		return (T[])(object)mLstItemRecords[index];
	}

	public static T GetRecordFromPreloadedTable<T>(int SchemaIndex, string strTableName, string recordKey)
	{
		int index = mListOfTableNames[SchemaIndex][strTableName];
		T[] array = (T[])(object)mLstItemRecords[index];
		int num = mListItemRecordKeys[index].IndexOf(recordKey);
		if (num >= 0 && num < mListItemRecordKeys[index].Count)
		{
			return array[num];
		}
		return default(T);
	}

	public static bool operator !(DataBundleRuntime inst)
	{
		return inst == null;
	}
}
