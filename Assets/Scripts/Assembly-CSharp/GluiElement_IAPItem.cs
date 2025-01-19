using UnityEngine;

[AddComponentMenu("Game/GluiElement IAPItem")]
public class GluiElement_IAPItem : GluiElement_DataAdaptor<DataAdaptor_IAPItem>
{
	private void Update()
	{
		adaptor.UpdateSaleTime();
	}
}
