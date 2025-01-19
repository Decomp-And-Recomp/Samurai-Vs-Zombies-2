using System;
using System.Collections.Generic;
using UnityEngine;

public class TestIAPScript : MonoBehaviour
{
	private const string ANDROID_MARKET_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiDe5FBbiqA55lvMO+FnsGzdtZpLsbhEMg2IiAIow4RUSDBHLnBf6m2BbxnusxZS47dpG0hq3Daqw1WkMxHj6UTjOQc9V+GYwvAX/h55nT4gFpMTzzjx+yECekyXUVZ/dEDl2Nfil3rTRlwiURbpA1CIObiuFQmP2vH8pRkXnvUaj60EGtrqZydGzsuR8QVX8fa7eOzopBNsUqU5kqrAsggi52bws08OBfYZ2ehlIrBxts5UipYQVwFuFKtmc53XI1jb4sQOs5UdT9wvJ87Cg+C8vI85Z1sDjBCn8XSkJsN3Y3TAdv4H2gA92pUza3K+kdIHUa24h+zQxwWe8gdQiNwIDAQAB";

	private Dictionary<string, string> PRODUCT_LIST = new Dictionary<string, string>
	{
		{ "com.glu.indestructible.hc_10", "1b33297a8a3ad91ef935efff4136f4c966debfefb953b65d8a1af6f8125178f8" },
		{ "com.glu.indestructible.hc_20", "7f2feed21bcefbe0a29234ccb7b27400fe9020892ab59182e63fae69a5097b17" },
		{ "com.glu.indestructible.hc_30", "c39e929a75e46bdf52a9c46ca852ef037fe9b8d5c59d2bb1164edad29d207d6e" },
		{ "com.glu.indestructible.hc_40", "796249a128b03a738ea6d47aab710a0b6cf442dcadad14d4599e6fd3e10b165a" },
		{ "com.glu.indestructible.boost_1", "b97fdfad9c8d62f18f89acf15f1859d932a297f2a4ed1039188b55b25b913b38" },
		{ "com.glu.indestructible.boost_2", "a986ae593013d9536d9df4c9eddaec283b3d4fa446156bbc5659fae4dda3016a" }
	};

	private CInAppPurchaseProduct[] m_productList;

	private bool m_transactionActive;

	private bool m_restoreTransactions;

	private string m_labelMessage;

	private int m_logStringsCount;

	private float m_profilerTransactionTime;

	private int m_profilerTransactionCount;

	private float m_profilerRetrieveTime;

	private void Start()
	{
		ICInAppPurchase.GetInstance().Init(PRODUCT_LIST, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiDe5FBbiqA55lvMO+FnsGzdtZpLsbhEMg2IiAIow4RUSDBHLnBf6m2BbxnusxZS47dpG0hq3Daqw1WkMxHj6UTjOQc9V+GYwvAX/h55nT4gFpMTzzjx+yECekyXUVZ/dEDl2Nfil3rTRlwiURbpA1CIObiuFQmP2vH8pRkXnvUaj60EGtrqZydGzsuR8QVX8fa7eOzopBNsUqU5kqrAsggi52bws08OBfYZ2ehlIrBxts5UipYQVwFuFKtmc53XI1jb4sQOs5UdT9wvJ87Cg+C8vI85Z1sDjBCn8XSkJsN3Y3TAdv4H2gA92pUza3K+kdIHUa24h+zQxwWe8gdQiNwIDAQAB");
		m_productList = null;
		m_transactionActive = false;
		m_restoreTransactions = false;
		m_labelMessage = string.Empty;
		m_logStringsCount = 0;
		m_profilerTransactionTime = 0f;
		m_profilerTransactionCount = 0;
		m_profilerRetrieveTime = 0f;
	}

	private void OnGUI()
	{
		if (!ICInAppPurchase.GetInstance().IsAvailable())
		{
			return;
		}
		int num = -1;
		if (m_transactionActive)
		{
			string text = "Transaction in progress... ";
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			ICInAppPurchase.TRANSACTION_STATE purchaseTransactionStatus = ICInAppPurchase.GetInstance().GetPurchaseTransactionStatus();
			m_profilerTransactionTime += Time.realtimeSinceStartup - realtimeSinceStartup;
			m_profilerTransactionCount++;
			switch (purchaseTransactionStatus)
			{
			case ICInAppPurchase.TRANSACTION_STATE.ACTIVE:
				text += "TRANSACTION_STATE.ACTIVE";
				break;
			case ICInAppPurchase.TRANSACTION_STATE.SUCCESS:
				text += "TRANSACTION_STATE.SUCCESS";
				m_transactionActive = false;
				break;
			case ICInAppPurchase.TRANSACTION_STATE.FAILED:
				text += "TRANSACTION_STATE.FAILED";
				m_transactionActive = false;
				break;
			case ICInAppPurchase.TRANSACTION_STATE.CANCELLED:
				text += "TRANSACTION_STATE.CANCELLED";
				m_transactionActive = false;
				break;
			case ICInAppPurchase.TRANSACTION_STATE.INTERRUPTED:
				text += "TRANSACTION_STATE.INTERRUPTED";
				m_transactionActive = false;
				break;
			}
			PostLogMessage(text);
			GUI.enabled = false;
		}
		if (m_restoreTransactions)
		{
			switch (ICInAppPurchase.GetInstance().GetRestoreStatus())
			{
			case ICInAppPurchase.RESTORE_STATE.ACTIVE:
				PostLogMessage("Restore transactions status - ACTIVE");
				break;
			case ICInAppPurchase.RESTORE_STATE.FAILED:
				m_restoreTransactions = false;
				PostLogMessage("Restore transactions status - FAILED");
				break;
			case ICInAppPurchase.RESTORE_STATE.SUCCESS_EMPTY:
			case ICInAppPurchase.RESTORE_STATE.SUCCESS_DELIVERY:
				m_restoreTransactions = false;
				PostLogMessage("Restore transactions status - SUCCESS");
				break;
			}
		}
		if (m_productList != null)
		{
			string[] array = new string[m_productList.GetLength(0)];
			for (int i = 0; i < m_productList.GetLength(0); i++)
			{
				array[i] = m_productList[i].GetProductIdentifier();
			}
			num = GUI.SelectionGrid(new Rect(10f, 10f, 450f, 250f), num, array, 2);
		}
		else
		{
			m_productList = ICInAppPurchase.GetInstance().GetAvailableProducts();
			PostLogMessage("Loading product list...");
		}
		GUI.enabled = true;
		GUI.Label(new Rect(10f, 300f, 450f, 100f), m_labelMessage);
		if (num >= 0)
		{
			ICInAppPurchase.GetInstance().BuyProduct(m_productList[num].GetProductIdentifier());
			m_transactionActive = true;
		}
		if (GUI.Button(new Rect(10f, 410f, 450f, 60f), "Refresh list"))
		{
			m_productList = null;
		}
		if (GUI.Button(new Rect(10f, 500f, 450f, 60f), "Restore transactions"))
		{
			ICInAppPurchase.GetInstance().RestoreCompletedTransactions();
			m_restoreTransactions = true;
		}
		if (GUI.Button(new Rect(10f, 590f, 100f, 60f), "Retrieve"))
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			string text2 = ICInAppPurchase.GetInstance().RetrieveProduct();
			m_profilerRetrieveTime = Time.realtimeSinceStartup - realtimeSinceStartup;
			if (text2 != null)
			{
				PostLogMessage("Retrieved... " + text2);
			}
			else
			{
				PostLogMessage("No product retrieved...");
			}
		}
		if (GUI.Button(new Rect(120f, 590f, 100f, 60f), "GetCount"))
		{
			PostLogMessage("Retrieval queue count..." + ICInAppPurchase.GetInstance().GetRetrievalQueueCount());
		}
		if (GUI.Button(new Rect(230f, 590f, 100f, 60f), "GetItems"))
		{
			for (int j = 0; j < ICInAppPurchase.GetInstance().GetRetrievalQueueCount(); j++)
			{
				PostLogMessage("In queue..." + ICInAppPurchase.GetInstance().GetRetrievalQueueItem(j));
			}
		}
		if (GUI.Button(new Rect(340f, 590f, 100f, 60f), "DisposeTwo"))
		{
			ICInAppPurchase.GetInstance().RetrievalQueueDispose(2);
		}
		if (GUI.Button(new Rect(10f, 680f, 450f, 60f), "Quit"))
		{
			Application.Quit();
		}
		if (m_profilerTransactionCount > 0)
		{
			GUI.Label(new Rect(10f, 770f, 450f, 30f), "GetTransactionStatus overage time - " + Convert.ToInt32(m_profilerTransactionTime * 100000f) / m_profilerTransactionCount + " / 100 ms");
			if (m_profilerTransactionCount > 10)
			{
				m_profilerTransactionCount = 0;
				m_profilerTransactionTime = 0f;
			}
		}
		if (m_profilerRetrieveTime > 0f)
		{
			GUI.Label(new Rect(10f, 790f, 450f, 30f), "RetrieveData time - " + Convert.ToInt32(m_profilerRetrieveTime * 100000f) + " / 100 ms");
		}
	}

	private void PostLogMessage(string msg)
	{
		m_logStringsCount++;
		if (m_logStringsCount > 5)
		{
			m_labelMessage = m_labelMessage.Substring(m_labelMessage.IndexOf("\n") + 1);
		}
		m_labelMessage = m_labelMessage + msg + "\n";
	}
}
