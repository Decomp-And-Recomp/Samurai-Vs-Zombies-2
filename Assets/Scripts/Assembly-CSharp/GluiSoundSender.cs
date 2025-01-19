using UnityEngine;

[AddComponentMenu("")]
public class GluiSoundSender
{
	public static void SendGluiSound(string sound, GameObject sender)
	{
		if (!string.IsNullOrEmpty(sound) && GluiGlobalSoundHandler.Instance != null)
		{
			GluiGlobalSoundHandler.Instance.HandleSound(sound, sender);
		}
	}
}
