using UnityEngine;

[AddComponentMenu("Glui Action/Listener - Play Sound")]
public class GluiActionListener_PlaySound : GluiListenerSingle
{
	public string soundToPlay;

	protected override void OnTrigger(GameObject sender, object data)
	{
		GluiSoundSender.SendGluiSound(soundToPlay, base.gameObject);
	}
}
