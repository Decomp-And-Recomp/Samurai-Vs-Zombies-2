using System.Collections.Generic;

public class LootListController : GluiSimpleCollectionController
{
	public float kInitialPopDelay = 1f;

	public float kPopDelayBetweenItems = 0.3f;

	public static List<PlayStatistics.Data.LootEntry> allLoots
	{
		get
		{
			List<PlayStatistics.Data.LootEntry> loot = Singleton<PlayStatistics>.Instance.data.loot;
			loot.Sort(delegate(PlayStatistics.Data.LootEntry a, PlayStatistics.Data.LootEntry b)
			{
				if (a.presentType.HasValue)
				{
					return -1;
				}
				return b.presentType.HasValue ? 1 : 0;
			});
			return loot;
		}
	}

	public override string GetCellPrefabForDataIndex(int dataIndex)
	{
		PlayStatistics.Data.LootEntry lootEntry = (PlayStatistics.Data.LootEntry)mData[dataIndex];
		if (lootEntry.presentType.HasValue)
		{
			return "UI/Prefabs/Results/Card_Loot_Gift";
		}
		return cardPath;
	}

	public override void OnDrawn(GluiElement_Base elem, int dataIndex)
	{
		GluiElement_PresentOpener gluiElement_PresentOpener = elem as GluiElement_PresentOpener;
		if (gluiElement_PresentOpener != null)
		{
			gluiElement_PresentOpener.SetOpeningTimer(kInitialPopDelay + kPopDelayBetweenItems * (float)dataIndex);
		}
	}

	public override void ReloadData(object arg)
	{
		mData = allLoots.ToArray();
	}
}
