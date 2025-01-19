using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/DivineInterventionHandler")]
public class DivineInterventionHandler : AbilityHandler
{
	private void SpawnHelper(Character executor, Hero hero, int randomHelper, BoxCollider helperSpawnAreaRef, float zAdjust)
	{
		Vector3 position = executor.transform.position;
		position.z += zAdjust + Random.Range(-1f, 1f);
		WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(hero.ownerId).SpawnForFree(randomHelper, helperSpawnAreaRef.bounds.size.x, position);
	}

	private void SpawnHelper(Character executor, Hero hero, string randomHelper, BoxCollider helperSpawnAreaRef, float zAdjust)
	{
		Vector3 position = executor.transform.position;
		position.z += zAdjust + Random.Range(-1f, 1f);
		WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(hero.ownerId).SpawnForFree(randomHelper, helperSpawnAreaRef.bounds.size.x, position);
	}

	public override void Execute(Character executor)
	{
		Hero hero = executor as Hero;
		if (hero == null)
		{
			return;
		}
		Leadership leadership = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(hero.ownerId);
		float num = Mathf.Max(Extrapolate((AbilityLevelSchema als) => als.effectModifier), 1f);
		List<Leadership.HelperTypeData> availableHelperTypes = leadership.availableHelperTypes;
		BoxCollider helperSpawnArea = leadership.helperSpawnArea;
		float num2 = schema.spawnOffsetHorizontal;
		if (executor != null && !executor.LeftToRight)
		{
			num2 *= -1f;
		}
		List<int> list = new List<int>(availableHelperTypes.Count);
		int num3 = 0;
		float num4 = float.MaxValue;
		int num5 = -1;
		foreach (Leadership.HelperTypeData item in availableHelperTypes)
		{
			if (!item.data.unique)
			{
				float leadership2 = item.data.leadershipCost.leadership;
				if (leadership2 < num4)
				{
					num4 = leadership2;
					num5 = num3;
				}
				list.Add(num3);
			}
			num3++;
		}
		GameObjectPool.DefaultObjectPool.Release(GameObjectPool.DefaultObjectPool.Acquire(schema.activateFX, executor.transform.position + new Vector3(0f, 0f, num2), Quaternion.identity), 3f);
		while (num > 0f)
		{
			if (list.Count <= 0)
			{
				SpawnHelper(executor, hero, "Farmer", helperSpawnArea, num2);
				num -= Singleton<HelpersDatabase>.Instance["Farmer"].resourcesCost;
				continue;
			}
			int num6 = list[Random.Range(0, list.Count)];
			float num7 = Mathf.Max(1, WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(hero.ownerId).GetCost(num6));
			if (num7 > num && num5 >= 0)
			{
				num7 = Mathf.Max(1, WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(hero.ownerId).GetCost(num5));
				SpawnHelper(executor, hero, num5, helperSpawnArea, num2);
			}
			else
			{
				if (num6 < 0)
				{
					break;
				}
				SpawnHelper(executor, hero, num6, helperSpawnArea, num2);
			}
			num -= num7;
		}
	}
}
