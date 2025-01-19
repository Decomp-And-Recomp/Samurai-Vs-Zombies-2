using Glu.Plugins.ASocial;

public class MultiplayerOpponentListController : GluiSimpleCollectionController
{
	public new MultiplayerOpponentsData dataSource;

	public string dataFilter;

	public override int dataCount
	{
		get
		{
			if (mData != null)
			{
				if (Glu.Plugins.ASocial.Facebook.IsLoggedIn())
				{
					return mData.Length + 1;
				}
				return mData.Length;
			}
			return 0;
		}
	}

	public override void ReloadData(object arg)
	{
		dataSource.Get_GluiData(dataFilter, null, null, out mData);
	}

	public override object GetDataAtIndex(int index)
	{
		if (index < mData.Length)
		{
			return mData[index];
		}
		return null;
	}

	public override string GetCellPrefabForDataIndex(int dataIndex)
	{
		if (dataIndex < mData.Length)
		{
			return cardPath;
		}
		return "UI/Prefabs/Multiplayer/Card_Invite";
	}
}
