using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_CollectionItem : DataAdaptorBase
{
	public GameObject text_displayName;

	public GameObject text_soulsToAttack;

	public GameObject text_soulsToAttack2;

	public GameObject text_attackTimeRemaining;

	public GameObject enable_sendCardTypeString;

	public GameObject button_ItemIcon;

	public GameObject button_Attack;

	public GameObject button_Defend;

	public GameObject button_Recover;

	private DateTime? timeToBeAttacked;

	private CollectionItemSchema mData;

	public override void SetData(object data)
	{
		CollectionItemSchema collectionItemSchema = (CollectionItemSchema)data;
		if (collectionItemSchema == null)
		{
			return;
		}
		mData = collectionItemSchema;
		SetGluiTextInChild(text_displayName, StringUtils.GetStringFromStringRef(collectionItemSchema.displayName));
		SetGluiTextInChild(text_soulsToAttack, collectionItemSchema.soulsToAttack.ToString());
		SetGluiTextInChild(text_soulsToAttack2, collectionItemSchema.soulsToAttack.ToString());
		if (button_ItemIcon != null)
		{
			Texture2D texturePath = GetTexturePath(collectionItemSchema.IconPath);
			SetGluiSpriteInChild(button_ItemIcon, texturePath);
		}
		MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.Find(collectionItemSchema.CollectionID);
		MultiplayerCollectionItemDescriptor multiplayerCollectionItemDescriptor = new MultiplayerCollectionItemDescriptor();
		multiplayerCollectionStatusQueryResponse.Aggregate(Singleton<Profile>.Instance.MultiplayerData.OwnerID, out multiplayerCollectionItemDescriptor.aggregateData);
		multiplayerCollectionItemDescriptor.itemSchema = collectionItemSchema;
		timeToBeAttacked = null;
		MultiplayerCollectionStatusQueryResponse.CardType cardType = multiplayerCollectionStatusQueryResponse.GetCardType(multiplayerCollectionItemDescriptor);
		if (text_attackTimeRemaining != null && multiplayerCollectionItemDescriptor.aggregateData.firstAttackerTime.HasValue && cardType == MultiplayerCollectionStatusQueryResponse.CardType.Danger)
		{
			timeToBeAttacked = multiplayerCollectionItemDescriptor.aggregateData.firstAttackerTime.Value.AddSeconds(MultiplayerCollectionStatus.GetDefenseTimeLimit());
			UpdateTimer();
		}
		if (enable_sendCardTypeString != null)
		{
			SendGluiEnableByValue(enable_sendCardTypeString, cardType.ToString());
		}
		if (!(button_Attack != null))
		{
			return;
		}
		switch (cardType)
		{
		case MultiplayerCollectionStatusQueryResponse.CardType.Available:
			SetGluiButtonOnReleaseInChild(button_Attack, new string[1] { "BUTTON_ATTACK" });
			break;
		case MultiplayerCollectionStatusQueryResponse.CardType.Danger:
			if (button_Defend != null)
			{
				SetGluiButtonOnReleaseInChild(button_Defend, new string[1] { "BUTTON_DEFEND" });
			}
			break;
		case MultiplayerCollectionStatusQueryResponse.CardType.Lost:
			SetGluiButtonOnReleaseInChild(button_Recover, new string[1] { "ATTACK_LOST_ITEM" });
			break;
		}
		GluiStandardButtonContainer component = button_Attack.GetComponent<GluiStandardButtonContainer>();
		component.GetActionData = () => mData;
		if (button_Defend != null)
		{
			component = button_Defend.GetComponent<GluiStandardButtonContainer>();
			component.GetActionData = () => mData;
		}
		if (button_Recover != null)
		{
			component = button_Recover.GetComponent<GluiStandardButtonContainer>();
			component.GetActionData = () => mData;
		}
	}

	public bool UpdateTimer()
	{
		DateTime? dateTime = timeToBeAttacked;
		if (!dateTime.HasValue)
		{
			return false;
		}
		if (timeToBeAttacked.Value.CompareTo(SntpTime.UniversalTime) <= 0)
		{
			timeToBeAttacked = null;
			int num = 0;
			SetGluiTextInChild(text_attackTimeRemaining, StringUtils.FormatTime(num, StringUtils.TimeFormatType.HourMinuteSecond_Colons));
			SetData(mData);
			Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.CheckForUndefendedItems();
			return false;
		}
		int num2 = (int)Mathf.Round((float)timeToBeAttacked.Value.Subtract(SntpTime.UniversalTime).TotalSeconds);
		SetGluiTextInChild(text_attackTimeRemaining, StringUtils.FormatTime(num2, StringUtils.TimeFormatType.HourMinuteSecond_Colons));
		return true;
	}

	public bool HasAttackTimer()
	{
		DateTime? dateTime = timeToBeAttacked;
		return dateTime.HasValue;
	}

	public Texture2D GetTexturePath(string path)
	{
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(path, 1);
		if (cachedResource != null)
		{
			return cachedResource.Resource as Texture2D;
		}
		return null;
	}
}
