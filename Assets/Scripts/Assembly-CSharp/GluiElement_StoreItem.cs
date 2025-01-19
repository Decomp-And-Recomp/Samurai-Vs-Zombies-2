public class GluiElement_StoreItem : GluiElement_DataAdaptor<DataAdaptor_StoreItem>
{
	public void HideNewBanner()
	{
		adaptor.root_newBadge.SetActive(false);
	}
}
