using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/IAP Data")]
public class IAPData : SingletonMonoBehaviour<IAPData>, IGluiDataSource
{
	private enum State
	{
		None = 0,
		Connecting = 1,
		Ready = 2,
		Unavailable = 3,
		Disconnected = 4,
		Blocked = 5,
		InProgress = 6
	}

	public GluiStandardButtonContainer hcTab;

	public GluiStandardButtonContainer scTab;

	public GluiStandardButtonContainer specialsTab;

	private State state;

	public static void Purchase(string productId)
	{
		bool fromVGP = false;
		if (SingletonMonoBehaviour<IAPData>.Exists)
		{
			if (SingletonMonoBehaviour<IAPData>.Instance.state == State.Ready)
			{
				SingletonMonoBehaviour<IAPData>.Instance.state = State.InProgress;
				SingletonSpawningMonoBehaviour<GluIap>.Instance.BuyProduct(productId, fromVGP);
			}
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluIap>.Instance.BuyProduct(productId, fromVGP);
		}
	}

	public void Get_GluiData(string dataFilterKey, string dataFilterKeySecondary, GluiDataScan_AdditionalParameters additionalParameters, out object[] records)
	{
		switch (dataFilterKey)
		{
		case "IAP":
		{
			if (!SingletonSpawningMonoBehaviour<GluIap>.Instance.Initialized)
			{
				break;
			}
			List<IAPSchema> products = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products;
			string text = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("IAP_TAB") as string;
			if (string.IsNullOrEmpty(text) || text.Equals("LocalizedStrings.hard_currency_tab"))
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("IAP_TAB", "LocalizedStrings.hard_currency_tab");
				records = products.FindAll((IAPSchema s) => (!s.hidden && string.IsNullOrEmpty(s.items) && s.hardCurrencyAmount > 0 && s.softCurrencyAmount == 0) || (!s.hidden && s.productId.Contains("STARTER_PACK") && (!string.IsNullOrEmpty(s.items) || (s.softCurrencyAmount > 0 && s.hardCurrencyAmount > 0)) && !Singleton<Profile>.Instance.HasIAPExpired(s) && !Singleton<Profile>.Instance.IsUniqueIAPItemAlreadyPurchased(s) && (s.hoursToExpire == 0 || (SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime != null && SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime.SNTPSuccessful && !Singleton<Profile>.Instance.HasIAPExpired(s))))).ConvertAll((Converter<IAPSchema, object>)((IAPSchema o) => o)).ToArray();
				scTab.Selected = false;
				hcTab.Selected = true;
				specialsTab.Selected = false;
			}
			else if (!string.IsNullOrEmpty(text) && text.Equals("LocalizedStrings.soft_currency_tab"))
			{
				records = products.FindAll((IAPSchema s) => !s.hidden && string.IsNullOrEmpty(s.items) && s.softCurrencyAmount > 0 && s.hardCurrencyAmount == 0).ConvertAll((Converter<IAPSchema, object>)((IAPSchema o) => o)).ToArray();
				scTab.Selected = true;
				hcTab.Selected = false;
				specialsTab.Selected = false;
			}
			else
			{
				records = products.FindAll((IAPSchema s) => !s.hidden && (!string.IsNullOrEmpty(s.items) || (s.softCurrencyAmount > 0 && s.hardCurrencyAmount > 0)) && !Singleton<Profile>.Instance.HasIAPExpired(s) && (s.hoursToExpire == 0 || (SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime != null && SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime.SNTPSuccessful && !Singleton<Profile>.Instance.HasIAPExpired(s)))).ConvertAll((Converter<IAPSchema, object>)((IAPSchema o) => o)).ToArray();
				scTab.Selected = false;
				hcTab.Selected = false;
				specialsTab.Selected = true;
			}
			return;
		}
		}
		records = new object[0];
	}

	private void Start()
	{
		SingletonSpawningMonoBehaviour<GluIap>.Instance.UpdateProductData();
		GluIap instance = SingletonSpawningMonoBehaviour<GluIap>.Instance;
		instance.OnStateChange = (Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>)Delegate.Combine(instance.OnStateChange, new Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>(OnIapStateChange));
		GluIap.AlertShown = false;
		UpdateCurrentState();
		ApplicationUtilities.StoreOpened();
	}

	protected override void OnDestroy()
	{
		if (!ApplicationUtilities.HasShutdown && SingletonSpawningMonoBehaviour<GluIap>.Exists)
		{
			GluIap instance = SingletonSpawningMonoBehaviour<GluIap>.Instance;
			instance.OnStateChange = (Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>)Delegate.Remove(instance.OnStateChange, new Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>(OnIapStateChange));
		}
		base.OnDestroy();
	}

	private void Update()
	{
		if (state == State.Connecting && SingletonSpawningMonoBehaviour<GluIap>.Instance.Initialized)
		{
			UpdateCurrentState();
		}
	}

	private void UpdateCurrentState()
	{
		if (!SingletonSpawningMonoBehaviour<GluIap>.Instance.Initialized)
		{
			if (state != State.Connecting)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.iap_connecting");
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "LocalizedStrings.cancel");
				GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
				WatchForPopupCancel();
				state = State.Connecting;
			}
		}
		else if (state != State.Ready)
		{
			if (state == State.Connecting)
			{
				GluiActionSender.SendGluiAction("ALERT_CLEAR", base.gameObject, null);
			}
			Singleton<Profile>.Instance.UpdateIAPTimers();
			GluiActionSender.SendGluiAction("IAP_PURCHASE_READY", base.gameObject, null);
			state = State.Ready;
		}
	}

	private void OnIapStateChange(ICInAppPurchase.TRANSACTION_STATE state, string productID, bool fromVGP)
	{
		UpdateCurrentState();
	}

	private void WatchForPopupCancel()
	{
		GluiPersistentDataWatcher gluiPersistentDataWatcher = new GluiPersistentDataWatcher();
		gluiPersistentDataWatcher.PersistentEntryToWatch = "ALERT_GENERIC_RESULT";
		gluiPersistentDataWatcher.Event_WatchedDataChanged += delegate(object data)
		{
			string text = data as string;
			if (text != null && text == "BUTTON")
			{
				GluiActionSender.SendGluiAction("POPUP_POP", null, null);
			}
		};
		gluiPersistentDataWatcher.StartWatching();
	}
}
