using UnityEngine;

public abstract class AbilityHandlerComponent : MonoBehaviour, IAbilityHandler
{
	protected Character mExecutor;

	protected AbilityHandler handlerObject { get; set; }

	public float gravityAccel
	{
		get
		{
			return handlerObject.gravityAccel;
		}
		private set
		{
		}
	}

	public bool leftToRightGameplay
	{
		get
		{
			return handlerObject.leftToRightGameplay;
		}
		protected set
		{
			handlerObject.leftToRightGameplay = value;
		}
	}

	public AbilitySchema schema
	{
		get
		{
			return handlerObject.schema;
		}
		set
		{
			handlerObject.schema = value;
		}
	}

	public string id
	{
		get
		{
			return handlerObject.id;
		}
	}

	public float levelDamage
	{
		get
		{
			return handlerObject.levelDamage;
		}
		private set
		{
		}
	}

	public int abilityLevel
	{
		get
		{
			return handlerObject.abilityLevel;
		}
		private set
		{
		}
	}

	public void Init(AbilitySchema sch, Character executor)
	{
		mExecutor = executor;
		handlerObject = new AbilityHandler();
		handlerObject.schema = sch;
		handlerObject.activatingPlayer = ((executor == null) ? 1 : executor.ownerId);
		leftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.LeftToRight;
		if (executor != null && executor.ownerId == 1)
		{
			leftToRightGameplay = !leftToRightGameplay;
		}
		float spawnOffsetHorizontal = schema.spawnOffsetHorizontal;
		float num = ((!leftToRightGameplay) ? (0f - spawnOffsetHorizontal) : spawnOffsetHorizontal);
		Vector3 position = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(0).position;
		if (executor != null)
		{
			position = GetSpawnPoint(executor.controlledObject);
		}
		position.y += schema.spawnOffsetVertical;
		position.z += num;
		base.gameObject.transform.position = position;
	}

	protected virtual Vector3 GetSpawnPoint(GameObject executor)
	{
		if ((bool)executor)
		{
			return executor.transform.position;
		}
		return WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.transform.position;
	}

	public float Extrapolate(LevelValueAccessor accessor)
	{
		return handlerObject.Extrapolate(accessor);
	}
}
