public class Localizer : Singleton<Localizer>
{
	private SDFTreeNode mData;

	public bool Has(string id)
	{
		CacheData();
		return mData.hasAttribute(id);
	}

	public string Get(string id)
	{
		CacheData();
		if (mData.hasAttribute(id))
		{
			return mData[id];
		}
		return "*MISSING*";
	}

	public string Parse(string source)
	{
		if (source.Length > 0 && source[0] == '@')
		{
			return Get(source.Substring(1));
		}
		return source;
	}

	private void CacheData()
	{
		if (mData == null)
		{
			mData = SDFTree.LoadFromResources("Text/en");
		}
	}
}
