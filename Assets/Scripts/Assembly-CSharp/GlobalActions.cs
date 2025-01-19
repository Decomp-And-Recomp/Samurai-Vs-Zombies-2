using System;
using System.Collections.Generic;
using Glu.Plugins.ASocial;
using UnityEngine;

public class GlobalActions : GluiGlobalActionHandler
{
	private string _iapProductId = string.Empty;

	public static bool AlertClearAll { get; set; }

	public override bool HandleGlobalAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "MENU_MAIN_MULTIPLAYER_ATTEMPT_INIT":
			MultiplayerLoginSequence.LoginStart(sender, true);
			return true;
		case "MULTIPLAYER_MENU_SHOW":
			if (SingletonMonoBehaviour<FrontEnd>.Instance.OnEquipScreen)
			{
				GluiActionSender.SendGluiAction("MENU_MAIN_MULTIPLAYER_BACK", sender, data);
			}
			else
			{
				GluiActionSender.SendGluiAction("MENU_MAIN_MULTIPLAYER", sender, data);
			}
			return true;
		case "MULTIPLAYER_GENERATE_DEFAULT_PLAYER_NAME":
		{
			string tag = "Player" + UnityEngine.Random.Range(100000, 999999);
			if (PlayGameServices.IsSignedIn)
			{
				tag = PlayGameServices.GetDisplayName();
			}
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_NAME_ENTRY_TEXT", tag);
			return true;
		}
		case "MULTIPLAYER_SAVE_NAME":
			MultiplayerGlobalHelpers.SaveMultiplayerName(sender);
			return true;
		case "QUERY_HAS_COLLECTIBLE_CONFLICT":
		{
			MultiplayerCollectionStatusQueryResponse.MultiplayerCollectionStatusAggregate multiplayerCollectionStatusAggregate = Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.FindFirstConflict();
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("COLLECTIBLE_CONFLICTS", multiplayerCollectionStatusAggregate.attackerCount.ToString());
			NUF.SetIconBadgeNumber(multiplayerCollectionStatusAggregate.attackerCount);
			return true;
		}
		case "QUERY_SELECTED_COLLECTIBLE_CARD_OPPONENTS":
		{
			CollectionItemSchema selectedCard = MultiplayerGlobalHelpers.GetSelectedCard();
			if (selectedCard == null)
			{
				return false;
			}
			MultiplayerOpponentQuerySequence.QueryStart(sender, selectedCard.CollectionID);
			return true;
		}
		case "ATTACK_QUERY_SUCCESS":
			GluiActionSender.SendGluiAction("ALERT_CLEAR", sender, data);
			GluiActionSender.SendGluiAction("POPUP_SELECTOPPONENT", sender, data);
			return true;
		case "TEST_CAN_AFFORD_TO_ATTACK_SELECTED_COLLECTIBLE":
		{
			CollectionItemSchema collectionItemSchema2 = data as CollectionItemSchema;
			if (collectionItemSchema2 != null)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("SELECTED_COLLECTIBLE_CARD", collectionItemSchema2);
			}
			else
			{
				collectionItemSchema2 = MultiplayerGlobalHelpers.GetSelectedCard();
			}
			if (Singleton<Profile>.Instance.souls < collectionItemSchema2.soulsToAttack)
			{
				if (Singleton<Profile>.Instance.GetMaxSouls() < collectionItemSchema2.soulsToAttack)
				{
					GluiActionSender.SendGluiAction("PURCHASE_IMPOSSIBLE", sender, data);
				}
				else
				{
					GluiActionSender.SendGluiAction("PURCHASE_FAIL", sender, data);
				}
			}
			else
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("ATTACK_TYPE", "Attack");
				GluiActionSender.SendGluiAction("PURCHASE_SUCCESS", sender, data);
			}
			return true;
		}
		case "ATTACK_LOST_ITEM":
		{
			CollectionItemSchema collectionItemSchema = data as CollectionItemSchema;
			if (collectionItemSchema != null)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("SELECTED_COLLECTIBLE_CARD", collectionItemSchema);
			}
			else
			{
				collectionItemSchema = MultiplayerGlobalHelpers.GetSelectedCard();
			}
			if (Singleton<Profile>.Instance.souls < collectionItemSchema.soulsToAttack)
			{
				if (Singleton<Profile>.Instance.GetMaxSouls() < collectionItemSchema.soulsToAttack)
				{
					GluiActionSender.SendGluiAction("PURCHASE_IMPOSSIBLE", sender, data);
				}
				else
				{
					GluiActionSender.SendGluiAction("PURCHASE_FAIL", sender, data);
				}
			}
			else
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("ATTACK_TYPE", "Revenge");
				SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = false;
				MultiplayerGlobalHelpers.ExtractSelectedCardDataForWave(EMultiplayerMode.kRecovering, delegate(bool success)
				{
					if (success)
					{
						GluiActionSender.SendGluiAction("RECOVER_QUERY_SUCCESS", sender, data);
					}
					SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
				});
			}
			return true;
		}
		case "TEST_MULTIPLAYER_SESSION_FINISHED_COLLECTION_SET":
			MultiplayerGlobalHelpers.TestFinishedCollection(sender, data);
			return true;
		case "AWARD_JUST_FINISHED_COLLECTION":
			return true;
		case "PREPARE_TO_DEFEND_SELECTED_COLLECTIBLE":
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("ATTACK_TYPE", "Revenge");
			CollectionItemSchema collectionItemSchema = data as CollectionItemSchema;
			if (collectionItemSchema != null)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("SELECTED_COLLECTIBLE_CARD", collectionItemSchema);
			}
			SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = false;
			MultiplayerGlobalHelpers.ExtractSelectedCardDataForWave(EMultiplayerMode.kDefending, delegate(bool success)
			{
				if (success)
				{
					GluiActionSender.SendGluiAction("POPUP_OPPONENT_DEFEND", sender, data);
				}
				SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
			});
			return true;
		}
		case "PREPARE_TO_ATTACK_SELECTED_COLLECTIBLE":
			MultiplayerGlobalHelpers.ExtractSelectedCardDataForWave(EMultiplayerMode.kAttacking, delegate
			{
			});
			return true;
		case "INVITE_FRIENDS":
		{
			Dictionary<Glu.Plugins.ASocial.Facebook.AppRequestType, string> dictionary = new Dictionary<Glu.Plugins.ASocial.Facebook.AppRequestType, string>();
			dictionary.Add(Glu.Plugins.ASocial.Facebook.AppRequestType.message, "I challenge you to battle my Samurai!");
			dictionary.Add(Glu.Plugins.ASocial.Facebook.AppRequestType.data, "Samurai Challenge!");
			Glu.Plugins.ASocial.Facebook.Request(dictionary);
			return true;
		}
		case "CLEAR_MULTIPLAYER_GAME_SESSION_DATA":
			ClearMultiplayerGameSession();
			return true;
		case "CONFIRM_BUY":
			ConfirmItemPurchase(data, 0, sender);
			return true;
		case "CONFIRM_BUY_PACK":
			ConfirmItemPurchase(data, 1, sender);
			return true;
		case "CONFIRM_BUY_1DAY_SHIELD":
			ConfirmItemPurchase(data, 0, sender);
			return true;
		case "CONFIRM_BUY_3DAY_SHIELD":
			ConfirmItemPurchase(data, 1, sender);
			return true;
		case "CONFIRM_BUY_7DAY_SHIELD":
			ConfirmItemPurchase(data, 2, sender);
			return true;
		case "CANCEL_BUY":
			CancelItemPurchase(data);
			return true;
		case "POPUP_CONFIRMPURCHASE":
			return BlockConfirmItemPurchase(data);
		case "BUY_REVIVES":
			GluiActionSender.SendGluiAction("POPUP_CONFIRMPURCHASE", sender, StoreAvailability.GetPotion("revivePotion"));
			return true;
		case "BUY_SOULS":
			GluiActionSender.SendGluiAction("POPUP_CONFIRMPURCHASE", sender, StoreAvailability.GetPotion("souls"));
			return true;
		case "BUY_SOUL_UPGRADE":
			GluiActionSender.SendGluiAction("POPUP_CONFIRMPURCHASE", sender, StoreAvailability.GetUpgrade("SoulJar"));
			return true;
		case "GET_MORE":
			if (GetCurrencyWarnType((string)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("NOT_ENOUGH")) == Cost.Currency.Hard)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("IAP_TAB", "LocalizedStrings.hard_currency_tab");
			}
			else
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("IAP_TAB", "LocalizedStrings.soft_currency_tab");
			}
			GluiActionSender.SendGluiAction("POPUP_IAP", sender, data);
			return true;
		case "IAP_PURCHASE":
			IAPPurchase(data);
			return true;
		case "HANDLE_ALERT_CLEAR":
			if (AlertClearAll)
			{
				AlertClearAll = false;
				GluiActionSender.SendGluiAction("ALERT_EMPTY", sender, data);
			}
			else
			{
				GluiActionSender.SendGluiAction("ALERT_CLEAR", sender, data);
			}
			return true;
		case "BUTTON_MODE_SELECT":
			switch (SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("GAME_MODE") as string)
			{
			case "Button_Story":
				ClearMultiplayerGameSession();
				GluiActionSender.SendGluiAction("MENU_MAIN_EQUIP", sender, data);
				FrontEnd_HUD.SetDefenseRatingMode(false);
				break;
			case "Button_DailyChallenge":
				ClearMultiplayerGameSession();
				GluiActionSender.SendGluiAction("MENU_MAIN_EQUIP", sender, data);
				Singleton<Profile>.Instance.RefreshDailyChallenge();
				FrontEnd_HUD.SetDefenseRatingMode(false);
				break;
			case "Button_Multiplayer":
				Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.UpdateLocalProfile();
				GluiActionSender.SendGluiAction("ALERT_MULTIPLAYER_LOGIN", sender, data);
				FrontEnd_HUD.SetDefenseRatingMode(true);
				break;
			}
			return true;
		case "SHOW_TAPJOY":
			return true;
		case "SHOW_PRIVACY":
			Application.OpenURL(ConfigSchema.Entry("GluPrivacyURL"));
			return true;
		case "PUSH_NOTIFICATION_TOGGLE":
			PushNotification.Toggle();
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("PUSH_NOTIFICATION_TEXT", StringUtils.GetStringFromStringRef("MenuFixedStrings", (!PushNotification.IsEnabled()) ? "Menu_Off" : "Menu_On"));
			break;
		case "MORE_GAMES":
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				AJavaTools.UI.ShowToast(StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_REQUIRE_INTERNET_ANDROID"));
			}
			return true;
		case "INGAME_PAUSE":
			if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = true;
			}
			return true;
		case "INGAME_UNPAUSE":
			if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = false;
			}
			return true;
		case "TIMER_SOULS":
		{
			int num = (int)data;
			Singleton<Profile>.Instance.souls += num;
			if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
			{
				Singleton<Profile>.Instance.Save();
			}
			return true;
		}
		case "SPINNER_ON":
			AJavaTools.UI.StartIndeterminateProgress(17);
			return true;
		case "SPINNER_OFF":
			NUF.StopSpinner();
			return true;
		case "CANCEL_NAME":
			if (Singleton<Profile>.Instance.MultiplayerData.Account.Status == GripAccount.LoginStatus.Complete)
			{
				GluiActionSender.SendGluiAction("MENU_MAIN_MULTIPLAYER", sender, data);
			}
			else
			{
				GluiActionSender.SendGluiAction("MENU_MAIN_STORE", sender, data);
			}
			return true;
		case "SHOW_POWER_RATING":
			if (FrontEnd_HUD.Instance == null || !FrontEnd_HUD.ShowingDefenseRating)
			{
				GluiActionSender.SendGluiAction("POPUP_POWER_RATING", sender, data);
			}
			else
			{
				DefenseRatingImpl.loadoutToDisplay = null;
				GluiActionSender.SendGluiAction("POPUP_DEFENSE_RATING", sender, data);
			}
			return true;
		}
		return false;
	}

	private void PlayHavenError()
	{
		MultiplayerData.NetworkRequiredDialog();
	}

	public static void ClearMultiplayerGameSession()
	{
		Singleton<Profile>.Instance.MultiplayerData.ClearMultiplayerGameSession();
		Profile.UpdatePlayMode();
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove("ATTACK_TYPE");
	}

	private bool BlockConfirmItemPurchase(object data)
	{
		if (data is StoreData.Item)
		{
			return false;
		}
		return true;
	}

	private void ConfirmItemPurchase(object data, int packPurchased, GameObject sender)
	{
		if (!(data is StoreData.Item))
		{
			return;
		}
		StoreData.Item item = data as StoreData.Item;
		if (item.id.Equals("souls"))
		{
			int souls = Singleton<Profile>.Instance.souls;
			int maxSouls = Singleton<Profile>.Instance.GetMaxSouls();
			if (souls == maxSouls)
			{
				return;
			}
		}
		if (packPurchased == 1 && item.id != "mpshield")
		{
			if (item.packCost.canAfford)
			{
				if (item.id == "mysterybox")
				{
					MultipleMysteryBoxImpl.MysteryBoxPackCost = item.packCost;
					MysteryBoxImpl.MysteryBoxCost = new Cost(0, Cost.Currency.Soft, 0f);
				}
				else
				{
					item.packCost.Spend();
					item.packCost.gwalletSpend(item.packCost, "DEBIT_IN_APP_PURCHASE", item.title);
				}
				if (item.packOverrideFunc != null)
				{
					item.packOverrideFunc();
				}
				else if (item.packPurchaseFunc != null)
				{
					item.packPurchaseFunc();
					GluiActionSender.SendGluiAction("PURCHASE_COMPLETE", sender, data);
				}
			}
			else
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("NOT_ENOUGH", GetCurrencyWarnStringID(item.packCost.currency));
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("NOT_ENOUGH_COST", item.packCost);
				GluiActionSender.SendGluiAction("ALERT_NOT_ENOUGH", sender, data);
			}
			return;
		}
		Cost cost = item.cost;
		switch (packPurchased)
		{
		case 1:
			cost = item.packCost;
			break;
		case 2:
			cost = item.cost3;
			break;
		}
		if (cost.canAfford)
		{
			if (item.id == "mysterybox")
			{
				MysteryBoxImpl.MysteryBoxCost = cost;
			}
			else if (item.id != "mpshield" || SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime.SNTPSuccessful)
			{
				cost.Spend();
				cost.gwalletSpend(item.cost, "DEBIT_IN_APP_PURCHASE", item.title);
			}
			item.Apply(packPurchased);
			if (!item.triggersOwnPopup)
			{
				GluiActionSender.SendGluiAction("PURCHASE_COMPLETE", sender, data);
			}
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("NOT_ENOUGH", GetCurrencyWarnStringID(cost.currency));
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("NOT_ENOUGH_COST", cost);
			GluiActionSender.SendGluiAction("ALERT_NOT_ENOUGH", sender, data);
		}
	}

	private void CancelItemPurchase(object data)
	{
		if (data is StoreData.Item)
		{
			StoreData.Item item = data as StoreData.Item;
			if (item.id == "mysterybox")
			{
				ApplicationUtilities.MakePlayHavenContentRequest("cancel_mysterybox_purchase");
			}
		}
	}

	private string GetCurrencyWarnStringID(Cost.Currency cur)
	{
		switch (cur)
		{
		case Cost.Currency.Soft:
			return "MenuFixedStrings.Warn_CurrencySoft";
		case Cost.Currency.Hard:
			return "MenuFixedStrings.Warn_CurrencyHard";
		default:
			throw new Exception("Missing Warn_Currency string hookup: " + cur);
		}
	}

	private Cost.Currency GetCurrencyWarnType(string stringID)
	{
		switch (stringID)
		{
		case "MenuFixedStrings.Warn_CurrencySoft":
			return Cost.Currency.Soft;
		case "MenuFixedStrings.Warn_CurrencyHard":
			return Cost.Currency.Hard;
		default:
			throw new Exception("Missing Warn_Currency string hookup: " + stringID);
		}
	}

	private void IAPPurchase(object data)
	{
		IAPSchema iAPSchema = data as IAPSchema;
		if (AJavaTools.Properties.GetBuildType() == "tstore")
		{
			confirmationPopup(iAPSchema);
		}
		else
		{
			IAPData.Purchase(iAPSchema.productId);
		}
	}

	private void confirmationPopup(IAPSchema item)
	{
		string message = "Error: " + item.displayedName + " not available.";
		_iapProductId = item.productId;
		if (item != null)
		{
			message = ((item.hardCurrencyAmount > 0) ? string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "iap_purchase_message"), item.hardCurrencyAmount + " Glu Credits", item.priceString) : ((item.softCurrencyAmount <= 0) ? string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "iap_purchase_message"), item.displayedName, item.priceString) : string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "iap_purchase_message"), item.softCurrencyAmount + " Coins", item.priceString)));
		}
		AJavaTools.UI.ShowAlert(GluiGlobalActionHandler.Instance.gameObject.name, "onBuyClick", string.Empty, message, StringUtils.GetStringFromStringRef("LocalizedStrings", "IDS_BUY"), StringUtils.GetStringFromStringRef("LocalizedStrings", "cancel"), string.Empty);
	}

	private void onBuyClick(string info)
	{
		switch (Convert.ToInt32(info))
		{
		case -1:
			IAPData.Purchase(_iapProductId);
			break;
		}
	}
}
