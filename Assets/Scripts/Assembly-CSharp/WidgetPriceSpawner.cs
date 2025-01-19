using UnityEngine;

public class WidgetPriceSpawner : MonoBehaviour
{
	public string widgetPath = "UI/Prefabs/Global/Widget_Price_Store";

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetCost(string customPriceString)
	{
		WidgetPriceHandler widgetPriceHandler = SpawnHandler();
		if (widgetPriceHandler != null)
		{
			widgetPriceHandler.SetCustomPriceString(customPriceString);
		}
	}

	public void SetCost(Cost c)
	{
		WidgetPriceHandler widgetPriceHandler = SpawnHandler();
		if (widgetPriceHandler != null)
		{
			widgetPriceHandler.cost = c;
		}
	}

	private WidgetPriceHandler SpawnHandler()
	{
		if (widgetPath == string.Empty)
		{
			return null;
		}
		GameObject gameObject = base.gameObject.FindChild("WidgetPrice");
		if (gameObject == null)
		{
			gameObject = ResourceCache.GetCachedResource(widgetPath, 1).Resource as GameObject;
			if (gameObject == null)
			{
				return null;
			}
			gameObject = Object.Instantiate(gameObject) as GameObject;
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.name = "WidgetPrice";
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = localScale;
			gameObject.BroadcastMessage("Start", SendMessageOptions.DontRequireReceiver);
		}
		return gameObject.GetComponent<WidgetPriceHandler>();
	}
}
