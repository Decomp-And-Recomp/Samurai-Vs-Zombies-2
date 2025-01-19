using System;

[Serializable]
public class DataAdaptor_ConfirmPurchaseBundle : DataAdaptorBase
{
	public object context;

	public GluiSprite icon;

	public GluiText title;

	public GluiText description;

	public GluiStandardButtonContainer purchaseButton;

	public GluiActionListener_Splitter actionsOnBuy;

	public WidgetPriceSpawner priceSpawner;

	public GluiBouncyScrollList scrollList;

	private bool mWillTriggerPopup;

	public override void SetData(object data)
	{
		if (data is IAPSchema)
		{
			context = data;
			purchaseButton.GetActionData = () => context;
			SetData((IAPSchema)data);
		}
		else if (data is StoreData.Item)
		{
			context = data;
			purchaseButton.GetActionData = () => context;
			SetData((StoreData.Item)data);
		}
	}

	private void SetData(IAPSchema iap)
	{
		icon.Texture = iap.icon;
		title.Text = iap.displayedName;
		description.Text = iap.description;
		scrollList.Redraw(iap);
		if (priceSpawner != null)
		{
			priceSpawner.SetCost(iap.priceString);
		}
		string[] array = iap.items.Split(',');
		string[] array2 = array;
		foreach (string id in array2)
		{
			if (CashIn.WillTriggerPopup(id))
			{
				mWillTriggerPopup = true;
				break;
			}
		}
	}

	private void SetData(StoreData.Item item)
	{
		icon.Texture = item.icon;
		title.Text = item.title;
		description.Text = item.details.Description;
		scrollList.Redraw(item);
		if (priceSpawner != null)
		{
			priceSpawner.SetCost(item.cost);
		}
		actionsOnBuy.actionsToSend = new string[2] { "CONFIRM_BUY", "POPUP_EMPTY" };
		foreach (string item2 in item.bundleContent)
		{
			if (CashIn.WillTriggerPopup(item2))
			{
				mWillTriggerPopup = true;
				break;
			}
		}
	}

	public void OnTransactionStateChange(ICInAppPurchase.TRANSACTION_STATE state, string productID, bool fromVGP)
	{
		if (state == ICInAppPurchase.TRANSACTION_STATE.SUCCESS && !mWillTriggerPopup)
		{
			GluiActionSender.SendGluiAction("POPUP_EMPTY", purchaseButton.gameObject, null);
		}
	}
}
