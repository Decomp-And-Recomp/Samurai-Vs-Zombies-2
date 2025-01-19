using UnityEngine;

public class AbilityHandler : IAbilityHandler
{
	public readonly float kGravityAccel = 9.8f;

	protected Vector3 mVelocity = Vector3.zero;

	public float gravityAccel
	{
		get
		{
			return kGravityAccel;
		}
	}

	public bool leftToRightGameplay { get; set; }

	public AbilitySchema schema { get; set; }

	public string id
	{
		get
		{
			return schema.id;
		}
	}

	public float levelDamage
	{
		get
		{
			float num = Extrapolate((AbilityLevelSchema als) => als.damage);
			if (activatingPlayer != 0 && Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
			{
				num *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroAbilityDamage;
			}
			return num;
		}
		set
		{
		}
	}

	public int abilityLevel
	{
		get
		{
			return AbilityLevel(schema);
		}
		set
		{
		}
	}

	public int activatingPlayer { get; set; }

	public virtual void Activate(Character executor)
	{
		activatingPlayer = ((executor == null) ? 1 : executor.ownerId);
	}

	public virtual void Execute(Character executor)
	{
	}

	private int AbilityLevel(AbilitySchema schema)
	{
		if (activatingPlayer == 0)
		{
			return Singleton<Profile>.Instance.GetAbilityLevel(schema.id);
		}
		return Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetAbilityLevel(schema.id);
	}

	public float Extrapolate(LevelValueAccessor accessor)
	{
		return schema.Extrapolate(AbilityLevel(schema), accessor);
	}
}
