using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataBundleRecordTable
{
	[SerializeField]
	private string recordTable;

	public string RecordTable
	{
		get
		{
			return recordTable;
		}
	}

	public DataBundleRecordTable()
	{
		recordTable = string.Empty;
	}

	public DataBundleRecordTable(string table)
	{
		recordTable = table;
	}

	public override string ToString()
	{
		return recordTable;
	}

	public T[] InitializeRecords<T>()
	{
		return DataBundleUtils.InitializeRecords<T>(recordTable);
	}

	public IEnumerable<T> EnumerateRecords<T>()
	{
		foreach (T item in DataBundleRuntime.Instance.EnumerateRecords<T>(recordTable))
		{
			yield return item;
		}
	}

	public static bool IsNullOrEmpty(DataBundleRecordTable key)
	{
		return object.ReferenceEquals(key, null) || key.ToString().Equals(string.Empty);
	}

	public static implicit operator string(DataBundleRecordTable table)
	{
		return table.recordTable;
	}

	public static implicit operator DataBundleRecordTable(string recTable)
	{
		return new DataBundleRecordTable(recTable);
	}
}
