using Glu.Plugins.ASocial;
using UnityEngine;

public class iCloudScreenImpl : MonoBehaviour, IGluiActionHandler
{
	private enum State
	{
		Default = 0,
		Connecting = 1,
		Ready = 2,
		Error = 3
	}

	private GluiStandardButtonContainer btn_enabled;

	private GluiText text_button;

	private State state;

	private void Start()
	{
		initializeAutoSaveButton();
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "ICLOUD_TOGGLE":
			setAutoSaveButton(!ApplicationUtilities._autoSave);
			return true;
		case "ICLOUD_SAVE":
			return true;
		case "ICLOUD_LOAD":
			return true;
		default:
			return false;
		}
	}

	private void ConnectToICloud(bool save)
	{
		state = State.Connecting;
		bool showConnectingPopup = true;
		bool cancelled = false;
		GluiPersistentDataWatcher gluiPersistentDataWatcher = new GluiPersistentDataWatcher();
		gluiPersistentDataWatcher.PersistentEntryToWatch = "ALERT_GENERIC_RESULT";
		gluiPersistentDataWatcher.Event_WatchedDataChanged += delegate(object data)
		{
			string text = data as string;
			if (text != null && text == "BUTTON")
			{
				cancelled = true;
				showConnectingPopup = false;
				NUF.StopSpinner();
				state = State.Default;
			}
		};
		gluiPersistentDataWatcher.StartWatching();
		NUF.StartSpinner(new Vector2(Screen.width / 2, Screen.height / 2));
		Singleton<Profile>.Instance.LoadCloudSave(delegate(SaveProvider.Result result)
		{
			if (!cancelled)
			{
				NUF.StopSpinner();
				showConnectingPopup = false;
				if (result == SaveProvider.Result.Success || (result == SaveProvider.Result.NotFound && save))
				{
					state = State.Ready;
					GluiActionSender.SendGluiAction("ALERT_ICLOUD_CONFIRM", base.gameObject, null);
				}
				else
				{
					state = State.Error;
					GlobalActions.AlertClearAll = true;
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.icloud_file_not_found_notification_message_text");
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
					GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
				}
			}
		});
		if (showConnectingPopup)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.icloud_connecting");
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "LocalizedStrings.cancel");
			GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", base.gameObject, null);
		}
	}

	private void initializeAutoSaveButton()
	{
		ApplicationUtilities._autoSave = PlayerPrefs.GetInt("autoSave", 0) == 1;
		btn_enabled = base.gameObject.FindChildComponent<GluiStandardButtonContainer>("Button_OnOff");
		btn_enabled.Selected = ApplicationUtilities._autoSave;
		text_button = base.gameObject.FindChildComponent<GluiText>("Text_Button");
		text_button.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", (!ApplicationUtilities._autoSave) ? "Menu_Off" : "Menu_On");
	}

	private void setAutoSaveButton(bool _value)
	{
		PlayerPrefs.SetInt("autoSave", _value ? 1 : 0);
		ApplicationUtilities._autoSave = PlayerPrefs.GetInt("autoSave", 0) == 1;
		btn_enabled.Selected = ApplicationUtilities._autoSave;
		text_button.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", (!ApplicationUtilities._autoSave) ? "Menu_Off" : "Menu_On");
	}
}
