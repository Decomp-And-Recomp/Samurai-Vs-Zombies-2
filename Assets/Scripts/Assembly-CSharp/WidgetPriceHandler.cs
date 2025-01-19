using UnityEngine;

public class WidgetPriceHandler : MonoBehaviour
{
	public GameObject mainParent;

	public GluiSprite mainCurrencyIcon;

	public GluiText mainPriceLabel;

	public GameObject originalParent;

	public GameObject originalCenteringObject;

	public GluiSprite originalCurrencyIcon;

	public GluiText originalPriceLabel;

	public GameObject slashObject;

	public GluiWidget wasWidget;

	private Cost mCost;

	private string mCustomPriceString;

	private float mHeight;

	private bool mStarted;

	private Vector3 mOriginalPriceBaseOffset = Vector3.zero;

	private Vector3 mOriginalSlashPosition = Vector3.zero;

	public Cost cost
	{
		get
		{
			return mCost;
		}
		set
		{
			mCost = value;
			mCustomPriceString = null;
			Redraw();
		}
	}

	public float height
	{
		get
		{
			return mHeight;
		}
	}

	private bool priceValid
	{
		get
		{
			return (string.IsNullOrEmpty(mCustomPriceString) && mCost.price > 0) || !string.IsNullOrEmpty(mCustomPriceString);
		}
	}

	private bool onSale
	{
		get
		{
			return string.IsNullOrEmpty(mCustomPriceString) && mCost.isOnSale;
		}
	}

	private string formatedPrice
	{
		get
		{
			if (string.IsNullOrEmpty(mCustomPriceString))
			{
				return FormatCurrency(mCost.price);
			}
			return mCustomPriceString;
		}
	}

	private void Start()
	{
		if (!mStarted)
		{
			mStarted = true;
			if (originalPriceLabel != null)
			{
				mOriginalPriceBaseOffset = originalPriceLabel.transform.localPosition;
			}
			if (slashObject != null)
			{
				mOriginalSlashPosition = slashObject.transform.localPosition;
			}
			Redraw();
		}
	}

	private void Update()
	{
	}

	public void SetCustomPriceString(string str)
	{
		mCustomPriceString = str;
		Redraw();
	}

	private void Redraw()
	{
		mHeight = 0f;
		if (!priceValid)
		{
			mainParent.SetActive(false);
			originalParent.SetActive(false);
			return;
		}
		mainParent.SetActive(true);
		mHeight = mainPriceLabel.Size.y + 4f;
		if (onSale)
		{
			mHeight += originalPriceLabel.Size.y;
			originalParent.SetActive(true);
			DrawPrice(mainCurrencyIcon, mainPriceLabel, formatedPrice);
			DrawPrice(originalCurrencyIcon, originalPriceLabel, FormatCurrency(mCost.preSalePrice), true);
			CenterOriginalPriceLayout();
			if (slashObject != null)
			{
				ShowSlash();
			}
		}
		else
		{
			originalParent.SetActive(false);
			DrawPrice(mainCurrencyIcon, mainPriceLabel, formatedPrice);
		}
	}

	private void DrawPrice(GluiSprite currencyIcon, GluiText priceLabel, string priceString, bool leftAligned = false)
	{
		if (string.IsNullOrEmpty(mCustomPriceString))
		{
			currencyIcon.gameObject.SetActive(true);
			switch (mCost.currency)
			{
			case Cost.Currency.Hard:
				currencyIcon.ApplyGluiAtlasedTexture("Global_A.T_Currency_Hard.1");
				break;
			case Cost.Currency.Soft:
				currencyIcon.ApplyGluiAtlasedTexture("Global_A.T_Currency_Soft.1");
				break;
			case Cost.Currency.Soul:
				currencyIcon.ApplyGluiAtlasedTexture("Global_A.T_Currency_Soul.1");
				break;
			}
		}
		else
		{
			currencyIcon.gameObject.SetActive(false);
		}
		priceLabel.Text = priceString;
		float num = currencyIcon.Size.x + (float)priceLabel.TextLength;
		float num2 = currencyIcon.Size.x + priceLabel.Size.x;
		if (leftAligned)
		{
			priceLabel.gameObject.transform.localPosition = new Vector3(num - num2, 0f, 0f) + mOriginalPriceBaseOffset;
		}
		else
		{
			priceLabel.gameObject.transform.localPosition = new Vector3((num - num2) / 2f, 0f, 0f) + mOriginalPriceBaseOffset;
		}
	}

	private void CenterOriginalPriceLayout()
	{
		if (originalCenteringObject != null && wasWidget != null)
		{
			float num = originalCurrencyIcon.Size.x + originalPriceLabel.Size.x + wasWidget.Size.x;
			float num2 = originalCurrencyIcon.Size.x + (float)originalPriceLabel.TextLength + wasWidget.Size.x;
			originalCenteringObject.transform.localPosition = new Vector3((num - num2) / 2f, 0f, 0f);
		}
	}

	private void ShowSlash()
	{
		slashObject.SetActive(true);
		float x = (float)originalPriceLabel.TextLength / originalPriceLabel.Size.x;
		slashObject.transform.localScale = new Vector3(x, slashObject.transform.localScale.y, slashObject.transform.localScale.z);
		slashObject.transform.localPosition = new Vector3(mOriginalSlashPosition.x + (originalPriceLabel.Size.x - (float)originalPriceLabel.TextLength) / 2f, mOriginalSlashPosition.y, mOriginalSlashPosition.z);
	}

	private string FormatCurrency(int val)
	{
		return StringUtils.FormatAmountString(val);
	}
}
