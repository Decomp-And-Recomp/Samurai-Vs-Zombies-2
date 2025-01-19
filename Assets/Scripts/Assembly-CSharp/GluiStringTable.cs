using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GluiStringTable
{
	public Dictionary<string, string> strings = new Dictionary<string, string>();

	public GluiStringTable(string path)
	{
		Load(path);
	}

	public string Find(string id)
	{
		string value;
		if (strings.TryGetValue(id, out value))
		{
			return value;
		}
		return id;
	}

	private void Load(string path)
	{
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
		StreamReader streamReader = new StreamReader(memoryStream);
		while (true)
		{
			string text = streamReader.ReadLine();
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			int num = text.IndexOf('=');
			if (num == -1)
			{
				return;
			}
			string text2 = text.Substring(0, num);
			text2 = text2.Trim();
			string text3 = text.Substring(num + 1);
			text3 = text3.Trim();
			strings.Add(text2, text3);
		}
		streamReader.Close();
		memoryStream.Close();
	}

	public void Save(string path)
	{
		StreamWriter streamWriter = new StreamWriter(path);
		if (streamWriter == null)
		{
			return;
		}
		foreach (string key in strings.Keys)
		{
			string value;
			if (strings.TryGetValue(key, out value))
			{
				streamWriter.WriteLine(key + " = " + value);
			}
		}
		streamWriter.Close();
	}
}
