public class MultiplayerWaveData
{
	public string missionName;

	public string waveName;

	public int waveToPlay;

	public int soulCostToAttack;

	public EMultiplayerMode gameMode;

	public CollectionItemSchema collectionItem_InConflict;

	public CollectionStatusRecord potentialConflictForAttack;

	public string playMode;

	public byte[] defensiveBuffs = new byte[2];
}
