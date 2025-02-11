using System;
using System.Collections.Generic;

namespace Glu
{
	public static class Tapjoy
	{
		public enum OfferwallState
		{
			Open = 0,
			Closed = 1,
			Failed = 2
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.Tapjoy");
			}
		}

		public delegate void VideoAdStateChangedHandler(bool videoIsPlaying);

		public delegate void OfferwallStateChangedHandler(OfferwallState state);

		private static TapjoyInterfaceInternal _instanceRef;

		private static bool _isInitialized;

		private static string _appId;

		private static string _userId;

		public static string appId
		{
			get
			{
				return _appId;
			}
		}

		public static string userId
		{
			get
			{
				return _userId;
			}
		}

		public static bool isVideoPlaying
		{
			get
			{
				return _instance.isVideoPlaying;
			}
		}

		public static bool isOfferwallOpen
		{
			get
			{
				return _instance.isOfferwallOpen;
			}
		}

		private static TapjoyInterfaceInternal _instance
		{
			get
			{
				if ((object)_instanceRef == null)
				{
					_instanceRef = TapjoyInterfaceInternal.CreateInstance();
				}
				return _instanceRef;
			}
		}

		public static event VideoAdStateChangedHandler onVideoAdStateChanged
		{
			add
			{
				if (_instance.videoAdStateChangedHandler == null)
				{
					_instance.videoAdStateChangedHandler = value.Invoke;
					return;
				}
				TapjoyInterfaceInternal instance = _instance;
				instance.videoAdStateChangedHandler = (VideoAdStateChangedHandler)Delegate.Combine(instance.videoAdStateChangedHandler, value);
			}
			remove
			{
				if (_isInitialized && _instance.videoAdStateChangedHandler != null)
				{
					TapjoyInterfaceInternal instance = _instance;
					instance.videoAdStateChangedHandler = (VideoAdStateChangedHandler)Delegate.Remove(instance.videoAdStateChangedHandler, value);
				}
			}
		}

		public static event OfferwallStateChangedHandler onOfferwallStateChanged
		{
			add
			{
				if (_instance.offerwallStateChangedHandler == null)
				{
					_instance.offerwallStateChangedHandler = value.Invoke;
					return;
				}
				TapjoyInterfaceInternal instance = _instance;
				instance.offerwallStateChangedHandler = (OfferwallStateChangedHandler)Delegate.Combine(instance.offerwallStateChangedHandler, value);
			}
			remove
			{
				if (_isInitialized && _instance.offerwallStateChangedHandler != null)
				{
					TapjoyInterfaceInternal instance = _instance;
					instance.offerwallStateChangedHandler = (OfferwallStateChangedHandler)Delegate.Remove(instance.offerwallStateChangedHandler, value);
				}
			}
		}

		public static void Initialize(string appId, string secretKey, bool showVideoAds)
		{
			Initialize(appId, secretKey, showVideoAds, null);
		}

		public static void Initialize(string appId, string secretKey, bool showVideoAds, string userId)
		{
			if (!_isInitialized)
			{
				_appId = appId;
				_userId = userId;
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				if (!showVideoAds)
				{
					dictionary.Add("TJC_OPTION_DISABLE_VIDEOS", "true");
				}
				_isInitialized = true;
			}
		}

		public static void OpenOfferwall()
		{
			_instance.isOfferwallOpen = true;
			if (_instance.offerwallStateChangedHandler != null)
			{
				_instance.offerwallStateChangedHandler(OfferwallState.Open);
			}
		}

		public static uint GetPoints()
		{
			return _instance.serverTapjoyPoints;
		}

		public static void ConsumePoints(uint points)
		{
			if (points <= _instance.serverTapjoyPoints)
			{
				_instance.serverTapjoyPoints -= points;
			}
		}
	}
}
