using System.Collections.Generic;

public class MultiplayerCollectionListController : GluiSimpleCollectionController
{
	public override void ReloadData(object arg)
	{
		if (!Singleton<Profile>.Exists || Singleton<Profile>.Instance.MultiplayerData == null)
		{
			mData = new object[0];
			return;
		}
		List<CollectionSchema> list = new List<CollectionSchema>(Singleton<Profile>.Instance.MultiplayerData.CollectionData);
		list.RemoveAll((CollectionSchema cs) => cs.disabled);
		list.Sort(SortCollectionList);
		mData = list.ToArray();
	}

	private static int SortCollectionList(CollectionSchema a, CollectionSchema b)
	{
		int num = 0;
		CollectionItemSchema[] items = a.Items;
		foreach (CollectionItemSchema collectionItemSchema in items)
		{
			num += ((collectionItemSchema != null) ? collectionItemSchema.soulsToAttack : 0);
		}
		int num2 = 0;
		CollectionItemSchema[] items2 = b.Items;
		foreach (CollectionItemSchema collectionItemSchema2 in items2)
		{
			num2 += ((collectionItemSchema2 != null) ? collectionItemSchema2.soulsToAttack : 0);
		}
		return num.CompareTo(num2);
	}
}
