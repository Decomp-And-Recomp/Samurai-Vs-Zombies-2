using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class PushNotification : MonoBehaviour
{
	private static int mResendCount;

	private static readonly int MaxRetries = 4;

	public static bool isAvailable = true;

	private Action mOnNotification;

	private static GameObject sPushNotificationObject;

	public Action OnNotification
	{
		get
		{
			return mOnNotification;
		}
		set
		{
			mOnNotification = value;
		}
	}

	public void Start()
	{
		NUF.RegisterPushNotificationCallback(base.gameObject.name, "OnReceivePush");
	}

	public static void SendPushNotification(string targetToken, string message, int badgeNumber = 1, string sound = null)
	{
		mResendCount = 0;
		isAvailable = false;
		SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(DoPushNotification(targetToken, message, badgeNumber, sound));
	}

	private static IEnumerator DoPushNotification(string targetToken, string message, int badgeNumber, string sound)
	{
		string body = ((!string.IsNullOrEmpty(sound)) ? string.Format("{{\"device_tokens\": [\"{0}\"], \"aps\": {{\"alert\": \"{1}\", \"badge\" : {2}, \"sound\" : \"{3}\"}} }}", targetToken, message, badgeNumber, sound) : string.Format("{{\"device_tokens\": [\"{0}\"], \"aps\": {{\"alert\": \"{1}\", \"badge\" : {2}}} }}", targetToken, message, badgeNumber));
		byte[] bytes = Encoding.UTF8.GetBytes(body);
		Hashtable headers = new Hashtable();
		string authString4 = string.Empty;
		authString4 += GeneralConfig.UrbanAirshipKey;
		authString4 += ":";
		authString4 += GeneralConfig.UrbanAirshipSecret;
		string encodedAuthString3 = string.Empty;
		encodedAuthString3 += "Basic ";
		encodedAuthString3 += Convert.ToBase64String(Encoding.Default.GetBytes(authString4));
		headers["Authorization"] = encodedAuthString3;
		headers["Content-Type"] = "application/json";
		do
		{
			WWW pushRequest = new WWW("HTTPS://go.urbanairship.com/api/push/", bytes, headers);
			yield return pushRequest;
			if (!string.IsNullOrEmpty(pushRequest.error))
			{
				mResendCount++;
				yield return null;
				continue;
			}
			break;
		}
		while (mResendCount < MaxRetries);
		isAvailable = true;
	}

	public static void RegisterCallback(Action onNotification)
	{
		if (!sPushNotificationObject)
		{
			sPushNotificationObject = new GameObject("PushNotification");
			UnityEngine.Object.DontDestroyOnLoad(sPushNotificationObject);
			PushNotification pushNotification = sPushNotificationObject.AddComponent<PushNotification>();
			pushNotification.OnNotification = onNotification;
		}
	}

	private void OnReceivePush()
	{
		if (mOnNotification != null)
		{
			mOnNotification();
		}
	}

	public static bool Toggle()
	{
		int num = ((PlayerPrefs.GetInt("PUSH_NOTIFICATIONS_DISABLED") == 0) ? 1 : 0);
		PlayerPrefs.SetInt("PUSH_NOTIFICATIONS_DISABLED", num);
		if (num != 0)
		{
			NUF.CancelAllLocalNotification();
		}
		ANotificationManager.SetEnabled(num == 0);
		return num == 0;
	}

	public static bool IsEnabled()
	{
		return PlayerPrefs.GetInt("PUSH_NOTIFICATIONS_DISABLED") == 0;
	}
}
