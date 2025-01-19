using System.Collections.Generic;

public class MultipleMysteryBoxContents : WeakGlobalInstance<MultipleMysteryBoxContents>
{
	public bool initialReviveGiven;

	public List<string> mGoldHelpers = new List<string>();

	public MultipleMysteryBoxContents()
	{
		SetUniqueInstance(this);
	}

	public void RegisterGoldenHelper(string id)
	{
		if (!mGoldHelpers.Contains(id))
		{
			mGoldHelpers.Add(id);
		}
	}

	public bool ContainsHelper(string id)
	{
		return mGoldHelpers.Contains(id);
	}
}
