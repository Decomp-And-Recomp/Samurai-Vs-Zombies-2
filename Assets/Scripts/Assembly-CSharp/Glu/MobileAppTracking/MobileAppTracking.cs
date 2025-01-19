namespace Glu.MobileAppTracking
{
	public class MobileAppTracking
	{
		private static bool isInitialized;

		private static void MAT_initialize(string advertiserId, string key)
		{
		}

		private static void MAT_trackAction(string eventName, string itemName, float itemPrice, string currencyCode)
		{
		}

		public static void Init(string advertiserId, string appKey)
		{
			MAT_initialize(advertiserId, appKey);
			isInitialized = true;
		}

		public static void TrackAction(string eventName)
		{
			AStats.MobileAppTracking.TrackAction(eventName);
		}

		public static void TrackAction(string eventName, string itemName, float itemPrice, string currencyCode)
		{
			if (itemName != null)
			{
			}
			MAT_trackAction(eventName, itemName, itemPrice, currencyCode);
		}
	}
}
