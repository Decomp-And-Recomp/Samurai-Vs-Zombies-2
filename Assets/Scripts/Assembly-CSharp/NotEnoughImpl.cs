using UnityEngine;

public class NotEnoughImpl : MonoBehaviour, IGluiActionHandler
{
	public Transform locator_enough;

	public Transform locator_more;

	public GameObject root_main;

	public GluiText text_disconnected;

	private bool mSetupDone;

	private void Start()
	{
		mSetupDone = false;
		UpdateStatus();
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			UpdateStatus();
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		return false;
	}

	private void UpdateStatus()
	{
		if (!SingletonSpawningMonoBehaviour<GluIap>.Instance.Initialized || Application.internetReachability == NetworkReachability.NotReachable)
		{
			root_main.SetActive(false);
			text_disconnected.gameObject.SetActive(true);
			text_disconnected.Text = StringUtils.GetStringFromStringRef("LocalizedStrings.iap_error_requestfailed");
		}
		else
		{
			if (mSetupDone)
			{
				return;
			}
			mSetupDone = true;
			root_main.SetActive(true);
			text_disconnected.gameObject.SetActive(false);
			Cost cost = (Cost)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("NOT_ENOUGH_COST");
			bool flag = cost.currency == Cost.Currency.Hard;
			int num = ((!flag) ? Singleton<Profile>.Instance.coins : Singleton<Profile>.Instance.gems);
			int num2 = cost.price - num;
			int num3 = 100000;
			int num4 = 0;
			int index = 0;
			IAPSchema iAPSchema = null;
			IAPSchema iAPSchema2 = null;
			for (int i = 0; i < SingletonSpawningMonoBehaviour<GluIap>.Instance.Products.Count; i++)
			{
				iAPSchema = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products[i];
				int num5 = ((!flag) ? iAPSchema.softCurrencyAmount : iAPSchema.hardCurrencyAmount);
				if (!iAPSchema.hidden && num5 >= num2 && num5 - num2 < num3)
				{
					num3 = num5 - num2;
					num4 = i;
				}
			}
			num3 = 100000;
			for (int j = 0; j < SingletonSpawningMonoBehaviour<GluIap>.Instance.Products.Count; j++)
			{
				iAPSchema = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products[j];
				int num6 = ((!flag) ? iAPSchema.softCurrencyAmount : iAPSchema.hardCurrencyAmount);
				if (!iAPSchema.hidden && num6 >= num2 && num6 - num2 < num3 && j != num4)
				{
					num3 = num6 - num2;
					index = j;
				}
			}
			iAPSchema = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products[num4];
			iAPSchema2 = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products[index];
			if (iAPSchema == null)
			{
				return;
			}
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/Global/Card_IAPBundle.prefab", 0);
			GameObject gameObject = Object.Instantiate(cachedResource.Resource) as GameObject;
			gameObject.transform.parent = locator_enough;
			gameObject.transform.localPosition = Vector3.zero;
			GluiElement_IAPItem component = gameObject.GetComponent<GluiElement_IAPItem>();
			component.SetGluiCustomElementData(iAPSchema);
			Transform transform = gameObject.FindChildComponent<Transform>("Rays");
			if (transform != null)
			{
				transform.gameObject.SetActive(false);
			}
			if (iAPSchema2 != null)
			{
				GameObject gameObject2 = Object.Instantiate(cachedResource.Resource) as GameObject;
				gameObject2.transform.parent = locator_more;
				gameObject2.transform.localPosition = Vector3.zero;
				GluiElement_IAPItem component2 = gameObject2.GetComponent<GluiElement_IAPItem>();
				component2.SetGluiCustomElementData(iAPSchema2);
				transform = gameObject2.FindChildComponent<Transform>("Rays");
				if (transform != null)
				{
					transform.gameObject.SetActive(true);
				}
			}
		}
	}
}
