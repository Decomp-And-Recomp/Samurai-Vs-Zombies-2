using System.Collections.Generic;
using UnityEngine;

public class EquipPageCharms : EquipPage, UIHandlerComponent
{
	private object[] mDataSet;

	private GluiBouncyScrollList mScrollListRef;

	private List<Transform> mSlotTransformsRef = new List<Transform>();

	private ListToSlotDragManager mListSlotManager;

	public EquipPageCharms(GameObject uiParent)
	{
		if (WeakGlobalInstance<EnemiesShowCase>.Instance != null)
		{
			WeakGlobalInstance<EnemiesShowCase>.Instance.highlight = false;
		}
		mScrollListRef = uiParent.FindChildComponent<GluiBouncyScrollList>("ScrollList_Charms_Available");
		AcquireSlotTransforms(uiParent, 1);
		mDataSet = uiParent.FindChildComponent<EquipListController>("ScrollList_Charms_Available").data;
		mListSlotManager = new ListToSlotDragManager(mSlotTransformsRef, mScrollListRef, mDataSet);
		mListSlotManager.onCheckAvailability = CheckIfAvailable;
		Load();
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep15_CharmSelect");
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
		if (selection.Count == 1)
		{
			Singleton<Profile>.Instance.selectedCharm = ((CharmSchema)mDataSet[selection[0]]).id;
		}
		else
		{
			Singleton<Profile>.Instance.selectedCharm = string.Empty;
		}
	}

	public void Load()
	{
		string selectedCharm = Singleton<Profile>.Instance.selectedCharm;
		if (!(selectedCharm != string.Empty))
		{
			return;
		}
		List<int> list = new List<int>(1);
		for (int i = 0; i < mDataSet.Length; i++)
		{
			if (string.Compare(((CharmSchema)mDataSet[i]).id, selectedCharm, true) == 0)
			{
				list.Add(i);
				break;
			}
		}
		mListSlotManager.selection = list;
	}

	private void AcquireSlotTransforms(GameObject uiParent, int numSlots)
	{
		GameObject parent = uiParent.FindChild(string.Format("Selected_Charms_{0}", numSlots));
		for (int i = 1; i <= numSlots; i++)
		{
			string id = string.Format("Locator_{0:D2}", i);
			mSlotTransformsRef.Add(parent.FindChild(id).transform);
		}
	}

	private bool CheckIfAvailable(int index)
	{
		string id = ((CharmSchema)mDataSet[index]).id;
		return Singleton<Profile>.Instance.GetNumCharms(id) > 0;
	}
}
