using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Sound")]
public class EffectConductor_Sound : EffectConductor
{
	public enum SoundActivity
	{
		StartImmediately = 0,
		WaitForActivityStart = 1,
		WaitForStartSound = 2
	}

	public SoundActivity activityMode = SoundActivity.WaitForStartSound;

	public string startSoundEventName = "effect_conductor_start";

	public string loopingSoundEventName = "effect_conductor_loop";

	public string stopSoundEventName = "effect_conductor_stop";

	protected USoundThemeEventClip loopingSound;

	protected bool started;

	private void PlayStartSounds()
	{
		if (!started)
		{
			if (!string.IsNullOrEmpty(loopingSoundEventName))
			{
				StopAndDestroyLoopingSound();
				loopingSound = GluiGlobalSoundHandler.Instance.PlaySoundEvent(loopingSoundEventName);
			}
			if (!string.IsNullOrEmpty(startSoundEventName))
			{
				GluiGlobalSoundHandler.Instance.PlaySoundEvent(startSoundEventName);
			}
			started = true;
		}
	}

	public void StartSound()
	{
		if (activityMode == SoundActivity.WaitForStartSound)
		{
			PlayStartSounds();
		}
	}

	public override void Activity_Start()
	{
		if (activityMode == SoundActivity.WaitForActivityStart)
		{
			PlayStartSounds();
		}
	}

	public override void Activity_Stop()
	{
		if (started)
		{
			StopAndDestroyLoopingSound();
			if (!string.IsNullOrEmpty(stopSoundEventName))
			{
				GluiGlobalSoundHandler.Instance.PlaySoundEvent(stopSoundEventName);
			}
			started = false;
		}
	}

	private void StopAndDestroyLoopingSound()
	{
		if (loopingSound != null)
		{
			loopingSound.Stop();
			loopingSound = null;
		}
	}

	public override void Activity_Reverse()
	{
		Activity_Start();
	}

	public override void OnEffectKilled(bool destroyingContainer = false)
	{
		base.OnEffectKilled(destroyingContainer);
		if (destroyingContainer)
		{
			StopAndDestroyLoopingSound();
		}
		else
		{
			Activity_Stop();
		}
	}

	public override void Start()
	{
		base.Start();
		if (activityMode == SoundActivity.StartImmediately)
		{
			PlayStartSounds();
		}
	}
}
