using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/RepelHandler")]
public class RepelHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
	}

	public override void Execute(Character executor)
	{
		Hero hero = executor as Hero;
		float range = Extrapolate((AbilityLevelSchema als) => als.distance);
		float num = Extrapolate((AbilityLevelSchema als) => als.effectModifier);
		float num2 = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
		List<Character> enemiesAhead = hero.GetEnemiesAhead(range);
		foreach (Character item in enemiesAhead)
		{
			bool isPlayer = item.isPlayer;
			hero.PerformKnockback(item, (int)num, isPlayer, new Vector3(0f, num2 * 0.1f, num2));
			item.RecievedAttack(EAttackType.Force, levelDamage, hero);
		}
	}
}
