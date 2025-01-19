using UnityEngine;

[AddComponentMenu("Game/InvincibilityHandler")]
public class InvincibilityHandler : AbilityHandlerComponent
{
	private float mDuration;

	private void Start()
	{
		mDuration = Extrapolate((AbilityLevelSchema als) => als.duration);
		Hero hero = mExecutor as Hero;
		if (hero != null)
		{
			hero.invuln = true;
			hero.controller.autoPaperdoll.AttachObjectToJoint(base.gameObject, "body_effect", true);
		}
	}

	private void Update()
	{
		mDuration -= Time.deltaTime;
		if (mDuration <= 0f)
		{
			Hero hero = mExecutor as Hero;
			if (hero != null)
			{
				hero.ResetInvulnToDefault();
			}
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
	}
}
