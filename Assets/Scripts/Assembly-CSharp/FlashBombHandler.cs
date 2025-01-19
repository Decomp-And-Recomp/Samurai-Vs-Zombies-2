using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/FlashBombHandler")]
public class FlashBombHandler : AbilityHandlerComponent
{
	private Vector3 mVelocity = Vector2.zero;

	private void Start()
	{
		float num = Mathf.Max(0.0001f, Extrapolate((AbilityLevelSchema als) => als.duration));
		mVelocity.z = Extrapolate((AbilityLevelSchema als) => als.distance) / num;
		if (!leftToRightGameplay)
		{
			mVelocity.z = 0f - mVelocity.z;
		}
		mVelocity.y = 0.5f * gravityAccel * num;
	}

	private void Update()
	{
		Vector3 translation = mVelocity * Time.deltaTime;
		base.transform.Translate(translation);
		float y = WeakGlobalInstance<RailManager>.Instance.GetY(base.transform.position.z);
		float num = Extrapolate((AbilityLevelSchema als) => als.radius);
		if (base.transform.position.y < y)
		{
			base.transform.Translate(0f, y - base.transform.position.y, 0f);
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.transform.position.z - num, base.transform.position.z + num, 1 - base.handlerObject.activatingPlayer);
			float num2 = Extrapolate((AbilityLevelSchema als) => als.damage);
			float healAmount = num2 * Extrapolate((AbilityLevelSchema als) => als.lifeSteal);
			bool flag = false;
			foreach (Character item in charactersInRange)
			{
				if (item != null)
				{
					item.controller.stunnedTimer = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
					item.RecievedAttack(EAttackType.Flash, num2, mExecutor);
					flag = true;
				}
			}
			if (flag && mExecutor != null)
			{
				mExecutor.RecievedHealing(healAmount);
			}
			GameObject obj = GameObjectPool.DefaultObjectPool.Acquire(schema.resultFX, base.transform.position, Quaternion.identity);
			GameObjectPool.DefaultObjectPool.Release(obj, 1f);
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
		else
		{
			mVelocity.y -= gravityAccel * Time.deltaTime;
		}
	}
}
