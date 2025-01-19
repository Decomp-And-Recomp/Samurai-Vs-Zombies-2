using UnityEngine;

public class LegendaryStrikeTornadoHandler : SummonTornadoHandler
{
	protected override void Start()
	{
		base.Start();
		Vector3 position = base.transform.position;
		if (mExecutor != null)
		{
			Character character = null;
			if (mExecutor.ownerId != 0)
			{
				character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(0);
			}
			if (character != null)
			{
				position.z = character.transform.position.z;
			}
			else if (mExecutor.LeftToRight)
			{
				position.z += 3f;
			}
			else
			{
				position.z -= 3f;
			}
			base.transform.position = position;
		}
	}
}
