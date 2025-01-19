public class GluiElement_StoreHeroPanelItem : GluiElement_DataAdaptor<DataAdaptor_StoreHeroPanelItem>
{
	public void HideNewBanner()
	{
		adaptor.root_newBadge.SetActive(false);
	}
}
