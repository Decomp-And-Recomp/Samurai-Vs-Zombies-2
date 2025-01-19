using System.Collections.Generic;
using UnityEngine;

public class SetTrapHandler : AbilityHandlerComponent
{
	private float mRemainingDuration;

	private float mEffectDuration;

	private float mSpeedModifier;

	private float mRadius;

	private float mTriggerRadius;

	private float mTriggerTimer;

	private bool mTriggered;

	private bool mFinished;

	private void Start()
	{
		mRemainingDuration = Extrapolate((AbilityLevelSchema als) => als.duration);
		mEffectDuration = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
		mSpeedModifier = Extrapolate((AbilityLevelSchema als) => als.effectModifier);
		mRadius = Extrapolate((AbilityLevelSchema als) => als.radius);
		mTriggerRadius = Extrapolate((AbilityLevelSchema als) => als.distance);
		mTriggerTimer = -1f;
		mTriggered = false;
		mFinished = false;
	}

	protected virtual Character GetAttacker()
	{
		return (mExecutor == null) ? WeakGlobalMonoBehavior<InGameImpl>.Instance.hero : mExecutor;
	}

	private void Update()
	{
		if (mFinished)
		{
			return;
		}
		if (!mTriggered)
		{
			mRemainingDuration -= Time.deltaTime;
			if (mRemainingDuration <= 0f)
			{
				GameObjectPool.DefaultObjectPool.Release(base.gameObject);
				return;
			}
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - mTriggerRadius, base.transform.position.z + mTriggerRadius, 1 - base.handlerObject.activatingPlayer);
			{
				foreach (Character item in charactersInRange)
				{
					if (!item.isBase && !item.isFlying)
					{
						mRemainingDuration = 5f;
						mTriggered = true;
						mTriggerTimer = 1f;
						break;
					}
				}
				return;
			}
		}
		mTriggerTimer -= Time.deltaTime;
		if (!(mTriggerTimer <= 0f))
		{
			return;
		}
		Animation animation = base.gameObject.GetComponent<Animation>();
		if (animation != null)
		{
			animation.Play("attack01");
			animation["attack01"].wrapMode = WrapMode.Once;
		}
		GameObject resultFX = schema.resultFX;
		Color color = new Color((float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute("Lethargy", "Red")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute("Lethargy", "Green")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute("Lethargy", "Blue")) / 255f);
		List<Character> charactersInRange2 = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - mRadius, base.transform.position.z + mRadius, 1 - base.handlerObject.activatingPlayer);
		foreach (Character item2 in charactersInRange2)
		{
			if (!item2.isBase && !item2.isFlying)
			{
				item2.RecievedAttack(EAttackType.Blade, levelDamage, GetAttacker());
				item2.ApplyBuff(0f, mSpeedModifier, mEffectDuration, base.gameObject, resultFX, "head_effect");
				item2.MaterialColorFadeInOut(color, 0.2f, mEffectDuration, 0.2f);
			}
		}
		GameObjectPool.DefaultObjectPool.Release(base.gameObject, 2f);
		mFinished = true;
	}
}
