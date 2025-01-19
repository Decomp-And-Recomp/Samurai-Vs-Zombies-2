using UnityEngine;

[AddComponentMenu("Game/ExplosiveCartHandler")]
public class ExplosiveCartHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
		base.Activate(executor);
		if (executor != null)
		{
			Character character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(executor.ownerId).ForceSpawn("CartOfDoom");
			character.meleeDamage = levelDamage;
			float radius = Extrapolate((AbilityLevelSchema als) => als.radius);
			character.explosionRange = () => radius;
		}
	}
}
