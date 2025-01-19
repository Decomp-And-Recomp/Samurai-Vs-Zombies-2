using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/TroopTrampleHandler")]
public class TroopTrampleHandler : AbilityHandlerComponent
{
	private float mRemainingDamage;

	private void Start()
	{
		float z = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(base.handlerObject.activatingPlayer).helperSpawnArea.transform.position.z;
		float y = WeakGlobalInstance<RailManager>.Instance.GetY(z);
		base.transform.Translate(0f, y - base.transform.position.y, z - base.transform.position.z);
		mRemainingDamage = levelDamage;
	}

	private void Update()
	{
		float z = base.gameObject.transform.position.z;
		if (mRemainingDamage <= 0f || (leftToRightGameplay && z >= WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkRightEdge.position.z) || (!leftToRightGameplay && z <= WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkLeftEdge.position.z))
		{
			GameObject obj = GameObjectPool.DefaultObjectPool.Acquire(schema.resultFX, base.transform.position, Quaternion.identity);
			GameObjectPool.DefaultObjectPool.Release(obj, 3f);
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
			return;
		}
		float num = 2f;
		float num2 = Extrapolate((AbilityLevelSchema als) => als.speed) * ((!leftToRightGameplay) ? (0f - Time.deltaTime) : Time.deltaTime);
		float num3 = base.transform.position.z;
		float num4 = base.transform.position.z;
		float num5 = num2 + num;
		if (leftToRightGameplay)
		{
			num4 += num5;
		}
		else
		{
			num3 -= num5;
		}
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(num3, num4, 1 - base.handlerObject.activatingPlayer);
		foreach (Character item in charactersInRange)
		{
			if (mRemainingDamage <= 0f)
			{
				break;
			}
			if (item != null && !item.isBase && !item.isPlayer)
			{
				float num6 = Mathf.Min(item.health, mRemainingDamage);
				item.RecievedAttack(EAttackType.Trample, num6, mExecutor);
				mRemainingDamage -= num6;
			}
		}
		float y = WeakGlobalInstance<RailManager>.Instance.GetY(base.transform.position.z + num2);
		base.transform.Translate(0f, y - base.transform.position.y, num2, Space.World);
	}
}
