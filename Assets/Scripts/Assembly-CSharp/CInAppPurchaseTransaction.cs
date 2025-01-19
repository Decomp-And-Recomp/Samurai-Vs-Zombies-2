public class CInAppPurchaseTransaction
{
	public enum TransactionState
	{
		TransactionStatePurchasing = 0,
		TransactionStatePurchased = 1,
		TransactionStateFailed = 2,
		TransactionStateRestored = 3
	}

	private string m_Param;

	private TransactionState m_TransactionState;

	private string m_ProductIdentifier;

	private string m_TransactionIdentifier;

	private string m_ErrorDescription;

	private string m_ReceiptDescription;

	private string m_Date;

	public CInAppPurchaseTransaction(string param)
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

	private CInAppPurchaseTransaction()
	{
	}

	public TransactionState GetTransactionState()
	{
		return m_TransactionState;
	}

	public string GetProductIdentifier()
	{
		return m_ProductIdentifier;
	}

	public string GetTransactionIdentifier()
	{
		return m_TransactionIdentifier;
	}

	public string GetErrorDescription()
	{
		return m_ErrorDescription;
	}

	public string GetReceiptDescription()
	{
		return m_ReceiptDescription;
	}

	public string GetDate()
	{
		return m_Date;
	}

	private void ExtractData(string param)
	{
		m_Param = param;
		m_TransactionState = TransactionState.TransactionStateFailed;
		switch (CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "TransactionState"))
		{
		case "TransactionStatePurchasing":
			m_TransactionState = TransactionState.TransactionStatePurchasing;
			break;
		case "TransactionStatePurchased":
			m_TransactionState = TransactionState.TransactionStatePurchased;
			break;
		case "TransactionStateFailed":
			m_TransactionState = TransactionState.TransactionStateFailed;
			break;
		case "TransactionStateRestored":
			m_TransactionState = TransactionState.TransactionStateRestored;
			break;
		}
		m_ProductIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "ProductIdentifier");
		m_TransactionIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "TransactionIdentifier");
		m_ErrorDescription = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "ErrorDescription");
		m_ReceiptDescription = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "ReceiptDescription");
		m_Date = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Date");
	}
}
