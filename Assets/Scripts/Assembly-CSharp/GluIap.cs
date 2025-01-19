using System;
using System.Collections;
using System.Collections.Generic;
using Glu.Kontagent;
using UnityEngine;

public class GluIap : SingletonSpawningMonoBehaviour<GluIap>
{
	public const string IAP_TABLE_NAME = "IAPTable";

	public bool _storeNeedsRefresh;

	public ICInAppPurchase.RESTORE_STATE restoreTransactionStatus;

	public static bool AlertShown;

	private ICInAppPurchase.TRANSACTION_STATE lastState;

	private ICInAppPurchase.TRANSACTION_STATE TransactionState;

	private float TickStartTime;

	private float TimeOutInSecs;

	private bool queryStoreDone;

	public static bool shownRestorePurchase;

	public ICInAppPurchase Plugin { get; private set; }

	public List<IAPSchema> Products { get; private set; }

	public bool Initialized { get; private set; }

	public bool Connected { get; private set; }

	public string LastProductID { get; private set; }

	public bool LastFromVGP { get; private set; }

	public Action<ICInAppPurchase.TRANSACTION_STATE, string, bool> OnStateChange { get; set; }

	public List<IAPSchema> CheckForCompletedTransactions()
	{
		if (Products != null)
		{
			List<IAPSchema> completedTransactions = GetCompletedTransactions();
			if (completedTransactions != null)
			{
				foreach (IAPSchema item in completedTransactions)
				{
					TransactionComplete(item);
				}
				Singleton<Profile>.Instance.Save();
				return completedTransactions;
			}
		}
		return null;
	}

	public IEnumerator UpdateProductsListFromStore()
	{
		if (Products == null || Plugin == null)
		{
			yield break;
		}
		Connected = false;
		CInAppPurchaseProduct[] serverProducts = Plugin.GetAvailableProducts();
		DateTime nextCheckTime = DateTime.Now;
		DateTime timeoutAt = DateTime.Now.AddSeconds(25.0);
		while (serverProducts == null)
		{
			if (!Plugin.IsTurnedOn() || !Plugin.IsAvailable() || DateTime.Now >= timeoutAt)
			{
				yield break;
			}
			if (DateTime.Now >= nextCheckTime)
			{
				serverProducts = Plugin.GetAvailableProducts();
				nextCheckTime = DateTime.Now.AddSeconds(0.5);
			}
			yield return null;
		}
		Connected = true;
		if (Products.Count != serverProducts.Length)
		{
		}
		if (Application.isEditor)
		{
			Initialized = true;
			yield break;
		}
		for (int i = 0; i < serverProducts.Length; i++)
		{
			IAPSchema product = Products.Find((IAPSchema p) => p.productId.Equals(serverProducts[i].GetProductIdentifier()));
			if (product != null)
			{
				product.SyncWith(serverProducts[i]);
			}
		}
	}

	public ICInAppPurchase.TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		return TransactionState;
	}

	public void BuyProduct(string product, bool fromVGP = false, bool isSubscription = false)
	{
		if (GetPurchaseTransactionStatus() != ICInAppPurchase.TRANSACTION_STATE.ACTIVE)
		{
			ApplicationUtilities.PausedForIAP = true;
			AlertShown = false;
			LastProductID = product;
			LastFromVGP = fromVGP;
			TickStartTime = Time.realtimeSinceStartup;
			if (AJavaTools.Properties.IsBuildAmazon())
			{
				TimeOutInSecs = 3f;
			}
			else
			{
				TimeOutInSecs = 60f;
			}
			TransactionState = ICInAppPurchase.TRANSACTION_STATE.ACTIVE;
		}
	}

	public void UpdateProductData()
	{
	}

	protected override void Awake()
	{
		if (!SingletonSpawningMonoBehaviour<GluIap>.Exists)
		{
			base.Awake();
		}
	}

	public void Init()
	{
		StartCoroutine("Initialize");
	}

	protected override void OnDestroy()
	{
		if (!(SingletonSpawningMonoBehaviour<GluIap>.Instance != this))
		{
			base.OnDestroy();
		}
	}

	private void Update()
	{
		if (!Initialized)
		{
			return;
		}
		if (!AJavaTools.Properties.IsBuildAmazon() && TransactionState == ICInAppPurchase.TRANSACTION_STATE.ACTIVE && Time.realtimeSinceStartup - TickStartTime > TimeOutInSecs)
		{
			TransactionState = ICInAppPurchase.TRANSACTION_STATE.TIMEDOUT;
			TickStartTime = 0f;
		}
		ICInAppPurchase.TRANSACTION_STATE transactionState = TransactionState;
		if (transactionState != lastState)
		{
			OnTransactionStateChange(transactionState, LastProductID, false);
			if (OnStateChange != null)
			{
				OnStateChange(transactionState, LastProductID, LastFromVGP);
			}
		}
		lastState = transactionState;
	}

	private void CheckForStateChange()
	{
		if (!Initialized)
		{
			return;
		}
		ICInAppPurchase.TRANSACTION_STATE purchaseTransactionStatus = Plugin.GetPurchaseTransactionStatus();
		if (purchaseTransactionStatus != lastState)
		{
			OnTransactionStateChange(purchaseTransactionStatus, LastProductID, LastFromVGP);
			if (OnStateChange != null)
			{
				OnStateChange(purchaseTransactionStatus, LastProductID, LastFromVGP);
			}
		}
		lastState = purchaseTransactionStatus;
	}

	private void OnTransactionStateChange(ICInAppPurchase.TRANSACTION_STATE state, string productID, bool fromVGP)
	{
		switch (state)
		{
		case ICInAppPurchase.TRANSACTION_STATE.ACTIVE:
			if (!AlertShown)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.iap_purchasing");
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", string.Empty);
				if (!AJavaTools.Properties.IsBuildAmazon())
				{
					GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
				}
			}
			break;
		case ICInAppPurchase.TRANSACTION_STATE.SUCCESS:
			if (!AlertShown)
			{
				GlobalActions.AlertClearAll = true;
				if (!ApplicationUtilities._gameAtStartUp)
				{
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.iap_purchase_complete");
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
				}
				if (!AJavaTools.Properties.IsBuildAmazon() && !ApplicationUtilities._gameAtStartUp)
				{
					GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
				}
			}
			ApplicationUtilities.PausedForIAP = false;
			break;
		case ICInAppPurchase.TRANSACTION_STATE.FAILED:
		case ICInAppPurchase.TRANSACTION_STATE.INTERRUPTED:
			if (!AlertShown)
			{
				AlertShown = true;
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.iap_error_requestfailed");
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
				if (!AJavaTools.Properties.IsBuildAmazon())
				{
					GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
				}
			}
			ApplicationUtilities.PausedForIAP = false;
			break;
		case ICInAppPurchase.TRANSACTION_STATE.CANCELLED:
			GluiActionSender.SendGluiAction("ALERT_CLEAR", null, null);
			ApplicationUtilities.MakePlayHavenContentRequest("iap_cancelled_in_store");
			ApplicationUtilities.PausedForIAP = false;
			break;
		case ICInAppPurchase.TRANSACTION_STATE.TIMEDOUT:
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.iap_error_requesttimedout");
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
			if (!AJavaTools.Properties.IsBuildAmazon())
			{
				GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
			}
			break;
		}
	}

	private IEnumerator Initialize()
	{
		Initialized = false;
		while (!DataBundleRuntime.Instance)
		{
			yield return null;
		}
		Products = new List<IAPSchema>(DataBundleRuntime.Instance.InitializeRecords<IAPSchema>("IAPTable"));
		TransactionState = ICInAppPurchase.TRANSACTION_STATE.NONE;
		updateIapRecommendations();
		foreach (IAPSchema p2 in Products)
		{
			p2.Initialize("IAPTable");
		}
		queryStoreDone = true;
		Initialized = true;
	}

	private IEnumerator UpdateProductDataInternal()
	{
		yield return StartCoroutine(UpdateProductsListFromStore());
		while (!Singleton<Profile>.Instance.Initialized)
		{
			yield return null;
		}
		Initialized = true;
	}

	private List<IAPSchema> GetCompletedTransactions()
	{
		List<IAPSchema> list = null;
		string productId = Plugin.RetrieveProduct();
		while (!string.IsNullOrEmpty(productId))
		{
			IAPSchema iAPSchema = Products.Find((IAPSchema p) => p.productId.Equals(productId));
			if (iAPSchema != null)
			{
				if (list == null)
				{
					list = new List<IAPSchema>();
				}
				list.Add(iAPSchema);
			}
			productId = Plugin.RetrieveProduct();
		}
		return list;
	}

	private bool TransactionComplete(IAPSchema transaction)
	{
		if (!Singleton<Profile>.Instance.IsUniqueIAPItemAlreadyPurchased(transaction))
		{
			Singleton<Profile>.Instance.NotifyUniqueIAPPurchase(transaction);
			if (transaction.hardCurrencyAmount > 0)
			{
				Singleton<Profile>.Instance.purchasedGems += transaction.hardCurrencyAmount;
				Singleton<Profile>.Instance.AddGems(transaction.hardCurrencyAmount, "IAP");
			}
			if (transaction.softCurrencyAmount > 0)
			{
				Singleton<Profile>.Instance.purchasedCoins += transaction.softCurrencyAmount;
				Singleton<Profile>.Instance.AddCoins(transaction.softCurrencyAmount, "IAP");
			}
			if (!string.IsNullOrEmpty(transaction.items))
			{
				string[] array = transaction.items.Split(',');
				string[] array2 = array;
				foreach (string id in array2)
				{
					CashIn.From(id, 1, "IAP");
				}
			}
			Singleton<Analytics>.Instance.LogEvent("IAP_Completed", Analytics.Param("IAP_Name", transaction.productId), Analytics.Param("Price", transaction.PriceInCents), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel), Analytics.Param("WaveNumber", Singleton<Profile>.Instance.wave_SinglePlayerGame), Analytics.Param("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon));
			Singleton<Analytics>.Instance.KontagentEvent(transaction.productId, "IAP_Completed", GeneralConfig.FlurryAppVersion, Singleton<Profile>.Instance.wave_SinglePlayerGame, transaction.PriceInCents, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
			Kontagent.RevenueTracking(transaction.PriceInCents);
			return false;
		}
		return true;
	}

	public void refreshRecommendations()
	{
		_storeNeedsRefresh = true;
		if (GameObject.Find("PopUp_IAPStore(Clone)") != null && Products != null)
		{
			_storeNeedsRefresh = false;
			Init();
		}
	}

	private void updateIapRecommendations()
	{
		//if (!GWalletHelper.IsBillingRecommendationAvailable() || Application.internetReachability == NetworkReachability.NotReachable || !correctRecommendationsAvailable())
		//{
		//	return;
		//}
		//if (GWalletHelper.IsSubscriptionRecommendationAvailable() && !GWalletHelper.IsIAPRecommendationAvailable() && (AJavaTools.Properties.IsBuildGoogle() || AJavaTools.Properties.IsBuildAmazon()))
		//{
		//	for (int i = 0; i < GWalletHelper.subscription_recommendations.Length; i++)
		//	{
		//		IAPSchema iAPSchema = new IAPSchema();
		//		iAPSchema.updateWithSubscriptionRecommendation(GWalletHelper.subscription_recommendations[i], findSubscriptionValue(GWalletHelper.subscription_recommendations[i]));
		//		int num = findLocationToPlaceSubscription(GWalletHelper.subscription_recommendations[i]);
		//		if (num > 0)
		//		{
		//			Products.Insert(num, iAPSchema);
		//		}
		//		else
		//		{
		//			Products.Add(iAPSchema);
		//		}
		//	}
		//	return;
		//}
		//if (GWalletHelper.IsIAPRecommendationAvailable())
		//{
		//	for (int j = 0; j < 52; j++)
		//	{
		//		Products[j].hidden = true;
		//	}
		//}
		//int num2 = 0;
		//int x;
		//for (x = 0; x < GWalletHelper.combined_recommendations.Length; x++)
		//{
		//	if (GWalletHelper.combined_recommendations[x].isSubscription)
		//	{
		//		if (AJavaTools.Properties.IsBuildGoogle() || AJavaTools.Properties.IsBuildAmazon())
		//		{
		//			IAPSchema iAPSchema2 = new IAPSchema();
		//			iAPSchema2.updateWithSubscriptionRecommendation(GWalletHelper.combined_recommendations[x].sub, findSubscriptionValue(GWalletHelper.combined_recommendations[x].sub));
		//			Products.Insert(num2++, iAPSchema2);
		//		}
		//		continue;
		//	}
		//	int num3 = Products.FindIndex((IAPSchema item) => item.referenceId == GWalletHelper.combined_recommendations[x].iap.m_itemName);
		//	if (num3 < 0)
		//	{
		//		continue;
		//	}
		//	if (ApplicationUtilities.iapPrices.ContainsKey(GWalletHelper.combined_recommendations[x].iap.m_storeSkuCode.ToLower()))
		//	{
		//		ApplicationUtilities.iapPrices.Remove(GWalletHelper.combined_recommendations[x].iap.m_storeSkuCode.ToLower());
		//		string value = ((float)GWalletHelper.combined_recommendations[x].iap.m_currencyValue / 100f).ToString("C2");
		//		ApplicationUtilities.iapPrices.Add(GWalletHelper.combined_recommendations[x].iap.m_storeSkuCode.ToLower(), value);
		//	}
		//	Products[num3].updateWithIapRecommendation(GWalletHelper.combined_recommendations[x].iap);
		//	Products.Insert(num2++, Products[num3]);
		//	Products.RemoveAt(num3 + 1);
		//	int num4 = num2;
		//	while (num4 < Products.Count)
		//	{
		//		if (Products[num4].referenceId.Contains(GWalletHelper.combined_recommendations[x].iap.m_itemName.Substring(11)))
		//		{
		//			Products.Insert(num2++, Products[num4]);
		//			Products.RemoveAt(num4 + 1);
		//		}
		//		else
		//		{
		//			num4++;
		//		}
		//	}
		//}
	}

	private int findSubscriptionValue(GWSubscriptionRecommendation_Unity sub)
	{
		int result = 0;
		int num = Products.FindIndex((IAPSchema item) => item.productId.ToUpper() == sub.m_storeSkuCode.Remove(sub.m_storeSkuCode.IndexOf(".sub.")).ToUpper());
		if (num >= 0)
		{
			result = ((Products[num].hardCurrencyAmount <= 0) ? Products[num].softCurrencyAmount : Products[num].hardCurrencyAmount);
			Products[num].percentBonus = 0;
		}
		return result;
	}

	private int findLocationToPlaceSubscription(GWSubscriptionRecommendation_Unity sub)
	{
		int num = Products.FindIndex((IAPSchema item) => item.productId.ToUpper() == sub.m_storeSkuCode.Remove(sub.m_storeSkuCode.IndexOf(".sub.")).ToUpper() + "_60");
		if (num >= 0)
		{
		}
		return num + 1;
	}

	private bool correctRecommendationsAvailable()
	{
		return true;
	}
}
