using System.Collections.Generic;
using UnityEngine;

public class DailyRewardsImpl : MonoBehaviour
{
	private List<Transform> mLocators = new List<Transform>();

	private void Start()
	{
		int num = 1;
		while (true)
		{
			GameObject gameObject = base.gameObject.FindChild("Locator_Day" + num);
			if (gameObject == null)
			{
				break;
			}
			mLocators.Add(gameObject.transform);
			num++;
		}
		DrawCurrentState();
	}

	private void Update()
	{
	}

	private void DrawCurrentState()
	{
		int lastDailyRewardIndex = Singleton<Profile>.Instance.lastDailyRewardIndex;
		int num = 1;
		foreach (Transform mLocator in mLocators)
		{
			string empty = string.Empty;
			empty = ((num < lastDailyRewardIndex) ? "UI/Prefabs/Global/Widget_DailyReward_Completed" : ((num != lastDailyRewardIndex) ? "UI/Prefabs/Global/Widget_DailyReward_Locked" : "UI/Prefabs/Global/Widget_DailyReward_Unlocked"));
			if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
			{
				break;
			}
			DailyRewardSchema dayData = DataBundleRuntime.Instance.InitializeRecord<DailyRewardSchema>("DailyRewards", "Day_" + num);
			GameObject gameObject = Object.Instantiate(ResourceCache.GetCachedResource(empty, 1).Resource) as GameObject;
			gameObject.transform.parent = mLocator;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.FindChildComponent<GluiText>("SwapText_Day").Text = "Day " + num;
			SetupCard(gameObject, dayData, num);
			if (num == lastDailyRewardIndex)
			{
				BankInReward(dayData);
			}
			num++;
		}
	}

	private void SetupCard(GameObject card, DailyRewardSchema dayData, int dayIndex)
	{
		card.FindChildComponent<GluiText>("SwapText_Description").Text = dayData.num.ToString();
		switch (dayData.type)
		{
		case DailyRewardSchema.Type.Coins:
			if (dayIndex >= 2 && dayIndex <= 4)
			{
				card.FindChildComponent<GluiSprite>("SwapIcon_Present").Texture = ResourceCache.GetCachedResource(string.Format("UI/Textures/DynamicIcons/Misc/DailyReward_{0}", dayIndex), 1).Resource as Texture2D;
			}
			break;
		case DailyRewardSchema.Type.Gems:
			card.FindChildComponent<GluiSprite>("SwapIcon_Present").Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Currency_Hard_Temp", 1).Resource as Texture2D;
			break;
		case DailyRewardSchema.Type.Revives:
			card.FindChildComponent<GluiSprite>("SwapIcon_Present").Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Consumables/Consume_Revive", 1).Resource as Texture2D;
			break;
		}
	}

	private void BankInReward(DailyRewardSchema dayData)
	{
		switch (dayData.type)
		{
		case DailyRewardSchema.Type.Coins:
			CashIn.From("coins", dayData.num, "DailyRewards");
			break;
		case DailyRewardSchema.Type.Gems:
			CashIn.From("gems", dayData.num, "DailyRewards");
			break;
		case DailyRewardSchema.Type.Revives:
			CashIn.From("revivePotion", dayData.num, "DailyRewards");
			break;
		}
		Singleton<Profile>.Instance.dailyRewardRating += Singleton<Profile>.Instance.lastDailyRewardIndex * 10;
		Singleton<Profile>.Instance.Save();
	}
}
