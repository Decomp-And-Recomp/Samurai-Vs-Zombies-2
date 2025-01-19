using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_StoreHeroPanelItem : DataAdaptorBase
{
	public GameObject text_Name;

	public GameObject sprite_icon;

	public GameObject sprite_padlock;

	public GluiStandardButtonContainer button;

	public GameObject root_newBadge;

	public GameObject locatorWidgetPrice;

	public GameObject root_maxUpgradeBadge;

	public GameObject sprite_secondIcon;

	public Transform locator_fx;

	private StoreData.Item item;

	private WidgetPriceHandler priceHandler;

	private Vector3 originalTextYOffset = Vector3.zero;

	private GameObject mMysteryBoxEffect;

	public override void SetData(object data)
	{
		if (!(data is StoreData.Item))
		{
			return;
		}
		item = data as StoreData.Item;
		if (priceHandler == null)
		{
			originalTextYOffset = text_Name.transform.localPosition;
			GameObject gameObject = UnityEngine.Object.Instantiate(ResourceCache.GetCachedResource("UI/Prefabs/Global/Widget_Price_Store", 1).Resource as GameObject) as GameObject;
			gameObject.BroadcastMessage("Start");
			gameObject.transform.parent = locatorWidgetPrice.transform;
			gameObject.transform.localPosition = Vector3.zero;
			priceHandler = gameObject.GetComponent<WidgetPriceHandler>();
		}
		button.GetActionData = () => item;
		button.action = item.customButtonAction;
		if (button.action != null)
		{
			button.onReleaseActions = null;
		}
		if (!item.locked)
		{
			if (!item.maxlevel)
			{
				priceHandler.cost = item.cost;
				SetGluiTextInChild(text_Name, item.title);
			}
			else
			{
				priceHandler.cost = default(Cost);
				SetGluiTextInChild(text_Name, item.details.Name);
			}
		}
		else
		{
			SetGluiTextInChild(text_Name, item.unlockCondition);
			priceHandler.cost = default(Cost);
		}
		if (sprite_secondIcon != null)
		{
			if (item.secondIcon != null)
			{
				sprite_secondIcon.SetActive(true);
				SetGluiSpriteInChild(sprite_secondIcon, item.secondIcon);
			}
			else
			{
				sprite_secondIcon.SetActive(false);
			}
		}
		text_Name.transform.localPosition = new Vector3(originalTextYOffset.x, originalTextYOffset.y - priceHandler.height, originalTextYOffset.z);
		SetGluiSpriteInChild(sprite_icon, item.icon);
		if (root_newBadge != null)
		{
			root_newBadge.SetActive(item.isNew);
		}
		if (root_maxUpgradeBadge != null)
		{
			root_maxUpgradeBadge.SetActive(item.maxlevel);
		}
		button.Locked = item.locked;
		if (button != null)
		{
			if (item.bundleContent.Count > 0)
			{
				button.onReleaseActions = new string[1] { "POPUP_CONFIRMPURCHASE_BUNDLE" };
			}
			else
			{
				button.onReleaseActions = new string[1] { "POPUP_CONFIRMPURCHASE" };
			}
		}
		if (item.id == "mysterybox")
		{
			if (mMysteryBoxEffect != null)
			{
				mMysteryBoxEffect.SetActive(true);
				return;
			}
			mMysteryBoxEffect = UnityEngine.Object.Instantiate(ResourceCache.GetCachedResource("UI/Prefabs/Store/FX_MysteryBox", 1).Resource) as GameObject;
			mMysteryBoxEffect.BroadcastMessage("Start");
			mMysteryBoxEffect.transform.parent = locator_fx;
			mMysteryBoxEffect.transform.localPosition = Vector3.zero;
		}
		else if (mMysteryBoxEffect != null)
		{
			mMysteryBoxEffect.SetActive(false);
		}
	}
}
