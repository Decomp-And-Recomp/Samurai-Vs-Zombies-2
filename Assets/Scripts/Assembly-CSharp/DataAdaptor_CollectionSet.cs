using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_CollectionSet : DataAdaptorBase
{
	public GameObject list_items;

	public GameObject reward;

	public GameObject text_displayName;

	public GameObject text_currentLevelToCollect;

	public GameObject text_timesCollected;

	public GameObject sprite_CurrentReward;

	public GameObject text_CurrentReward;

	public GameObject text_dummyRewardAmount;

	public bool isRewardScreen;

	public bool useItemList = true;

	public DataAdaptor_CollectionItem item1;

	public DataAdaptor_CollectionItem item2;

	public DataAdaptor_CollectionItem item3;

	public DataAdaptor_CollectionItem item4;

	public DataAdaptor_CollectionItem item5;

	public override void SetData(object data)
	{
		CollectionSchema collectionSchema = (CollectionSchema)data;
		if (collectionSchema == null)
		{
			return;
		}
		int num = Singleton<Profile>.Instance.MultiplayerData.TotalTimesCompletedSet(collectionSchema.id);
		if (isRewardScreen && num <= 3)
		{
			num = Mathf.Max(0, num - 1);
		}
		SetGluiTextInChild(text_displayName, StringUtils.GetStringFromStringRef(collectionSchema.displayName));
		if (text_dummyRewardAmount != null)
		{
			text_dummyRewardAmount.SetActive(false);
		}
		if (num >= 3)
		{
			CollectionDummyRewardsSchema collectionDummyRewardsSchema = ((!isRewardScreen) ? null : GluiElement_CollectionSet.LastDummyAwarded);
			if (collectionDummyRewardsSchema == null && Singleton<Profile>.Instance.MultiplayerData != null)
			{
				collectionDummyRewardsSchema = Singleton<Profile>.Instance.MultiplayerData.GetDummyReward(collectionSchema.dummyRewards, collectionSchema.id);
			}
			if (collectionDummyRewardsSchema != null)
			{
				SetGluiSpriteInChild(sprite_CurrentReward, collectionDummyRewardsSchema.rewardIcon);
				SetGluiTextInChild(text_CurrentReward, StringUtils.GetStringFromStringRef(collectionDummyRewardsSchema.displayName));
				if (text_dummyRewardAmount != null && !collectionDummyRewardsSchema.id.Contains("Tea") && !collectionDummyRewardsSchema.id.Contains("Sushi"))
				{
					SetGluiTextInChild(text_dummyRewardAmount, collectionDummyRewardsSchema.rewardAmount.ToString());
					text_dummyRewardAmount.SetActive(true);
				}
			}
		}
		else
		{
			string rewardText;
			SetGluiSpriteInChild(sprite_CurrentReward, collectionSchema.GetRewardIcon(num, out rewardText));
			SetGluiTextInChild(text_CurrentReward, rewardText);
		}
		if (text_timesCollected != null)
		{
			SetGluiTextFormatInChild(text_timesCollected, num.ToString());
		}
		if (collectionSchema.Items.GetLength(0) >= 5)
		{
			if (useItemList)
			{
				SetGluiListDataInChild(list_items, collectionSchema.Items);
				return;
			}
			item1.SetData(collectionSchema.Items[0]);
			item2.SetData(collectionSchema.Items[1]);
			item3.SetData(collectionSchema.Items[2]);
			item4.SetData(collectionSchema.Items[3]);
			item5.SetData(collectionSchema.Items[4]);
		}
	}
}
