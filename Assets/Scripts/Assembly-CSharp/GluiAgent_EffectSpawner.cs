using System;
using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Effect Spawner")]
public class GluiAgent_EffectSpawner : GluiAgent_Effect
{
	[HideInInspector]
	[DataBundleSchemaFilter(typeof(MaestroEffectSchema), false)]
	public DataBundleRecordKey effectToSpawn;

	public int maxSimultaneousEffects = 1;

	public GameObject redirectOwnerForEffect;

	private int uniqueID;

	protected void Spawn(Action<EffectContainer> onSpawnDone)
	{
		GameObject gameObject = base.gameObject;
		if (redirectOwnerForEffect != null)
		{
			gameObject = redirectOwnerForEffect;
		}
		if (gameObject.activeInHierarchy)
		{
			MaestroEffectSchema.InputEffect_Factory(effectToSpawn, gameObject, delegate(EffectContainer newEffectContainer)
			{
				Add(newEffectContainer);
				onSpawnDone(newEffectContainer);
			});
		}
	}

	public override void Activity_Enable()
	{
		Spawn(base.Activity_Enable);
	}

	public override void Activity_Enable_Reverse()
	{
		Spawn(base.Activity_Enable_Reverse);
	}

	protected void Add(EffectContainer newContainer)
	{
		if (!(newContainer == null))
		{
			containers.Add(newContainer);
			RemoveOld();
		}
	}

	protected void RemoveOld()
	{
		containers.RemoveAll((EffectContainer container) => container == null);
		while (containers.Count > maxSimultaneousEffects)
		{
			Remove(containers[0]);
		}
	}

	protected override void Remove(EffectContainer container)
	{
		MaestroEffectSchema.RemoveEffect(container);
		containers.Remove(container);
	}
}
