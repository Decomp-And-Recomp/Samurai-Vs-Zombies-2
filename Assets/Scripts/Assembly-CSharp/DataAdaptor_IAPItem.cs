using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_IAPItem : DataAdaptorBase
{
	public GameObject text_displayName;

	public GameObject text_price;

	public GameObject text_quantity;

	public GluiStandardButtonContainer button_icon;

	public GameObject sprite_icon;

	public GameObject root_sale;

	public GameObject text_salePrice;

	public GameObject root_bonus;

	public GluiText text_bonusPercent;

	public GameObject root_offerTimer;

	public GameObject text_offerTimer;

	public GameObject root_rays;

	private IAPSchema iap;

	private DateTime? iapExpireTime;

	public override void SetData(object data)
	{
		iap = data as IAPSchema;
		if (iap == null)
		{
			return;
		}
		bool flag = !string.IsNullOrEmpty(iap.items);
		if (iap.priceString == null && ApplicationUtilities.iapPrices.ContainsKey(iap.productId.ToLower()))
		{
			iap.priceString = (string)ApplicationUtilities.iapPrices[iap.productId.ToLower()];
		}
		if (AJavaTools.Properties.GetBuildType() == "tstore" && iap.priceString.Contains("???"))
		{
			iap.priceString = iap.priceString.Replace("???", string.Empty);
			iap.priceString += " Won";
		}
		SetGluiTextInChild(text_price, iap.priceString);
		SaleItemSchema saleData = SaleItemSchema.FindActiveSaleDataForItem(iap.referenceId);
		if (saleData != null)
		{
			if (!string.IsNullOrEmpty(saleData.replacementItem))
			{
				iap = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products.Find((IAPSchema s) => string.Equals(s.referenceId, saleData.replacementItem));
				if (iap == null)
				{
					return;
				}
			}
			iap.Sale = saleData;
			root_sale.SetActive(true);
			root_offerTimer.SetActive(true);
			SetGluiTextInChild(text_salePrice, iap.priceString);
			int num = (int)Mathf.Round((float)iap.Sale.SaleEvent.EndDate.Subtract(SntpTime.UniversalTime).TotalSeconds);
			SetGluiTextInChild(text_offerTimer, StringUtils.FormatTime(num, StringUtils.TimeFormatType.DaysOrHMS));
		}
		else
		{
			root_sale.SetActive(false);
			iap.Sale = null;
			DateTime iAPExpireTime = Singleton<Profile>.Instance.GetIAPExpireTime(iap);
			if (iAPExpireTime != DateTime.MaxValue)
			{
				root_offerTimer.SetActive(true);
				int num2 = (int)Mathf.Round((float)iAPExpireTime.Subtract(SntpTime.UniversalTime).TotalSeconds);
				SetGluiTextInChild(text_offerTimer, StringUtils.FormatTime(num2, StringUtils.TimeFormatType.DaysOrHMS));
				iapExpireTime = iAPExpireTime;
			}
			else
			{
				root_offerTimer.SetActive(false);
			}
		}
		SetGluiTextInChild(text_displayName, iap.displayedName);
		int num3 = ((iap.hardCurrencyAmount <= 0) ? iap.softCurrencyAmount : iap.hardCurrencyAmount);
		if (num3 > 0 && string.IsNullOrEmpty(iap.items))
		{
			text_quantity.SetActive(true);
			SetGluiTextInChild(text_quantity, StringUtils.FormatAmountString(num3));
		}
		else
		{
			text_quantity.SetActive(false);
		}
		if (iap.percentBonus > 0)
		{
			root_bonus.SetActive(true);
			text_bonusPercent.Text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "percent"), iap.percentBonus);
		}
		else
		{
			root_bonus.SetActive(false);
		}
		SetGluiSpriteInChild(sprite_icon, iap.icon);
		if (Singleton<Profile>.Instance.IsUniqueIAPItemAlreadyPurchased(iap))
		{
			button_icon.Locked = true;
			button_icon.Enabled = false;
			root_rays.SetActive(false);
		}
		else
		{
			button_icon.Locked = false;
			button_icon.Enabled = true;
			button_icon.GetActionData = () => iap;
			root_rays.SetActive(flag);
		}
		if (button_icon != null)
		{
			if (flag)
			{
				button_icon.onReleaseActions = new string[1] { "POPUP_CONFIRMPURCHASE_BUNDLE" };
			}
			else
			{
				button_icon.onReleaseActions = new string[1] { "IAP_PURCHASE_PRESS" };
			}
		}
	}

	public void UpdateSaleTime()
	{
		TimeSpan? timeSpan = null;
		if (iap.Sale != null)
		{
			timeSpan = iap.Sale.SaleEvent.EndDate.Subtract(SntpTime.UniversalTime);
		}
		else if (iapExpireTime.HasValue)
		{
			timeSpan = iapExpireTime.Value.Subtract(SntpTime.UniversalTime);
		}
		if (timeSpan.HasValue)
		{
			if (timeSpan.Value.TotalSeconds > 0.0)
			{
				int num = (int)Mathf.Round((float)timeSpan.Value.TotalSeconds);
				SetGluiTextInChild(text_offerTimer, StringUtils.FormatTime(num, StringUtils.TimeFormatType.DaysOrHMS));
			}
			else
			{
				root_sale.SetActive(false);
				root_offerTimer.SetActive(false);
				iap.Sale = null;
			}
		}
	}
}
