using UnityEngine;

[AddComponentMenu("Game/FriendshipHandler")]
public class FriendshipHandler : AbilityHandler
{
	public override void Activate(Character executor)
	{
		WeakGlobalMonoBehavior<InGameImpl>.Instance.SpawnFriendshipHelper();
	}

	public override void Execute(Character executor)
	{
	}
}
