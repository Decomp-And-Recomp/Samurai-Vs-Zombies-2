using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DataBundleRuntimeLegacyUdaman : IUdamanRuntimeDataProvider
{
	private static readonly int typeShift = 48;

	private static readonly int tableShift = 32;

	private static readonly int keyShift = 16;

	[NonSerialized]
	private Hashtable schemaTableKeyFieldValue;

	[NonSerialized]
	private List<string> stringList;

	public void Initialize(bool forceUpdate)
	{
		BundleLoader.LoadAssetInfo();
		stringList = DataBundleSerializer.DeserializeStringList();
		schemaTableKeyFieldValue = DataBundleSerializer.DeserializeHashTable();
	}

	public int GetTableCount(Type schemaName)
	{
		return GetTableNames(schemaName).Count;
	}

	public List<string> GetTableNames(Type schemaName)
	{
		List<string> list = new List<string>();
		if (schemaName == null)
		{
			return list;
		}
		list.AddRange(EnumerateTables(schemaName));
		return list;
	}

	public IEnumerable<string> EnumerateTables(Type schemaName)
	{
		List<string> list = new List<string>();
		long hashCode = GetHashCode(schemaName.ToString(), null, null, null);
		if (schemaTableKeyFieldValue.ContainsKey(hashCode))
		{
			IEnumerable<long> enumerable = schemaTableKeyFieldValue[hashCode] as IEnumerable<long>;
			IEnumerator<long> enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add(ReverseHashCode(enumerator.Current));
			}
		}
		return list;
	}

	public int GetTableLength(Type schemaName, string tableName)
	{
		if (!string.IsNullOrEmpty(tableName))
		{
			long hashCode = GetHashCode(schemaName.ToString(), tableName, null, null);
			if (schemaTableKeyFieldValue.ContainsKey(hashCode))
			{
				return ((List<long>)schemaTableKeyFieldValue[hashCode]).Count;
			}
		}
		return 0;
	}

	public List<string> GetRecordKeys(Type schemaName, string tableName, bool bFullKey)
	{
		List<string> list = new List<string>();
		if (schemaName == null)
		{
			return list;
		}
		if (!string.IsNullOrEmpty(tableName))
		{
			IEnumerable<string> enumerable = EnumerateRecordKeys(schemaName, tableName);
			IEnumerator<string> enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string text = enumerator.Current;
				if (bFullKey)
				{
					text = tableName + DataBundleRuntime.separator + text;
				}
				list.Add(text);
			}
		}
		return list;
	}

	public IEnumerable<string> EnumerateRecordKeys(Type schemaName, string tableName)
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(tableName))
		{
			long hashCode = GetHashCode(schemaName.ToString(), tableName, null, null);
			if (schemaTableKeyFieldValue.ContainsKey(hashCode))
			{
				IEnumerable<long> enumerable = schemaTableKeyFieldValue[hashCode] as IEnumerable<long>;
				IEnumerator<long> enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					long current = enumerator.Current;
					list.Add(ReverseHashCode(current));
				}
			}
		}
		return list;
	}

	public IEnumerable<DataBundleRuntime.DataBundleResourceInfo> EnumerateUnityObjectPaths(Type schemaName, string tableName, bool followRecordLinks)
	{
		List<DataBundleRuntime.DataBundleResourceInfo> list = new List<DataBundleRuntime.DataBundleResourceInfo>();
		IEnumerable<string> enumerable = EnumerateRecordKeys(schemaName, tableName);
		IEnumerator<string> enumerator = enumerable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			list.AddRange(EnumerateUnityObjectPaths(schemaName, tableName, enumerator.Current, followRecordLinks));
		}
		return list;
	}

	public IEnumerable<DataBundleRuntime.DataBundleResourceInfo> EnumerateUnityObjectPaths(Type schemaName, string tableName, string recordName, bool followRecordLinks)
	{
		List<DataBundleRuntime.DataBundleResourceInfo> list = new List<DataBundleRuntime.DataBundleResourceInfo>();
		if (schemaName == null || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(recordName))
		{
			return list;
		}
		DataBundleRuntimeCacheData.CacheFieldData(schemaName.Name, schemaName);
		IEnumerable<DataBundleRuntimeCacheData.DataBundleFieldData> fieldData = DataBundleRuntimeCacheData.GetFieldData(schemaName.Name);
		IEnumerator<DataBundleRuntimeCacheData.DataBundleFieldData> enumerator = fieldData.GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataBundleRuntimeCacheData.DataBundleFieldData current = enumerator.Current;
			Type fieldType = current.fInfo.FieldType;
			if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) || (followRecordLinks && (fieldType.Equals(typeof(DataBundleRecordKey)) || fieldType.Equals(typeof(DataBundleRecordTable)))))
			{
				list.AddRange(EnumerateUnityObjectPaths(schemaName, tableName, recordName, followRecordLinks, current));
			}
		}
		return list;
	}

	private IEnumerable<DataBundleRuntime.DataBundleResourceInfo> EnumerateUnityObjectPaths(Type schemaName, string tableName, string recordName, bool followRecordLinks, DataBundleRuntimeCacheData.DataBundleFieldData fieldData)
	{
		string fieldID = fieldData.fieldID;
		Type fieldType = fieldData.fInfo.FieldType;
		bool flag = fieldType.IsSubclassOf(typeof(UnityEngine.Object));
		long hashCode = GetHashCode(schemaName.ToString(), tableName, recordName, fieldID);
		List<DataBundleRuntime.DataBundleResourceInfo> list = new List<DataBundleRuntime.DataBundleResourceInfo>();
		if (schemaTableKeyFieldValue.ContainsKey(hashCode))
		{
			object obj = schemaTableKeyFieldValue[hashCode];
			if (flag)
			{
				DataBundleRuntime.DataBundleResourceInfo dataBundleResourceInfo = new DataBundleRuntime.DataBundleResourceInfo();
				dataBundleResourceInfo.table = tableName;
				dataBundleResourceInfo.recordKey = recordName;
				dataBundleResourceInfo.path = ((obj == null) ? string.Empty : BundleAssetInfo.Instance.GetAssetPath((int)obj));
				dataBundleResourceInfo.data = fieldData;
				list.Add(dataBundleResourceInfo);
			}
			else if (followRecordLinks && obj != null)
			{
				string text = ReverseHashCode((long)obj);
				if (!string.IsNullOrEmpty(text))
				{
					object[] customAttributes = fieldData.fInfo.GetCustomAttributes(typeof(DataBundleSchemaFilterAttribute), false);
					if (customAttributes != null && customAttributes.Length > 0)
					{
						DataBundleSchemaFilterAttribute dataBundleSchemaFilterAttribute = (DataBundleSchemaFilterAttribute)customAttributes[0];
						if (!dataBundleSchemaFilterAttribute.DontFollowRecordLink)
						{
							Type schema = dataBundleSchemaFilterAttribute.Schema;
							if (fieldType.Equals(typeof(DataBundleRecordKey)))
							{
								string[] array = text.Split(DataBundleRuntime.separator);
								if (array != null && array.Length > 1)
								{
									list.AddRange(EnumerateUnityObjectPaths(schema, array[0], array[1], followRecordLinks));
								}
							}
							else if (fieldType.Equals(typeof(DataBundleRecordTable)))
							{
								list.AddRange(EnumerateUnityObjectPaths(schema, text, followRecordLinks));
							}
						}
					}
				}
			}
		}
		return list;
	}

	public T GetFieldValue<T>(Type schemaName, string tableName, string recordName, string fieldName, T defaultValue, bool queryAssetPath, string language = null)
	{
		return GetValue(GetStringIndex(schemaName.Name), GetStringIndex(tableName), GetStringIndex(recordName), GetStringIndex(fieldName), defaultValue, queryAssetPath);
	}

	private T GetValue<T>(ushort typeCode, ushort tableCode, ushort recordCode, ushort fieldCode, T defaultValue, bool queryAssetPath)
	{
		if (typeCode <= 0 || tableCode <= 0 || recordCode <= 0 || fieldCode <= 0)
		{
			return defaultValue;
		}
		Type typeFromHandle = typeof(T);
		long hashCode = GetHashCode((int)typeCode, (int)tableCode, recordCode, fieldCode);
		if (schemaTableKeyFieldValue.ContainsKey(hashCode))
		{
			if (typeFromHandle == typeof(DataBundleRecordKey) || typeFromHandle == typeof(DataBundleRecordTable))
			{
				string text = ReverseHashCode((long)schemaTableKeyFieldValue[hashCode]);
				return (T)Activator.CreateInstance(typeof(T), text);
			}
			if (queryAssetPath)
			{
				object obj = schemaTableKeyFieldValue[hashCode];
				string text2 = ((obj == null) ? string.Empty : BundleAssetInfo.Instance.GetAssetPath((int)obj));
				return (T)(object)text2;
			}
			T val = (T)schemaTableKeyFieldValue[hashCode];
			if (typeFromHandle == typeof(string) && val == null)
			{
				return (T)(object)string.Empty;
			}
			return val;
		}
		return defaultValue;
	}

	public T InitializeRecord<T>(string tableName, string recordName)
	{
		return InitializeRecordByCode<T>(GetStringIndex(tableName), GetStringIndex(recordName));
	}

	public T[] InitializeRecords<T>(string tableName)
	{
		List<T> list = new List<T>();
		if (!string.IsNullOrEmpty(tableName))
		{
			ushort stringIndex = GetStringIndex(typeof(T).ToString());
			ushort stringIndex2 = GetStringIndex(tableName);
			long hashCode = GetHashCode((int)stringIndex, (int)stringIndex2, 0, 0);
			if (schemaTableKeyFieldValue.ContainsKey(hashCode))
			{
				IEnumerable<long> enumerable = schemaTableKeyFieldValue[hashCode] as IEnumerable<long>;
				IEnumerator<long> enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					list.Add(InitializeRecordByCode<T>(stringIndex2, ReverseHashCodeToIndex_Key(enumerator.Current)));
				}
			}
		}
		return list.ToArray();
	}

	private T InitializeRecordByCode<T>(ushort tableCode, ushort recordCode)
	{
		T val = DataBundleUtils.CreateDefault<T>();
		if (InitializeRecord(val, tableCode, recordCode))
		{
			return val;
		}
		return default(T);
	}

	private bool InitializeRecord<T>(T objectData, ushort tableCode, ushort recordCode)
	{
		bool result = false;
		if (objectData != null && tableCode > 0 && recordCode > 0)
		{
			Type type = objectData.GetType();
			string name = type.Name;
			DataBundleRuntimeCacheData.CacheFieldData(name, type);
			ushort stringIndex = GetStringIndex(name);
			IEnumerable<DataBundleRuntimeCacheData.DataBundleFieldData> fieldData = DataBundleRuntimeCacheData.GetFieldData(name);
			IEnumerator<DataBundleRuntimeCacheData.DataBundleFieldData> enumerator = fieldData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				DataBundleRuntimeCacheData.DataBundleFieldData current = enumerator.Current;
				FieldInfo fInfo = current.fInfo;
				string fieldID = current.fieldID;
				bool staticResource = current.staticResource;
				ushort stringIndex2 = GetStringIndex(fieldID);
				long hashCode = GetHashCode((int)stringIndex, (int)tableCode, recordCode, stringIndex2);
				if (schemaTableKeyFieldValue.ContainsKey(hashCode))
				{
					result = true;
					object obj = schemaTableKeyFieldValue[hashCode];
					if (fInfo.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						string text = ((obj == null) ? string.Empty : BundleAssetInfo.Instance.GetAssetPath((int)obj));
						if (string.IsNullOrEmpty(text))
						{
							continue;
						}
						string relativePath;
						if (!staticResource)
						{
							fInfo.SetValue(objectData, BundleLoader.GetLoadedAsset(text));
						}
						else if (DataBundleUtils.IsAssetRelativeToResourcesFolder(text, out relativePath))
						{
							UnityEngine.Object value = null;
							SharedResourceLoader.SharedResource loadedAsset = SharedResourceLoader.GetLoadedAsset(text);
							if (loadedAsset != null)
							{
								value = loadedAsset.Resource;
							}
							fInfo.SetValue(objectData, value);
						}
					}
					else if (fInfo.FieldType == typeof(DataBundleRecordTable) || fInfo.FieldType == typeof(DataBundleRecordKey))
					{
						string text2 = ((obj == null) ? string.Empty : ReverseHashCode((long)obj));
						object value2 = Activator.CreateInstance(fInfo.FieldType, text2);
						fInfo.SetValue(objectData, value2);
					}
					else if (fInfo.FieldType == typeof(string) && obj == null)
					{
						fInfo.SetValue(objectData, string.Empty);
					}
					else
					{
						fInfo.SetValue(objectData, obj);
					}
				}
				else
				{
					fInfo.SetValue(objectData, DataBundleUtils.DefaultValueRuntime(fInfo));
				}
			}
		}
		return result;
	}

	public ushort GetStringIndex(string stringLookup)
	{
		if (string.IsNullOrEmpty(stringLookup))
		{
			return 0;
		}
		int num = stringList.BinarySearch(stringLookup);
		if (num < 0)
		{
			return 0;
		}
		return (ushort)num;
	}

	public string GetStringValueAtIndex(ushort stringIndex)
	{
		if (stringIndex <= 0 || stringIndex >= stringList.Count)
		{
			return string.Empty;
		}
		return stringList[stringIndex];
	}

	public long GetHashCode(string key)
	{
		return GetHashCode(key, null);
	}

	public long GetHashCode(string key, List<string> lookup)
	{
		if (string.IsNullOrEmpty(key))
		{
			return 0L;
		}
		string[] array = key.Split(DataBundleRuntime.separator);
		return GetHashCode((array.Length <= 0) ? string.Empty : array[0], (array.Length <= 1) ? string.Empty : array[1], (array.Length <= 2) ? string.Empty : array[2], (array.Length <= 3) ? string.Empty : array[3], lookup);
	}

	public long GetHashCode(string type, string table, string record, string field)
	{
		return GetHashCode(type, table, record, field, null);
	}

	public long GetHashCode(string type, string table, string record, string field, List<string> lookup)
	{
		if (lookup != null)
		{
			return GetHashCode((int)ItemLookup(type, lookup), (int)ItemLookup(table, lookup), ItemLookup(record, lookup), ItemLookup(field, lookup));
		}
		return GetHashCode((int)GetStringIndex(type), (int)GetStringIndex(table), GetStringIndex(record), GetStringIndex(field));
	}

	private ushort ItemLookup(string item, List<string> lookup)
	{
		if (string.IsNullOrEmpty(item))
		{
			return 0;
		}
		int num = lookup.IndexOf(item);
		if (num < 0)
		{
			throw new IndexOutOfRangeException(item + " does not exist in the String List, skipping the key");
		}
		return (ushort)num;
	}

	public long GetHashCode(long type, long table, ushort record, ushort field)
	{
		return (type << typeShift) + (table << tableShift) + (record << keyShift) + (int)field;
	}

	public string ReverseHashCode(long hashCode)
	{
		ushort stringIndex = ReverseHashCodeToIndex_Type(hashCode);
		ushort stringIndex2 = ReverseHashCodeToIndex_Table(hashCode);
		ushort stringIndex3 = ReverseHashCodeToIndex_Key(hashCode);
		ushort stringIndex4 = ReverseHashCodeToIndex_Field(hashCode);
		return DataBundleHashObject.GenerateKey(GetStringValueAtIndex(stringIndex), GetStringValueAtIndex(stringIndex2), GetStringValueAtIndex(stringIndex3), GetStringValueAtIndex(stringIndex4));
	}

	public ushort ReverseHashCodeToIndex_Type(long hashCode)
	{
		return (ushort)((hashCode >> typeShift) & 0xFFFF);
	}

	public ushort ReverseHashCodeToIndex_Table(long hashCode)
	{
		return (ushort)((hashCode >> tableShift) & 0xFFFF);
	}

	public ushort ReverseHashCodeToIndex_Key(long hashCode)
	{
		return (ushort)((hashCode >> keyShift) & 0xFFFF);
	}

	public ushort ReverseHashCodeToIndex_Field(long hashCode)
	{
		return (ushort)(hashCode & 0xFFFF);
	}
}
