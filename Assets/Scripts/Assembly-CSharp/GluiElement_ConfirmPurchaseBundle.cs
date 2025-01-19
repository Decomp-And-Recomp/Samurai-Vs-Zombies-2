using System;

public class GluiElement_ConfirmPurchaseBundle : GluiElement_DataAdaptor<DataAdaptor_ConfirmPurchaseBundle>, IMenuContextBase
{
	private void Start()
	{
		GluIap instance = SingletonSpawningMonoBehaviour<GluIap>.Instance;
		instance.OnStateChange = (Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>)Delegate.Combine(instance.OnStateChange, new Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>(adaptor.OnTransactionStateChange));
	}

	private void OnDestroy()
	{
		GluIap instance = SingletonSpawningMonoBehaviour<GluIap>.Instance;
		instance.OnStateChange = (Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>)Delegate.Remove(instance.OnStateChange, new Action<ICInAppPurchase.TRANSACTION_STATE, string, bool>(adaptor.OnTransactionStateChange));
	}

	public object GetMenuContext()
	{
		return adaptor.context;
	}

	public void SetMenuContext(object context)
	{
		SetGluiCustomElementData(context);
	}
}
