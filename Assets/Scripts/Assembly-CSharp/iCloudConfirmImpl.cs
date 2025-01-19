using System;
using UnityEngine;

public class iCloudConfirmImpl : MonoBehaviour, IGluiActionHandler
{
	[Serializable]
	public class Slot
	{
		public GluiText text_title;

		public GluiText text_description;

		public GluiText text_currencyHard;

		public GluiText text_currencySoft;

		public GluiText text_lastPlayed;

		public GluiText text_powerRating;
	}

	public GluiText text_header;

	public GluiText text_button;

	public Slot[] slots;

	private void Start()
	{
		string text = (string)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("ICLOUD_ACTION");
		DeviceData deviceData = Singleton<Profile>.Instance.saveData.GetDeviceData("local");
		DeviceData deviceData2 = ((Singleton<Profile>.Instance.CloudSaveResult != SaveProvider.Result.Success) ? null : Singleton<Profile>.Instance.CloudSave.GetDeviceData("cloud"));
		int num;
		int num2;
		if (text == "SAVE")
		{
			num = 0;
			num2 = 1;
			text_header.Text = StringUtils.GetStringFromStringRef("LocalizedStrings", "icloud_save_button_text");
			text_button.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_Save");
			if (deviceData2 == null)
			{
				slots[num2].text_description.Text = StringUtils.GetStringFromStringRef("LocalizedStrings", "icloud_no_save");
			}
			else
			{
				slots[num2].text_description.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_LocalAlert");
			}
		}
		else
		{
			num = 1;
			num2 = 0;
			text_header.Text = StringUtils.GetStringFromStringRef("LocalizedStrings", "icloud_load_button_text");
			text_button.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_Load");
			slots[num].text_description.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_iCloudAlert");
		}
		slots[num].text_title.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_Local");
		slots[num2].text_title.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_iCloud");
		slots[num].text_currencyHard.Text = StringUtils.FormatAmountString(Singleton<Profile>.Instance.gems);
		slots[num].text_currencySoft.Text = StringUtils.FormatAmountString(Singleton<Profile>.Instance.coins);
		DateTime dateTime = deviceData.Current.SaveTime.ToLocalTime();
		GluiText text_lastPlayed = slots[num].text_lastPlayed;
		string stringFromStringRef = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_TimeStamp");
		string arg = dateTime.Date.ToShortDateString();
		DateTime dateTime2 = new DateTime(dateTime.TimeOfDay.Ticks);
		text_lastPlayed.Text = string.Format(stringFromStringRef, arg, dateTime2.ToShortTimeString());
		slots[num].text_powerRating.Text = StringUtils.FormatAmountString(Singleton<Profile>.Instance.playerAttackRating);
		if (deviceData2 != null)
		{
			slots[num2].text_currencyHard.gameObject.SetActive(true);
			slots[num2].text_currencySoft.gameObject.SetActive(true);
			slots[num2].text_lastPlayed.gameObject.SetActive(true);
			slots[num2].text_powerRating.gameObject.SetActive(true);
			slots[num2].text_currencyHard.Text = StringUtils.FormatAmountString(Singleton<Profile>.Instance.CloudSave.GetValueInt("gems"));
			slots[num2].text_currencySoft.Text = StringUtils.FormatAmountString(Singleton<Profile>.Instance.CloudSave.GetValueInt("coins"));
			dateTime = deviceData2.Latest.SaveTime.ToLocalTime();
			GluiText text_lastPlayed2 = slots[num2].text_lastPlayed;
			string stringFromStringRef2 = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_TimeStamp");
			string arg2 = dateTime.Date.ToShortDateString();
			DateTime dateTime3 = new DateTime(dateTime.TimeOfDay.Ticks);
			text_lastPlayed2.Text = string.Format(stringFromStringRef2, arg2, dateTime3.ToShortTimeString());
			slots[num2].text_powerRating.Text = StringUtils.FormatAmountString((int)deviceData2.Latest["attackRating"]);
		}
		else
		{
			slots[num2].text_currencyHard.gameObject.SetActive(false);
			slots[num2].text_currencySoft.gameObject.SetActive(false);
			slots[num2].text_lastPlayed.gameObject.SetActive(false);
			slots[num2].text_powerRating.gameObject.SetActive(false);
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "BUTTON_CONFIRM":
			if ((string)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("ICLOUD_ACTION") == "SAVE")
			{
				Singleton<Profile>.Instance.SaveToCloud();
			}
			else
			{
				Singleton<Profile>.Instance.CopyCloudSaveToLocal();
			}
			GluiActionSender.SendGluiAction("ALERT_EMPTY", base.gameObject, null);
			GluiActionSender.SendGluiAction("POPUP_POP", base.gameObject, null);
			return true;
		default:
			return false;
		}
	}
}
