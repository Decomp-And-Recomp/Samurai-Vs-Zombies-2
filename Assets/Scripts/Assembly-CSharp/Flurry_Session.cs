using System.Collections.Generic;

public class Flurry_Session
{
	public MultiplayerData MultiplayerData
	{
		get
		{
			return Singleton<Profile>.Instance.MultiplayerData;
		}
	}

	public int MP_AttacksPlayed { get; set; }

	public int MP_AttacksWon { get; set; }

	public int MP_DefensesPlayed { get; set; }

	public int MP_DefensesWon { get; set; }

	public int CollectiblesForfeited { get; set; }

	public int MP_FriendAttacks { get; set; }

	public int MP_SoulsAtStart { get; set; }

	public int MP_SoulsUsed { get; set; }

	public static int CurrentSPWave()
	{
		return (!Singleton<Profile>.Instance.inDailyChallenge) ? Singleton<Profile>.Instance.wave_SinglePlayerGame : (-1);
	}

	public void StartSession()
	{
		MP_AttacksPlayed = 0;
		MP_AttacksWon = 0;
		MP_DefensesPlayed = 0;
		MP_DefensesWon = 0;
		MP_FriendAttacks = 0;
		CollectiblesForfeited = 0;
		MP_SoulsUsed = 0;
		MP_SoulsAtStart = Singleton<Profile>.Instance.souls;
	}

	public void ReportSessionEnd()
	{
		Singleton<Analytics>.Instance.LogEvent("MultiplayerWavesDuringSession", Analytics.Param("MP_AttacksPlayed", MP_AttacksPlayed), Analytics.Param("MP_AttacksWon", MP_AttacksWon), Analytics.Param("MP_DefensesPlayed", MP_DefensesPlayed), Analytics.Param("MP_DefensesWon", MP_DefensesWon), Analytics.Param("CollectiblesForfeited", CollectiblesForfeited), Analytics.Param("TotalCompleteCollections", MultiplayerData.TotalCompleteCollections()), Analytics.Param("MP_FriendAttacks", MP_FriendAttacks), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		Singleton<Analytics>.Instance.LogEvent("EnergyUseDuringSession", Analytics.Param("EnergyAtStart", MP_SoulsAtStart), Analytics.Param("EnergyAtStop", Singleton<Profile>.Instance.souls), Analytics.Param("EnergyUsed", MP_SoulsUsed), Analytics.Param("MaxEnergy", Singleton<Profile>.Instance.GetMaxSouls()), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		Singleton<Analytics>.Instance.KontagentEvent("MultiplayerWavesDuringSession", "MultiplayerWavesDuringSession", Singleton<Profile>.Instance.wave_SinglePlayerGame, MP_AttacksPlayed + MP_DefensesPlayed, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPAttacksWon", MP_AttacksWon.ToString()), Analytics.KParam("MPAttacksPlayed", MP_AttacksPlayed.ToString()), Analytics.KParam("MPDefensesWon", MP_DefensesWon.ToString()), Analytics.KParam("MPDefensesPlayed", MP_DefensesPlayed.ToString()));
		StartSession();
	}

	public void ReportWaveLost()
	{
		if (MultiplayerData.IsMultiplayerGameSessionActive())
		{
			if (MultiplayerData.MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending)
			{
				ReportMPWaveStats("MP_AttackLost");
			}
			else
			{
				ReportMPWaveStats("MP_DefenseLost");
			}
		}
		else if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			ReportSinglePlayerWaveStats("WaveLost", "DAILY_CHALLENGE_FAILED");
		}
		else
		{
			ReportSinglePlayerWaveStats("WaveLost", "MISSION_FAILED");
		}
	}

	public void ReportWaveWon()
	{
		Singleton<Profile>.Instance.playerLevel++;
		if (MultiplayerData.IsMultiplayerGameSessionActive())
		{
			Singleton<Profile>.Instance.mpWavesWon++;
			if (MultiplayerData.MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending)
			{
				ReportMPWaveStats("MP_AttackWon");
			}
			else
			{
				ReportMPWaveStats("MP_DefenseWon");
			}
			return;
		}
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			ReportSinglePlayerWaveStats("WaveLost", "DAILY_CHALLENGE_COMPLETED");
		}
		else
		{
			ReportSinglePlayerWaveStats("WaveWon", "MISSION_COMPLETED");
		}
		if (!Singleton<Profile>.Instance.inDailyChallenge)
		{
			if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 1 && Singleton<Profile>.Instance.GetWaveLevel(1) == 1)
			{
				Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep7_CompleteWave1");
			}
			else if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 2 && Singleton<Profile>.Instance.GetWaveLevel(2) == 1)
			{
				Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep19_CompleteWave2");
			}
		}
	}

	private void ReportWaveConsumableUse()
	{
		Singleton<Analytics>.Instance.LogEvent("WaveConsumableUse", Analytics.Param("WaveNumber", CurrentSPWave()), Analytics.Param("RevivesAtStart", Singleton<PlayStatistics>.Instance.data.revivesAtStart), Analytics.Param("RevivesUsed", Singleton<PlayStatistics>.Instance.data.revivesUsed), Analytics.Param("TeaAtStart", Singleton<PlayStatistics>.Instance.data.teaAtStart), Analytics.Param("TeaUsed", Singleton<PlayStatistics>.Instance.data.teaUsed), Analytics.Param("SushiAtStart", Singleton<PlayStatistics>.Instance.data.sushiAtStart), Analytics.Param("SushiUsed", Singleton<PlayStatistics>.Instance.data.sushiUsed), Analytics.Param("CharmUsed", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm ?? string.Empty));
		if (Singleton<PlayStatistics>.Instance.data.revivesUsed > 0)
		{
			Singleton<Analytics>.Instance.KontagentEvent("Revive", "CONSUMABLE_USED", Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, Singleton<PlayStatistics>.Instance.data.revivesUsed, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
		}
		if (Singleton<PlayStatistics>.Instance.data.sushiUsed > 0)
		{
			Singleton<Analytics>.Instance.KontagentEvent("Sushi", "CONSUMABLE_USED", Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, Singleton<PlayStatistics>.Instance.data.sushiUsed, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
		}
		if (Singleton<PlayStatistics>.Instance.data.teaUsed > 0)
		{
			Singleton<Analytics>.Instance.KontagentEvent("Tea", "CONSUMABLE_USED", Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, Singleton<PlayStatistics>.Instance.data.teaUsed, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
		}
		if (!string.IsNullOrEmpty(WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm))
		{
			Singleton<Analytics>.Instance.KontagentEvent(WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm, "CONSUMABLE_USED", Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, 1, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
		}
	}

	private void ReportMPWaveStats(string eventName)
	{
		Singleton<Analytics>.Instance.LogEvent(eventName, Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		CollectionItemSchema selectedCard = MultiplayerGlobalHelpers.GetSelectedCard();
		string name = ((selectedCard == null) ? "UnknownItem" : selectedCard.CollectionID.ToString());
		Singleton<Analytics>.Instance.KontagentEvent(name, eventName, Singleton<Profile>.Instance.heroID, 0, 0, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()));
	}

	private void ReportSinglePlayerWaveStats(string eventName, string kEventName)
	{
		int num = (int)((float)WeakGlobalInstance<WaveManager>.Instance.enemiesKilledSoFar / (float)WeakGlobalInstance<WaveManager>.Instance.totalEnemies * 100f);
		Singleton<Analytics>.Instance.LogEvent(eventName, Analytics.Param("WaveNumber", CurrentSPWave()), Analytics.Param("PercentCompleted", num), Analytics.Param("Hero", Singleton<Profile>.Instance.heroID), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		Singleton<Analytics>.Instance.KontagentEvent(CurrentSPWave().ToString(), kEventName, Singleton<Profile>.Instance.heroID, 0, 0, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()));
		ReportWaveConsumableUse();
	}

	public void ReportWaveStarted()
	{
		if (MultiplayerData.IsMultiplayerGameSessionActive())
		{
			if (MultiplayerData.MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending)
			{
				ReportMultiPlayerWaveStarted("MP_AttackStarted", true, 0);
			}
			else
			{
				ReportMultiPlayerWaveStarted("MP_DefenseStarted", false, 0);
			}
		}
		else
		{
			ReportSinglePlayerWaveStarted("WaveStarted", CurrentSPWave());
		}
	}

	private void ReportMultiPlayerWaveStarted(string eventName, bool attack, int revengeAttack)
	{
		string text = SortAndSeparate(Singleton<Profile>.Instance.GetSelectedHelpers(), ",");
		string text2 = SortAndSeparate(Singleton<Profile>.Instance.GetSelectedAbilities(), ",");
		if (attack)
		{
			Singleton<Analytics>.Instance.LogEvent(eventName, Analytics.Param("MissionName", MultiplayerData.MultiplayerGameSessionData.missionName), Analytics.Param("AttemptNumber", 0), Analytics.Param("Hero", Singleton<Profile>.Instance.heroID), Analytics.Param("Allies", text), Analytics.Param("Abilities", text2), Analytics.Param("Charms", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm), Analytics.Param("IsRevengeAttack", revengeAttack), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		}
		else
		{
			Singleton<Analytics>.Instance.LogEvent(eventName, Analytics.Param("MissionName", MultiplayerData.MultiplayerGameSessionData.missionName), Analytics.Param("AttemptNumber", 0), Analytics.Param("Hero", Singleton<Profile>.Instance.heroID), Analytics.Param("Allies", text), Analytics.Param("Abilities", text2), Analytics.Param("Charms", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		}
		CollectionItemSchema selectedCard = MultiplayerGlobalHelpers.GetSelectedCard();
		string name = ((selectedCard == null) ? "UnknownItem" : selectedCard.CollectionID.ToString());
		Singleton<Analytics>.Instance.KontagentEvent(name, eventName, Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, 0, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()), Analytics.KParam("Allies", text), Analytics.KParam("Abilities", text2), Analytics.KParam("Charm", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm));
	}

	private string SortAndSeparate(List<string> strings, string separator)
	{
		strings.Sort();
		string text = string.Empty;
		for (int i = 0; i < strings.Count; i++)
		{
			text += strings[i];
			if (i < strings.Count - 1)
			{
				text += separator;
			}
		}
		return text;
	}

	private void ReportSinglePlayerWaveStarted(string eventName, int currentWave)
	{
		int waveAttemptCount = Singleton<Profile>.Instance.GetWaveAttemptCount(currentWave);
		string text = SortAndSeparate(Singleton<Profile>.Instance.GetSelectedHelpers(), ",");
		string text2 = SortAndSeparate(Singleton<Profile>.Instance.GetSelectedAbilities(), ",");
		Singleton<Analytics>.Instance.LogEvent(eventName, new KeyValuePair<string, object>("WaveNumber", currentWave), new KeyValuePair<string, object>("AttemptNumber", waveAttemptCount), new KeyValuePair<string, object>("Hero", Singleton<Profile>.Instance.heroID), new KeyValuePair<string, object>("Allies", text), new KeyValuePair<string, object>("Abilities", text2), new KeyValuePair<string, object>("Charms", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm));
		if (waveAttemptCount == 1)
		{
			Singleton<Analytics>.Instance.LogEvent("WaveStartedForFirstTime", new KeyValuePair<string, object>("WaveNumber", currentWave));
		}
		string st = ((!Singleton<Profile>.Instance.inDailyChallenge) ? "MISSION_START" : "DAILY_CHALLENGE_START");
		Singleton<Analytics>.Instance.KontagentEvent(currentWave.ToString(), st, Singleton<Profile>.Instance.heroID, Singleton<Profile>.Instance.wave_SinglePlayerGame, waveAttemptCount, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()), Analytics.KParam("Allies", text), Analytics.KParam("Abilities", text2), Analytics.KParam("Charm", WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm));
	}
}
