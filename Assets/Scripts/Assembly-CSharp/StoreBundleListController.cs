using System.Collections.Generic;

public class StoreBundleListController : GluiSimpleCollectionController
{
	public override void ReloadData(object arg)
	{
		string[] array = null;
		if (arg is IAPSchema)
		{
			array = ((IAPSchema)arg).items.Split(',');
		}
		else
		{
			if (!(arg is StoreData.Item))
			{
				mData = new object[0];
				return;
			}
			array = ((StoreData.Item)arg).bundleContent.ToArray();
		}
		List<PlayStatistics.Data.LootEntry> list = new List<PlayStatistics.Data.LootEntry>(array.Length);
		string[] array2 = array;
		foreach (string id in array2)
		{
			PlayStatistics.Data.LootEntry lootEntry = new PlayStatistics.Data.LootEntry();
			lootEntry.id = id;
			lootEntry.num = 1;
			list.Add(lootEntry);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string key = CashIn.StandardizeItemID(list[num].id).Key;
			if (Singleton<HeroesDatabase>.Instance.Contains(key))
			{
				string strB = key + ".level";
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					string key2 = CashIn.StandardizeItemID(list[num2].id).Key;
					if (string.Compare(key2, strB, true) == 0)
					{
						list.RemoveAt(num);
						break;
					}
				}
			}
		}
		mData = list.ToArray();
	}
}
