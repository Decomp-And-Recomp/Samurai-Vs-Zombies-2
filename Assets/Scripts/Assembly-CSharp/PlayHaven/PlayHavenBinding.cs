using System;
using System.Collections;
using System.Runtime.CompilerServices;
using LitJson;
using UnityEngine;

namespace PlayHaven
{
	public class PlayHavenBinding : IDisposable
	{
		public enum RequestType
		{
			Open = 0,
			Metadata = 1,
			Content = 2,
			Preload = 3,
			CrossPromotionWidget = 4
		}

		public interface IPlayHavenRequest
		{
			int HashCode { get; }

			event GeneralHandler OnWillDisplay;

			event GeneralHandler OnDidDisplay;

			event SuccessHandler OnSuccess;

			event ErrorHandler OnError;

			event DismissHandler OnDismiss;

			event RewardHandler OnReward;

			event PurchaseHandler OnPurchasePresented;

			void Send();

			void Send(bool showsOverlayImmediately);

			void TriggerEvent(string eventName, JsonData eventData);
		}

		public class OpenRequest : IPlayHavenRequest
		{
			private int hashCode;

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			[method: MethodImpl(32)]
			public event SuccessHandler OnSuccess = delegate
			{
			};

			[method: MethodImpl(32)]
			public event ErrorHandler OnError = delegate
			{
			};

			[method: MethodImpl(32)]
			public event DismissHandler OnDismiss;

			[method: MethodImpl(32)]
			public event RewardHandler OnReward;

			[method: MethodImpl(32)]
			public event PurchaseHandler OnPurchasePresented;

			[method: MethodImpl(32)]
			public event GeneralHandler OnWillDisplay;

			[method: MethodImpl(32)]
			public event GeneralHandler OnDidDisplay;

			public OpenRequest(string customUDID)
			{
			}

			public OpenRequest()
			{
				sRequests.Add(GetHashCode(), this);
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				hashCode = GetHashCode();
				if (Application.isEditor)
				{
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "success";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else if (PlayHavenManager.IsAndroidSupported)
				{
					obj_PlayHavenFacade.Call("openRequest", hashCode);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "success") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnError(this, eventData);
				}
			}
		}

		public class MetadataRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			[method: MethodImpl(32)]
			public event SuccessHandler OnSuccess = delegate
			{
			};

			[method: MethodImpl(32)]
			public event ErrorHandler OnError = delegate
			{
			};

			[method: MethodImpl(32)]
			public event DismissHandler OnDismiss;

			[method: MethodImpl(32)]
			public event RewardHandler OnReward;

			[method: MethodImpl(32)]
			public event PurchaseHandler OnPurchasePresented;

			[method: MethodImpl(32)]
			public event GeneralHandler OnWillDisplay = delegate
			{
			};

			[method: MethodImpl(32)]
			public event GeneralHandler OnDidDisplay = delegate
			{
			};

			public MetadataRequest(string placement)
			{
				mPlacement = placement;
				sRequests.Add(GetHashCode(), this);
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				hashCode = GetHashCode();
				if (Application.isEditor)
				{
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["type"] = "badge";
						hashtable["value"] = "1";
						Hashtable hashtable2 = new Hashtable();
						hashtable2["notification"] = hashtable;
						Hashtable hashtable3 = new Hashtable();
						hashtable3["data"] = hashtable2;
						hashtable3["hash"] = hashCode;
						hashtable3["name"] = "success";
						hashtable3["content"] = mPlacement;
						string json = JsonMapper.ToJson(hashtable3);
						instance.HandleNativeEvent(json);
					}
				}
				else if (PlayHavenManager.IsAndroidSupported)
				{
					obj_PlayHavenFacade.Call("metaDataRequest", hashCode, mPlacement);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "success") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "willdisplay") == 0)
				{
					this.OnWillDisplay(this);
				}
				else if (string.Compare(eventName, "diddisplay") == 0)
				{
					this.OnDidDisplay(this);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnError(this, eventData);
				}
			}
		}

		public class ContentRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			[method: MethodImpl(32)]
			public event SuccessHandler OnSuccess;

			[method: MethodImpl(32)]
			public event DismissHandler OnDismiss = delegate
			{
			};

			[method: MethodImpl(32)]
			public event ErrorHandler OnError = delegate
			{
			};

			[method: MethodImpl(32)]
			public event RewardHandler OnReward = delegate
			{
			};

			[method: MethodImpl(32)]
			public event PurchaseHandler OnPurchasePresented = delegate
			{
			};

			[method: MethodImpl(32)]
			public event GeneralHandler OnWillDisplay = delegate
			{
			};

			[method: MethodImpl(32)]
			public event GeneralHandler OnDidDisplay = delegate
			{
			};

			public ContentRequest(string placement)
			{
				mPlacement = placement;
				sRequests.Add(GetHashCode(), this);
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				hashCode = GetHashCode();
				if (Application.isEditor)
				{
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "dismiss";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else if (PlayHavenManager.IsAndroidSupported)
				{
					obj_PlayHavenFacade.Call("contentRequest", hashCode, mPlacement);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "reward") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnReward(this, eventData);
				}
				else if (string.Compare(eventName, "purchasePresentation") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnPurchasePresented(this, eventData);
				}
				else if (string.Compare(eventName, "dismiss") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnDismiss(this, eventData);
				}
				else if (string.Compare(eventName, "willdisplay") == 0)
				{
					this.OnWillDisplay(this);
				}
				else if (string.Compare(eventName, "diddisplay") == 0)
				{
					this.OnDidDisplay(this);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnError(this, eventData);
				}
			}
		}

		public class ContentPreloadRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			[method: MethodImpl(32)]
			public event SuccessHandler OnSuccess = delegate
			{
			};

			[method: MethodImpl(32)]
			public event DismissHandler OnDismiss;

			[method: MethodImpl(32)]
			public event ErrorHandler OnError = delegate
			{
			};

			[method: MethodImpl(32)]
			public event RewardHandler OnReward;

			[method: MethodImpl(32)]
			public event PurchaseHandler OnPurchasePresented;

			[method: MethodImpl(32)]
			public event GeneralHandler OnWillDisplay;

			[method: MethodImpl(32)]
			public event GeneralHandler OnDidDisplay;

			public ContentPreloadRequest(string placement)
			{
				mPlacement = placement;
				sRequests.Add(GetHashCode(), this);
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				hashCode = GetHashCode();
				if (Application.isEditor)
				{
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "dismiss";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else if (PlayHavenManager.IsAndroidSupported)
				{
					obj_PlayHavenFacade.Call("preloadRequest", hashCode, mPlacement);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "gotcontent") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					if (Debug.isDebugBuild)
					{
					}
					this.OnError(this, eventData);
				}
			}
		}

		public delegate void SuccessHandler(IPlayHavenRequest request, JsonData responseData);

		public delegate void ErrorHandler(IPlayHavenRequest request, JsonData errorData);

		public delegate void RewardHandler(IPlayHavenRequest request, JsonData rewardData);

		public delegate void PurchaseHandler(IPlayHavenRequest request, JsonData purchaseData);

		public delegate void DismissHandler(IPlayHavenRequest request, JsonData dismissData);

		public delegate void GeneralHandler(IPlayHavenRequest request);

		public static string token;

		public static string secret;

		public static IPlayHavenListener listener;

		public static AndroidJavaObject obj_PlayHavenFacade;

		protected static Hashtable sRequests = new Hashtable();

		private static bool optOutStatus;

		public static bool OptOutStatus
		{
			get
			{
				return optOutStatus;
			}
			set
			{
				optOutStatus = value;
			}
		}

		public void Dispose()
		{
			if (obj_PlayHavenFacade != null)
			{
				obj_PlayHavenFacade.Dispose();
			}
		}

		public static void Initialize()
		{
			if (!PlayHavenManager.IsAndroidSupported)
			{
				return;
			}
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					obj_PlayHavenFacade = new AndroidJavaObject("com.playhaven.unity3d.PlayHavenFacade", androidJavaObject, token, secret);
				}
			}
		}

		public static void SetKeys(string token, string secret)
		{
			PlayHavenBinding.token = token;
			PlayHavenBinding.secret = secret;
			if (obj_PlayHavenFacade != null)
			{
				obj_PlayHavenFacade.Call("setKeys", token, secret);
			}
		}

		public static int Open()
		{
			return SendRequest(RequestType.Open, string.Empty);
		}

		public static int Open(string customUDID)
		{
			return SendRequest(RequestType.Open, customUDID);
		}

		public static void CancelRequest(int requestId)
		{
		}

		public static void RegisterActivityForTracking(bool register)
		{
			if (PlayHavenManager.IsAndroidSupported)
			{
				obj_PlayHavenFacade.Call((!register) ? "unregister" : "register");
			}
		}

		public static void SendProductPurchaseResolution(PurchaseResolution resolution)
		{
			if (!Application.isEditor)
			{
				obj_PlayHavenFacade.Call("reportResolution", (int)resolution);
			}
		}

		public static void SendIAPTrackingRequest(Purchase purchase, PurchaseResolution resolution)
		{
			if (!Application.isEditor)
			{
				obj_PlayHavenFacade.Call("iapTrackingRequest", purchase.productIdentifier, purchase.quantity, (int)resolution);
			}
		}

		public static int SendRequest(RequestType type, string placement)
		{
			return SendRequest(type, placement, false);
		}

		public static int SendRequest(RequestType type, string placement, bool showsOverlayImmediately)
		{
			IPlayHavenRequest playHavenRequest = null;
			switch (type)
			{
			case RequestType.Open:
				playHavenRequest = new OpenRequest(placement);
				playHavenRequest.OnSuccess += HandleOpenRequestOnSuccess;
				playHavenRequest.OnError += HandleOpenRequestOnError;
				break;
			case RequestType.Metadata:
				playHavenRequest = new MetadataRequest(placement);
				playHavenRequest.OnSuccess += HandleMetadataRequestOnSuccess;
				playHavenRequest.OnError += HandleMetadataRequestOnError;
				playHavenRequest.OnWillDisplay += HandleMetadataRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleMetadataRequestOnDidDisplay;
				break;
			case RequestType.Content:
				playHavenRequest = new ContentRequest(placement);
				playHavenRequest.OnError += HandleContentRequestOnError;
				playHavenRequest.OnDismiss += HandleContentRequestOnDismiss;
				playHavenRequest.OnReward += HandleContentRequestOnReward;
				playHavenRequest.OnPurchasePresented += HandleRequestOnPurchasePresented;
				playHavenRequest.OnWillDisplay += HandleContentRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleContentRequestOnDidDisplay;
				break;
			case RequestType.Preload:
				playHavenRequest = new ContentPreloadRequest(placement);
				playHavenRequest.OnError += HandleContentRequestOnError;
				playHavenRequest.OnSuccess += HandlePreloadRequestOnSuccess;
				break;
			case RequestType.CrossPromotionWidget:
				playHavenRequest = new ContentRequest("more_games");
				playHavenRequest.OnError += HandleCrossPromotionWidgetRequestOnError;
				playHavenRequest.OnDismiss += HandleCrossPromotionWidgetRequestOnDismiss;
				playHavenRequest.OnWillDisplay += HandleCrossPromotionWidgetRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleCrossPromotionWidgetRequestOnDidDisplay;
				break;
			}
			if (playHavenRequest != null)
			{
				playHavenRequest.Send(showsOverlayImmediately);
				return playHavenRequest.HashCode;
			}
			return 0;
		}

		private static void HandlePreloadRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyPreloadSuccess(request.HashCode);
			}
		}

		private static Error CreateErrorFromJSON(JsonData errorData)
		{
			Error error = new Error();
			try
			{
				error.code = (int)errorData["code"];
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
			try
			{
				error.description = (string)errorData["description"];
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
			return error;
		}

		private static void HandleCrossPromotionWidgetRequestOnDismiss(IPlayHavenRequest request, JsonData dismissData)
		{
			if (listener != null)
			{
				listener.NotifyCrossPromotionWidgetDismissed();
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyCrossPromotionWidgetError(request.HashCode, error);
			}
		}

		private static void HandleContentRequestOnDismiss(IPlayHavenRequest request, JsonData dismissData)
		{
			DismissType dismissType = DismissType.Unknown;
			try
			{
				switch ((string)dismissData["type"])
				{
				case "ApplicationTriggered":
					dismissType = DismissType.PHPublisherApplicationBackgroundTriggeredDismiss;
					break;
				case "ContentUnitTriggered":
					dismissType = DismissType.PHPublisherContentUnitTriggeredDismiss;
					break;
				case "CloseButtonTriggered":
					dismissType = DismissType.PHPublisherNativeCloseButtonTriggeredDismiss;
					break;
				case "NoContentTriggered":
					dismissType = DismissType.PHPublisherNoContentTriggeredDismiss;
					break;
				}
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
			if (listener != null)
			{
				listener.NotifyContentDismissed(request.HashCode, dismissType);
			}
		}

		private static void HandleContentRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleContentRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleContentRequestOnReward(IPlayHavenRequest request, JsonData rewardData)
		{
			Reward reward = new Reward();
			try
			{
				reward.receipt = (string)rewardData["receipt"];
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
			try
			{
				reward.name = (string)rewardData["name"];
				reward.quantity = (int)rewardData["quantity"];
				if (listener != null)
				{
					listener.NotifyRewardGiven(request.HashCode, reward);
				}
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
		}

		private static void HandleRequestOnPurchasePresented(IPlayHavenRequest request, JsonData purchaseData)
		{
			Purchase purchase = new Purchase();
			try
			{
				purchase.receipt = (string)purchaseData["receipt"];
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
			try
			{
				purchase.productIdentifier = (string)purchaseData["productIdentifier"];
				purchase.quantity = (int)purchaseData["quantity"];
				if (listener != null)
				{
					listener.NotifyPurchasePresented(request.HashCode, purchase);
				}
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
		}

		private static void HandleContentRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyContentError(request.HashCode, error);
			}
		}

		private static void HandleMetadataRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyMetaDataError(request.HashCode, error);
			}
		}

		private static void HandleMetadataRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			string text;
			try
			{
				text = (string)responseData["notification"]["type"];
			}
			catch (Exception)
			{
				if (Debug.isDebugBuild)
				{
				}
				text = string.Empty;
			}
			if (!(text == "badge"))
			{
				return;
			}
			try
			{
				string badge = (string)responseData["notification"]["value"];
				if (listener != null)
				{
					listener.NotifyBadgeUpdate(request.HashCode, badge);
				}
			}
			catch (Exception)
			{
				if (!Debug.isDebugBuild)
				{
				}
			}
		}

		private static void HandleMetadataRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleMetadataRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleOpenRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyOpenError(request.HashCode, error);
			}
		}

		private static void HandleOpenRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyOpenSuccess(request.HashCode);
			}
		}

		public static IPlayHavenRequest GetRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
			{
				return (IPlayHavenRequest)sRequests[hash];
			}
			return null;
		}

		public static void ClearRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
			{
				sRequests.Remove(hash);
			}
		}
	}
}
