using System.Collections.Generic;
using UnityEngine;

public class EquipPageAllies : EquipPage, UIHandlerComponent
{
	private object[] mDataSet;

	private GluiBouncyScrollList mScrollListRef;

	private List<Transform> mSlotTransformsRef = new List<Transform>();

	private ListToSlotDragManager mListSlotManager;

	private bool mAlliesSelectionRequired;

	public EquipPageAllies(GameObject uiParent)
	{
		if (WeakGlobalInstance<EnemiesShowCase>.Instance != null)
		{
			WeakGlobalInstance<EnemiesShowCase>.Instance.highlight = false;
		}
		mScrollListRef = uiParent.FindChildComponent<GluiBouncyScrollList>("ScrollList_Allies_Available");
		HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID];
		AcquireSlotTransforms(uiParent, heroSchema.allySlots);
		mDataSet = uiParent.FindChildComponent<EquipListController>("ScrollList_Allies_Available").data;
		mListSlotManager = new ListToSlotDragManager(mSlotTransformsRef, mScrollListRef, mDataSet);
		mListSlotManager.onCheckAvailability = CheckIfAvailable;
		Load();
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep13_AllySelect");
		mListSlotManager.AutoSave = Save;
	}

	public void Update(bool updateExpensiveVisuals)
	{
		mListSlotManager.Update(updateExpensiveVisuals);
	}

	public bool OnUIEvent(string eventID)
	{
		if (mListSlotManager.OnUIEvent(eventID))
		{
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	public void Save()
	{
		List<int> selection = mListSlotManager.selection;
		List<string> list = new List<string>(selection.Count);
		foreach (int item in selection)
		{
			list.Add(((HelperSchema)mDataSet[item]).id);
		}
		Singleton<Profile>.Instance.SetSelectedHelpers(list);
	}

	public void Load()
	{
		List<string> list;
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			list = Singleton<Profile>.Instance.dailyChallengeHelpers;
			mAlliesSelectionRequired = true;
		}
		else if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 2 && Singleton<Profile>.Instance.GetWaveLevel(2) == 1 && !Singleton<Profile>.Instance.inMultiplayerWave)
		{
			list = new List<string>(new string[1] { "Farmer" });
			mAlliesSelectionRequired = true;
		}
		else
		{
			list = Singleton<Profile>.Instance.GetSelectedHelpers();
		}
		List<int> list2 = new List<int>(list.Count);
		foreach (string item in list)
		{
			for (int i = 0; i < mDataSet.Length; i++)
			{
				if (string.Compare(((HelperSchema)mDataSet[i]).id, item, true) != 0 || (!mAlliesSelectionRequired && !CheckIfAvailable(i)))
				{
					continue;
				}
				bool flag = false;
				foreach (int item2 in list2)
				{
					if (item2 == i)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.Add(i);
				}
				break;
			}
		}
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			string id = ((HelperSchema)mDataSet[num]).id;
			for (int j = 0; j < num; j++)
			{
				if (string.Compare(((HelperSchema)mDataSet[j]).id, id, true) == 0)
				{
					list2.RemoveAt(num);
					break;
				}
			}
		}
		mListSlotManager.selection = list2;
		mListSlotManager.selectionRequired = mAlliesSelectionRequired;
	}

	private void AcquireSlotTransforms(GameObject uiParent, int numSlots)
	{
		GameObject parent = uiParent.FindChild(string.Format("Selected_Allies_{0}", numSlots));
		for (int i = 1; i <= numSlots; i++)
		{
			string id = string.Format("Locator_{0:D2}", i);
			mSlotTransformsRef.Add(parent.FindChild(id).transform);
		}
	}

	private bool CheckIfAvailable(int index)
	{
		HelperSchema helperSchema = (HelperSchema)mDataSet[index];
		string text = helperSchema.requiredHero.Key.ToString();
		return !helperSchema.Locked && (string.IsNullOrEmpty(text) || text == Singleton<Profile>.Instance.heroID);
	}
}
