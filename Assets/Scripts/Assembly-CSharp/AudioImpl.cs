using UnityEngine;

public class AudioImpl : MonoBehaviour, IGluiActionHandler
{
	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "PLAY_UI_SOUNDTEST":
			GluiSoundSender.SendGluiSound("UI_SoundTest", sender);
			return true;
		default:
			return false;
		}
	}
}
