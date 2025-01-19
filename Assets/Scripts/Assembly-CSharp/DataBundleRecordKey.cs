using System;
using UnityEngine;

[Serializable]
public class DataBundleRecordKey
{
	[SerializeField]
	private string recordKey;

	public string Key
	{
		get
		{
			if (!string.IsNullOrEmpty(recordKey))
			{
				string[] array = recordKey.Split(DataBundleRuntime.separator);
				return array[array.Length - 1];
			}
			return string.Empty;
		}
	}

	public string Table
	{
		get
		{
			if (!string.IsNullOrEmpty(recordKey))
			{
				string[] array = recordKey.Split(DataBundleRuntime.separator);
				if (array.Length > 1)
				{
					return array[array.Length - 2];
				}
			}
			return string.Empty;
		}
	}

	public DataBundleRecordKey()
	{
		recordKey = string.Empty;
	}

	public DataBundleRecordKey(string key)
	{
		recordKey = key;
	}

	public DataBundleRecordKey(string table, string key)
	{
		recordKey = string.Format("{0}{1}{2}", table, DataBundleRuntime.separator, key);
	}

	public override string ToString()
	{
		return recordKey;
	}

	public T InitializeRecord<T>()
	{
		return DataBundleUtils.InitializeRecord<T>(recordKey);
	}

	public static bool IsNullOrEmpty(DataBundleRecordKey key)
	{
		if (object.ReferenceEquals(key, null) || string.IsNullOrEmpty(key.ToString()))
		{
			return true;
		}
		string[] array = key.ToString().Split(DataBundleRuntime.separator);
		if (array.Length <= 1 || string.IsNullOrEmpty(array[0]) || string.IsNullOrEmpty(array[1]))
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		DataBundleRecordKey dataBundleRecordKey = obj as DataBundleRecordKey;
		if (dataBundleRecordKey == null)
		{
			return false;
		}
		return CompareKeys(recordKey, dataBundleRecordKey);
	}

	private static bool CompareKeys(DataBundleRecordKey compareKey1, DataBundleRecordKey compareKey2)
	{
		string[] array = compareKey1.ToString().Split(DataBundleRuntime.separator);
		string[] array2 = compareKey2.ToString().Split(DataBundleRuntime.separator);
		if (array.Length <= 0 || array2.Length <= 0)
		{
			return false;
		}
		for (int i = 1; i <= array.Length && i <= array2.Length; i++)
		{
			if (!array[array.Length - i].Equals(array2[array2.Length - i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ recordKey.GetHashCode();
	}

	public static implicit operator string(DataBundleRecordKey key)
	{
		return key.recordKey;
	}

	public static implicit operator DataBundleRecordKey(string key)
	{
		return new DataBundleRecordKey(key);
	}

	public static bool operator ==(DataBundleRecordKey key1, DataBundleRecordKey key2)
	{
		if (object.ReferenceEquals(key1, key2))
		{
			return true;
		}
		if (object.ReferenceEquals(key1, null) || object.ReferenceEquals(key2, null))
		{
			return false;
		}
		return CompareKeys(key1, key2);
	}

	public static bool operator !=(DataBundleRecordKey key1, DataBundleRecordKey key2)
	{
		return !(key1 == key2);
	}
}
