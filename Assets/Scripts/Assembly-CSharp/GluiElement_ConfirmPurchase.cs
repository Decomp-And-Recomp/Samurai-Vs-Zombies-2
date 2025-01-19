public class GluiElement_ConfirmPurchase : GluiElement_DataAdaptor<DataAdaptor_ConfirmPurchase>, IMenuContextBase
{
	public object GetMenuContext()
	{
		return adaptor.context;
	}

	public void SetMenuContext(object context)
	{
		SetGluiCustomElementData(context);
	}
}
