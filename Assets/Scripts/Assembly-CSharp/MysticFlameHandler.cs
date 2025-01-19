using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/MysticFlameHandler")]
public class MysticFlameHandler : AbilityHandlerComponent
{
	private const float kDamageFrequency = 0.15f;

	private float mDamagePerHit;

	private float mTimeUntilNextDamage;

	private float mRemainingDuration;

	private float mRadius;

	protected virtual void Start()
	{
		mRemainingDuration = Extrapolate((AbilityLevelSchema als) => als.duration);
		mDamagePerHit = levelDamage / (mRemainingDuration / 0.15f);
		mTimeUntilNextDamage = 0f;
		mRadius = Extrapolate((AbilityLevelSchema als) => als.radius);
	}

	protected virtual Character GetAttacker()
	{
		return (mExecutor == null) ? WeakGlobalMonoBehavior<InGameImpl>.Instance.hero : mExecutor;
	}

	private void Update()
	{
		if (mRemainingDuration > 0f)
		{
			mRemainingDuration -= Time.deltaTime;
			mTimeUntilNextDamage -= Time.deltaTime;
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - mRadius, base.transform.position.z + mRadius, 1 - base.handlerObject.activatingPlayer);
			while (mTimeUntilNextDamage <= 0f)
			{
				mTimeUntilNextDamage += 0.15f;
				foreach (Character item in charactersInRange)
				{
					if (item != null)
					{
						item.RecievedAttack(EAttackType.Flame, mDamagePerHit, GetAttacker());
					}
				}
			}
		}
		else
		{
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
	}
}
