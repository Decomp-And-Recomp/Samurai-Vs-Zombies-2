using UnityEngine;

public class InspireHandler : AbilityHandlerComponent
{
	private float mDamageBonus;

	private float mSpeedBonus;

	private float mRemainingDuration;

	private GameObject mHeroEffect;

	private void Start()
	{
		mRemainingDuration = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
		mDamageBonus = Extrapolate((AbilityLevelSchema als) => als.damageMultEachTarget);
		mSpeedBonus = Extrapolate((AbilityLevelSchema als) => als.speed);
		Hero hero = mExecutor as Hero;
		if (hero != null)
		{
			GameObject prop = schema.prop;
			mHeroEffect = hero.controller.SpawnEffectAtJoint(prop, "body_effect", true);
			hero.damageBuffPercent = mDamageBonus * 0.01f;
			hero.damageBuffEffect = prop;
			hero.speedBuffModifier = mSpeedBonus;
			hero.buffAffectsSelf = true;
		}
		else
		{
			mHeroEffect = null;
		}
	}

	private void Update()
	{
		mRemainingDuration -= Time.deltaTime;
		if (mRemainingDuration <= 0f)
		{
			Hero hero = mExecutor as Hero;
			hero.damageBuffPercent = 0f;
			hero.damageBuffEffect = null;
			hero.speedBuffModifier = 1f;
			GameObjectPool.DefaultObjectPool.Release(mHeroEffect);
			mHeroEffect = null;
			hero.buffAffectsSelf = false;
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
	}
}
