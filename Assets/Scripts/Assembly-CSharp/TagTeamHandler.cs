using UnityEngine;

public class TagTeamHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
		base.Activate(executor);
		if (executor != null)
		{
			Vector3 position = executor.transform.position;
			position.z += schema.spawnOffsetHorizontal * ((!executor.LeftToRight) ? (-1f) : 1f);
			Hero hero = new Hero(position, executor.ownerId, WeakGlobalMonoBehavior<InGameImpl>.Instance.TagHeroID, false);
			TagTeamAbilityComponent tagTeamAbilityComponent = hero.controlledObject.GetComponent<TagTeamAbilityComponent>();
			if (tagTeamAbilityComponent == null)
			{
				tagTeamAbilityComponent = hero.controlledObject.AddComponent<TagTeamAbilityComponent>();
			}
			tagTeamAbilityComponent.TagHero = hero;
			tagTeamAbilityComponent.schema = schema;
			GameObject obj = GameObjectPool.DefaultObjectPool.Acquire(schema.prop, position, Quaternion.identity);
			GameObjectPool.DefaultObjectPool.Release(obj, 1.5f);
		}
	}
}
