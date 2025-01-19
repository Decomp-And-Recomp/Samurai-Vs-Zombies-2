using System;
using UnityEngine;

public class MultiplayerGlobalHelpers : MonoBehaviour
{
	private static TypedWeakReference<GameObject> cachedSender;

	public static GluiPersistentDataWatcher mWatcher;

	public static CollectionItemSchema GetSelectedCard()
	{
		return SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("SELECTED_COLLECTIBLE_CARD") as CollectionItemSchema;
	}

	public static CollectionStatusRecord GetSelectedPotentialConflict()
	{
		return SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("SELECTED_MULTIPLAYER_POTENTIAL_CONFLICT") as CollectionStatusRecord;
	}

	public static void ExtractSelectedCardDataForWave(EMultiplayerMode mode, Action<bool> OnEnemyLoaded)
	{
		CollectionItemSchema itemSchema = GetSelectedCard();
		Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent = null;
		CollectionSchema collectionSet;
		Singleton<Profile>.Instance.MultiplayerData.GetCollectionItemData(itemSchema.CollectionID, out collectionSet);
		if (itemSchema == null)
		{
			OnEnemyLoaded(false);
			return;
		}
		MultiplayerWaveData waveData = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData;
		bool flag = false;
		if (waveData == null)
		{
			waveData = new MultiplayerWaveData();
			flag = true;
		}
		waveData.gameMode = mode;
		waveData.collectionItem_InConflict = itemSchema;
		waveData.playMode = itemSchema.playMode.Key;
		Singleton<PlayModesManager>.Instance.Attacking = mode == EMultiplayerMode.kRecovering || mode == EMultiplayerMode.kAttacking;
		Singleton<PlayerWaveEventData>.Instance.Reset();
		Singleton<PlayModesManager>.Instance.DetermineGameDirection();
		int opponentId = 0;
		if (mode == EMultiplayerMode.kRecovering || mode == EMultiplayerMode.kDefending)
		{
			waveData.potentialConflictForAttack = Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.FindItem(itemSchema.CollectionID);
			if (mode == EMultiplayerMode.kDefending)
			{
				waveData.soulCostToAttack = 0;
				waveData.potentialConflictForAttack.SoulCostToAttack = 0;
			}
			MultiplayerCollectionStatusQueryResponse.MultiplayerCollectionStatusAggregate aggregate;
			Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.Find(itemSchema.CollectionID).Aggregate(Singleton<Profile>.Instance.MultiplayerData.OwnerID, out aggregate);
			waveData.defensiveBuffs = aggregate.defensiveBuffs;
			Singleton<PlayerWaveEventData>.Instance.Unpack(aggregate.attackerData);
			opponentId = (aggregate.firstAttackerID.HasValue ? aggregate.firstAttackerID.Value : 0);
		}
		else
		{
			waveData.potentialConflictForAttack = GetSelectedPotentialConflict();
			if (waveData.potentialConflictForAttack.DefensiveBuffs.Length > 1)
			{
				waveData.defensiveBuffs = waveData.potentialConflictForAttack.DefensiveBuffs;
			}
			opponentId = waveData.potentialConflictForAttack.OwnerID;
			waveData.soulCostToAttack = itemSchema.soulsToAttack;
			Singleton<PlayerWaveEventData>.Instance.Unpack(waveData.potentialConflictForAttack.AttackData);
		}
		Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData = waveData;
		Profile.UpdatePlayMode();
		if (flag)
		{
			string recordTable = Singleton<PlayModesManager>.Instance.selectedModeData.waves.RecordTable;
			string text = WaveSchema.FromIndex(recordTable, WaveSchema.PickRandomRecord(recordTable));
			waveData.waveToPlay = int.Parse(text);
			waveData.missionName = itemSchema.displayName.Key + "." + collectionSet.displayName.Key;
			waveData.waveName = recordTable + "." + text;
		}
		if (opponentId == 0)
		{
			Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent = waveData.potentialConflictForAttack.AIOpponent;
			OnEnemyLoaded(true);
			return;
		}
		Singleton<Profile>.Instance.MultiplayerData.GetOtherUserData(opponentId, delegate(MultiplayerData.MultiplayerUserDataQueryResponse response)
		{
			Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent = response;
			if (response == null)
			{
				if (mode == EMultiplayerMode.kRecovering)
				{
					waveData.gameMode = EMultiplayerMode.kAttacking;
					OnEnemyLoaded(true);
				}
				else
				{
					SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "MenuFixedStrings.Menu_MPVsFail");
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.Menu_MPVsFailButton");
					GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
					mWatcher = new GluiPersistentDataWatcher();
					mWatcher.PersistentEntryToWatch = "ALERT_GENERIC_RESULT";
					mWatcher.Event_WatchedDataChanged += ReturnToStoreMenu;
					mWatcher.StartWatching();
					Singleton<Profile>.Instance.MultiplayerData.Logout();
				}
			}
			else
			{
				waveData.defensiveBuffs[0] = (byte)response.defensiveBuff1;
				waveData.defensiveBuffs[1] = (byte)response.defensiveBuff2;
				if (WeakGlobalInstance<EnemiesShowCase>.Instance != null && Singleton<Profile>.Instance.inVSMultiplayerWave)
				{
					WeakGlobalInstance<EnemiesShowCase>.Instance.Reload(WaveManager.WaveType.Wave_Multiplayer, Singleton<Profile>.Instance.waveToPlay);
				}
				if (mode == EMultiplayerMode.kRecovering)
				{
					Singleton<Profile>.Instance.MultiplayerData.GetOwnedItem(opponentId, itemSchema.CollectionID, delegate(CollectionStatusRecord record)
					{
						if (record != null)
						{
							waveData.potentialConflictForAttack = record;
							waveData.potentialConflictForAttack.CalculateSoulCost();
						}
						else
						{
							waveData.gameMode = EMultiplayerMode.kAttacking;
						}
						OnEnemyLoaded(true);
					});
				}
				else
				{
					OnEnemyLoaded(true);
				}
			}
		});
	}

	public static void ReturnToStoreMenu(object data)
	{
		string text = data as string;
		if (text != null && text == "BUTTON")
		{
			GluiActionSender.SendGluiAction("MENU_MAIN_STORE", null, null);
			mWatcher.StopWatching();
			mWatcher.Event_WatchedDataChanged -= ReturnToStoreMenu;
			mWatcher = null;
		}
	}

	public static void TestFinishedCollection(GameObject sender, object data)
	{
		if (ResultsMenuImpl.CheckedForCompletedCollections)
		{
			return;
		}
		if (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove("COLLECTION_JUST_COMPLETED");
			GluiActionSender.SendGluiAction("COLLECTION_COMPLETED_FAIL", sender, data);
			return;
		}
		ResultsMenuImpl.CheckedForCompletedCollections = true;
		CollectionItemSchema collectionItem_InConflict = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.collectionItem_InConflict;
		CollectionSchema itemSet;
		Singleton<Profile>.Instance.MultiplayerData.GetCollectionItemData(collectionItem_InConflict.CollectionID, out itemSet);
		Singleton<Profile>.Instance.MultiplayerData.RetrieveMyCollection(delegate(GripNetwork.Result result, bool setFinished)
		{
			if (result != 0)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "MenuFixedStrings.Menu_MPCollectionUpdateFail");
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.Menu_MPVsFailButton");
				GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
				mWatcher = new GluiPersistentDataWatcher();
				mWatcher.PersistentEntryToWatch = "ALERT_GENERIC_RESULT";
				mWatcher.Event_WatchedDataChanged += ReturnToStoreMenu;
				mWatcher.StartWatching();
			}
			else if (setFinished)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("COLLECTION_JUST_COMPLETED", itemSet);
				GluiActionSender.SendGluiAction("COLLECTION_COMPLETED_SUCCESS", sender, data);
			}
			else
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove("COLLECTION_JUST_COMPLETED");
				GluiActionSender.SendGluiAction("COLLECTION_COMPLETED_FAIL", sender, data);
			}
		});
	}

	public static void SaveMultiplayerName(GameObject sender)
	{
		string text = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("MULTIPLAYER_NAME_ENTRY_TEXT") as string;
		if (ProfanityFilter.IsStringAcceptable(text))
		{
			Singleton<Profile>.Instance.MultiplayerData.UserName = text;
			SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = false;
			cachedSender = new TypedWeakReference<GameObject>(sender);
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(Singleton<Profile>.Instance.MultiplayerData.SeedRandomCollectionIfNeeded(SaveMultiplayerName_Done));
		}
	}

	public static void SaveMultiplayerName_Done(bool success)
	{
		SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
		if (cachedSender != null && !object.ReferenceEquals(cachedSender.ptr, null))
		{
			GluiActionSender.SendGluiAction("ACTION_NAME_SAVED", cachedSender.ptr, null);
		}
		if (!success)
		{
		}
	}
}
