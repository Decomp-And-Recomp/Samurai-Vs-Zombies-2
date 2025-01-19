using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/DestructionHandler")]
public class DestructionHandler : AbilityHandlerComponent
{
	private const int kNumExplosions = 10;

	private float mDamage;

	private float mRadius;

	private float mDuration;

	private float mExplosionInterval;

	private float mNextExplosionTime;

	private void Start()
	{
		mDuration = Extrapolate((AbilityLevelSchema als) => als.duration);
		mDamage = Extrapolate((AbilityLevelSchema als) => als.damage) / mDuration;
		mRadius = Extrapolate((AbilityLevelSchema als) => als.radius);
		mExplosionInterval = mDuration / 10f;
		mNextExplosionTime = mExplosionInterval;
	}

	private void Update()
	{
		mDuration -= Time.deltaTime;
		mNextExplosionTime -= Time.deltaTime;
		if (mNextExplosionTime <= 0f)
		{
			mNextExplosionTime = mExplosionInterval;
			Hero hero = mExecutor as Hero;
			if (hero != null)
			{
				List<Character> enemiesAround = hero.GetEnemiesAround(mRadius);
				float damage = mDamage * Mathf.Max(Time.deltaTime, mExplosionInterval);
				foreach (Character item in enemiesAround)
				{
					item.RecievedAttack(EAttackType.Explosion, damage, hero);
				}
				GameObject gameObject = GameObjectPool.DefaultObjectPool.Acquire(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/ExplosionBig.prefab", 1).Resource as GameObject);
				gameObject.transform.position = hero.transform.position + Random.rotation * Vector3.forward * 3f;
				EffectKiller.AddKiller(gameObject, GameObjectPool.DefaultObjectPool);
				CameraShaker.RequestShake(hero.position, 10f);
			}
		}
		if (mDuration <= 0f)
		{
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
	}
}
