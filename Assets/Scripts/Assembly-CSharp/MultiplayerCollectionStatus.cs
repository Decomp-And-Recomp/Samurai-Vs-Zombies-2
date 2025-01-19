using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerCollectionStatus
{
	private MultiplayerCollectionStatusQueryPool pool = new MultiplayerCollectionStatusQueryPool();

	private List<CollectionStatusRecord> collection = new List<CollectionStatusRecord>();

	public List<CollectionStatusRecord> Collection
	{
		get
		{
			return collection;
		}
	}

	public void Clear()
	{
		collection.Clear();
	}

	public void ClearQueryCache()
	{
		pool.Clear();
	}

	public void Add_ItemOwned(CollectionItemSchema itemData, int ownerID)
	{
		byte[] attackData = new byte[0];
		Add_ItemOwned(itemData, ownerID, attackData);
	}

	public void Add_ItemOwned(CollectionItemSchema itemData, int ownerID, byte[] attackData)
	{
		if (itemData == null)
		{
			return;
		}
		if (Singleton<Profile>.Instance.MultiplayerData.Account == null || Singleton<Profile>.Instance.MultiplayerData.Account.Status != GripAccount.LoginStatus.Complete)
		{
			Singleton<Profile>.Instance.saveData.SetValueInt("CollectionItemToAdd", itemData.CollectionID + 1);
			return;
		}
		List<CollectionStatusRecord> list = collection.FindAll((CollectionStatusRecord r) => r.CollectionID == itemData.CollectionID);
		if (list.Count > 0)
		{
			if (list.Count > 1)
			{
				for (int i = 1; i < list.Count; i++)
				{
					Remove(list[i]);
				}
			}
			GripField[] fields = new GripField[2]
			{
				list[0].Fields[3],
				list[0].Fields[1]
			};
			list[0].AttackData = attackData;
			list[0].AttackerID = 0;
			GripNetwork.UpdateRecord("PlayerCollection", list[0].RecordID, fields, delegate
			{
			});
			return;
		}
		CollectionStatusRecord newRecord = new CollectionStatusRecord(ownerID, itemData.CollectionID);
		newRecord.AttackData = attackData;
		collection.Add(newRecord);
		GripNetwork.CreateRecord("PlayerCollection", newRecord.Fields, delegate(GripNetwork.Result result, int recordID)
		{
			if (result == GripNetwork.Result.Success)
			{
				newRecord.RecordID = recordID;
				Singleton<Profile>.Instance.saveData.SetValueInt("CollectionItemToAdd", 0);
			}
			else
			{
				Singleton<Profile>.Instance.saveData.SetValueInt("CollectionItemToAdd", itemData.CollectionID + 1);
				Singleton<Profile>.Instance.MultiplayerData.Logout();
			}
		});
	}

	public void Add(CollectionStatusRecord record)
	{
		collection.Add(record);
	}

	public void RemoveAll()
	{
		collection.ForEach(delegate(CollectionStatusRecord record)
		{
			GripNetwork.RemoveRecord("PlayerCollection", record.RecordID, delegate
			{
			});
		});
		collection.Clear();
	}

	public void RemoveRandom(int count)
	{
		while (count > 0 && collection.Count > 0)
		{
			count--;
			CollectionStatusRecord record = collection[UnityEngine.Random.Range(0, collection.Count)];
			Remove(record);
		}
	}

	public void Remove(CollectionItemSchema itemData)
	{
		List<CollectionStatusRecord> list = collection.FindAll((CollectionStatusRecord r) => r.CollectionID == itemData.CollectionID);
		foreach (CollectionStatusRecord item in list)
		{
			Remove(item);
		}
	}

	public bool Remove(CollectionStatusRecord record)
	{
		if (Singleton<Profile>.Instance.MultiplayerData.Account == null || Singleton<Profile>.Instance.MultiplayerData.Account.Status != GripAccount.LoginStatus.Complete)
		{
			return false;
		}
		if (record == null)
		{
			return false;
		}
		if (!collection.Remove(record))
		{
		}
		if (record.RecordID != 0)
		{
			Singleton<Profile>.Instance.FlurrySession.CollectiblesForfeited++;
			GripNetwork.RemoveRecord("PlayerCollection", record.RecordID, delegate
			{
				GluiActionSender.SendGluiAction("QUERY_HAS_COLLECTIBLE_CONFLICT", null, null);
			});
		}
		return true;
	}

	public static int GetDefenseTimeLimit()
	{
		if (Profile.FastLoseItem)
		{
			return 120;
		}
		return (int)Singleton<Profile>.Instance.GetCurrentUpgradeAmount("DefenseTimer");
	}

	public void CheckForUndefendedItems()
	{
		int defensePeriodSeconds = GetDefenseTimeLimit();
		DateTime markedTime = new DateTime(2000, 1, 1);
		List<CollectionStatusRecord> list = collection.FindAll(delegate(CollectionStatusRecord record)
		{
			int result2;
			if (record.AttackerID.HasValue)
			{
				int? attackerID = record.AttackerID;
				if ((attackerID.GetValueOrDefault() != 0 || !attackerID.HasValue) && record.AttackTime.Value.AddSeconds(defensePeriodSeconds).CompareTo(SntpTime.UniversalTime) <= 0)
				{
					DateTime? attackTime = record.AttackTime;
					result2 = ((attackTime.GetValueOrDefault() != markedTime || !attackTime.HasValue) ? 1 : 0);
					goto IL_0093;
				}
			}
			result2 = 0;
			goto IL_0093;
			IL_0093:
			return (byte)result2 != 0;
		});
		list.ForEach(delegate(CollectionStatusRecord record)
		{
			record.AttackTime = markedTime;
			GripField[] fields = new GripField[1] { record.Fields[2] };
			GripNetwork.UpdateRecord("PlayerCollection", record.RecordID, fields, delegate(GripNetwork.Result result)
			{
				if (result == GripNetwork.Result.Success)
				{
					Singleton<Profile>.Instance.collectionItemLossRating += 20;
				}
			});
		});
	}

	public int GetShortestDefenseDuration()
	{
		List<CollectionStatusRecord> list = collection.FindAll(delegate(CollectionStatusRecord record)
		{
			int result;
			if (record.AttackerID.HasValue)
			{
				int? attackerID = record.AttackerID;
				result = ((attackerID.GetValueOrDefault() != 0 || !attackerID.HasValue) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		});
		int num = -1;
		int defenseTimeLimit = GetDefenseTimeLimit();
		foreach (CollectionStatusRecord item in list)
		{
			int num2 = (int)(item.AttackTime.Value.AddSeconds(defenseTimeLimit) - SntpTime.UniversalTime).TotalSeconds;
			if (num2 > 0 && (num2 < num || num < 0))
			{
				num = num2;
			}
		}
		return num;
	}

	public void Clamp_AttackedItemTimes()
	{
		collection.ForEach(delegate(CollectionStatusRecord record)
		{
			if (record.AttackTime.HasValue)
			{
				DateTime? attackTime = record.AttackTime;
				if (attackTime.HasValue && attackTime.Value > SntpTime.UniversalTime)
				{
					record.AttackTime = SntpTime.UniversalTime;
					GripField[] fields = new GripField[1] { record.Fields[2] };
					GripNetwork.UpdateRecord("PlayerCollection", record.RecordID, fields, delegate
					{
					});
				}
			}
		});
	}

	public MultiplayerCollectionStatusQueryResponse Find(int collectionID)
	{
		MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = new MultiplayerCollectionStatusQueryResponse("FIND_USER_RECORDS", collectionID);
		multiplayerCollectionStatusQueryResponse.records = collection.FindAll((CollectionStatusRecord record) => record.CollectionID == collectionID);
		return multiplayerCollectionStatusQueryResponse;
	}

	public CollectionStatusRecord FindItem(int collectionID)
	{
		return collection.Find((CollectionStatusRecord record) => record.CollectionID == collectionID);
	}

	public MultiplayerCollectionStatusQueryResponse.MultiplayerCollectionStatusAggregate FindFirstConflict()
	{
		bool isNewQuery;
		MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = pool.Query("FIND_FIRST_CONFLICT", 0, out isNewQuery);
		if (isNewQuery)
		{
			multiplayerCollectionStatusQueryResponse.records = collection;
		}
		MultiplayerCollectionStatusQueryResponse.MultiplayerCollectionStatusAggregate aggregate;
		multiplayerCollectionStatusQueryResponse.Aggregate(Singleton<Profile>.Instance.MultiplayerData.OwnerID, out aggregate);
		return aggregate;
	}

	public void FindPotentialChallenges(int collectionID, int AIOpponentsToAdd, Action<MultiplayerCollectionStatusQueryResponse> onComplete)
	{
		if (Singleton<Profile>.Instance.MultiplayerData.Account == null || Singleton<Profile>.Instance.MultiplayerData.Account.Status != GripAccount.LoginStatus.Complete)
		{
			onComplete(null);
			return;
		}
		if (onComplete == null)
		{
			onComplete(null);
			return;
		}
		bool isNewQuery;
		MultiplayerCollectionStatusQueryResponse response = pool.Query("FIND_POTENTIAL_CHALLENGES", collectionID, out isNewQuery);
		if (isNewQuery)
		{
			response.records = new List<CollectionStatusRecord>();
			response.friendRecords = new List<CollectionStatusRecord>();
			response.nonFriendRecords = new List<CollectionStatusRecord>();
			DateTime currentTime = SntpTime.UniversalTime;
			string sqlFilter = string.Format("collectionID={0} AND version={1} AND attackerID={2}", collectionID, CollectionStatusRecord.kCollectionVersion, 0);
			CollectionItemSchema selectedCard = MultiplayerGlobalHelpers.GetSelectedCard();
			int num = ((selectedCard != null) ? selectedCard.soulsToAttack : 0);
			int num2 = 3;
			while (num2 > 0 && Singleton<Profile>.Instance.souls < num + MultiplayerData.AmuletExtraCost(num2))
			{
				num2--;
			}
			if (num2 < 3)
			{
				sqlFilter += string.Format(" AND defenseBuff2<={0}", num2);
			}
			string nonFriendQuery = sqlFilter;
			int myDefense = Singleton<Profile>.Instance.MaxDefensiveRating;
			int num3 = myDefense * 2;
			int num4 = myDefense / 2;
			nonFriendQuery += string.Format(" AND attackRating > {0} AND attackRating < {1}", num4, num3);
			if (Integrity.IsJailbroken())
			{
				nonFriendQuery += " AND JailBroken = 1";
			}
			GripNetwork.CountRecords("PlayerCollection", nonFriendQuery, delegate(GripNetwork.Result countResult, int count)
			{
				if (countResult == GripNetwork.Result.Success)
				{
					Action<int> action = delegate(int recordCount)
					{
						int num6 = recordCount * 2;
						int num7 = count / num6;
						if (num7 > 0 && count % num6 < 10)
						{
							num7--;
						}
						int pageNumber = UnityEngine.Random.Range(0, num7);
						GripNetwork.SearchRecords("PlayerCollection", Singleton<Profile>.Instance.MultiplayerData.kPlayerCollectionFields, nonFriendQuery, null, null, num6, pageNumber, delegate(GripNetwork.Result result, GripField[,] fields)
						{
							List<CollectionStatusRecord> list = new List<CollectionStatusRecord>();
							if (fields != null)
							{
								for (int j = 0; j < fields.GetLength(0); j++)
								{
									CollectionStatusRecord collectionStatusRecord2 = CollectionStatusRecord.FromFields(fields, j);
									if (collectionStatusRecord2.OwnerID != Singleton<Profile>.Instance.MultiplayerData.OwnerID)
									{
										int? attackerID2 = collectionStatusRecord2.AttackerID;
										if (attackerID2.GetValueOrDefault() == 0 && attackerID2.HasValue && (currentTime - collectionStatusRecord2.ShieldTime).TotalSeconds > 0.0)
										{
											list.Add(collectionStatusRecord2);
										}
									}
								}
							}
							if (list.Count > 0)
							{
								list.Sort(new MultiplayerCollectionStatusQueryResponse.RelativeAttackRatingComparer());
								int num8 = 0;
								foreach (CollectionStatusRecord item in list)
								{
									if (item.AttackRating > myDefense)
									{
										break;
									}
									num8++;
								}
								int num9 = list.Count - num8;
								if (num8 > 0)
								{
									int num10 = Mathf.Max(1, recordCount - num9);
									int num11 = num8 - 1;
									while (num10 > 0 && num11 > 0)
									{
										CollectionStatusRecord collectionStatusRecord3 = list[num11];
										bool flag = false;
										foreach (CollectionStatusRecord record in response.records)
										{
											if (record.OwnerID == collectionStatusRecord3.OwnerID)
											{
												flag = true;
												break;
											}
										}
										if (!flag)
										{
											response.records.Insert(0, collectionStatusRecord3);
											collectionStatusRecord3.CalculateSoulCost();
											response.nonFriendRecords.Add(collectionStatusRecord3);
											num10--;
										}
										num11--;
									}
								}
								int count2 = response.nonFriendRecords.Count;
								for (int num11 = num8; num11 < list.Count; num11++)
								{
									CollectionStatusRecord collectionStatusRecord4 = list[num11];
									bool flag2 = false;
									foreach (CollectionStatusRecord record2 in response.records)
									{
										if (record2.OwnerID == collectionStatusRecord4.OwnerID)
										{
											flag2 = true;
											break;
										}
									}
									if (!flag2)
									{
										response.records.Add(collectionStatusRecord4);
										collectionStatusRecord4.CalculateSoulCost();
										response.nonFriendRecords.Add(collectionStatusRecord4);
									}
								}
								int num12 = response.nonFriendRecords.Count - recordCount;
								num9 = Mathf.Max(1, response.nonFriendRecords.Count - count2);
								int num13 = Mathf.Max(2, num9 / (num12 + 1));
								int num14 = count2;
								for (int k = 0; k < num12; k++)
								{
									num14 += num13;
									if (num14 < response.nonFriendRecords.Count - 1 && num14 > 0)
									{
										response.nonFriendRecords.RemoveAt(num14);
										num14--;
									}
								}
							}
							response.DeduplicateRecords();
							AIOpponentsToAdd = Mathf.Min(AIOpponentsToAdd, 30 - response.records.Count);
							if (AIOpponentsToAdd > 0)
							{
								AddAIOpponents(response, AIOpponentsToAdd);
							}
							response.SortRecords();
							onComplete(response);
						});
					};
					if (Singleton<Profile>.Instance.MultiplayerData.Friends != null && Singleton<Profile>.Instance.MultiplayerData.Friends.Count > 0)
					{
						string text = sqlFilter + " AND ownerid IN (";
						int num5 = 0;
						foreach (MultiplayerData.FriendData friend in Singleton<Profile>.Instance.MultiplayerData.Friends)
						{
							if (num5 > 0)
							{
								text += ", ";
							}
							text = text + "'" + friend.gamespyID + "'";
							num5++;
						}
						text += ")";
						int a = 30;
						GripNetwork.SearchRecords("PlayerCollection", Singleton<Profile>.Instance.MultiplayerData.kPlayerCollectionFields, text, null, null, num5, 1, delegate(GripNetwork.Result result, GripField[,] fields)
						{
							if (fields != null)
							{
								for (int i = 0; i < fields.GetLength(0); i++)
								{
									CollectionStatusRecord collectionStatusRecord = CollectionStatusRecord.FromFields(fields, i);
									if (collectionStatusRecord.OwnerID != Singleton<Profile>.Instance.MultiplayerData.OwnerID)
									{
										int? attackerID = collectionStatusRecord.AttackerID;
										if (attackerID.GetValueOrDefault() == 0 && attackerID.HasValue && (currentTime - collectionStatusRecord.ShieldTime).TotalSeconds > 0.0)
										{
											collectionStatusRecord.SoulCostToAttack = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.collectionItem_InConflict.soulsToAttack;
											response.records.Add(collectionStatusRecord);
											response.friendRecords.Add(collectionStatusRecord);
										}
									}
								}
							}
						});
						a = Mathf.Max(a, 15);
						action(a);
					}
					else
					{
						action(30);
					}
				}
				else
				{
					AddAIOpponents(response, AIOpponentsToAdd);
					onComplete(response);
				}
			});
		}
		else
		{
			onComplete(response);
		}
	}

	public void TestAttackAll()
	{
		collection.ForEach(delegate(CollectionStatusRecord record)
		{
			if (record.OwnerID == Singleton<Profile>.Instance.MultiplayerData.OwnerID)
			{
				Singleton<Profile>.Instance.MultiplayerData.ReportChallengeSuccess(record);
			}
		});
	}

	public void TestDefendAll()
	{
		collection.ForEach(delegate(CollectionStatusRecord record)
		{
			if (record.AttackerID.HasValue)
			{
				Singleton<Profile>.Instance.MultiplayerData.ReportDefenseSuccess(record);
			}
		});
	}

	public List<int> GetRandomSet(int count, int maxValue)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			int item;
			do
			{
				item = UnityEngine.Random.Range(0, maxValue);
			}
			while (list.Contains(item));
			list.Add(item);
		}
		return list;
	}

	public void AddAIOpponents(MultiplayerCollectionStatusQueryResponse response, int countToAdd)
	{
		List<int> randomSet = GetRandomSet(countToAdd, MultiplayerAIOpponentSchema.Count("MultiplayerAIOpponentData"));
		randomSet.ForEach(delegate(int value)
		{
			CollectionStatusRecord collectionStatusRecord = new CollectionStatusRecord(0, 0)
			{
				AttackerID = value,
				SoulCostToAttack = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.collectionItem_InConflict.soulsToAttack
			};
			string tableRecordKey = MultiplayerAIOpponentSchema.FromIndex("MultiplayerAIOpponentData", value);
			MultiplayerAIOpponentSchema record = MultiplayerAIOpponentSchema.GetRecord(tableRecordKey);
			collectionStatusRecord.AIOpponent.loadout.playerName = StringUtils.GetStringFromStringRef(record.displayName);
			response.records.Add(collectionStatusRecord);
			response.nonFriendRecords.Add(collectionStatusRecord);
		});
	}
}
