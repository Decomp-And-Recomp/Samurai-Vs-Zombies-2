using System;
using System.Text;
using UnityEngine;

[Serializable]
public class DataBundleHashObject
{
	private enum SchemaLayout
	{
		Schema = 0,
		Table = 1,
		Key = 2,
		FieldID = 3,
		Count = 4
	}

	[SerializeField]
	private string key;

	[SerializeField]
	private string valueString;

	private string[] keyArr;

	public string ValueAsString
	{
		get
		{
			return valueString;
		}
		private set
		{
			valueString = value;
		}
	}

	public bool Valid
	{
		get
		{
			return keyArr != null && keyArr.Length == 4;
		}
	}

	public string Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
			if (value == null)
			{
				keyArr = null;
				return;
			}
			keyArr = value.Split(DataBundleRuntime.separator);
		}
	}

	public string Schema
	{
		get
		{
			return key.Split(DataBundleRuntime.separator)[0];
		}
	}

	public string Table
	{
		get
		{
			return key.Split(DataBundleRuntime.separator)[1];
		}
	}

	public string RecordKey
	{
		get
		{
			return key.Split(DataBundleRuntime.separator)[2];
		}
	}

	public string FieldID
	{
		get
		{
			return key.Split(DataBundleRuntime.separator)[3];
		}
	}

	public string GroupID
	{
		get
		{
			return key.Substring(0, key.LastIndexOf(DataBundleRuntime.separator));
		}
	}

	public DataBundleHashObject()
	{
		Key = null;
		ValueAsString = null;
	}

	public DataBundleHashObject(string key)
		: this()
	{
		Key = key;
	}

	public DataBundleHashObject(string key, string asString)
		: this(key)
	{
		ValueAsString = asString;
	}

	public static string GenerateKey(Type type, string table, string recordKey, string fieldId)
	{
		return GenerateKey(type.Name, table, recordKey, fieldId);
	}

	public static string GenerateKey(string typeName, string table, string recordKey, string fieldId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(typeName))
		{
			stringBuilder.Append(typeName);
			stringBuilder.Append(DataBundleRuntime.separator);
		}
		if (!string.IsNullOrEmpty(table))
		{
			stringBuilder.Append(table);
			stringBuilder.Append(DataBundleRuntime.separator);
		}
		if (!string.IsNullOrEmpty(recordKey))
		{
			stringBuilder.Append(recordKey);
			stringBuilder.Append(DataBundleRuntime.separator);
		}
		if (!string.IsNullOrEmpty(fieldId))
		{
			stringBuilder.Append(fieldId);
		}
		return stringBuilder.ToString().TrimEnd(DataBundleRuntime.separator);
	}

	public static string GenerateKey(Type type, string recordKey, string fieldId)
	{
		string[] array = recordKey.Split(DataBundleRuntime.separator);
		if (array.Length == 2)
		{
			return GenerateKey(type, array[0], array[1], fieldId);
		}
		return string.Empty;
	}
}
