using UnityEngine;

[AddComponentMenu("Samurai Data/GluiElement CollectionItem")]
public class GluiElement_CollectionItem : GluiElement_DataAdaptor<DataAdaptor_CollectionItem>
{
	public override void SetGluiCustomElementData(object data)
	{
		CancelInvoke();
		base.SetGluiCustomElementData(data);
		if (adaptor.HasAttackTimer())
		{
			InvokeRepeating("UpdateItemTimer", 1f, 1f);
		}
	}

	private void UpdateItemTimer()
	{
		if (!adaptor.UpdateTimer())
		{
			CancelInvoke("UpdateItemTimer");
		}
	}
}
