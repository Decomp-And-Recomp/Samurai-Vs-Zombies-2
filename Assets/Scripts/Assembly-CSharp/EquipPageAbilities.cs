using System.Collections.Generic;
using UnityEngine;

public class EquipPageAbilities : EquipPage, UIHandlerComponent
{
	private object[] mDataSet;

	private GluiBouncyScrollList mScrollListRef;

	private List<Transform> mSlotTransformsRef = new List<Transform>();

	private ListToSlotDragManager mListSlotManager;

	public EquipPageAbilities(GameObject uiParent)
	{
		if (WeakGlobalInstance<EnemiesShowCase>.Instance != null)
		{
			WeakGlobalInstance<EnemiesShowCase>.Instance.highlight = false;
		}
		mScrollListRef = uiParent.FindChildComponent<GluiBouncyScrollList>("ScrollList_Abilities_Available");
		AcquireSlotTransforms(uiParent, Singleton<Profile>.Instance.maxSelectedAbilities);
		mDataSet = uiParent.FindChildComponent<EquipListController>("ScrollList_Abilities_Available").data;
		mListSlotManager = new ListToSlotDragManager(mSlotTransformsRef, mScrollListRef, mDataSet);
		mListSlotManager.onCheckAvailability = CheckIfAvailable;
		Load();
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep14_AbilitySelect");
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
			list.Add(((AbilitySchema)mDataSet[item]).id);
		}
		Singleton<Profile>.Instance.SetSelectedAbilities(ReverseList(list));
	}

	public void Load()
	{
		List<string> original;
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			original = Singleton<Profile>.Instance.dailyChallengeAbilities;
			mListSlotManager.selectionRequired = true;
		}
		else
		{
			original = Singleton<Profile>.Instance.GetSelectedAbilities();
		}
		original = ReverseList(original);
		List<int> list = new List<int>(original.Count);
		foreach (string item in original)
		{
			for (int i = 0; i < mDataSet.Length; i++)
			{
				AbilitySchema abilitySchema = (AbilitySchema)mDataSet[i];
				if (string.Compare(abilitySchema.id, item, true) == 0)
				{
					if (Singleton<Profile>.Instance.inDailyChallenge || (float)Singleton<Profile>.Instance.highestUnlockedWave >= abilitySchema.levelToUnlock)
					{
						list.Add(i);
					}
					break;
				}
			}
		}
		mListSlotManager.selection = list;
	}

	private void AcquireSlotTransforms(GameObject uiParent, int numSlots)
	{
		GameObject parent = uiParent.FindChild(string.Format("Selected_Abilities_{0}", numSlots));
		for (int i = 1; i <= numSlots; i++)
		{
			string id = string.Format("Locator_{0:D2}", i);
			mSlotTransformsRef.Add(parent.FindChild(id).transform);
		}
	}

	private List<string> ReverseList(List<string> original)
	{
		List<string> list = new List<string>(original.Count);
		for (int num = original.Count - 1; num >= 0; num--)
		{
			list.Add(original[num]);
		}
		return list;
	}

	private bool CheckIfAvailable(int index)
	{
		AbilitySchema abilitySchema = (AbilitySchema)mDataSet[index];
		return !abilitySchema.EquipLocked;
	}
}
