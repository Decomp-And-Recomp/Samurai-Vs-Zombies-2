using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Chain")]
public class EffectConductor_Chain : EffectConductor
{
	public string actionOnStart;

	[DataBundleSchemaFilter(typeof(MaestroEffectSchema), false)]
	[HideInInspector]
	public DataBundleRecordKey effectSpawnOnStart;

	public string actionOnKill;

	[DataBundleSchemaFilter(typeof(MaestroEffectSchema), false)]
	[HideInInspector]
	public DataBundleRecordKey effectSpawnOnKill;

	public override void Start()
	{
		base.Start();
		GluiActionSender.SendGluiAction(actionOnStart, base.gameObject, null);
		SpawnChainedEffect(effectSpawnOnStart);
	}

	public override void OnEffectKilled(bool destroyingContainer = false)
	{
		GluiActionSender.SendGluiAction(actionOnKill, base.gameObject, null);
		if (!destroyingContainer)
		{
			SpawnChainedEffect(effectSpawnOnKill);
		}
		base.OnEffectKilled();
	}

	private void SpawnChainedEffect(DataBundleRecordKey effectToSpawn)
	{
		Vector2 screenPosition = effectContainer.EffectScreenPosition;
		MaestroEffectSchema.InputEffect_Factory(effectToSpawn, effectContainer.Owner, delegate(EffectContainer spawnedEffectContainer)
		{
			spawnedEffectContainer.SetStartPosition(screenPosition);
		});
	}
}
