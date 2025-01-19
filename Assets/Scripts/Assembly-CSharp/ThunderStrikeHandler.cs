using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/ThunderStrikeHandler")]
public class ThunderStrikeHandler : AbilityHandlerComponent
{
	private void Start()
	{
		base.transform.position = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(base.handlerObject.activatingPlayer).controller.position;
		float num = Extrapolate((AbilityLevelSchema als) => als.radius);
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - num, base.transform.position.z + num, 1 - base.handlerObject.activatingPlayer);
		foreach (Character item in charactersInRange)
		{
			if (item != null)
			{
				item.RecievedAttack(EAttackType.Stomp, levelDamage, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero);
			}
		}
		GameObjectPool.DefaultObjectPool.Release(base.gameObject, 1.5f);
	}
}
