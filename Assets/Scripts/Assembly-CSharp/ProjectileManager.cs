using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : WeakGlobalInstance<ProjectileManager>
{
	private List<Projectile> mProjectiles = new List<Projectile>();

	private Dictionary<string, DataBundleRecordHandle<ProjectileSchema>> data = new Dictionary<string, DataBundleRecordHandle<ProjectileSchema>>();

	public static string UdamanTableName
	{
		get
		{
			return "Projectiles";
		}
	}

	public ProjectileSchema this[string type]
	{
		get
		{
			DataBundleRecordHandle<ProjectileSchema> value;
			if (data.TryGetValue(type, out value))
			{
				return value.Data;
			}
			return null;
		}
	}

	public List<Projectile> projectiles
	{
		get
		{
			return mProjectiles;
		}
		private set
		{
			mProjectiles = value;
		}
	}

	public ProjectileManager()
	{
		SetUniqueInstance(this);
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys<ProjectileSchema>(UdamanTableName))
		{
			DataBundleRecordHandle<ProjectileSchema> dataBundleRecordHandle = new DataBundleRecordHandle<ProjectileSchema>(UdamanTableName, item);
			dataBundleRecordHandle.Load(DataBundleResourceGroup.InGame, true, null);
			data[dataBundleRecordHandle.Data.id.Key] = dataBundleRecordHandle;
		}
	}

	public void UnloadData()
	{
		foreach (DataBundleRecordHandle<ProjectileSchema> value in data.Values)
		{
			value.Unload();
		}
		data.Clear();
	}

	public void Update()
	{
		for (int num = mProjectiles.Count - 1; num >= 0; num--)
		{
			Projectile projectile = mProjectiles[num];
			if (!projectile.isDone)
			{
				projectile.Update();
			}
			if (projectile.isDone)
			{
				projectile.Destroy();
				mProjectiles.RemoveAt(num);
			}
		}
	}

	public void SpawnProjectile(string type, float damage, Character shooter, Character target, Vector3 spawnPos)
	{
		if (!(type == "None") && !(type == "SpawnFriend"))
		{
			mProjectiles.Add(new Arrow(type, shooter, target, damage, spawnPos));
		}
	}

	public bool ProjectileArcs(string type)
	{
		ProjectileSchema projectileSchema = this[type];
		if (projectileSchema != null)
		{
			return projectileSchema.arcs;
		}
		return false;
	}

	public bool ProjectileNeedsBothHands(string type)
	{
		ProjectileSchema projectileSchema = this[type];
		if (projectileSchema != null)
		{
			return projectileSchema.needsBothHands;
		}
		return false;
	}

	public bool ProjectileShownWhileAiming(string type)
	{
		ProjectileSchema projectileSchema = this[type];
		if (projectileSchema != null)
		{
			return projectileSchema.shownWhileAiming;
		}
		return false;
	}

	public Vector3 ProjectileAimPosForTarget(string type, Vector3 spawnPos, Vector3 targetPos)
	{
		if (ProjectileArcs(type))
		{
			if (Mathf.Abs(spawnPos.z - targetPos.z) <= 3f && Mathf.Abs(spawnPos.y - targetPos.y) <= 0.5f)
			{
				return targetPos;
			}
			float num = Vector3.Distance(spawnPos, targetPos) - 0.2f;
			float num2 = num * 0.2f;
			Vector3 result = (targetPos + spawnPos) * 0.5f;
			result.y += num2 * 2f;
			return result;
		}
		return targetPos;
	}

	public bool IsProjectileInRange(GameRange range, bool bIsEnemy)
	{
		foreach (Projectile projectile in projectiles)
		{
			if (bIsEnemy != projectile.shooter.isEnemy)
			{
				float z = projectile.transform.position.z;
				if (range.Contains(z))
				{
					return true;
				}
			}
		}
		return false;
	}
}
