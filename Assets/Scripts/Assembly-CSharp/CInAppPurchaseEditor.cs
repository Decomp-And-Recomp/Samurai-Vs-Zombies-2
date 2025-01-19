using System.Collections.Generic;
using System.Threading;

public class CInAppPurchaseEditor : ICInAppPurchase
{
	private bool isProductListInitialized;

	private List<CInAppPurchaseProduct> validProductList;

	public Timer timer;

	private TRANSACTION_STATE transactionState;

	private List<string> productToBuy = new List<string>();

	public override void Init(Dictionary<string, string> products, string marketPublicKey)
	{
		string[] array = new string[products.Keys.Count];
		products.Keys.CopyTo(array, 0);
		Init(array, marketPublicKey);
	}

	public override void Init(string[] products, string marketPublicKey)
	{
		isProductListInitialized = false;
		validProductList = new List<CInAppPurchaseProduct>();
		foreach (string arg in products)
		{
			string format = "<UnityIAP_Key=Product><UnityIAP_Key=ProductIdentifier>{0}</UnityIAP_Key><UnityIAP_Key=Price>{1}</UnityIAP_Key><UnityIAP_Key=CurrencySymbol>{2}</UnityIAP_Key></UnityIAP_Key>";
			validProductList.Add(new CInAppPurchaseProduct(string.Format(format, arg, "4.99", '$')));
		}
	}

	public override CInAppPurchaseProduct[] GetAvailableProducts()
	{
		if (isProductListInitialized)
		{
			return validProductList.ToArray();
		}
		timer = new Timer(validateProductList, null, 5000, 0);
		return null;
	}

	public override void BuyProduct(string product)
	{
		transactionState = TRANSACTION_STATE.ACTIVE;
		timer = new Timer(buyProductCallback, product, 5000, 0);
	}

	public override bool IsTurnedOn()
	{
		return true;
	}

	public override bool IsAvailable()
	{
		return true;
	}

	public override string RetrieveProduct()
	{
		string result = null;
		if (productToBuy.Count > 0)
		{
			result = productToBuy[0];
			productToBuy.RemoveAt(0);
		}
		return result;
	}

	public override TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		return transactionState;
	}

	public override void RestoreCompletedTransactions()
	{
	}

	public override RESTORE_STATE GetRestoreStatus()
	{
		return RESTORE_STATE.SUCCESS_EMPTY;
	}

	public override int GetRetrievalQueueCount()
	{
		return productToBuy.Count;
	}

	public override string GetRetrievalQueueItem(int index)
	{
		return productToBuy[index];
	}

	public override void RetrievalQueueDispose(int numItems)
	{
		productToBuy.RemoveRange(0, numItems);
	}

	private void validateProductList(object obj)
	{
		isProductListInitialized = true;
		timer = null;
	}

	private void buyProductCallback(object obj)
	{
		int num = -1;
		for (int i = 0; i < validProductList.Count; i++)
		{
			if (string.Compare(validProductList[i].GetProductIdentifier(), obj as string, false) == 0)
			{
				num = i;
				break;
			}
		}
		switch (num)
		{
		case -1:
		case 1:
			transactionState = TRANSACTION_STATE.FAILED;
			break;
		case 2:
			transactionState = TRANSACTION_STATE.CANCELLED;
			break;
		default:
			transactionState = TRANSACTION_STATE.SUCCESS;
			productToBuy.Add(obj as string);
			break;
		}
		timer = null;
	}
}
