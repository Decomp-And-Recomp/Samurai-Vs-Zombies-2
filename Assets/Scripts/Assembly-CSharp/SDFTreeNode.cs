using System.Collections.Generic;

public class SDFTreeNode
{
	protected Dictionary<string, SDFTreeNode> mChilds = new Dictionary<string, SDFTreeNode>();

	protected Dictionary<string, string> mAttributes = new Dictionary<string, string>();

	protected List<string> mFileLinks = new List<string>();

	public string this[string attribName]
	{
		get
		{
			try
			{
				int num = attribName.IndexOf('/');
				if (num < 0)
				{
					return mAttributes[key(attribName)];
				}
				string path = attribName.Substring(0, num);
				string attribName2 = attribName.Substring(num + 1);
				SDFTreeNode sDFTreeNode = to(path);
				if (sDFTreeNode != null)
				{
					return sDFTreeNode[attribName2];
				}
			}
			catch
			{
			}
			return string.Empty;
		}
		set
		{
			int num = attribName.IndexOf('/');
			if (num >= 0)
			{
				string path = attribName.Substring(0, num);
				string attribName2 = attribName.Substring(num + 1);
				SDFTreeNode sDFTreeNode = to(path);
				if (sDFTreeNode == null)
				{
					sDFTreeNode = new SDFTreeNode();
					mChilds.Add(path, sDFTreeNode);
				}
				sDFTreeNode[attribName2] = value;
			}
			else
			{
				attribName = key(attribName);
				if (mAttributes.ContainsKey(attribName))
				{
					mAttributes[attribName] = value;
				}
				else
				{
					mAttributes.Add(attribName, value);
				}
			}
		}
	}

	public string this[int listIndex]
	{
		get
		{
			return this[string.Format("{0:000}", listIndex)];
		}
		set
		{
			this[string.Format("{0:000}", listIndex)] = value;
		}
	}

	public int attributeCount
	{
		get
		{
			return mAttributes.Count;
		}
	}

	public int childCount
	{
		get
		{
			return mChilds.Count;
		}
	}

	public SDFChildEnumerator childs
	{
		get
		{
			return new SDFChildEnumerator(mChilds);
		}
	}

	public SDFAttributesEnumerator attributes
	{
		get
		{
			return new SDFAttributesEnumerator(mAttributes);
		}
	}

	private static string key(string k)
	{
		return k.ToLower();
	}

	public bool hasChild(string childName)
	{
		return mChilds.ContainsKey(key(childName));
	}

	public bool hasChild(int val)
	{
		return hasChild(string.Format("{0:000}", val));
	}

	public bool hasAttribute(string attributeName)
	{
		return mAttributes.ContainsKey(key(attributeName));
	}

	public bool hasAttribute(int listIndex)
	{
		return hasAttribute(string.Format("{0:000}", listIndex));
	}

	public SDFTreeNode to(int index)
	{
		return to(string.Format("{0:000}", index));
	}

	public SDFTreeNode to(string path)
	{
		if (path == null)
		{
			return null;
		}
		path = key(path.Trim());
		if (path.Length == 0)
		{
			return null;
		}
		return to(new List<string>(path.Split('/')), 0);
	}

	public void SetChild(string childName, SDFTreeNode node)
	{
		childName = key(childName);
		if (mChilds.ContainsKey(childName))
		{
			mChilds[childName] = node;
		}
		else
		{
			mChilds.Add(childName, node);
		}
	}

	public void ClearAttributes()
	{
		mAttributes.Clear();
	}

	public void ClearChilds()
	{
		mChilds.Clear();
	}

	public void AddFileLink(string fileLink)
	{
		if (!(fileLink == string.Empty))
		{
			mFileLinks.Add(fileLink);
		}
	}

	public void MergeFrom(SDFTreeNode node)
	{
		foreach (KeyValuePair<string, SDFTreeNode> child in node.childs)
		{
			SetChild(child.Key, child.Value);
		}
		foreach (KeyValuePair<string, string> attribute in node.attributes)
		{
			this[attribute.Key] = attribute.Value;
		}
	}

	public void ExpandLinks()
	{
		if (mFileLinks.Count == 0)
		{
			return;
		}
		foreach (string mFileLink in mFileLinks)
		{
			SDFTreeNode sDFTreeNode = SDFTree.LoadFromResources(mFileLink);
			if (sDFTreeNode != null)
			{
				MergeFrom(sDFTreeNode);
			}
		}
		mFileLinks.Clear();
	}

	public SDFTreeNode Clone()
	{
		SDFTreeNode sDFTreeNode = new SDFTreeNode();
		foreach (KeyValuePair<string, string> mAttribute in mAttributes)
		{
			sDFTreeNode.mAttributes.Add(mAttribute.Key, mAttribute.Value);
		}
		foreach (string mFileLink in mFileLinks)
		{
			sDFTreeNode.mFileLinks.Add(mFileLink);
		}
		foreach (KeyValuePair<string, SDFTreeNode> mChild in mChilds)
		{
			sDFTreeNode.mChilds.Add(mChild.Key, mChild.Value.Clone());
		}
		return sDFTreeNode;
	}

	private SDFTreeNode to(List<string> path, int level)
	{
		ExpandLinks();
		if (level == path.Count)
		{
			return this;
		}
		SDFTreeNode sDFTreeNode = null;
		try
		{
			sDFTreeNode = mChilds[path[level]];
		}
		catch
		{
		}
		if (sDFTreeNode == null)
		{
			return null;
		}
		return sDFTreeNode.to(path, level + 1);
	}
}
