using System.Collections;
using System.Runtime.CompilerServices;
using PlayHaven;
using UnityEngine;

[AddComponentMenu("PlayHaven/VGP Handler")]
public class PlayHavenVGPHandler : MonoBehaviour
{
	public delegate void PurchaseEventHandler(int requestId, Purchase purchase);

	private static PlayHavenVGPHandler instance;

	private PlayHavenManager playHaven;

	private Hashtable purchases = new Hashtable(4);

	public static PlayHavenVGPHandler Instance
	{
		get
		{
			if (!instance)
			{
				instance = Object.FindObjectOfType(typeof(PlayHavenVGPHandler)) as PlayHavenVGPHandler;
			}
			return instance;
		}
	}

	[method: MethodImpl(32)]
	public event PurchaseEventHandler OnPurchasePresented;

	private void Awake()
	{
		playHaven = PlayHavenManager.instance;
	}

	private void OnEnable()
	{
		playHaven.OnPurchasePresented += PlayHavenOnPurchasePresented;
	}

	private void OnDisable()
	{
		playHaven.OnPurchasePresented -= PlayHavenOnPurchasePresented;
	}

	private void PlayHavenOnPurchasePresented(int requestId, Purchase purchase)
	{
		if (this.OnPurchasePresented != null)
		{
			purchases.Add(requestId, purchase);
			this.OnPurchasePresented(requestId, purchase);
		}
	}

	public void ResolvePurchase(int requestId, PurchaseResolution resolution, bool track)
	{
		if (purchases.ContainsKey(requestId))
		{
			Purchase purchase = (Purchase)purchases[requestId];
			purchases.Remove(requestId);
			playHaven.ProductPurchaseResolutionRequest(resolution);
			if (track)
			{
				playHaven.ProductPurchaseTrackingRequest(purchase, resolution);
			}
		}
		else if (!Debug.isDebugBuild)
		{
		}
	}

	public void ResolvePurchase(Purchase purchase, PurchaseResolution resolution, bool track)
	{
		if (!purchases.ContainsValue(purchase))
		{
			if (Debug.isDebugBuild)
			{
			}
			if (track)
			{
				playHaven.ProductPurchaseTrackingRequest(purchase, resolution);
			}
			return;
		}
		int num = -1;
		foreach (int key in purchases.Keys)
		{
			if (purchases[key] == purchase)
			{
				num = key;
				break;
			}
		}
		if (num > -1)
		{
			purchases.Remove(num);
			playHaven.ProductPurchaseResolutionRequest(resolution);
			if (track)
			{
				playHaven.ProductPurchaseTrackingRequest(purchase, resolution);
			}
		}
	}
}
