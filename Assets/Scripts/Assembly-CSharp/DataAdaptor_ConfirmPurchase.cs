using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataAdaptor_ConfirmPurchase : DataAdaptorBase
{
	public object context;

	public GameObject root_purchaseUpgrades;

	public GameObject root_purchaseConsumables;

	public GameObject root_purchaseShield;

	public GluiStandardButtonContainer button_buy;

	public GluiStandardButtonContainer button_cancelPurchase;

	public GameObject upgradePriceLocator;

	public GameObject text_description;

	public GameObject text_name;

	public GameObject text_quantity;

	public GameObject sprite_icon;

	public GameObject sprite_goldStar;

	public GameObject root_saleBadge;

	public GameObject text_sale;

	public GameObject root_numericalStats;

	public GameObject text_numericalLevelCurrent;

	public GameObject text_numericalLevelNext;

	public GameObject root_numericalStatA;

	public GameObject root_numericalStatB;

	public GameObject sprite_numericalIconStatA;

	public GameObject sprite_numericalIconStatB;

	public GameObject text_numericalStatCurrentA;

	public GameObject text_numericalStatCurrentB;

	public GameObject text_numericalStatNextA;

	public GameObject text_numericalStatNextB;

	public GameObject root_textStats;

	public GameObject sprite_textIconStatA;

	public GameObject sprite_textIconStatB;

	public GameObject text_textLevelNextA;

	public GameObject text_textLevelNextB;

	public GameObject text_textStatNextA;

	public GameObject text_textStatNextB;

	public GluiStandardButtonContainer button_1;

	public GluiStandardButtonContainer button_5;

	public GluiStandardButtonContainer button_shield1;

	public GluiStandardButtonContainer button_shield3;

	public GluiStandardButtonContainer button_shield7;

	public Transform price_shield1;

	public Transform price_shield3;

	public Transform price_shield7;

	public GameObject root_buy1;

	public GameObject root_buy5;

	public GameObject root_sale_1;

	public GameObject root_sale_5;

	public GameObject text_amount1;

	public GameObject text_amount5;

	public GameObject text_sale1;

	public GameObject text_sale5;

	public GameObject root_bonus_5;

	public GameObject text_bonus_5;

	public GameObject consumablePriceLocator1;

	public GameObject consumablePriceLocator5;

	public GameObject sprite_icon1;

	public GameObject[] sprite_icon5;

	public GameObject PurchaseAlert;

	private StoreData.Item item;

	private SharedResourceLoader.SharedResource statIconRes1;

	private SharedResourceLoader.SharedResource statIconRes2;

	public override void SetData(object data)
	{
		context = data;
		PurchaseAlert.SetActive(false);
		if (!(data is StoreData.Item))
		{
			return;
		}
		item = data as StoreData.Item;
		button_cancelPurchase.GetActionData = () => context;
		if (SingletonMonoBehaviour<StoreMenuImpl>.Exists && item.isNew)
		{
			SingletonMonoBehaviour<StoreMenuImpl>.Instance.SetHasViewedNewItem(item.id);
		}
		if (!item.locked)
		{
			SetGluiTextInChild(text_name, item.title);
			SetGluiTextInChild(text_description, item.details.Description);
		}
		else
		{
			SetGluiTextInChild(text_name, item.unlockTitle);
			SetGluiTextInChild(text_description, item.unlockCondition);
		}
		if (sprite_goldStar != null)
		{
			if (Singleton<HelpersDatabase>.Instance.Contains(item.id) && Singleton<Profile>.Instance.GetGoldenHelperUnlocked(item.id))
			{
				sprite_goldStar.SetActive(true);
				sprite_goldStar.GetComponent<GluiSprite>().Texture = Singleton<HelpersDatabase>.Instance[item.id].TryGetChampionIcon();
			}
			else
			{
				sprite_goldStar.SetActive(false);
			}
		}
		SetGluiSpriteInChild(sprite_icon, item.icon);
		if (item.id == "mpshield")
		{
			root_purchaseShield.SetActive(true);
			root_purchaseConsumables.SetActive(false);
			root_purchaseUpgrades.SetActive(false);
			button_shield1.GetActionData = () => context;
			button_shield3.GetActionData = () => context;
			button_shield7.GetActionData = () => context;
			SpawnPriceWidget(price_shield1.gameObject, item.cost);
			SpawnPriceWidget(price_shield3.gameObject, item.packCost);
			SpawnPriceWidget(price_shield7.gameObject, item.cost3);
			TimeSpan timeSpan = Singleton<Profile>.Instance.MultiplayerShieldExpireTime - SntpTime.UniversalTime;
			int num = Mathf.Max(0, (int)timeSpan.TotalDays);
			int num2 = Mathf.Max(0, (int)timeSpan.TotalHours);
			num2 %= 24;
			string text = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings.mpShieldTime"), num, num2);
			SetGluiTextInChild(text_quantity, text);
		}
		else if (!item.isConsumable)
		{
			button_buy.GetActionData = () => context;
			root_purchaseConsumables.SetActive(false);
			root_purchaseShield.SetActive(false);
			root_saleBadge.SetActive(false);
			bool flag = false;
			foreach (KeyValuePair<string, string[]> stat in item.details.Stats)
			{
				if (stat.Value.Length == 2 && stat.Value[0] != stat.Value[1] && !string.IsNullOrEmpty(stat.Value[0]))
				{
					flag = true;
					break;
				}
			}
			if (!item.locked && !item.maxlevel)
			{
				SpawnPriceWidget(upgradePriceLocator, item.cost);
			}
			else
			{
				button_buy.gameObject.SetActive(false);
			}
			if (item.details.MaxLevel.HasValue && (item.details.LevelA != item.details.LevelB || item.maxlevel) && !item.locked)
			{
				SetGluiTextFormatInChild(text_quantity, item.details.LevelA, item.details.MaxLevel.Value);
			}
			else
			{
				text_quantity.SetActive(false);
			}
			if (!flag || item.locked || item.maxlevel)
			{
				root_textStats.SetActive(false);
				root_numericalStats.SetActive(false);
			}
			else
			{
				root_textStats.SetActive(item.details.UpgradeDisplayAsText);
				root_numericalStats.SetActive(!item.details.UpgradeDisplayAsText);
				if (!item.details.UpgradeDisplayAsText)
				{
					if (item.details.Stats[0].Key == "icon")
					{
						SetGluiSpriteInChild(sprite_numericalIconStatA, item.icon);
					}
					else
					{
						string value = DataBundleRuntime.Instance.GetValue<string>(typeof(IconSchema), "StatIcons", item.details.Stats[0].Key, "icon", true);
						statIconRes1 = SharedResourceLoader.LoadAsset(value);
						if (statIconRes1.Resource != null)
						{
							SetGluiSpriteInChild(sprite_numericalIconStatA, statIconRes1.Resource as Texture2D);
						}
					}
					SetGluiTextInChild(text_numericalStatCurrentA, item.details.Stats[0].Value[0]);
					SetGluiTextInChild(text_numericalStatNextA, item.details.Stats[0].Value[1]);
					if (item.details.Stats.Count > 1 && item.details.Stats[1].Value[0] != item.details.Stats[1].Value[1] && !item.maxlevel)
					{
						string value2 = DataBundleRuntime.Instance.GetValue<string>(typeof(IconSchema), "StatIcons", item.details.Stats[1].Key, "icon", true);
						statIconRes2 = SharedResourceLoader.LoadAsset(value2);
						if (statIconRes2.Resource != null)
						{
							SetGluiSpriteInChild(sprite_numericalIconStatB, statIconRes2.Resource as Texture2D);
						}
						SetGluiTextInChild(text_numericalStatCurrentB, item.details.Stats[1].Value[0]);
						SetGluiTextInChild(text_numericalStatNextB, item.details.Stats[1].Value[1]);
					}
					else
					{
						root_numericalStatB.SetActive(false);
					}
				}
				else
				{
					string value3 = DataBundleRuntime.Instance.GetValue<string>(typeof(IconSchema), "StatIcons", item.details.Stats[0].Key, "icon", true);
					statIconRes1 = SharedResourceLoader.LoadAsset(value3);
					if (statIconRes1.Resource != null)
					{
						SetGluiSpriteInChild(sprite_textIconStatA, statIconRes1.Resource as Texture2D);
						SetGluiSpriteInChild(sprite_textIconStatB, statIconRes1.Resource as Texture2D);
					}
					SetGluiTextInChild(text_textStatNextA, item.details.Stats[0].Value[0]);
					SetGluiTextInChild(text_textStatNextB, item.details.Stats[0].Value[1]);
				}
			}
		}
		else
		{
			root_purchaseUpgrades.SetActive(false);
			root_purchaseShield.SetActive(false);
			if (item.details.MaxLevel.HasValue)
			{
				SetGluiTextInChild(text_quantity, string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_QtyOfQtyName"), item.details.Count.Value, item.details.MaxLevel.Value, item.details.Name));
			}
			else if (item.details.Count.HasValue && !string.IsNullOrEmpty(item.details.Name))
			{
				string text2 = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings.Menu_Owned"), item.details.Count.Value);
				SetGluiTextInChild(text_quantity, text2);
			}
			else
			{
				SetGluiTextInChild(text_quantity, string.Empty);
			}
			SetGluiTextFormatInChild(text_amount1, item.amount, item.details.Name);
			if (item.id.Equals("souls"))
			{
				SetGluiTextInChild(text_amount5, StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_FillSoulJar"));
				if (item.details.Count.Value == item.details.MaxLevel.Value)
				{
					root_buy1.SetActive(false);
					root_buy5.SetActive(false);
					PurchaseAlert.SetActive(true);
					return;
				}
			}
			else
			{
				SetGluiTextFormatInChild(text_amount5, item.packAmount, item.details.Name);
			}
			button_1.GetActionData = () => context;
			button_5.GetActionData = () => context;
			root_sale_1.SetActive(false);
			root_sale_5.SetActive(false);
			if (item.id != "souls" && item.packShowBonus)
			{
				root_bonus_5.SetActive(true);
				SetGluiTextFormatInChild(text_bonus_5, item.packDiscount.ToString());
			}
			else
			{
				root_bonus_5.SetActive(false);
			}
			SpawnPriceWidget(consumablePriceLocator1, item.cost);
			SpawnPriceWidget(consumablePriceLocator5, item.packCost);
			SetGluiSpriteInChild(sprite_icon1, item.icon);
			GameObject[] array = sprite_icon5;
			foreach (GameObject child in array)
			{
				SetGluiSpriteInChild(child, item.icon);
			}
		}
		if (item.id == "mysterybox")
		{
			string text3 = ((item.analyticsParams == null || !item.analyticsParams.ContainsKey("ItemName")) ? item.id : item.analyticsParams["ItemName"].ToString());
			if (text3.Contains("Hero"))
			{
				ApplicationUtilities.MakePlayHavenContentRequest("mystery_box_opened");
			}
		}
	}

	private void SpawnPriceWidget(GameObject locator, Cost cost)
	{
		if (locator != null)
		{
			WidgetPriceSpawner component = locator.GetComponent<WidgetPriceSpawner>();
			if (component != null)
			{
				component.SetCost(cost);
			}
		}
	}
}
