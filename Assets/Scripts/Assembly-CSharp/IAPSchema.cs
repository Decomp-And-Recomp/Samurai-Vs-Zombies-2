using System;
using UnityEngine;

[DataBundleClass]
public class IAPSchema
{
	[DataBundleKey(ColumnWidth = 250)]
	public string referenceId;

	[DataBundleField(ColumnWidth = 250)]
	public string productId;

	public string priceString;

	public string displayedName;

	public bool unique;

	[DataBundleField(ColumnWidth = 200)]
	public string description;

	[DataBundleField(ColumnWidth = 150)]
	public int softCurrencyAmount;

	[DataBundleField(ColumnWidth = 150)]
	public int hardCurrencyAmount;

	public int percentBonus;

	[DataBundleField(ColumnWidth = 350)]
	public string items;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.All)]
	public Texture2D icon;

	public bool hidden;

	public int hoursToExpire;

	private double _priceInDollars;

	public float PriceInDollars { get; private set; }

	public int PriceInCents
	{
		get
		{
			return (int)(_priceInDollars * 1000.0) / 10;
		}
	}

	public SaleItemSchema Sale { get; set; }

	public void SyncWith(CInAppPurchaseProduct product)
	{
		PriceInDollars = product.GetPrice();
		description = product.GetDescription();
		productId = product.GetProductIdentifier();
		priceString = StringUtils.FormatPriceString(PriceInDollars);
		displayedName = product.GetTitle();
	}

	public void SyncAndroid(string price)
	{
		priceString = price;
	}

	public void Initialize(string tableName)
	{
		string text = referenceId;
		text = ((!referenceId.Contains(".sub.")) ? referenceId : referenceId.Remove(referenceId.IndexOf(".sub.")).ToUpper());
		string value = DataBundleRuntime.Instance.GetValue<string>(typeof(IAPSchema), tableName, text, "icon", true);
		_priceInDollars = double.Parse(DataBundleRuntime.Instance.GetValue<string>(typeof(IAPSchema), "IAPTable", text, "priceString", false));
		PriceInDollars = (float)_priceInDollars;
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(value, 1);
		if (cachedResource != null && !object.ReferenceEquals(cachedResource.Resource, null))
		{
			icon = cachedResource.Resource as Texture2D;
		}
	}

	public void updateWithIapRecommendation(GWIAPRecommendation_Unity item)
	{
		referenceId = item.m_itemName;
		string text = ((!item.m_itemName.Contains("GEM")) ? "Coins" : "Glu Credits");
		productId = item.m_storeSkuCode;
		hidden = false;
		description = "Purchase " + item.m_currencyValue + ((!item.m_itemName.Contains("gem")) ? " Coins" : " Glu Credits");
		if (text == "Glu Credits")
		{
			hardCurrencyAmount = Convert.ToInt32(item.m_displayUrl);
		}
		else
		{
			softCurrencyAmount = Convert.ToInt32(item.m_displayUrl);
		}
	}

	public void updateWithSubscriptionRecommendation(GWSubscriptionRecommendation_Unity sub, int amount)
	{
		string text = ((!sub.m_storeSkuCode.Contains("gem")) ? "Coins" : "Glu Credits");
		productId = sub.m_storeSkuCode;
		referenceId = "SAMUZOMBIE2 " + sub.m_storeSkuCode.Substring(20);
		description = "Purchase " + amount + " " + text + ".";
		hidden = false;
		percentBonus = 50;
		int num = amount / 2;
		displayedName = amount + " " + text + " plus " + num + " FREE";
		if (text == "Glu Credits")
		{
			hardCurrencyAmount = amount + num;
		}
		else
		{
			softCurrencyAmount = amount + num;
		}
	}
}
