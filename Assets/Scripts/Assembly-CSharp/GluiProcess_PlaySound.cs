using UnityEngine;

[AddComponentMenu("Glui Process/Process Play Sound")]
public class GluiProcess_PlaySound : GluiProcessSingleState
{
	public string sound;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		GluiSoundSender.SendGluiSound(sound, base.gameObject);
		return false;
	}
}
