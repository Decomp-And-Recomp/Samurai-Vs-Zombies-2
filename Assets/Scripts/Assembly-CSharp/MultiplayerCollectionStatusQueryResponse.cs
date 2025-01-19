using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerCollectionStatusQueryResponse
{
	public enum CardType
	{
		Available = 0,
		Danger = 1,
		Owned = 2,
		Lost = 3
	}

	public class MultiplayerCollectionStatusAggregate
	{
		public int? firstAttackerID;

		public DateTime? firstAttackerTime;

		public int ownerCount;

		public bool ownedByPlayer;

		public int attackerCount;

		public byte[] defensiveBuffs = new byte[2];

		public byte[] attackerData;
	}

	public class CollectionStatusOwnerComparer : IComparer<CollectionStatusRecord>
	{
		public int Compare(CollectionStatusRecord a, CollectionStatusRecord b)
		{
			return a.OwnerID.CompareTo(b.OwnerID);
		}
	}

	public class RelativeAttackRatingComparer : IComparer<CollectionStatusRecord>
	{
		public int Compare(CollectionStatusRecord a, CollectionStatusRecord b)
		{
			int num = a.AttackRating - Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.defenseRating;
			int value = b.AttackRating - Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.defenseRating;
			return num.CompareTo(value);
		}
	}

	public class AttackRatingComparer : IComparer<CollectionStatusRecord>
	{
		public int Compare(CollectionStatusRecord a, CollectionStatusRecord b)
		{
			return a.AttackRating.CompareTo(b.AttackRating);
		}
	}

	public List<CollectionStatusRecord> records;

	public List<CollectionStatusRecord> friendRecords;

	public List<CollectionStatusRecord> nonFriendRecords;

	public float createdTime;

	public string queryType;

	public int queryID;

	public MultiplayerCollectionStatusQueryResponse(string queryType, int queryID)
	{
		createdTime = Time.time;
		this.queryType = queryType;
		this.queryID = queryID;
	}

	public void Aggregate(int playerID, out MultiplayerCollectionStatusAggregate aggregate)
	{
		aggregate = new MultiplayerCollectionStatusAggregate();
		foreach (CollectionStatusRecord record in records)
		{
			if (record.AttackerID.HasValue)
			{
				int? attackerID = record.AttackerID;
				if (attackerID.GetValueOrDefault() != 0 || !attackerID.HasValue)
				{
					aggregate.firstAttackerID = record.AttackerID;
					aggregate.firstAttackerTime = record.AttackTime;
					aggregate.attackerData = record.AttackData;
					aggregate.defensiveBuffs = record.DefensiveBuffs;
					aggregate.attackerCount++;
				}
			}
			if (record.OwnerID != 0)
			{
				aggregate.ownerCount++;
			}
			if (record.OwnerID == playerID)
			{
				aggregate.ownedByPlayer = true;
			}
		}
	}

	public CardType GetCardType(MultiplayerCollectionItemDescriptor itemDescriptor)
	{
		int? firstAttackerID = itemDescriptor.aggregateData.firstAttackerID;
		if (firstAttackerID.HasValue)
		{
			int defenseTimeLimit = MultiplayerCollectionStatus.GetDefenseTimeLimit();
			if (itemDescriptor.aggregateData.firstAttackerTime.Value.AddSeconds(defenseTimeLimit).CompareTo(SntpTime.UniversalTime) <= 0)
			{
				return CardType.Lost;
			}
			return CardType.Danger;
		}
		if (itemDescriptor.aggregateData.ownedByPlayer)
		{
			return CardType.Owned;
		}
		if (itemDescriptor.aggregateData.ownerCount > 0)
		{
			return CardType.Available;
		}
		return CardType.Available;
	}

	public void SortRecords()
	{
		if (records.Count > 1)
		{
			records.Sort(new AttackRatingComparer());
			friendRecords.Sort(new AttackRatingComparer());
			nonFriendRecords.Sort(new AttackRatingComparer());
		}
	}

	public void DeduplicateRecords()
	{
		if (records.Count > 1)
		{
			records.DeduplicateSortedList(new CollectionStatusOwnerComparer());
		}
	}

	public void ShuffleRecords()
	{
		if (records.Count > 1)
		{
			records.Shuffle();
		}
	}
}
