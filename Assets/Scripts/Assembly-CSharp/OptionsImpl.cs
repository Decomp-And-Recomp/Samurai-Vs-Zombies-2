using UnityEngine;

public class OptionsImpl : MonoBehaviour, IGluiActionHandler
{
	public GameObject notificationToggleObject;

	private GluiStandardButtonContainer mNotificationButton;

	private GluiSprite mNotificationTogglerLabel;

	private void Start()
	{
		SetupNotificationToggler();
		if (!GeneralConfig.iCloudEnabled)
		{
			GameObject gameObject = base.gameObject.FindChild("Button_iCloud");
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		GameObject gameObject2 = base.gameObject.FindChild("Button_Credits");
		GameObject gameObject3 = base.gameObject.FindChild("Button_Privacy");
		GameObject gameObject4 = base.gameObject.FindChild("Button_Restore");
		if (gameObject2 != null && gameObject3 != null && gameObject4 != null)
		{
			gameObject3.transform.localPosition = gameObject4.transform.localPosition;
			gameObject4.SetActive(false);
			gameObject2.transform.localPosition += new Vector3(300f, 0f, 0f);
		}
	}

	private void Update()
	{
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "PUSH_NOTIFICATION_TOGGLE":
			ToggleNotifications();
			return true;
		case "BUTTON_FAQ":
		{
			AStats.Flurry.LogEvent("FORUM_BUTTON_CLICKED");
			string text = ConfigSchema.Entry("FAQ_URL");
			if (!string.IsNullOrEmpty(text))
			{
				Application.OpenURL(text);
			}
			break;
		}
		}
		return false;
	}

	private void SetupNotificationToggler()
	{
		mNotificationButton = notificationToggleObject.GetComponent<GluiStandardButtonContainer>();
		mNotificationTogglerLabel = notificationToggleObject.FindChildComponent<GluiSprite>("Art_Checkbox_On");
		RefreshNotificationToggler();
	}

	private void ToggleNotifications()
	{
		PushNotification.Toggle();
		RefreshNotificationToggler();
	}

	private void RefreshNotificationToggler()
	{
		bool flag = PushNotification.IsEnabled();
		mNotificationButton.Selected = flag;
		if (mNotificationTogglerLabel != null)
		{
			mNotificationTogglerLabel.Visible = flag;
		}
	}
}
