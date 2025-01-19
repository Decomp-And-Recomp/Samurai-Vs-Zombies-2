using System;
using UnityEngine;

[Serializable]
public class GluiButtonSpawnAction : GluiButtonAction
{
	[SerializeField]
	public GameObject Prefab;

	[SerializeField]
	public Transform Location;

	private GameObject spawnedObject;

	public override string GetActionName()
	{
		return "Spawn";
	}

	public override void OnEnterState()
	{
		if (spawnedObject != null)
		{
			UnityEngine.Object.Destroy(spawnedObject);
			spawnedObject = null;
		}
		if (Prefab != null)
		{
			spawnedObject = GameObjectPool.DefaultObjectPool.Acquire(Prefab);
			ObjectUtils.SetLayerRecursively(spawnedObject, base.gameObject.layer);
			if (Location != null)
			{
				spawnedObject.transform.parent = Location;
			}
			else
			{
				spawnedObject.transform.parent = base.transform;
			}
			spawnedObject.transform.localPosition = Vector3.zero;
			spawnedObject.transform.localRotation = Quaternion.identity;
			spawnedObject.transform.localScale = Vector3.one;
		}
	}

	public override void OnLeaveState()
	{
	}
}
