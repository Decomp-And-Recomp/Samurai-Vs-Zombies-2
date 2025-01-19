using UnityEngine;

[AddComponentMenu("Game/DragonDamageHandler")]
public class DragonDamageHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
		base.Activate(executor);
		if (executor != null)
		{
			Character character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(executor.ownerId).ForceSpawn("DragonDamageHelper");
			character.meleeDamage = levelDamage;
		}
	}
}
