using System;
using UnityEngine;

[DataBundleClass]
public class MaestroEffectSchema
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 200)]
	public string name;

	[DataBundleField(ColumnWidth = 250, TooltipInfo = "Object that must at least have an EffectContainer to use with the Effect.  Any EffectConductors are also automatically assigned.")]
	public GameObject effectContainer;

	[DataBundleSchemaFilter(typeof(MaestroEffectSchema), false)]
	[DataBundleField(ColumnWidth = 250, TooltipInfo = "Effect to play after this effect is killed")]
	public DataBundleRecordKey chainedEffect;

	public static void InputEffect_Factory(DataBundleRecordKey key, GameObject effectOwner, Action<EffectContainer> onLoadDone)
	{
		if (key == null || key.Key == string.Empty)
		{
			return;
		}
		DataBundleRecordHandle<MaestroEffectSchema> dataBundleRecordHandle = new DataBundleRecordHandle<MaestroEffectSchema>(key.ToString());
		dataBundleRecordHandle.LoadTableAgnostic(DataBundleResourceGroup.All, true, delegate(MaestroEffectSchema effectSchema)
		{
			if (effectSchema != null)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(effectSchema.effectContainer, effectOwner.transform.position, effectOwner.transform.rotation);
				gameObject.transform.parent = effectOwner.transform;
				EffectContainer component = gameObject.GetComponent<EffectContainer>();
				if (component == null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					component.Owner = effectOwner;
					component.DestroyObjectWhenDone = true;
					if (effectSchema.chainedEffect.Key != string.Empty)
					{
						EffectConductor_Chain effectConductor_Chain = gameObject.AddComponent(typeof(EffectConductor_Chain)) as EffectConductor_Chain;
						effectConductor_Chain.effectSpawnOnKill = effectSchema.chainedEffect;
					}
					InputContainer_Forwarder inputContainer_Forwarder = effectOwner.AddComponent(typeof(InputContainer_Forwarder)) as InputContainer_Forwarder;
					inputContainer_Forwarder.SetObjectToForwardTo(gameObject);
					if (onLoadDone != null)
					{
						onLoadDone(component);
					}
				}
			}
		});
	}

	public static void RemoveEffect(GameObject effectObject)
	{
		if (!(effectObject == null))
		{
			EffectContainer effectContainer = effectObject.GetComponent(typeof(EffectContainer)) as EffectContainer;
			if (effectContainer != null)
			{
				RemoveEffect(effectContainer);
			}
		}
	}

	public static void RemoveEffect(EffectContainer effectContainer)
	{
		if (!(effectContainer == null))
		{
			effectContainer.EffectDisable();
			effectContainer.EffectKill();
		}
	}
}
