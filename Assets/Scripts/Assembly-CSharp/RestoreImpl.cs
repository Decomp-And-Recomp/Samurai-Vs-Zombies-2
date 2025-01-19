using UnityEngine;

public class RestoreImpl : MonoBehaviour, IGluiActionHandler
{
	private bool mRestoring;

	private float TickStartTime;

	private float TimeOutInSecs = 3f;

	private string delayedMessage = string.Empty;

	private void Start()
	{
	}

	private void Update()
	{
		if (!mRestoring)
		{
			return;
		}
		if (delayedMessage != string.Empty)
		{
			if (Time.realtimeSinceStartup - TickStartTime > TimeOutInSecs)
			{
				EndWaitMode(delayedMessage, true);
			}
			else if (SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus == ICInAppPurchase.RESTORE_STATE.SUCCESS_EMPTY)
			{
				return;
			}
		}
		switch (SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus)
		{
		case ICInAppPurchase.RESTORE_STATE.NONE:
		case ICInAppPurchase.RESTORE_STATE.ACTIVE:
			break;
		case ICInAppPurchase.RESTORE_STATE.FAILED:
			EndWaitMode("LocalizedStrings.restore_failed");
			break;
		case ICInAppPurchase.RESTORE_STATE.SUCCESS_EMPTY:
			EndWaitMode("LocalizedStrings.restore_empty");
			break;
		case ICInAppPurchase.RESTORE_STATE.SUCCESS_DELIVERY:
			EndWaitMode("LocalizedStrings.restore_delivery");
			break;
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "RESTORE_PURCHASES":
			RestorePurchases();
			return true;
		default:
			return false;
		}
	}

	private void RestorePurchases()
	{
		SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus = ICInAppPurchase.RESTORE_STATE.ACTIVE;
		StartWaitMode();
	}

	private void StartWaitMode()
	{
		mRestoring = true;
		AJavaTools.UI.StartIndeterminateProgress(17);
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.restore_inprogress");
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", null);
		GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
	}

	private void EndWaitMode(string msg, bool isDelayed = false)
	{
		if (!isDelayed && SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus == ICInAppPurchase.RESTORE_STATE.SUCCESS_EMPTY)
		{
			TickStartTime = Time.realtimeSinceStartup;
			delayedMessage = msg;
			return;
		}
		delayedMessage = string.Empty;
		SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus = ICInAppPurchase.RESTORE_STATE.NONE;
		mRestoring = false;
		NUF.StopSpinner();
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", msg);
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
		GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
	}
}
