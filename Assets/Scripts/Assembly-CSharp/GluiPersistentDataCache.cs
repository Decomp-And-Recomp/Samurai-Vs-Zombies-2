using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("Glui Data/Persistent Data Cache")]
public class GluiPersistentDataCache : SingletonSpawningMonoBehaviour<GluiPersistentDataCache>
{
	public class PersistentData
	{
		public string name;

		public object tag;

		public override string ToString()
		{
			if (tag == null)
			{
				return name + " = [null]";
			}
			if (tag.GetType() == typeof(string))
			{
				return name + " = \"" + tag.ToString() + "\"";
			}
			return name + " = [" + tag.ToString() + "]";
		}
	}

	public delegate void PersistentDataChangedHandler(PersistentData data);

	private List<PersistentData> nodes = new List<PersistentData>();

	public List<PersistentData> Nodes
	{
		get
		{
			return nodes;
		}
	}

	[method: MethodImpl(32)]
	public event PersistentDataChangedHandler DataChanged;

	public void Save(string name, object tag)
	{
		PersistentData persistentData = Find(name);
		if (persistentData == null)
		{
			persistentData = Add(name);
			persistentData.tag = tag;
			GluiPersistentDataLog.OnAdded(persistentData);
		}
		else
		{
			persistentData.tag = tag;
			GluiPersistentDataLog.OnUpdated(persistentData);
		}
		OnDataChanged(persistentData);
	}

	public void SaveIfChange(string name, object tag)
	{
		PersistentData persistentData = Find(name);
		if (persistentData == null)
		{
			Save(name, tag);
		}
		else if (persistentData.tag != tag)
		{
			Save(name, tag);
		}
	}

	public void Copy(string target, string source)
	{
		Save(target, GetData(source));
	}

	public void Remove(string name)
	{
		PersistentData persistentData = Find(name);
		if (persistentData != null)
		{
			nodes.Remove(persistentData);
			GluiPersistentDataLog.OnRemoved(name);
		}
	}

	public PersistentData Find(string name)
	{
		return nodes.Find((PersistentData thisNode) => thisNode.name == name);
	}

	public object GetData(string name)
	{
		if (name == string.Empty)
		{
			return null;
		}
		PersistentData persistentData = Find(name);
		if (persistentData != null)
		{
			return persistentData.tag;
		}
		return null;
	}

	private PersistentData Add(string name)
	{
		PersistentData persistentData = new PersistentData();
		persistentData.name = name;
		nodes.Add(persistentData);
		return persistentData;
	}

	protected void OnDataChanged(PersistentData data)
	{
		if (this.DataChanged != null)
		{
			this.DataChanged(data);
		}
	}
}
