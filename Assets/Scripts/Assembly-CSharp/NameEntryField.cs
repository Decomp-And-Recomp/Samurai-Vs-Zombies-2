using UnityEngine;

public class NameEntryField : MonoBehaviour, IGluiActionHandler
{
	public GluiText PlayerName;

	private void Start()
	{
		if (PlayerName != null)
		{
			PlayerName.Text = Singleton<Profile>.Instance.MultiplayerData.UserName;
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "CHANGE_NAME")
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_NAME_ENTRY_TEXT", PlayerName.Text);
			GluiActionSender.SendGluiAction("MENU_MAIN_NAME_ENTRY", sender, data);
			return true;
		}
		return false;
	}
}
