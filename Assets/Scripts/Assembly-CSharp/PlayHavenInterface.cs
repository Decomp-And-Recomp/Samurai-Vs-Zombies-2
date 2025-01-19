using System;
using System.Runtime.CompilerServices;
using PlayHaven;
using UnityEngine;

public static class PlayHavenInterface
{
	public delegate void DismissHandler(string placement);

	public delegate void ErrorHandler(string placement, string errorDescription);

	public delegate void PurchaseInitiatedHandler(string productId);

	public delegate void RewardGivenHandler(string name, int quantity);

	private static string activePlacement = string.Empty;

	private static int lastContentRequestHash;

	private static bool showAlert;

	private static bool isPrecachedModal;

	private static DateTime lastBadgeRequest;

	public static Version Version
	{
		get
		{
			return new Version(1, 1, 8);
		}
	}

	[method: MethodImpl(32)]
	public static event DismissHandler OnDismiss;

	[method: MethodImpl(32)]
	public static event ErrorHandler OnError;

	[method: MethodImpl(32)]
	public static event RewardGivenHandler OnRewarded;

	[method: MethodImpl(32)]
	public static event PurchaseInitiatedHandler OnPurchaseInitiated;

	public static void Init(string appToken, string secretCode)
	{
		Component component = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as Component;
		if (component == null)
		{
			GameObject gameObject = new GameObject("PlayHavenManager");
			gameObject.active = false;
			PlayHavenManager playHavenManager = gameObject.AddComponent<PlayHavenManager>();
			playHavenManager.token = appToken;
			playHavenManager.secret = secretCode;
			gameObject.active = true;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			PlayHavenManager.instance.cancelAllOnLevelLoad = true;
			PlayHavenManager.instance.OnErrorContentRequest += ContentErrorHandler;
			PlayHavenManager.instance.OnErrorCrossPromotionWidget += ContentErrorHandler;
			PlayHavenManager.instance.OnDismissContent += ContentDismissHandler;
			PlayHavenManager.instance.OnDismissCrossPromotionWidget += OldDismissHandler;
			PlayHavenManager.instance.OnPurchasePresented += PurchaseTriggered;
			PlayHavenManager.instance.OnSuccessPreloadRequest += PreloadSuccessHandler;
			PlayHavenManager.instance.OnRewardGiven += RewardHandler;
			lastBadgeRequest = DateTime.Now;
		}
	}

	public static void Destroy()
	{
		Component component = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as Component;
		if (component != null)
		{
			UnityEngine.Object.Destroy(component.gameObject);
			PlayHavenManager.instance.OnErrorContentRequest -= ContentErrorHandler;
			PlayHavenManager.instance.OnErrorCrossPromotionWidget -= ContentErrorHandler;
			PlayHavenManager.instance.OnDismissContent -= ContentDismissHandler;
			PlayHavenManager.instance.OnDismissCrossPromotionWidget -= OldDismissHandler;
			PlayHavenManager.instance.OnPurchasePresented -= PurchaseTriggered;
			PlayHavenManager.instance.OnSuccessPreloadRequest -= PreloadSuccessHandler;
			PlayHavenManager.instance.OnRewardGiven -= RewardHandler;
		}
	}

	public static void StartPublisherContentRequest(string placement, bool showOverlayImmediately, bool showErrorMessage)
	{
		activePlacement = placement;
		showAlert = showErrorMessage;
		lastContentRequestHash = PlayHavenManager.instance.ContentRequest(placement, showOverlayImmediately);
		if (lastContentRequestHash == 0 && showAlert)
		{
			displayAlert("PlayHaven error", "Network connection is required to use this feature");
		}
	}

	public static void StartPublisherContentRequest(string placement, bool showOverlayImmediately)
	{
		StartPublisherContentRequest(placement, showOverlayImmediately, false);
	}

	public static void StartPublisherContentRequestWithPreload(string placement, bool showOverlayImmediately, bool showErrorMessage)
	{
		if (lastContentRequestHash == 0 || PlayHavenBinding.GetRequestWithHash(lastContentRequestHash) == null)
		{
			activePlacement = placement;
			isPrecachedModal = showOverlayImmediately;
			showAlert = showErrorMessage;
			lastContentRequestHash = PlayHavenManager.instance.ContentPreloadRequest(placement);
			if (lastContentRequestHash == 0 && showAlert)
			{
				displayAlert("PlayHaven error", "Network connection is required to use this feature");
			}
		}
	}

	public static void StartPublisherContentRequestWithPreload(string placement, bool showOverlayImmediately)
	{
		StartPublisherContentRequestWithPreload(placement, showOverlayImmediately, false);
	}

	public static void SendIapTrackingRequest(string productId, PurchaseResolution resolution)
	{
		if (!Integrity.IsJailbroken())
		{
			Purchase purchase = new Purchase();
			purchase.productIdentifier = productId;
			purchase.quantity = 1;
			PlayHavenManager.instance.ProductPurchaseTrackingRequest(purchase, resolution);
		}
	}

	public static void SendVgpResolutionRequest(PurchaseResolution resolution)
	{
		PlayHavenManager.instance.ProductPurchaseResolutionRequest(resolution);
	}

	private static void PurchaseTriggered(int requestId, Purchase purchase)
	{
		if (PlayHavenInterface.OnPurchaseInitiated != null)
		{
			PlayHavenInterface.OnPurchaseInitiated(purchase.productIdentifier);
		}
	}

	public static void CancelCurrentContentRequest()
	{
		if (lastContentRequestHash != 0)
		{
			PlayHavenBinding.CancelRequest(lastContentRequestHash);
			lastContentRequestHash = 0;
		}
	}

	public static int GetBadgeNumber()
	{
		if ((DateTime.Now - lastBadgeRequest).TotalHours > 3.0)
		{
			PlayHavenManager.instance.BadgeRequest();
			lastBadgeRequest = DateTime.Now;
		}
		return Convert.ToInt32("0" + PlayHavenManager.instance.Badge);
	}

	private static void ContentErrorHandler(int requestId, Error error)
	{
		lastContentRequestHash = 0;
		if (PlayHavenInterface.OnError != null)
		{
			PlayHavenInterface.OnError(activePlacement, error.description);
		}
		if (showAlert)
		{
			displayAlert("PlayHaven error", "Network connection is required to use this feature");
		}
	}

	private static void ContentDismissHandler(int requestId, DismissType type)
	{
		lastContentRequestHash = 0;
		if (PlayHavenInterface.OnDismiss != null)
		{
			PlayHavenInterface.OnDismiss(activePlacement);
		}
	}

	private static void PreloadSuccessHandler(int requestId)
	{
		lastContentRequestHash = 0;
		StartPublisherContentRequest(activePlacement, isPrecachedModal, showAlert);
	}

	private static void OldDismissHandler()
	{
		lastContentRequestHash = 0;
		if (PlayHavenInterface.OnDismiss != null)
		{
			PlayHavenInterface.OnDismiss(activePlacement);
		}
	}

	private static void RewardHandler(int requestId, Reward reward)
	{
		if (PlayHavenInterface.OnRewarded != null)
		{
			PlayHavenInterface.OnRewarded(reward.name, reward.quantity);
		}
	}

	private static void displayAlert(string title, string text)
	{
	}
}
