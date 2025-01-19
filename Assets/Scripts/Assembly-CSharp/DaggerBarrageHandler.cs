using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/DaggerBarrageHandler")]
public class DaggerBarrageHandler : AbilityHandlerComponent
{
	private const int daggersPerOct = 6;

	private float mDamagePerHit;

	private Vector3 mDaggerVelocity;

	private Vector3 mSpawnPos;

	private GameObject daggerFX;

	private List<GameObject> mDaggers;

	private void Start()
	{
		int capacity = (int)((Extrapolate((AbilityLevelSchema als) => als.distance) + 1f) / 45f) * 6;
		if (mDaggers == null)
		{
			mDaggers = new List<GameObject>(capacity);
		}
		else
		{
			mDaggers.Clear();
		}
		float num = Extrapolate((AbilityLevelSchema als) => als.duration);
		GameObjectPool.DefaultObjectPool.Release(base.gameObject, num + 0.5f);
		mDamagePerHit = levelDamage / 6f;
		float num2 = Extrapolate((AbilityLevelSchema als) => als.speed);
		float num3 = num2 * num;
		mDaggerVelocity.z = (num3 - schema.spawnOffsetHorizontal) / num;
		daggerFX = schema.prop;
		mSpawnPos = base.transform.position;
		mSpawnPos.z += schema.spawnOffsetHorizontal;
		mSpawnPos.x += schema.spawnOffsetVertical;
		for (int i = 0; i < mDaggers.Capacity; i++)
		{
			float num4 = (float)mDaggers.Count * (0f - Extrapolate((AbilityLevelSchema als) => als.distance)) / (float)(mDaggers.Capacity - 1);
			if (mExecutor != null && !mExecutor.LeftToRight)
			{
				num4 = 180f - num4;
			}
			Quaternion value = Quaternion.Euler(num4, 0f, 0f);
			GameObject gameObject = GameObjectPool.DefaultObjectPool.Acquire(daggerFX, mSpawnPos, value);
			GameObjectPool.DefaultObjectPool.Release(gameObject, Extrapolate((AbilityLevelSchema als) => als.duration));
			gameObject.transform.parent = null;
			mDaggers.Add(gameObject);
		}
	}

	private void Update()
	{
		float num = Extrapolate((AbilityLevelSchema als) => als.radius);
		foreach (GameObject mDagger in mDaggers)
		{
			if (!(mDagger != null))
			{
				continue;
			}
			float z = mDagger.transform.position.z;
			float zMax = mDagger.transform.position.z + num + mDaggerVelocity.z * Time.deltaTime;
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(z, zMax, 1 - base.handlerObject.activatingPlayer);
			foreach (Character item in charactersInRange)
			{
				bool flag = false;
				float num2 = mDagger.transform.position.y - WeakGlobalInstance<RailManager>.Instance.GetY(mDagger.transform.position.z);
				if (item.isFlying)
				{
					if (num2 < 3f && (double)num2 > 1.5)
					{
						flag = true;
					}
				}
				else if ((double)num2 <= 1.5)
				{
					flag = true;
				}
				if (flag)
				{
					item.RecievedAttack(EAttackType.Blade, mDamagePerHit, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero);
					break;
				}
			}
			mDagger.transform.Translate(mDaggerVelocity * Time.deltaTime);
		}
	}
}
