using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/SummonLightningHandler")]
public class SummonLightningHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
		base.Activate(executor);
	}

	public override void Execute(Character executor)
	{
		float range = Extrapolate((AbilityLevelSchema als) => als.radius);
		Hero hero = executor as Hero;
		if (hero != null)
		{
			List<Character> enemiesAhead = hero.GetEnemiesAhead(range);
			DoEffectOnCharacters(enemiesAhead, hero, hero);
		}
	}

	protected void DoEffectOnCharacters(List<Character> opponents, Hero hero, Character attacker)
	{
		float num = Extrapolate((AbilityLevelSchema als) => als.DOTDuration);
		float damage = levelDamage;
		float damagePerTick = Extrapolate((AbilityLevelSchema als) => als.DOTDamage);
		float tickFrequency = Extrapolate((AbilityLevelSchema als) => als.DOTFrequency);
		float fadeInTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "ColorEffectFadeIn"));
		float fadeOutTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "ColorEffectFadeOut"));
		float holdTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "ColorEffectHoldColor"));
		GameObject resultFX = schema.resultFX;
		GameObject activateFX = schema.activateFX;
		GameObject prefab = schema.prefab;
		if (hero != null)
		{
			float z = hero.controller.transform.position.z + schema.spawnOffsetHorizontal;
			float y = WeakGlobalInstance<RailManager>.Instance.GetY(z);
			if ((bool)prefab)
			{
				GameObjectPool.DefaultObjectPool.Release(GameObjectPool.DefaultObjectPool.Acquire(prefab, new Vector3(hero.controller.transform.position.x, y, z), Quaternion.identity), 3f);
			}
		}
		foreach (Character opponent in opponents)
		{
			if (!(opponent is Gate))
			{
				if ((bool)resultFX)
				{
					GameObjectPool.DefaultObjectPool.Release(opponent.controller.SpawnEffectAtJoint(resultFX, "head_effect", true), num);
				}
				if ((bool)activateFX)
				{
					GameObjectPool.DefaultObjectPool.Release(opponent.controller.SpawnEffectAtJoint(activateFX, "body_effect", true), num);
				}
				opponent.RecievedAttack(EAttackType.Lightning, damage, attacker);
				opponent.SetDamageOverTime(damagePerTick, tickFrequency, num, attacker);
				opponent.MaterialColorFlicker(Color.yellow, fadeInTime, holdTime, fadeOutTime, num);
			}
		}
	}
}
