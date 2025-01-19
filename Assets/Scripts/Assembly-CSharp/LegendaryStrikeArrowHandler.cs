using System.Collections.Generic;
using UnityEngine;

public class LegendaryStrikeArrowHandler : AbilityHandlerComponent
{
	private float mDamagePerHit;

	private Vector3 mSpawnPos;

	private GameObject arrowFX;

	private List<GameObject> mArrows = new List<GameObject>(8);

	private List<Vector3> mArrowVelocity = new List<Vector3>();

	private void Start()
	{
		GameObjectPool.DefaultObjectPool.Release(base.gameObject, Extrapolate((AbilityLevelSchema als) => als.duration) + 0.5f);
		mDamagePerHit = levelDamage / (float)mArrows.Capacity;
		Vector3 vector = new Vector3(0f, 0f, Extrapolate((AbilityLevelSchema als) => als.speed));
		arrowFX = schema.prop;
		bool flag = false;
		Character character = null;
		if (mExecutor != null)
		{
			if (mExecutor.ownerId != 0)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(0);
			}
		}
		else
		{
			character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(0);
		}
		mSpawnPos = base.transform.position;
		if (character != null)
		{
			mSpawnPos = character.transform.position;
			if (character.LeftToRight)
			{
				mSpawnPos.z += schema.spawnOffsetHorizontal;
			}
			else
			{
				mSpawnPos.z -= schema.spawnOffsetHorizontal;
				flag = true;
			}
		}
		else if (mExecutor != null)
		{
			mSpawnPos = mExecutor.transform.position;
			if (mExecutor.LeftToRight)
			{
				mSpawnPos.z -= schema.spawnOffsetHorizontal * 0.5f;
				flag = true;
			}
			else
			{
				mSpawnPos.z += schema.spawnOffsetHorizontal * 0.5f;
			}
		}
		mSpawnPos.y += schema.spawnOffsetVertical;
		mArrows.Clear();
		mArrowVelocity.Clear();
		float num = Extrapolate((AbilityLevelSchema als) => als.distance);
		for (int i = 0; i < mArrows.Capacity; i++)
		{
			float num2 = 180f - (float)mArrows.Count * num / (float)(mArrows.Capacity - 1);
			if (flag)
			{
				num2 = 180f - num2;
			}
			Quaternion quaternion = Quaternion.Euler(num2, 0f, 0f);
			GameObject gameObject = GameObjectPool.DefaultObjectPool.Acquire(arrowFX, mSpawnPos, quaternion);
			GameObjectPool.DefaultObjectPool.Release(gameObject, Extrapolate((AbilityLevelSchema als) => als.duration));
			gameObject.transform.parent = null;
			mArrows.Add(gameObject);
			mArrowVelocity.Add(quaternion * vector);
		}
	}

	private void Update()
	{
		float num = Extrapolate((AbilityLevelSchema als) => als.radius);
		int num2 = 0;
		foreach (GameObject mArrow in mArrows)
		{
			if (mArrow != null)
			{
				float num3 = mArrow.transform.position.z;
				float num4 = mArrow.transform.position.z;
				if (mArrowVelocity[num2].z < 0f)
				{
					num3 += num + mArrowVelocity[num2].z * Time.deltaTime;
				}
				else
				{
					num4 += num + mArrowVelocity[num2].z * Time.deltaTime;
				}
				List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(num3, num4, 1 - base.handlerObject.activatingPlayer);
				foreach (Character item in charactersInRange)
				{
					bool flag = false;
					float num5 = mArrow.transform.position.y - WeakGlobalInstance<RailManager>.Instance.GetY(mArrow.transform.position.z);
					if (item.isFlying)
					{
						if (num5 < 3f && (double)num5 > 1.5)
						{
							flag = true;
						}
					}
					else if ((double)num5 <= 1.5)
					{
						flag = true;
					}
					if (flag)
					{
						item.RecievedAttack(EAttackType.Blade, mDamagePerHit, null);
						GameObjectPool.DefaultObjectPool.Release(mArrow);
						break;
					}
				}
				Vector3 vector = mArrowVelocity[num2];
				int index;
				int index2 = (index = 1);
				float num6 = vector[index];
				vector[index2] = num6 - 8f * Time.deltaTime;
				mArrowVelocity[num2] = vector;
				Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, vector);
				mArrow.transform.Translate(vector * Time.deltaTime, Space.World);
				mArrow.transform.rotation = rotation;
			}
			num2++;
		}
	}
}
