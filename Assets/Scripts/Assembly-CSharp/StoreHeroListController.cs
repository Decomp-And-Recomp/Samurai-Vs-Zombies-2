public class StoreHeroListController : GluiSimpleCollectionController
{
	public override void ReloadData(object arg)
	{
		string dataFilterKey = "Heroes";
		if (arg is string)
		{
			dataFilterKey = (string)arg;
		}
		else if (!string.IsNullOrEmpty(dataKey))
		{
			dataFilterKey = dataKey;
		}
		SingletonMonoBehaviour<StoreMenuImpl>.Instance.Get_GluiData(dataFilterKey, null, null, out mData);
	}

	public override void OnDrawn(GluiElement_Base elem, int dataIndex)
	{
		object obj = mData[dataIndex];
		GluiAgent_Redraw_BouncyList component = ((GluiElement_StoreHeroPanel)elem).adaptor.scrollListObject.GetComponent<GluiAgent_Redraw_BouncyList>();
		if (obj is string)
		{
			component.ID = (string)obj;
		}
		else if (obj is DataBundleRecordHandle<HeroSchema>)
		{
			component.ID = ((DataBundleRecordHandle<HeroSchema>)obj).Data.id;
		}
	}
}
