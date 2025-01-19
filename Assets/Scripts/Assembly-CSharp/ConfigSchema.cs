using System;
using System.Collections.Generic;
using System.Text;

[DataBundleClass]
public class ConfigSchema : IComparable<ConfigSchema>
{
	[DataBundleKey(ColumnWidth = 200)]
	public string key;

	[DataBundleField(ColumnWidth = 650)]
	public string value;

	private static Dictionary<string, string> entries;

	public static void Init()
	{
		if (DataBundleRuntime.Instance == null)
		{
			return;
		}
		entries = new Dictionary<string, string>();
		foreach (string item in DataBundleRuntime.Instance.EnumerateTables<ConfigSchema>())
		{
			foreach (string item2 in DataBundleRuntime.Instance.EnumerateRecordKeys(typeof(ConfigSchema), item))
			{
				string text = DataBundleRuntime.Instance.GetValue<string>(typeof(ConfigSchema), item, item2, "value", false);
				entries[item2] = text;
			}
		}
	}

	public static string Entry(string key)
	{
		if (DataBundleRuntime.Instance == null)
		{
			return null;
		}
		if (entries == null)
		{
			Init();
		}
		string result;
		if (!entries.TryGetValue(key, out result))
		{
			return null;
		}
		return result;
	}

	public static string AllEntriesToString()
	{
		if (DataBundleRuntime.Instance == null)
		{
			return null;
		}
		if (entries == null)
		{
			Init();
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> entry in entries)
		{
			stringBuilder.AppendFormat("{0} = {1}\n", entry.Key, entry.Value);
		}
		return stringBuilder.ToString();
	}

	public int CompareTo(ConfigSchema other)
	{
		return string.Compare(key, other.key);
	}
}
