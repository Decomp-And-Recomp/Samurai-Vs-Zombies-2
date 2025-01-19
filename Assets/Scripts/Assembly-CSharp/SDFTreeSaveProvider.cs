using System;
using System.Collections.Generic;
using System.IO;

public class SDFTreeSaveProvider : SaveProvider
{
	private SDFTreeNode data = new SDFTreeNode();

	public SDFTreeNode Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public void Clear()
	{
		data = new SDFTreeNode();
	}

	protected override bool Write(Stream dataStream, IEnumerable<SaveTarget> targets, DeviceData deviceData)
	{
		if (dataStream == null)
		{
			return false;
		}
		if (deviceData != null)
		{
			deviceData.SerializeToStream(dataStream);
		}
		return SDFTree.Save(data, dataStream);
	}

	protected override bool Read(SaveHeader dataHeader, Stream dataStream, SaveTarget target, DeviceData deviceData)
	{
		if (dataStream == null)
		{
			return false;
		}
		if (dataHeader.UseDeviceData)
		{
			deviceData.DeserializeFromStream(dataStream);
		}
		data = SDFTree.LoadFromStream(dataStream);
		return data != null;
	}

	public void SetValue(string attrib, string val)
	{
		data[attrib] = val;
	}

	public void SetValue(string attrib, string val, string subSection)
	{
		GetSubSection(subSection)[attrib] = val;
	}

	public void SetValueInt(string attrib, int val)
	{
		SetValue(attrib, val.ToString());
	}

	public void SetValueInt(string attrib, int val, string subSection)
	{
		SetValue(attrib, val.ToString(), subSection);
	}

	public void SetValueFloat(string attrib, float val)
	{
		SetValue(attrib, val.ToString());
	}

	public void SetValueFloat(string attrib, float val, string subSection)
	{
		SetValue(attrib, val.ToString(), subSection);
	}

	public void SetValueBool(string attrib, bool val)
	{
		SetValue(attrib, val.ToString());
	}

	public void SetValueBool(string attrib, bool val, string subSection)
	{
		SetValue(attrib, val.ToString(), subSection);
	}

	public string GetValue(string attrib)
	{
		return GetValue(attrib, null);
	}

	public string GetValue(string attrib, string subSection)
	{
		if (!GetSubSection(subSection).hasAttribute(attrib))
		{
			return string.Empty;
		}
		return GetSubSection(subSection)[attrib];
	}

	public int GetValueInt(string attrib)
	{
		return GetValueInt(attrib, null);
	}

	public int GetValueInt(string attrib, string subSection)
	{
		string value = GetValue(attrib, subSection);
		if (value.Length == 0)
		{
			return 0;
		}
		return int.Parse(value);
	}

	public float GetValueFloat(string attrib)
	{
		return GetValueFloat(attrib, null);
	}

	public float GetValueFloat(string attrib, string subSection)
	{
		string value = GetValue(attrib);
		if (value.Length == 0)
		{
			return 0f;
		}
		return float.Parse(value);
	}

	public bool GetValueBool(string attrib)
	{
		return GetValueBool(attrib, null);
	}

	public bool GetValueBool(string attrib, string subSection)
	{
		string value = GetValue(attrib);
		if (value.Length == 0)
		{
			return false;
		}
		return bool.Parse(value);
	}

	public void SetDictionaryValue<T>(string subNode, string attrib, T val)
	{
		SetDictionaryValue(subNode, attrib, val, null);
	}

	public void SetDictionaryValue<T>(string subNode, string attrib, T val, string subSection)
	{
		SDFTreeNode sDFTreeNode = GetSubSection(subSection).to(subNode);
		if (sDFTreeNode == null)
		{
			sDFTreeNode = new SDFTreeNode();
			GetSubSection(subSection).SetChild(subNode, sDFTreeNode);
		}
		string value = ConvertToString(val);
		sDFTreeNode[attrib] = value;
	}

	public T GetDictionaryValue<T>(string subNode, string attrib)
	{
		return GetDictionaryValue<T>(subNode, attrib, null);
	}

	public T GetDictionaryValue<T>(string subNode, string attrib, string subSection)
	{
		SDFTreeNode sDFTreeNode = GetSubSection(subSection).to(subNode);
		if (sDFTreeNode == null)
		{
			return default(T);
		}
		if (!sDFTreeNode.hasAttribute(attrib))
		{
			return default(T);
		}
		return ConvertFromString<T>(sDFTreeNode[attrib]);
	}

	public List<string> GetSubNodeValueList(string subNode)
	{
		return GetSubNodeValueList(subNode, null);
	}

	public List<string> GetSubNodeValueList(string subNode, string subSection)
	{
		List<string> list = new List<string>();
		if (GetSubSection(subSection).hasChild(subNode))
		{
			SDFTreeNode sDFTreeNode = GetSubSection(subSection).to(subNode);
			for (int i = 0; sDFTreeNode.hasAttribute(i); i++)
			{
				list.Add(sDFTreeNode[i]);
			}
		}
		return list;
	}

	public void SetSubNodeValueList(string subNode, List<string> vals)
	{
		SetSubNodeValueList(subNode, vals, null);
	}

	public void SetSubNodeValueList(string subNode, List<string> vals, string subSection)
	{
		SDFTreeNode sDFTreeNode = null;
		if (GetSubSection(subSection).hasChild(subNode))
		{
			sDFTreeNode = GetSubSection(subSection).to(subNode);
		}
		else
		{
			sDFTreeNode = new SDFTreeNode();
			GetSubSection(subSection).SetChild(subNode, sDFTreeNode);
		}
		sDFTreeNode.ClearAttributes();
		int num = 0;
		foreach (string val in vals)
		{
			sDFTreeNode[num] = val;
			num++;
		}
	}

	public void SetSimpleList(string subNode, List<string> values)
	{
		SetSimpleList(subNode, values, null);
	}

	public void SetSimpleList(string subNode, List<string> values, string subSection)
	{
		SDFTreeNode sDFTreeNode = null;
		if (GetSubSection(subSection).hasChild(subNode))
		{
			sDFTreeNode = GetSubSection(subSection).to(subNode);
		}
		else
		{
			sDFTreeNode = new SDFTreeNode();
			GetSubSection(subSection).SetChild(subNode, sDFTreeNode);
		}
		sDFTreeNode.ClearAttributes();
		if (values == null)
		{
			return;
		}
		int num = 0;
		foreach (string value in values)
		{
			sDFTreeNode[num] = value;
			num++;
		}
	}

	public List<string> GetSimpleList(string subNode)
	{
		return GetSimpleList(subNode, null);
	}

	public List<string> GetSimpleList(string subNode, string subSection)
	{
		List<string> list = new List<string>();
		SDFTreeNode sDFTreeNode = GetSubSection(subSection).to(subNode);
		if (sDFTreeNode != null)
		{
			for (int i = 0; sDFTreeNode.hasAttribute(i); i++)
			{
				list.Add(sDFTreeNode[i]);
			}
		}
		return list;
	}

	public T ConvertFromString<T>(string val)
	{
		return (T)Convert.ChangeType(val, typeof(T));
	}

	public string ConvertToString<T>(T val)
	{
		return Convert.ToString(val);
	}

	private SDFTreeNode GetSubSection(string subSection)
	{
		if (subSection == null || subSection == string.Empty)
		{
			return data;
		}
		if (!data.hasChild(subSection))
		{
			data.SetChild(subSection, new SDFTreeNode());
		}
		return data.to(subSection);
	}
}
