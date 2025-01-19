using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_CollectionStatusRecord : DataAdaptorBase
{
	public GameObject text_ownerName;

	public GameObject text_attackRating;

	public GameObject text_buff1;

	public GameObject text_buff2;

	public GameObject sprite_buff1Icon;

	public GameObject sprite_buff2Icon;

	public GameObject FacebookFriendButton;

	public GameObject GameCenterFriendButton;

	public GameObject AttackRatingButton;

	public GluiText SoulCostText;

	private CollectionStatusRecord mStatusRecord;

	public CollectionStatusRecord statusRecord
	{
		get
		{
			return mStatusRecord;
		}
	}

	public override void SetData(object data)
	{
		mStatusRecord = (CollectionStatusRecord)data;
		if (mStatusRecord == null)
		{
			return;
		}
		string text = mStatusRecord.UserName;
		MultiplayerAIOpponentSchema multiplayerAIOpponentSchema = null;
		if (mStatusRecord.OwnerID == 0)
		{
			string tableRecordKey = MultiplayerAIOpponentSchema.FromIndex("MultiplayerAIOpponentData", mStatusRecord.AttackerID.Value);
			multiplayerAIOpponentSchema = MultiplayerAIOpponentSchema.GetRecord(tableRecordKey);
			text = StringUtils.GetStringFromStringRef(multiplayerAIOpponentSchema.displayName);
		}
		GluiText component = text_ownerName.GetComponent<GluiText>();
		if (component != null)
		{
			component.Start();
			if (component.font != null && component.font.glyphs != null)
			{
				bool flag = false;
				string text2 = text;
				foreach (char key in text2)
				{
					if (!component.font.glyphs.ContainsKey(key))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					component.FontName = ((!(component.FontName == "Font_CJK")) ? "Font_CJK" : "Font_EFIGSPR");
				}
			}
		}
		GameCenterFriendButton.GetComponent<GluiSprite>().Texture = ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Button_GooglePlus_PGS", 1).Resource as Texture2D;
		if (FacebookFriendButton != null)
		{
			FacebookFriendButton.SetActive(mStatusRecord.IsFacebookFriend);
		}
		if (GameCenterFriendButton != null)
		{
			GameCenterFriendButton.SetActive(mStatusRecord.IsGameCenterFriend && !mStatusRecord.IsFacebookFriend);
		}
		if (AttackRatingButton != null)
		{
			AttackRatingButton.SetActive(true);
		}
		if (mStatusRecord.OwnerID == 0)
		{
			SetGluiTextInChild(text_ownerName, text);
			SetGluiTextFormatInChild(text_attackRating, mStatusRecord.AttackRating);
			mStatusRecord.DefensiveBuffs[0] = (byte)multiplayerAIOpponentSchema.buffLevelEnemy;
			ShowDefensiveBuff(sprite_buff1Icon, text_buff1, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Enemy"), multiplayerAIOpponentSchema.buffLevelEnemy);
			mStatusRecord.DefensiveBuffs[1] = (byte)multiplayerAIOpponentSchema.buffLevelAmulet;
			ShowDefensiveBuff(sprite_buff2Icon, text_buff2, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Amulet"), multiplayerAIOpponentSchema.buffLevelAmulet);
		}
		else
		{
			SetGluiTextInChild(text_ownerName, text);
			SetGluiTextFormatInChild(text_attackRating, mStatusRecord.AttackRating);
			ShowDefensiveBuff(sprite_buff1Icon, text_buff1, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Enemy"), mStatusRecord.DefensiveBuffs[0]);
			ShowDefensiveBuff(sprite_buff2Icon, text_buff2, Singleton<Profile>.Instance.MultiplayerData.GetCollectionSetData("Amulet"), mStatusRecord.DefensiveBuffs[1]);
		}
		if (SoulCostText != null)
		{
			SoulCostText.Text = mStatusRecord.SoulCostToAttack.ToString();
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
		bool flag = true;
		string text = null;
		if (!timesCollected.HasValue)
		{
			goto IL_00f0;
		}
		switch (timesCollected.Value)
		{
		case 1:
			break;
		case 2:
			goto IL_0086;
		case 3:
			goto IL_00bb;
		default:
			goto IL_00f0;
		}
		SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel1);
		text = StringUtils.GetStringFromStringRef(setData.shortDescription1);
		if (string.IsNullOrEmpty(text))
		{
			text = StringUtils.GetStringFromStringRef(setData.rewardDesc1);
		}
		goto IL_00f7;
		IL_00bb:
		SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel3);
		text = StringUtils.GetStringFromStringRef(setData.shortDescription3);
		if (string.IsNullOrEmpty(text))
		{
			text = StringUtils.GetStringFromStringRef(setData.rewardDesc3);
		}
		goto IL_00f7;
		IL_0086:
		SetGluiSpriteInChild(sprite_buffIcon, setData.rewardIconLevel2);
		text = StringUtils.GetStringFromStringRef(setData.shortDescription2);
		if (string.IsNullOrEmpty(text))
		{
			text = StringUtils.GetStringFromStringRef(setData.rewardDesc2);
		}
		goto IL_00f7;
		IL_00f0:
		flag = false;
		goto IL_00f7;
		IL_00f7:
		if (flag)
		{
			GluiText component = textObj.GetComponent<GluiText>();
			component.Text = text;
		}
		sprite_buffIcon.SetActive(flag);
		textObj.SetActive(flag);
	}
}
