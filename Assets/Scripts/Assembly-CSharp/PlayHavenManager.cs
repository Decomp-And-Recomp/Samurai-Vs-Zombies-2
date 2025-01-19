using System;
using System.Collections;
using System.Runtime.CompilerServices;
using LitJson;
using PlayHaven;
using UnityEngine;

[AddComponentMenu("PlayHaven/Manager")]
public class PlayHavenManager : MonoBehaviour, IPlayHavenListener
{
	public enum WhenToOpen
	{
		Awake = 0,
		Start = 1,
		Manual = 2
	}

	public enum WhenToGetNotifications
	{
		Disabled = 0,
		Awake = 1,
		Start = 2,
		OnEnable = 3,
		Manual = 4,
		Poll = 5
	}

	public delegate void CancelRequestHandler(int requestId);

	public const int NO_HASH_CODE = 0;

	public static string KEY_LAUNCH_COUNT = "playhaven-launch-count";

	public string token = string.Empty;

	[HideInInspector]
	public bool lockToken;

	public string secret = string.Empty;

	[HideInInspector]
	public bool lockSecret;

	public string tokenAndroid = string.Empty;

	[HideInInspector]
	public bool lockTokenAndroid;

	public string secretAndroid = string.Empty;

	[HideInInspector]
	public bool lockSecretAndroid;

	public bool doNotDestroyOnLoad = true;

	public bool defaultShowsOverlayImmediately;

	public bool maskShowsOverlayImmediately;

	public WhenToOpen whenToSendOpen;

	public WhenToGetNotifications whenToGetNotifications = WhenToGetNotifications.Start;

	public float notificationPollDelay = 1f;

	public float notificationPollRate = 15f;

	public bool cancelAllOnLevelLoad;

	private ArrayList requestsInProgress = new ArrayList(8);

	public int suppressContentRequestsForLaunches;

	public string[] suppressedPlacements;

	public string[] suppressionExceptions;

	private int launchCount;

	public bool showContentUnitsInEditor = true;

	private string badge = string.Empty;

	private string customUDID = string.Empty;

	private bool networkReachable = true;

	public bool maskNetworkReachable;

	public bool isAndroidSupported;

	private static PlayHavenManager _instance;

	private static bool wasWarned;

	public bool isNetworkReachable
	{
		get
		{
			return networkReachable;
		}
	}

	public static PlayHavenManager instance
	{
		get
		{
			if (!_instance)
			{
				_instance = FindInstance();
			}
			return _instance;
		}
	}

	public string CustomUDID
	{
		get
		{
			return customUDID;
		}
		set
		{
			customUDID = value;
		}
	}

	public bool OptOutStatus
	{
		get
		{
			return PlayHavenBinding.OptOutStatus;
		}
		set
		{
			PlayHavenBinding.OptOutStatus = value;
		}
	}

	public static bool IsAndroidSupported
	{
		get
		{
			PlayHavenManager playHavenManager = instance;
			if (playHavenManager != null)
			{
				return playHavenManager.isAndroidSupported;
			}
			return false;
		}
	}

	public string Badge
	{
		get
		{
			return badge;
		}
	}

	[method: MethodImpl(32)]
	public event RequestCompletedHandler OnRequestCompleted;

	[method: MethodImpl(32)]
	public event BadgeUpdateHandler OnBadgeUpdate;

	[method: MethodImpl(32)]
	public event RewardTriggerHandler OnRewardGiven;

	[method: MethodImpl(32)]
	public event PurchasePresentedTriggerHandler OnPurchasePresented;

	[method: MethodImpl(32)]
	public event SimpleDismissHandler OnDismissCrossPromotionWidget;

	[method: MethodImpl(32)]
	public event DismissHandler OnDismissContent;

	[method: MethodImpl(32)]
	public event WillDisplayContentHandler OnWillDisplayContent;

	[method: MethodImpl(32)]
	public event DidDisplayContentHandler OnDidDisplayContent;

	[method: MethodImpl(32)]
	public event SuccessHandler OnSuccessOpenRequest;

	[method: MethodImpl(32)]
	public event SuccessHandler OnSuccessPreloadRequest;

	[method: MethodImpl(32)]
	public event ErrorHandler OnErrorOpenRequest;

	[method: MethodImpl(32)]
	public event ErrorHandler OnErrorCrossPromotionWidget;

	[method: MethodImpl(32)]
	public event ErrorHandler OnErrorContentRequest;

	[method: MethodImpl(32)]
	public event ErrorHandler OnErrorMetadataRequest;

	[method: MethodImpl(32)]
	public event CancelRequestHandler OnSuccessCancelRequest;

	[method: MethodImpl(32)]
	public event CancelRequestHandler OnErrorCancelRequest;

	private static PlayHavenManager FindInstance()
	{
		PlayHavenManager playHavenManager = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as PlayHavenManager;
		if (!playHavenManager)
		{
			GameObject gameObject = GameObject.Find("PlayHavenManager");
			if (gameObject != null)
			{
				playHavenManager = gameObject.GetComponent<PlayHavenManager>();
			}
		}
		if (!playHavenManager && !wasWarned)
		{
			wasWarned = true;
		}
		return playHavenManager;
	}

	private void Awake()
	{
		_instance = FindInstance();
		DetectNetworkReachable();
		if (token.Length == 0)
		{
		}
		if (secret.Length == 0)
		{
		}
		base.gameObject.name = GetType().ToString();
		if (doNotDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		PlayHavenBinding.SetKeys(tokenAndroid, secretAndroid);
		PlayHavenBinding.listener = this;
		PlayHavenBinding.Initialize();
		if (suppressContentRequestsForLaunches > 0)
		{
			launchCount = PlayerPrefs.GetInt(KEY_LAUNCH_COUNT, 0);
			launchCount++;
			PlayerPrefs.SetInt(KEY_LAUNCH_COUNT, launchCount);
			PlayerPrefs.Save();
			if (!Debug.isDebugBuild)
			{
			}
		}
		if (whenToSendOpen == WhenToOpen.Awake)
		{
			OpenNotification();
		}
		if (whenToGetNotifications == WhenToGetNotifications.Awake)
		{
			BadgeRequest();
		}
	}

	private void OnEnable()
	{
		if (whenToGetNotifications == WhenToGetNotifications.OnEnable)
		{
			BadgeRequest();
		}
	}

	private void Start()
	{
		if (whenToSendOpen == WhenToOpen.Start)
		{
			OpenNotification();
		}
		if (whenToGetNotifications == WhenToGetNotifications.Start)
		{
			BadgeRequest();
		}
		else if (whenToGetNotifications == WhenToGetNotifications.Poll)
		{
			PollForBadgeRequests();
		}
	}

	private void DetectNetworkReachable()
	{
		networkReachable = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		networkReachable &= !maskNetworkReachable;
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			DetectNetworkReachable();
		}
		PlayHavenBinding.RegisterActivityForTracking(!pause);
	}

	private void OnLevelWasLoaded(int level)
	{
		if (cancelAllOnLevelLoad)
		{
			CancelAllPendingRequests();
		}
	}

	public bool IsPlacementSuppressed(string placement)
	{
		if (suppressContentRequestsForLaunches > 0 && launchCount < suppressContentRequestsForLaunches)
		{
			if (suppressedPlacements != null && suppressedPlacements.Length > 0)
			{
				string[] array = suppressedPlacements;
				foreach (string text in array)
				{
					if (text == placement)
					{
						return true;
					}
				}
				return false;
			}
			if (suppressionExceptions != null && suppressionExceptions.Length > 0)
			{
				string[] array2 = suppressionExceptions;
				foreach (string text2 in array2)
				{
					if (text2 == placement)
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}
		return false;
	}

	public int OpenNotification(string customUDID)
	{
		if (networkReachable)
		{
			CustomUDID = customUDID;
			int num = PlayHavenBinding.Open(customUDID);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int OpenNotification()
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.Open(CustomUDID);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public void CancelAllPendingRequests()
	{
		foreach (int item in requestsInProgress)
		{
			PlayHavenBinding.CancelRequest(item);
		}
		requestsInProgress.Clear();
	}

	public void ProductPurchaseResolutionRequest(PurchaseResolution resolution)
	{
		PlayHavenBinding.SendProductPurchaseResolution(resolution);
	}

	public void ProductPurchaseTrackingRequest(Purchase purchase, PurchaseResolution resolution)
	{
		PlayHavenBinding.SendIAPTrackingRequest(purchase, resolution);
	}

	public int ContentPreloadRequest(string placement)
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Preload, placement);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int ContentRequest(string placement)
	{
		if (IsPlacementSuppressed(placement))
		{
			return 0;
		}
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Content, placement, defaultShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int ContentRequest(string placement, bool showsOverlayImmediately)
	{
		if (IsPlacementSuppressed(placement))
		{
			return 0;
		}
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Content, placement, showsOverlayImmediately && !maskShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	[Obsolete("This method is obsolete; it assumes that you will have a placement called more_games; instead, simply use ContentRequest() but with the relevant placement.", false)]
	public int ShowCrossPromotionWidget()
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.CrossPromotionWidget, string.Empty, defaultShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int BadgeRequest()
	{
		if (networkReachable && whenToGetNotifications != 0)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Metadata, "more_games");
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public void PollForBadgeRequests()
	{
		CancelInvoke("BadgeRequest");
		if (notificationPollRate > 0f)
		{
			InvokeRepeating("BadgeRequest", notificationPollDelay, notificationPollRate);
		}
	}

	public void NotifyRequestCompleted(int requestId)
	{
		requestsInProgress.Remove(requestId);
		if (this.OnRequestCompleted != null)
		{
			this.OnRequestCompleted(requestId);
		}
	}

	public void NotifyOpenSuccess(int requestId)
	{
		if (this.OnSuccessOpenRequest != null)
		{
			this.OnSuccessOpenRequest(requestId);
		}
	}

	public void NotifyOpenError(int requestId, Error error)
	{
		if (this.OnErrorOpenRequest != null)
		{
			this.OnErrorOpenRequest(requestId, error);
		}
	}

	public void NotifyWillDisplayContent(int requestId)
	{
		if (this.OnWillDisplayContent != null)
		{
			this.OnWillDisplayContent(requestId);
		}
	}

	public void NotifyDidDisplayContent(int requestId)
	{
		if (this.OnDidDisplayContent != null)
		{
			this.OnDidDisplayContent(requestId);
		}
	}

	public void NotifyPreloadSuccess(int requestId)
	{
		if (this.OnSuccessPreloadRequest != null)
		{
			this.OnSuccessPreloadRequest(requestId);
		}
	}

	public void NotifyBadgeUpdate(int requestId, string badge)
	{
		this.badge = badge;
		if (this.OnBadgeUpdate != null)
		{
			this.OnBadgeUpdate(requestId, badge);
		}
	}

	public void NotifyRewardGiven(int requestId, Reward reward)
	{
		if (this.OnRewardGiven != null)
		{
			this.OnRewardGiven(requestId, reward);
		}
	}

	public void NotifyPurchasePresented(int requestId, Purchase purchase)
	{
		if (this.OnPurchasePresented != null)
		{
			this.OnPurchasePresented(requestId, purchase);
		}
	}

	public void NotifyCrossPromotionWidgetDismissed()
	{
		if (this.OnDismissCrossPromotionWidget != null)
		{
			this.OnDismissCrossPromotionWidget();
		}
	}

	public void NotifyCrossPromotionWidgetError(int requestId, Error error)
	{
		if (this.OnErrorCrossPromotionWidget != null)
		{
			this.OnErrorCrossPromotionWidget(requestId, error);
		}
	}

	public void NotifyContentDismissed(int requestId, DismissType dismissType)
	{
		if (this.OnDismissContent != null)
		{
			this.OnDismissContent(requestId, dismissType);
		}
	}

	public void NotifyContentError(int requestId, Error error)
	{
		if (this.OnErrorContentRequest != null)
		{
			this.OnErrorContentRequest(requestId, error);
		}
	}

	public void NotifyMetaDataError(int requestId, Error error)
	{
		if (this.OnErrorMetadataRequest != null)
		{
			this.OnErrorMetadataRequest(requestId, error);
		}
	}

	public void ClearBadge()
	{
		badge = string.Empty;
	}

	public void HandleNativeEvent(string json)
	{
		if (Debug.isDebugBuild)
		{
		}
		JsonData jsonData = JsonMapper.ToObject(json);
		int hash = (int)jsonData["hash"];
		PlayHavenBinding.IPlayHavenRequest requestWithHash = PlayHavenBinding.GetRequestWithHash(hash);
		if (requestWithHash != null)
		{
			string text = (string)jsonData["name"];
			JsonData eventData = jsonData["data"];
			requestWithHash.TriggerEvent(text, eventData);
			if (requestWithHash is PlayHavenBinding.ContentRequest && text == "reward")
			{
				StartCoroutine(DelayedClearRequestWithHash(1f, hash));
			}
			else if (text != "willdisplay" && text != "diddisplay" && text != "gotcontent")
			{
				PlayHavenBinding.ClearRequestWithHash(hash);
			}
		}
	}

	private IEnumerator DelayedClearRequestWithHash(float delay, int hash)
	{
		yield return new WaitForSeconds(delay);
		PlayHavenBinding.ClearRequestWithHash(hash);
	}

	public void RequestCancelSuccess(string hashCodeString)
	{
		int num = Convert.ToInt32(hashCodeString);
		PlayHavenBinding.ClearRequestWithHash(num);
		if (this.OnSuccessCancelRequest != null)
		{
			this.OnSuccessCancelRequest(num);
		}
	}

	public void RequestCancelFailed(string hashCodeString)
	{
		if (this.OnErrorCancelRequest != null)
		{
			int requestId = Convert.ToInt32(hashCodeString);
			this.OnErrorCancelRequest(requestId);
		}
	}
}
