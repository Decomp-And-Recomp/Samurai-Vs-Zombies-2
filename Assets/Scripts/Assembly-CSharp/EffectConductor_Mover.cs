using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Mover")]
public class EffectConductor_Mover : EffectConductor
{
	public enum Reaction
	{
		None = 0,
		DisableEffect = 1,
		KillEffect = 2
	}

	public enum MoverActivity
	{
		Start = 0,
		Reverse = 1,
		Stop = 2,
		None = 3
	}

	public float motionStartDelay;

	public MotionPath requiredMotionPathComponent_OverrideSearchOnSelf;

	protected MotionPath motionSystem;

	public string actionOnDone;

	public Reaction reactionOnDone = Reaction.KillEffect;

	public MoverActivity activityOnAwake;

	public EffectConductor_Sound sound;

	public override void Awake()
	{
		base.Awake();
		if (motionSystem == null)
		{
			motionSystem = requiredMotionPathComponent_OverrideSearchOnSelf;
			if (motionSystem == null)
			{
				motionSystem = base.gameObject.GetComponent(typeof(MotionPath)) as MotionPath;
			}
			if (!(motionSystem == null))
			{
			}
		}
		if (sound == null)
		{
			sound = GetComponent<EffectConductor_Sound>();
		}
	}

	public override void Start()
	{
		base.Start();
		switch (activityOnAwake)
		{
		case MoverActivity.Start:
			Activity_Start();
			break;
		case MoverActivity.Reverse:
			Activity_Reverse();
			break;
		case MoverActivity.Stop:
			Activity_Stop();
			break;
		}
	}

	private void Update()
	{
		if (!(motionSystem != null) || !motionSystem.PathActive || !(effectContainer != null))
		{
			return;
		}
		if (motionStartDelay <= 0f)
		{
			Vector2 position = motionSystem.UpdatePosition();
			effectContainer.EffectMove(position);
			if (motionSystem.useScale)
			{
				effectContainer.EffectScale(motionSystem.UpdateScale());
			}
			if (!motionSystem.PathActive)
			{
				OnDone();
			}
		}
		else
		{
			motionStartDelay -= Time.deltaTime;
			if (sound != null && motionStartDelay <= 0f)
			{
				sound.StartSound();
			}
		}
	}

	public override void Activity_Start()
	{
		if (motionSystem != null && effectContainer != null)
		{
			Vector2 startPosition = motionSystem.GoForward(effectContainer.Owner);
			StartMove(startPosition);
		}
	}

	public override void Activity_Reverse()
	{
		if (motionSystem != null && effectContainer != null)
		{
			Vector2 startPosition = motionSystem.GoBackward(effectContainer.Owner);
			StartMove(startPosition);
		}
	}

	public override void Activity_Stop()
	{
		if (motionSystem != null && effectContainer != null)
		{
			motionSystem.Stop();
			OnDone();
		}
	}

	private void StartMove(Vector2 startPosition)
	{
		effectContainer.EffectEnable();
		effectContainer.EffectMove(startPosition);
		if (sound != null && motionStartDelay <= 0f)
		{
			sound.StartSound();
		}
	}

	private void OnDone()
	{
		DoReaction(reactionOnDone);
		GluiActionSender.SendGluiAction(actionOnDone, base.gameObject, null);
		if (sound != null)
		{
			sound.Activity_Stop();
		}
	}

	private void DoReaction(Reaction reaction)
	{
		switch (reaction)
		{
		case Reaction.KillEffect:
			effectContainer.EffectKill();
			break;
		case Reaction.DisableEffect:
			effectContainer.EffectDisable();
			break;
		}
	}

	public override void SetStartPosition(Vector2 position)
	{
		if (motionSystem != null)
		{
			motionSystem.nodes.SetStartPosition(position);
		}
	}

	public void SetEndPosition(Vector2 position)
	{
		if (motionSystem != null)
		{
			motionSystem.nodes.SetEndPosition(position);
		}
	}
}
