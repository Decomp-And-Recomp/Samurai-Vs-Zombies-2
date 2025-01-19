using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeUnlockPopupImpl : MonoBehaviour
{
	private void Start()
	{
		List<ResultsMenuImpl.UnlockedFeature> upgradeUnlockedFeatures = ResultsMenuImpl.GetUpgradeUnlockedFeatures(false);
		if (upgradeUnlockedFeatures == null || upgradeUnlockedFeatures.Count == 0)
		{
			throw new Exception("Invalid Upgrade Unlock list");
		}
		Transform parent = base.gameObject.FindChild("Unlock_" + upgradeUnlockedFeatures.Count).transform;
		List<Transform> list = new List<Transform>();
		int num = 1;
		while (true)
		{
			Transform transform = ObjectUtils.FindTransformInChildren(parent, "Locator_" + num);
			if (transform == null)
			{
				break;
			}
			list.Add(transform);
			num++;
		}
		if (list.Count != upgradeUnlockedFeatures.Count)
		{
			throw new Exception("Error with the locators in the Unlock Popup.");
		}
		SpawnCards(upgradeUnlockedFeatures, list);
	}

	private void Update()
	{
	}

	private void SpawnCards(List<ResultsMenuImpl.UnlockedFeature> unlocks, List<Transform> locators)
	{
		GameObject original = ResourceCache.GetCachedResource("UI/Prefabs/Results/Card_Unlocked", 1).Resource as GameObject;
		for (int i = 0; i < unlocks.Count; i++)
		{
			ResultsMenuImpl.UnlockedFeature unlockedFeature = unlocks[i];
			Transform parent = locators[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.localRotation = Quaternion.identity;
			GluiSprite gluiSprite = gameObject.FindChildComponent<GluiSprite>("Swap_Icon");
			gluiSprite.Texture = unlockedFeature.icon;
			GluiText gluiText = gameObject.FindChildComponent<GluiText>("SwapText_Name");
			gluiText.Text = unlockedFeature.text;
		}
	}
}
