using System;
using UnityEngine;

public class CollectionStatusRecord
{
	public enum ECollectionStatusField
	{
		kCollectionId = 0,
		kAttackerId = 1,
		kAttackTime = 2,
		kAttackData = 3,
		kDefenseBuff1 = 4,
		kDefenseBuff2 = 5,
		kUserName = 6,
		kVersion = 7,
		kAttackRating = 8,
		kJailBroken = 9,
		kShieldTime = 10,
		COUNT = 11
	}

	public static readonly int kCollectionVersion = 11;

	public MultiplayerData.MultiplayerUserDataQueryResponse AIOpponent;

	private GripField[] fields;

	public int RecordID { get; set; }

	public int OwnerID { get; set; }

	public int CollectionID
	{
		get
		{
			return fields[0].mInt.Value;
		}
		set
		{
			fields[0].mInt = value;
		}
	}

	public int? AttackerID
	{
		get
		{
			return fields[1].mInt;
		}
		set
		{
			fields[1].mInt = value;
		}
	}

	public DateTime? AttackTime
	{
		get
		{
			return fields[2].mDateAndTime;
		}
		set
		{
			fields[2].mDateAndTime = value;
		}
	}

	public byte[] AttackData
	{
		get
		{
			return fields[3].mBinaryData;
		}
		set
		{
			fields[3].mBinaryData = value;
		}
	}

	public int DefenseBuff1
	{
		get
		{
			return fields[4].mInt.GetValueOrDefault();
		}
		set
		{
			fields[4].mInt = value;
			DefensiveBuffs[0] = (byte)value;
		}
	}

	public int DefenseBuff2
	{
		get
		{
			return fields[5].mInt.GetValueOrDefault();
		}
		set
		{
			fields[5].mInt = value;
			DefensiveBuffs[1] = (byte)value;
		}
	}

	public string UserName
	{
		get
		{
			return fields[6].mString;
		}
		set
		{
			fields[6].mString = value;
		}
	}

	public int Version
	{
		get
		{
			return fields[7].mInt.Value;
		}
		set
		{
			fields[7].mInt = value;
		}
	}

	public int AttackRating
	{
		get
		{
			return fields[8].mInt.Value;
		}
		set
		{
			fields[8].mInt = value;
		}
	}

	public sbyte JailBroken
	{
		get
		{
			return fields[9].mByte.Value;
		}
		set
		{
			fields[9].mByte = value;
		}
	}

	public DateTime ShieldTime
	{
		get
		{
			return fields[10].mDateAndTime.Value;
		}
		set
		{
			fields[10].mDateAndTime = value;
		}
	}

	public GripField[] Fields
	{
		get
		{
			return fields;
		}
	}

	public bool IsFacebookFriend { get; set; }

	public bool IsGameCenterFriend { get; set; }

	public int SoulCostToAttack { get; set; }

	public byte[] DefensiveBuffs { get; private set; }

	public CollectionStatusRecord(int ownerID, int collectionID)
	{
		DefensiveBuffs = new byte[2];
		fields = new GripField[11]
		{
			new GripField("collectionID", GripField.GripFieldType.Int),
			new GripField("attackerID", GripField.GripFieldType.Int),
			new GripField("attackTime", GripField.GripFieldType.DateAndTime),
			new GripField("attackData", GripField.GripFieldType.BinaryData),
			new GripField("defenseBuff1", GripField.GripFieldType.Int),
			new GripField("defenseBuff2", GripField.GripFieldType.Int),
			new GripField("username", GripField.GripFieldType.UnicodeString),
			new GripField("version", GripField.GripFieldType.Int),
			new GripField("attackRating", GripField.GripFieldType.Int),
			new GripField("JailBroken", GripField.GripFieldType.Byte),
			new GripField("ShieldTime", GripField.GripFieldType.DateAndTime)
		};
		RecordID = 0;
		OwnerID = ownerID;
		CollectionID = collectionID;
		AttackerID = 0;
		AttackTime = SntpTime.UniversalTime;
		AttackData = new byte[0];
		DefenseBuff1 = Singleton<Profile>.Instance.MultiplayerData.DefenseBuff1;
		DefenseBuff2 = Singleton<Profile>.Instance.MultiplayerData.DefenseBuff2;
		UserName = Singleton<Profile>.Instance.MultiplayerData.UserName;
		Version = kCollectionVersion;
		AttackRating = Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.defenseRating;
		JailBroken = (sbyte)(Integrity.IsJailbroken() ? 1 : 0);
		ShieldTime = new DateTime(2000, 1, 1);
		SwapFacebookName();
		if (ownerID == 0)
		{
			AIOpponent = new MultiplayerData.MultiplayerUserDataQueryResponse();
			AIOpponent.loadout = new MultiplayerProfileLoadout();
			AttackRating = AIOpponent.loadout.defenseRating;
		}
	}

	private void SwapFacebookName()
	{
		foreach (MultiplayerData.FriendData friend in Singleton<Profile>.Instance.MultiplayerData.Friends)
		{
			if (friend.gamespyID == OwnerID)
			{
				UserName = friend.friendName;
				IsFacebookFriend = friend.isFacebookFriend;
				IsGameCenterFriend = friend.isGameCenterFriend;
				break;
			}
		}
	}

	public static CollectionStatusRecord FromFields(GripField[,] fields, int index)
	{
		int value = fields[index, 1].mInt.Value;
		int value2 = fields[index, 2].mInt.Value;
		CollectionStatusRecord collectionStatusRecord = new CollectionStatusRecord(value, value2);
		collectionStatusRecord.RecordID = fields[index, 0].mInt.Value;
		collectionStatusRecord.DefensiveBuffs = new byte[4];
		collectionStatusRecord.AttackerID = fields[index, 3].mInt.Value;
		collectionStatusRecord.AttackTime = fields[index, 4].mDateAndTime.Value;
		collectionStatusRecord.AttackData = fields[index, 5].mBinaryData;
		collectionStatusRecord.DefenseBuff1 = fields[index, 6].mInt.GetValueOrDefault();
		collectionStatusRecord.DefenseBuff2 = fields[index, 7].mInt.GetValueOrDefault();
		collectionStatusRecord.UserName = fields[index, 8].mString;
		collectionStatusRecord.Version = fields[index, 9].mInt.GetValueOrDefault();
		collectionStatusRecord.AttackRating = fields[index, 10].mInt.GetValueOrDefault();
		collectionStatusRecord.JailBroken = fields[index, 11].mByte.GetValueOrDefault();
		collectionStatusRecord.ShieldTime = fields[index, 12].mDateAndTime.GetValueOrDefault();
		collectionStatusRecord.SwapFacebookName();
		return collectionStatusRecord;
	}

	public void CalculateSoulCost()
	{
		int num = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.collectionItem_InConflict.soulsToAttack;
		if (DefenseBuff2 > 0)
		{
			num += MultiplayerData.AmuletExtraCost(DefenseBuff2);
		}
		if (AttackRating > Singleton<Profile>.Instance.MaxDefensiveRating && Singleton<Profile>.Instance.MaxDefensiveRating > 0)
		{
			float value = 0.5f * ((float)(AttackRating - Singleton<Profile>.Instance.MaxDefensiveRating) / (float)Singleton<Profile>.Instance.MaxDefensiveRating);
			value = Mathf.Clamp(value, 0f, 0.5f);
			num = (int)((float)num * (1f - value));
		}
		SoulCostToAttack = num;
	}

	public override string ToString()
	{
		return string.Format("[CollectionStatusRecord: RecordID={0}, OwnerID={1}, CollectionID={2}, AttackerID={3}, AttackTime={4}, AttackData.Length={5}]", RecordID, OwnerID, CollectionID, AttackerID, AttackTime, (AttackData != null) ? AttackData.Length : 0);
	}
}
