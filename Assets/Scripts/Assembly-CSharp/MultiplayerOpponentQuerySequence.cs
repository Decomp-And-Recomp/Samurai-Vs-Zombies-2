using UnityEngine;

public class MultiplayerOpponentQuerySequence
{
	private static GameObject sender;

	public static void QueryStart(GameObject sender, int collectionID)
	{
		GluiActionSender.SendGluiAction("SPINNER_ON", sender, null);
		SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = false;
		CollectionItemSchema selectedCard = MultiplayerGlobalHelpers.GetSelectedCard();
		MultiplayerWaveData multiplayerWaveData = new MultiplayerWaveData();
		multiplayerWaveData.collectionItem_InConflict = selectedCard;
		multiplayerWaveData.playMode = selectedCard.playMode.Key;
		Singleton<PlayerWaveEventData>.Instance.Reset();
		Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData = multiplayerWaveData;
		Profile.UpdatePlayMode();
		string recordTable = Singleton<PlayModesManager>.Instance.selectedModeData.waves.RecordTable;
		string text = WaveSchema.FromIndex(recordTable, WaveSchema.PickRandomRecord(recordTable));
		multiplayerWaveData.waveToPlay = int.Parse(text);
		multiplayerWaveData.missionName = selectedCard.displayName.Key;
		multiplayerWaveData.waveName = recordTable + "." + text;
		MultiplayerOpponentQuerySequence.sender = sender;
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_OPPONENT_QUERY_DONE", "False");
		Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.FindPotentialChallenges(collectionID, 1, QueryComplete);
	}

	private static void QueryComplete(MultiplayerCollectionStatusQueryResponse response)
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_OPPONENT_RECORDS", response);
		if (response != null)
		{
			QuerySuccess();
			return;
		}
		GluiActionSender.SendGluiAction("SPINNER_OFF", sender, null);
		SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
	}

	private static void QuerySuccess()
	{
		GluiActionSender.SendGluiAction("SPINNER_OFF", sender, null);
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_OPPONENT_QUERY_DONE", "True");
		GluiDelayedAction.Create("ATTACK_QUERY_SUCCESS", 0.25f, sender, true);
		sender = null;
	}
}
