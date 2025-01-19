using System.Collections.Generic;

public class PlayStatistics : Singleton<PlayStatistics>
{
	public class Data
	{
		public class LootEntry
		{
			public string id;

			public int num;

			public string packId;

			public ECollectableType? presentType;
		}

		public int wavePlayed;

		public int wavePlayedLevel;

		public bool victory;

		public int inGameGoldGained;

		public int inGameSoulsAwarded;

		public int inGameSoulsGained;

		public List<LootEntry> loot;

		public int totalLootDroppedValue;

		public int droppedCoins;

		public int droppedGems;

		public int droppedSouls;

		public int droppedUnlockables;

		public int droppedOtherValue;

		public int revivesAtStart;

		public int revivesUsed;

		public int teaAtStart;

		public int teaUsed;

		public int sushiAtStart;

		public int sushiUsed;

		public bool shouldShowRateMeDialog;

		public bool shouldAwardMysteryBox;

		public WaveManager.WaveType waveTypePlayed;

		public Data()
		{
			loot = new List<LootEntry>();
		}

		public void AddLoot(string id, int num, ECollectableType? presentType = null)
		{
			if (num <= 0)
			{
				return;
			}
			if (!presentType.HasValue || presentType.Value < ECollectableType.presentA || presentType.Value > ECollectableType.presentD)
			{
				presentType = null;
			}
			switch (CashIn.GetType(id))
			{
			case CashIn.ItemType.Unknown:
				return;
			case CashIn.ItemType.Gems:
				id = "gems";
				break;
			case CashIn.ItemType.Coins:
				id = "coins";
				break;
			case CashIn.ItemType.Balls:
				id = "balls";
				break;
			}
			if (!presentType.HasValue)
			{
				foreach (LootEntry item in loot)
				{
					if (item.id == id && !item.presentType.HasValue)
					{
						item.num += num;
						return;
					}
				}
			}
			LootEntry lootEntry = new LootEntry();
			lootEntry.id = id;
			lootEntry.num = num;
			lootEntry.presentType = presentType;
			loot.Add(lootEntry);
		}
	}

	public Data data;

	public PlayStatistics()
	{
		Reset();
	}

	public void Reset()
	{
		data = new Data();
		data.revivesAtStart = Singleton<Profile>.Instance.GetNumPotions("revivePotion");
		data.teaAtStart = Singleton<Profile>.Instance.GetNumPotions("leadershipPotion");
		data.sushiAtStart = Singleton<Profile>.Instance.GetNumPotions("healthPotion");
	}
}
