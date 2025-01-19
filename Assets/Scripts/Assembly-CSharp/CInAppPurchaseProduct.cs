using System;

public class CInAppPurchaseProduct
{
	private string m_Param;

	private string m_Description;

	private string m_Title;

	private string m_ProductIdentifier;

	private float m_fPrice;

	private string m_szPrice;

	private string m_CurrencySymbol;

	private string m_LocaleIdentifier;

	public CInAppPurchaseProduct(string param)
	{
		if (CStringUtils.IsIAPXmlFormat(param))
		{
			ExtractData(param);
		}
		else
		{
			m_ProductIdentifier = param;
		}
	}

	private CInAppPurchaseProduct()
	{
	}

	public string GetDescription()
	{
		return m_Description;
	}

	public string GetTitle()
	{
		return m_Title;
	}

	public string GetProductIdentifier()
	{
		return m_ProductIdentifier;
	}

	public float GetPrice()
	{
		return m_fPrice;
	}

	public string GetPriceAsString()
	{
		return m_szPrice;
	}

	public string GetCurrencySymbol()
	{
		return m_CurrencySymbol;
	}

	public string GetLocaleIdentifier()
	{
		return m_LocaleIdentifier;
	}

	private void ExtractData(string param)
	{
		m_Param = param;
		m_Description = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Description");
		m_Title = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Title");
		m_ProductIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "ProductIdentifier");
		string text = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Price");
		m_fPrice = 0f;
		if (text != string.Empty)
		{
			m_fPrice = Convert.ToSingle(text);
		}
		m_szPrice = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "PriceFormatted");
		m_CurrencySymbol = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "CurrencySymbol");
		m_LocaleIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "LocaleIdentifier");
	}
}
