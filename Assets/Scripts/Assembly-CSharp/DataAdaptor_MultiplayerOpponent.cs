using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_MultiplayerOpponent : DataAdaptorBase
{
	public GameObject DefendButton;

	public GameObject FindOpponentButton;

	public GameObject OpponentNameText;

	public GameObject AttackRatingText;

	public GameObject ItemIcon;

	public GameObject ItemNameText;

	public GameObject BuffAIcon;

	public GameObject BuffAText;

	public GameObject BuffBIcon;

	public GameObject BuffBText;

	public GluiText AttackButtonText;

	public override void SetData(object data)
	{
		MultiplayerWaveData multiplayerWaveData = (MultiplayerWaveData)data;
		if (multiplayerWaveData == null)
		{
			return;
		}
		CollectionItemSchema collectionItem_InConflict = multiplayerWaveData.collectionItem_InConflict;
		SetGluiTextInChild(ItemNameText, StringUtils.GetStringFromStringRef(collectionItem_InConflict.displayName));
		if (ItemIcon != null)
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(collectionItem_InConflict.IconPath, 1);
			if (cachedResource != null)
			{
				Texture2D texture = cachedResource.Resource as Texture2D;
				SetGluiSpriteInChild(ItemIcon, texture);
			}
		}
		ShowDefensiveBuff(BuffAIcon, BuffAText, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Enemy"), multiplayerWaveData.defensiveBuffs[0]);
		ShowDefensiveBuff(BuffBIcon, BuffBText, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Amulet"), multiplayerWaveData.defensiveBuffs[1]);
		if (Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null)
		{
			SetGluiTextFormatInChild(OpponentNameText, Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.userName);
			string stringFromStringRef = StringUtils.GetStringFromStringRef("MenuFixedStrings.Menu_DefenseRating");
			stringFromStringRef += " : ";
			stringFromStringRef += Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.attackRating;
			SetGluiTextInChild(AttackRatingText, stringFromStringRef);
		}
		else
		{
			SetGluiTextTagInChild(OpponentNameText, "MenuFixedStrings.Menu_MPVsFail");
			AttackRatingText.SetActive(false);
		}
		SetGluiButtonOnReleaseInChild(DefendButton, new string[2] { "POPUP_POP", "MENU_MAIN_EQUIP" });
		if (multiplayerWaveData.gameMode == EMultiplayerMode.kDefending)
		{
			FindOpponentButton.SetActive(false);
			return;
		}
		SetGluiButtonOnReleaseInChild(FindOpponentButton, new string[2] { "POPUP_POP", "QUERY_SELECTED_COLLECTIBLE_CARD_OPPONENTS" });
		if (multiplayerWaveData.gameMode == EMultiplayerMode.kAttacking)
		{
			DefendButton.SetActive(false);
		}
		else if (multiplayerWaveData.gameMode == EMultiplayerMode.kRecovering)
		{
			AttackButtonText.TaggedStringReference = "MenuFixedStrings.Card_Collection_Lost";
		}
	}

	private void ShowDefensiveBuff(GameObject sprite_buffIcon, GameObject textObj, CollectionSchema setData, int? timesCollected)
	{
		if (sprite_buffIcon == null)
		{
			return;
		}
		if (setData == null)
		{
			timesCollected = 0;
		}
		if (timesCollected.HasValue)
		{
			switch (timesCollected.Value)
			{
			case 1:
				SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel1);
				SetGluiTextTagInChild(textObj, string.IsNullOrEmpty(setData.shortDescription1) ? setData.rewardDesc1 : setData.shortDescription1);
				return;
			case 2:
				SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel2);
				SetGluiTextTagInChild(textObj, string.IsNullOrEmpty(setData.shortDescription2) ? setData.rewardDesc2 : setData.shortDescription2);
				return;
			case 3:
				SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel3);
				SetGluiTextTagInChild(textObj, string.IsNullOrEmpty(setData.shortDescription3) ? setData.rewardDesc3 : setData.shortDescription3);
				return;
			}
		}
		sprite_buffIcon.SetActive(false);
		textObj.SetActive(false);
	}
}
