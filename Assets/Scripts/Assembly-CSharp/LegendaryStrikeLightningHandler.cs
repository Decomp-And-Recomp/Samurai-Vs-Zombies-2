using System.Collections.Generic;
using UnityEngine;

public class LegendaryStrikeLightningHandler : SummonLightningHandler
{
	public override void Execute(Character executor)
	{
		float num = Mathf.Max(2f, schema.spawnOffsetHorizontal);
		Hero hero = WeakGlobalMonoBehavior<InGameImpl>.Instance.hero;
		float num2 = 0f;
		if (hero != null)
		{
			num2 = hero.transform.position.z;
			if (executor == hero)
			{
				num2 = ((!executor.LeftToRight) ? (num2 - num) : (num2 + num));
			}
		}
		int ownerId = ((executor != null) ? (1 - executor.ownerId) : 0);
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(num2 - num, num2 + num, ownerId);
		if (hero != null && hero != executor)
		{
			charactersInRange.Add(hero);
		}
		DoEffectOnCharacters(charactersInRange, executor as Hero, null);
	}
}
