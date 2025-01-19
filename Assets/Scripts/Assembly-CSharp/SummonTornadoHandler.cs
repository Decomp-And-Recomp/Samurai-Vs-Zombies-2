using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/SummonTornadoHandler")]
public class SummonTornadoHandler : AbilityHandlerComponent
{
	private const float kDamageFrequency = 0.25f;

	private float mDamagePerHit;

	private float mTimeUntilNextDamage;

	private float mRemainingDuration;

	protected virtual void Start()
	{
		mRemainingDuration = Extrapolate((AbilityLevelSchema als) => als.duration);
		mDamagePerHit = levelDamage / (mRemainingDuration / 0.25f);
		mTimeUntilNextDamage = 0f;
	}

	protected virtual Character GetAttacker()
	{
		return (mExecutor == null) ? WeakGlobalMonoBehavior<InGameImpl>.Instance.hero : mExecutor;
	}

	private void Update()
	{
		if (mRemainingDuration > 0f)
		{
			float num = Extrapolate((AbilityLevelSchema als) => als.radius);
			mRemainingDuration -= Time.deltaTime;
			mTimeUntilNextDamage -= Time.deltaTime;
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - num, base.transform.position.z + num, 1 - base.handlerObject.activatingPlayer);
			float num2 = Extrapolate((AbilityLevelSchema als) => als.flyerDamageMultiplier);
			while (mTimeUntilNextDamage <= 0f)
			{
				mTimeUntilNextDamage += 0.25f;
				foreach (Character item in charactersInRange)
				{
					if (item != null)
					{
						item.RecievedAttack(EAttackType.Wind, (!item.isFlying) ? mDamagePerHit : (mDamagePerHit * num2), GetAttacker());
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
