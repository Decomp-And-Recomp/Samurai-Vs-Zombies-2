public class IAPListController : GluiSimpleCollectionController
{
	public override void ReloadData(object arg)
	{
		mData = null;
		if (!SingletonMonoBehaviour<IAPData>.Exists)
		{
		}
		SingletonMonoBehaviour<IAPData>.Instance.Get_GluiData("IAP", null, null, out mData);
		if (mData == null)
		{
			mData = new object[0];
		}
	}
}
