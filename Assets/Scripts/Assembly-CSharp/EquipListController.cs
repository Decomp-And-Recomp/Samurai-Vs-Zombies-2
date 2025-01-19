using System;
using System.Collections.Generic;

public class EquipListController : GluiSimpleCollectionController
{
	public enum DataType
	{
		Helpers = 0,
		Abilities = 1,
		Charms = 2
	}

	private int mFirstToShow;

	public DataType dataType;

	public override int initialIndexToShow
	{
		get
		{
			return mFirstToShow;
		}
	}

	public override void ReloadData(object arg)
	{
		switch (dataType)
		{
		case DataType.Helpers:
			mData = GetHelpers();
			break;
		case DataType.Abilities:
			mData = GetAbilities();
			break;
		case DataType.Charms:
			mData = GetCharms();
			break;
		default:
			mData = new object[0];
			break;
		}
	}

	private object[] GetHelpers()
	{
		HelperSchema[] allHelpers = Singleton<HelpersDatabase>.Instance.AllHelpers;
		List<HelperSchema> list = new List<HelperSchema>();
		HelperSchema[] array = allHelpers;
		foreach (HelperSchema helperSchema in array)
		{
			string text = helperSchema.requiredHero.Key.ToString();
			if (!helperSchema.hideInEquip && (string.IsNullOrEmpty(text) || text == Singleton<Profile>.Instance.heroID))
			{
				list.Add(helperSchema);
			}
		}
		list.Sort(delegate(HelperSchema a, HelperSchema b)
		{
			if (a.Locked == b.Locked)
			{
				if (a.waveToUnlock == b.waveToUnlock)
				{
					return a.availableAtWave - b.availableAtWave;
				}
				return a.waveToUnlock - b.waveToUnlock;
			}
			return a.Locked ? 1 : (-1);
		});
		mFirstToShow = 0;
		foreach (HelperSchema item in list)
		{
			if (item.Locked)
			{
				mFirstToShow--;
				break;
			}
			mFirstToShow++;
		}
		if (mFirstToShow >= list.Count || mFirstToShow < 0)
		{
			mFirstToShow = 0;
		}
		return list.ToArray();
	}

	private object[] GetAbilities()
	{
		AbilitySchema[] abRef = Singleton<AbilitiesDatabase>.Instance.AllAbilitiesForActiveHero;
		AbilitySchema[] array = new AbilitySchema[abRef.Length];
		Array.Copy(abRef, array, abRef.Length);
		Array.Sort(array, delegate(AbilitySchema a, AbilitySchema b)
		{
			if (a.EquipLocked == b.EquipLocked)
			{
				if (a.EquipLocked)
				{
					return (int)(a.levelToUnlock - b.levelToUnlock);
				}
				if (!string.IsNullOrEmpty(a.exclusiveHero) && string.IsNullOrEmpty(b.exclusiveHero))
				{
					return -1;
				}
				if (!string.IsNullOrEmpty(b.exclusiveHero) && string.IsNullOrEmpty(a.exclusiveHero))
				{
					return 1;
				}
				return Array.IndexOf(abRef, a) - Array.IndexOf(abRef, b);
			}
			return a.EquipLocked ? 1 : (-1);
		});
		mFirstToShow = 0;
		AbilitySchema[] array2 = array;
		foreach (AbilitySchema abilitySchema in array2)
		{
			if (abilitySchema.EquipLocked)
			{
				mFirstToShow--;
				break;
			}
			mFirstToShow++;
		}
		if (mFirstToShow >= array.Length || mFirstToShow < 0)
		{
			mFirstToShow = 0;
		}
		return array;
	}

	private object[] GetCharms()
	{
		List<CharmSchema> list = new List<CharmSchema>(Singleton<CharmsDatabase>.Instance.AllPlayerAvailableCharms);
		list.RemoveAll((CharmSchema c) => c.hideInEquipMenu);
		List<CharmSchema> list2 = new List<CharmSchema>();
		foreach (CharmSchema item in list)
		{
			int i = 0;
			for (Cost cost = new Cost(item.cost, 0f); i < list2.Count && new Cost(list2[i].cost, 0f).price > cost.price; i++)
			{
			}
			list2.Insert(i, item);
		}
		mFirstToShow = 0;
		foreach (CharmSchema item2 in list2)
		{
			if (Singleton<Profile>.Instance.GetNumCharms(item2.id) > 0)
			{
				break;
			}
			mFirstToShow++;
		}
		if (mFirstToShow >= list2.Count)
		{
			mFirstToShow = 0;
		}
		return list2.ToArray();
	}
}
