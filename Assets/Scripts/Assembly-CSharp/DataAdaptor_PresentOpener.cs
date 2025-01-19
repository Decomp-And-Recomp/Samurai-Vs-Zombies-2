using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataAdaptor_PresentOpener : DataAdaptorBase
{
	public GluiStandardButtonContainer giftButton;

	public GluiButtonContainer_StateTracker stateTracker;

	public GluiSprite presentSprite;

	public GluiElement_ResultsLoot lootRenderer;

	public GameObject contentParent;

	public override void SetData(object data)
	{
		lootRenderer.adaptor.SetData(data);
		contentParent.SetActive(false);
		PlayStatistics.Data.LootEntry lootEntry = (PlayStatistics.Data.LootEntry)data;
		if (lootEntry == null || !lootEntry.presentType.HasValue)
		{
			return;
		}
		GluiButtonSpawnAction gluiButtonSpawnAction = null;
		List<GluiButtonAction> buttonActions = stateTracker.GetButtonActions();
		foreach (GluiButtonAction item in buttonActions)
		{
			if (item is GluiButtonSpawnAction)
			{
				gluiButtonSpawnAction = (GluiButtonSpawnAction)item;
				break;
			}
		}
		string path;
		string path2;
		switch (lootEntry.presentType.Value)
		{
		case ECollectableType.presentA:
			path = "UI/Textures/DynamicIcons/Misc/Present_Red";
			path2 = "UI/Prefabs/Global/FX_Present_Red_Unwrap";
			break;
		case ECollectableType.presentB:
			path = "UI/Textures/DynamicIcons/Misc/Present_Blue";
			path2 = "UI/Prefabs/Global/FX_Present_Blue_Unwrap";
			break;
		case ECollectableType.presentC:
		case ECollectableType.presentD:
			path = "UI/Textures/DynamicIcons/Misc/Present_Gold";
			path2 = "UI/Prefabs/Global/FX_Present_Gold_Unwrap";
			break;
		default:
			throw new Exception("Unknown Present Type!");
		}
		if (gluiButtonSpawnAction != null)
		{
			gluiButtonSpawnAction.Prefab = ResourceCache.GetCachedResource(path2, 1).Resource as GameObject;
		}
		presentSprite.Texture = ResourceCache.GetCachedResource(path, 1).Resource as Texture2D;
	}

	public void OpenPresent()
	{
		giftButton.Selected = true;
		contentParent.SetActive(true);
	}
}
