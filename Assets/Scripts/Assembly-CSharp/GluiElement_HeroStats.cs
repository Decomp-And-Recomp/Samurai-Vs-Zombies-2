public class GluiElement_HeroStats : GluiElement_DataAdaptor<DataAdaptor_HeroStats>, IMenuContextBase
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
