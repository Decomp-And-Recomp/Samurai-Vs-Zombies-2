using System.Collections.Generic;

public abstract class ICInAppPurchase
{
	public enum TRANSACTION_STATE
	{
		NONE = 0,
		ACTIVE = 1,
		SUCCESS = 2,
		FAILED = 3,
		CANCELLED = 4,
		INTERRUPTED = 5,
		TIMEDOUT = 6
	}

	public enum RESTORE_STATE
	{
		NONE = 0,
		ACTIVE = 1,
		SUCCESS_EMPTY = 2,
		SUCCESS_DELIVERY = 3,
		FAILED = 4
	}

	protected const string DEBUG_TAG = "[Glu IAP]";

	private static ICInAppPurchase m_instance;

	public static ICInAppPurchase GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new CInAppPurchaseAndroid();
		}
		return m_instance;
	}

	public void Init(string[] products)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(products.GetLength(0));
		foreach (string key in products)
		{
			dictionary.Add(key, string.Empty);
		}
		Init(dictionary, null);
	}

	public abstract void Init(Dictionary<string, string> products, string marketPublicKey);

	public abstract void Init(string[] products, string marketPublicKey);

	public abstract bool IsTurnedOn();

	public abstract bool IsAvailable();

	public abstract CInAppPurchaseProduct[] GetAvailableProducts();

	public abstract void BuyProduct(string product);

	public abstract void RestoreCompletedTransactions();

	public abstract RESTORE_STATE GetRestoreStatus();

	public abstract TRANSACTION_STATE GetPurchaseTransactionStatus();

	public abstract string RetrieveProduct();

	public abstract int GetRetrievalQueueCount();

	public abstract string GetRetrievalQueueItem(int index);

	public abstract void RetrievalQueueDispose(int numItems);
}
