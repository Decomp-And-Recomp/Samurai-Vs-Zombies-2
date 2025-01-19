using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ConsoleFunctions_Game
{
	public static void register()
	{
		GrConsole console = Singleton<GrConsole>.Instance;
		console.add("tapjoy", delegate(string[] args)
		{
			if (args.Length == 2)
			{
				int result = 0;
				if (int.TryParse(args[1], out result))
				{
					return string.Empty;
				}
			}
			return "Usage: tapjoy <amount>";
		});
		int num = 3;
		if (Singleton<Profile>.Exists)
		{
			GrConsole.Hotkey hotkey = console.SetHotkey(num++, "Profile", string.Empty, true);
			int num2 = 0;
			console.add("printprofile", delegate
			{
				string str = Singleton<Profile>.Instance.PrintToString();
				console.addMessage(GrConsole.eType.Message, str);
				return string.Empty;
			});
			hotkey.SetChildHotkey(num2++, "Print", "printprofile", false);
			GrConsole.Hotkey hotkey2 = hotkey.SetChildHotkey(num2++, "Currency", string.Empty, true);
			int num3 = 0;
			console.add("addcurrency", AddCurrency);
			hotkey2.SetChildHotkey(num3++, "Add SC", "addcurrency soft 10000", false);
			hotkey2.SetChildHotkey(num3++, "Add HC", "addcurrency hard 1000", false);
			hotkey2.SetChildHotkey(num3++, "Add Souls", "addcurrency souls 100", false);
			console.add("clearcurrency", ClearCurrency);
			hotkey2.SetChildHotkey(num3++, "Clear SC", "clearcurrency soft", false);
			hotkey2.SetChildHotkey(num3++, "Clear HC", "clearcurrency hard", false);
			hotkey2.SetChildHotkey(num3++, "Clear Souls", "clearcurrency souls", false);
			console.add("profile", ModifyProfile);
			GrConsole.Hotkey hotkeyA = hotkey.SetChildHotkey(num2++, "Hero", string.Empty, true);
			AddUdamanButtonGroup(hotkeyA, 7, "profile set heroID", ModifyProfile, "Heroes", (HeroSchema s) => s.id);
			hotkey.SetChildHotkey(num2++, "Unlock All Waves", "profile unlockallwaves", false);
			hotkey.SetChildHotkey(num2++, "Reset Achievements", "profile resetachievements", false);
			console.add("timers", Timers);
			hotkey.SetChildHotkey(num2++, "Timer +Hr", "timers add 60", false);
			hotkey.SetChildHotkey(num2++, "Timer Show", "timers show", false);
			GrConsole.Hotkey hotkey3 = hotkey.SetChildHotkey(num2++, "Tutorial", string.Empty, true);
			int num4 = 0;
			console.add("tutorialPassive", TutorialPassive);
			hotkey3.SetChildHotkey(num4++, "SetAll", "tutorialPassive setall", false);
			hotkey3.SetChildHotkey(num4++, "ClearAll", "tutorialPassive clearall", false);
			AddUdamanButtonGroup(hotkey.SetChildHotkey(num2++, "GoldenHelper", string.Empty, true), 7, "goldenhelper", ToggleGoldenHelper, "Helpers", (HelperSchema s) => s.id);
			console.add("waveStats", WaveStats);
			hotkey.SetChildHotkey(num2++, "WaveStats", "waveStats", false);
		}
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.MultiplayerData != null && Singleton<Profile>.Instance.MultiplayerData.Account != null && Singleton<Profile>.Instance.MultiplayerData.Account.Status == GripAccount.LoginStatus.Complete)
		{
			GrConsole.Hotkey hotkey4 = console.SetHotkey(num++, "Multiplayer", string.Empty, true);
			int num5 = 0;
			console.add("collection", Collection);
			hotkey4.SetChildHotkey(num5++, "Display", "collection displayall", false);
			hotkey4.SetChildHotkey(num5++, "Reset", "collection clearall", false);
			console.add("sets", CollectionSets);
			GrConsole.Hotkey hotkey5 = hotkey4.SetChildHotkey(num5++, "Sets Add", string.Empty, true);
			int num6 = 0;
			hotkey5.SetChildHotkey(num6++, "ClearAll", "sets clearall", false);
			hotkey5.SetChildHotkey(num6++, "Sushi +1", "sets add Sushi 1", false);
			hotkey5.SetChildHotkey(num6++, "Tea +1", "sets add Tea 1", false);
			hotkey5.SetChildHotkey(num6++, "Flower +1", "sets add Flower 1", false);
			hotkey5.SetChildHotkey(num6++, "Banner +1", "sets add Banner 1", false);
			hotkey5.SetChildHotkey(num6++, "Enemy +1", "sets add Enemy 1", false);
			hotkey5.SetChildHotkey(num6++, "Sword +1", "sets add Sword 1", false);
			hotkey5.SetChildHotkey(num6++, "Bow +1", "sets add Bow 1", false);
			hotkey5.SetChildHotkey(num6++, "Armor +1", "sets add Armor 1", false);
			hotkey5.SetChildHotkey(num6++, "Horse +1", "sets add Horse 1", false);
			hotkey5 = hotkey4.SetChildHotkey(num5++, "Sets Remove", string.Empty, true);
			num6 = 0;
			hotkey5.SetChildHotkey(num6++, "Sushi -1", "sets add Sushi -1", false);
			hotkey5.SetChildHotkey(num6++, "Tea -1", "sets add Tea -1", false);
			hotkey5.SetChildHotkey(num6++, "Flower -1", "sets add Flower -1", false);
			hotkey5.SetChildHotkey(num6++, "Banner -1", "sets add Banner -1", false);
			hotkey5.SetChildHotkey(num6++, "Enemy -1", "sets add Enemy -1", false);
			hotkey5.SetChildHotkey(num6++, "Sword -1", "sets add Sword -1", false);
			hotkey5.SetChildHotkey(num6++, "Bow -1", "sets add Bow -1", false);
			hotkey5.SetChildHotkey(num6++, "Armor -1", "sets add Armor -1", false);
			hotkey5.SetChildHotkey(num6++, "Horse -1", "sets add Horse -1", false);
			GrConsole.Hotkey hotkey6 = hotkey4.SetChildHotkey(num5++, "VSMode", string.Empty, true);
			int num7 = 0;
			console.add("vsMode", VsMode);
			hotkey6.SetChildHotkey(num7++, "Enable", "vsMode enable", false);
			hotkey6.SetChildHotkey(num7++, "Disable", "vsMode disable", false);
			GrConsole.Hotkey hotkey7 = hotkey4.SetChildHotkey(num5++, "FastLose", string.Empty, true);
			int num8 = 0;
			console.add("FastLoseItem", FastLoseItem);
			hotkey7.SetChildHotkey(num8++, "Enable", "FastLoseItem enable", false);
			hotkey7.SetChildHotkey(num8++, "Disable", "FastLoseItem disable", false);
		}
		if (SingletonMonoBehaviour<FrontEnd>.Exists)
		{
			AddUdamanButtonGroup(console.SetHotkey(num++, "Narratives", string.Empty, true), 7, "narrative", Narrative, NarrativeSchema.UdamanTableName, (NarrativeSchema s) => s.id);
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance != null)
		{
			int num9 = 0;
			console.add("setability", SetAbility);
			GrConsole.Hotkey hotkey8 = console.SetHotkey(num++, "Ability", string.Empty, true);
			hotkey8.SetChildHotkey(num9++, "DivineWind", "setability DivineWind", false);
			hotkey8.SetChildHotkey(num9++, "ThunderStrike", "setability ThunderStrike", false);
			hotkey8.SetChildHotkey(num9++, "DaggerBarrage", "setability DaggerBarrage", false);
			hotkey8.SetChildHotkey(num9++, "ExplosiveCart", "setability ExplosiveCart", false);
			hotkey8.SetChildHotkey(num9++, "FlashBomb", "setability FlashBomb", false);
			hotkey8.SetChildHotkey(num9++, "KatanaSlash", "setability KatanaSlash", false);
			hotkey8.SetChildHotkey(num9++, "SummonTornado", "setability SummonTornado", false);
			hotkey8.ButtonWidth = console.Size.x / (float)num9;
			AddUdamanButtonGroup(console.SetHotkey(num++, "Helpers", string.Empty, true), 7, "spawnhelper", SpawnHelper, "Helpers", (HelperSchema s) => s.id);
			AddUdamanButtonGroup(console.SetHotkey(num++, "Enemies", string.Empty, true), 7, "spawnenemy", SpawnEnemy, "Enemies", (EnemySchema s) => s.id);
			AddUdamanButtonGroup(console.SetHotkey(num++, "Consumable", string.Empty, true), 7, "useconsumable", UseConsumable, "Potions", (PotionSchema s) => s.id);
			int num10 = 0;
			console.add("endwave", EndWave);
			hotkey8 = console.SetHotkey(num++, "Wave", string.Empty, true);
			hotkey8.SetChildHotkey(num10++, "Seppuku", "endwave suicide", false);
			hotkey8.SetChildHotkey(num10++, "Kill", "endwave kill", false);
			hotkey8.SetChildHotkey(num10++, "Win", "endwave win", false);
			hotkey8.SetChildHotkey(num10++, "Lose", "endwave lose", false);
			hotkey8.SetChildHotkey(num10++, "Restart", "endwave restart", false);
			AddUdamanButtonGroup(hotkey8.SetChildHotkey(num10++, "Goto", string.Empty, true), 10, "wave", GotoWave, "Waves", (WaveSchema s) => s.commands, (WaveSchema s) => string.Format("wave {0}", s.index));
			hotkey8.ButtonWidth = console.Size.x / (float)num10;
			int num11 = 0;
			console.add("game", GameCheat);
			hotkey8 = console.SetHotkey(num++, "Game", string.Empty, true);
			hotkey8.SetChildHotkey(num11++, "Invuln On", "game invuln_on", false);
			hotkey8.SetChildHotkey(num11++, "Invuln Off", "game invuln_off", false);
			hotkey8.SetChildHotkey(num11++, "HUD Toggle", "game toggle_hud", false);
			hotkey8.SetChildHotkey(num11++, "Dmg Text Toggle", "game toggle_dmgText", false);
			AddQualityFilterGroup(hotkey8.SetChildHotkey(num11++, "QualityFilter", string.Empty, true), 5, "qualityfilter", QualityFilter);
			GrConsole.Hotkey hotkey9 = hotkey8.SetChildHotkey(num11++, "Spawn Loot", string.Empty, true);
			int num12 = 0;
			console.add("loot", SpawnLoot);
			hotkey9.SetChildHotkey(num12++, "Coins", "loot coin", false);
			hotkey9.SetChildHotkey(num12++, "Gems", "loot gem", false);
			hotkey9.SetChildHotkey(num12++, "Leadership", "loot leadership", false);
			hotkey9.SetChildHotkey(num12++, "Souls", "loot soul", false);
			hotkey9.SetChildHotkey(num12++, "Present A", "loot presentA", false);
			hotkey9.SetChildHotkey(num12++, "Present B", "loot presentB", false);
			hotkey9.SetChildHotkey(num12++, "Present C", "loot presentC", false);
			hotkey9.SetChildHotkey(num12++, "Present D", "loot presentD", false);
			hotkey9.ButtonWidth = console.Size.x / (float)num12;
			hotkey8.ButtonWidth = console.Size.x / (float)num11;
		}
	}

	private static void AddUdamanButtonGroup<T>(GrConsole.Hotkey hotkeyA, int numChildButtons, string command, Func<string[], string> action, string tableName, Func<T, string> getId)
	{
		AddUdamanButtonGroup(hotkeyA, numChildButtons, command, action, tableName, getId, null);
	}

	private static void AddUdamanButtonGroup<T>(GrConsole.Hotkey hotkeyA, int numChildButtons, string command, Func<string[], string> action, string tableName, Func<T, string> getId, Func<T, string> getCommand)
	{
		if (DataBundleRuntime.Instance == null)
		{
			return;
		}
		GrConsole instance = Singleton<GrConsole>.Instance;
		int recordTableLength = DataBundleRuntime.Instance.GetRecordTableLength(typeof(T), tableName);
		int num = Mathf.CeilToInt((float)recordTableLength / (float)numChildButtons);
		hotkeyA.Setup(hotkeyA.displayableName, hotkeyA.command, true, num);
		hotkeyA.ButtonWidth = instance.Size.x / (float)num;
		float value = instance.Size.x / (float)numChildButtons;
		instance.add(command, action);
		int num2 = 0;
		int num3 = 0;
		GrConsole.Hotkey hotkey = hotkeyA.SetChildHotkey(num2, num2.ToString(), string.Empty, true);
		hotkey.ButtonWidth = value;
		foreach (T item in DataBundleRuntime.Instance.EnumerateRecords<T>(tableName))
		{
			if (num3 >= numChildButtons)
			{
				num2++;
				num3 = 0;
				hotkey = hotkeyA.SetChildHotkey(num2, num2.ToString(), string.Empty, true);
				if (hotkey == null)
				{
					break;
				}
				hotkey.ButtonWidth = value;
			}
			string text = getId(item);
			string command2 = ((getCommand == null) ? string.Format("{0} {1}", command, text) : getCommand(item));
			hotkey.SetChildHotkey(num3, text, command2, false);
			num3++;
		}
	}

	private static void AddQualityFilterGroup(GrConsole.Hotkey hotkeyA, int numChildButtons, string command, Func<string[], string> action)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		float value = instance.Size.x / (float)numChildButtons;
		instance.add(command, action);
		int num = 0;
		int num2 = 0;
		GrConsole.Hotkey hotkey = hotkeyA.SetChildHotkey(num, num.ToString(), string.Empty, true);
		hotkey.ButtonWidth = value;
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(QualityFilter));
		for (int i = 0; i < array.Length; i++)
		{
			QualityFilter qualityFilter = (QualityFilter)array[i];
			QualityFilter.FilterSetting[] settings = qualityFilter.settings;
			foreach (QualityFilter.FilterSetting filterSetting in settings)
			{
				GameObject[] objects = filterSetting.objects;
				foreach (GameObject gameObject in objects)
				{
					if (gameObject != null)
					{
						if (num2 >= numChildButtons)
						{
							num++;
							num2 = 0;
							hotkey = hotkeyA.SetChildHotkey(num, num.ToString(), string.Empty, true);
							hotkey.ButtonWidth = value;
						}
						string name = gameObject.name;
						string command2 = string.Format("{0} {1}", command, name);
						hotkey.SetChildHotkey(num2, name, command2, false);
						num2++;
					}
				}
			}
		}
	}

	private static string AddCurrency(string[] args)
	{
		if (args.Length == 3)
		{
			if (Singleton<Profile>.Exists)
			{
				int result = 0;
				int.TryParse(args[2], out result);
				GrConsole instance = Singleton<GrConsole>.Instance;
				switch (args[1])
				{
				case "soft":
					Singleton<Profile>.Instance.AddCoins(result, "Cheat");
					instance.addMessage(GrConsole.eType.Message, "Coins = " + Singleton<Profile>.Instance.coins);
					break;
				case "hard":
					Singleton<Profile>.Instance.AddGems(result, "Cheat");
					instance.addMessage(GrConsole.eType.Message, "Gems = " + Singleton<Profile>.Instance.gems);
					break;
				case "souls":
					Singleton<Profile>.Instance.souls += result;
					instance.addMessage(GrConsole.eType.Message, "Souls = " + Singleton<Profile>.Instance.souls);
					break;
				}
			}
			return string.Empty;
		}
		return "Usage: addcurrency <soft/hard> <amount>";
	}

	private static string ClearCurrency(string[] args)
	{
		if (args.Length == 2)
		{
			if (Singleton<Profile>.Exists)
			{
				GrConsole instance = Singleton<GrConsole>.Instance;
				switch (args[1])
				{
				case "soft":
					Singleton<Profile>.Instance.coins = 0;
					instance.addMessage(GrConsole.eType.Message, "Coins = 0");
					break;
				case "hard":
					Singleton<Profile>.Instance.SpendGems(Singleton<Profile>.Instance.gems);
					instance.addMessage(GrConsole.eType.Message, "Gems = 0");
					Singleton<Profile>.Instance.gems = 0;
					break;
				case "souls":
					Singleton<Profile>.Instance.souls = 0;
					instance.addMessage(GrConsole.eType.Message, "Souls = 0");
					break;
				}
			}
			return string.Empty;
		}
		return "Usage: clearcurrency <soft/hard>";
	}

	private static string VsMode(string[] args)
	{
		if (args.Length >= 2 && Singleton<Profile>.Exists)
		{
			switch (args[1])
			{
			case "enable":
				Profile.UseVsMode = true;
				break;
			case "disable":
				Profile.UseVsMode = false;
				break;
			}
		}
		return string.Empty;
	}

	private static string FastLoseItem(string[] args)
	{
		if (args.Length >= 2 && Singleton<Profile>.Exists)
		{
			switch (args[1])
			{
			case "enable":
				Profile.FastLoseItem = true;
				break;
			case "disable":
				Profile.FastLoseItem = false;
				break;
			}
		}
		return string.Empty;
	}

	private static string Collection(string[] args)
	{
		if (args.Length >= 2)
		{
			if (Singleton<Profile>.Exists)
			{
				GrConsole console = Singleton<GrConsole>.Instance;
				switch (args[1])
				{
				case "clearall":
					Singleton<Profile>.Instance.MultiplayerData.Reset();
					break;
				case "seedrandom":
					SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(Singleton<Profile>.Instance.MultiplayerData.SeedRandomCollectionIfNeeded(null));
					break;
				case "ownall":
					Singleton<Profile>.Instance.MultiplayerData.CollectionData.ForEach(delegate(CollectionSchema record)
					{
						record.Items.ForEachWithIndex(delegate(CollectionItemSchema item, int index)
						{
							Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.Add_ItemOwned(item, Singleton<Profile>.Instance.MultiplayerData.OwnerID);
						});
					});
					break;
				case "clear":
				{
					int count = 1;
					if (args.Length == 1)
					{
						count = int.Parse(args[2]);
					}
					Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.RemoveRandom(count);
					break;
				}
				case "attackall":
					Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.TestAttackAll();
					break;
				case "defendall":
					Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.TestDefendAll();
					break;
				case "displayall":
				{
					if (Singleton<Profile>.Instance.MultiplayerData.AllUserDataFields != null)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("UserData: ");
						foreach (GripField allUserDataField in Singleton<Profile>.Instance.MultiplayerData.AllUserDataFields)
						{
							stringBuilder.AppendFormat("{0}={1}, ", allUserDataField.mName, allUserDataField.GetField().ToString());
						}
						console.addMessage(GrConsole.eType.Message, stringBuilder.ToString());
					}
					float num = MultiplayerCollectionStatus.GetDefenseTimeLimit();
					TimeSpan timeSpan = new TimeSpan(0, 0, (int)num);
					console.addMessage(GrConsole.eType.Message, " -- Attack time being added is " + timeSpan.ToString());
					List<string> collectionsToDisplay = new List<string>();
					Singleton<Profile>.Instance.MultiplayerData.CollectionStatus.Collection.ForEach(delegate(CollectionStatusRecord record)
					{
						CollectionSchema collectionSet;
						CollectionItemSchema collectionItemData = Singleton<Profile>.Instance.MultiplayerData.GetCollectionItemData(record.CollectionID, out collectionSet);
						string text2 = "NONE";
						if (record.AttackTime.HasValue)
						{
							TimeSpan timeSpan2 = record.AttackTime.Value.AddSeconds(MultiplayerCollectionStatus.GetDefenseTimeLimit()).Subtract(ApplicationUtilities.Now);
							text2 = new TimeSpan(timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds).ToString();
						}
						collectionsToDisplay.Add(string.Concat("ID", record.CollectionID, "\t ", collectionItemData.displayName.Key, "\t [Attacker ID", record.AttackerID, " Time:", record.AttackTime, " Remaining:", text2, "] from set ", collectionSet.displayName.Key));
					});
					collectionsToDisplay.Sort();
					collectionsToDisplay.ForEach(delegate(string text)
					{
						console.addMessage(GrConsole.eType.Message, text);
					});
					break;
				}
				}
			}
			return string.Empty;
		}
		return "Usage: collection <clearall/seedrandom>";
	}

	private static string CollectionSets(string[] args)
	{
		if (args.Length >= 2)
		{
			if (Singleton<Profile>.Exists)
			{
				GrConsole instance = Singleton<GrConsole>.Instance;
				switch (args[1])
				{
				case "clearall":
					Singleton<Profile>.Instance.MultiplayerData.CollectionData.ForEach(delegate(CollectionSchema record)
					{
						Singleton<Profile>.Instance.MultiplayerData.AddCompletedSet(record.id, -999);
					});
					break;
				case "add":
				{
					int countToAdd = 1;
					string text = string.Empty;
					if (args.Length == 4)
					{
						text = args[2];
						countToAdd = int.Parse(args[3]);
					}
					Singleton<Profile>.Instance.MultiplayerData.AddCompletedSet(text, countToAdd);
					instance.addMessage(GrConsole.eType.Message, "Set " + text + " collected " + Singleton<Profile>.Instance.MultiplayerData.TotalTimesCompletedSet(text) + " times");
					break;
				}
				}
			}
			return string.Empty;
		}
		return "Usage: collection <clearall/seedrandom>";
	}

	private static string SetAbility(string[] args)
	{
		if (args.Length == 2 && WeakGlobalMonoBehavior<InGameImpl>.Instance.SetCheatAbility(args[1]))
		{
			return string.Empty;
		}
		return "Usage: setability <ability_name>";
	}

	private static string GameCheat(string[] args)
	{
		if (args.Length == 2)
		{
			switch (args[1])
			{
			case "invuln_on":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.SetHeroInvulnByDefault(true);
				return string.Empty;
			case "invuln_off":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.SetHeroInvulnByDefault(false);
				return string.Empty;
			case "toggle_hud":
				WeakGlobalMonoBehavior<HUD>.Instance.gameObject.SetActive(!WeakGlobalMonoBehavior<HUD>.Instance.gameObject.activeSelf);
				return string.Empty;
			case "toggle_dmgText":
				Singleton<Profile>.Instance.showHealthText = !Singleton<Profile>.Instance.showHealthText;
				return string.Empty;
			}
		}
		return "Usage: game <option>";
	}

	private static string SpawnHelper(string[] args)
	{
		if (args.Length == 2)
		{
			string text = args[1];
			Singleton<HelpersDatabase>.Instance.LoadInGameData(text);
			Character character = WeakGlobalInstance<Leadership>.Instance.ForceSpawn(text);
			if (character != null)
			{
				GrConsole instance = Singleton<GrConsole>.Instance;
				instance.addMessage(GrConsole.eType.Message, "Spawned helper \"" + text + "\"");
				return string.Empty;
			}
		}
		return "Usage: spawnhelper <helper_name>";
	}

	private static string SpawnEnemy(string[] args)
	{
		if (args.Length == 2)
		{
			string text = args[1];
			Singleton<EnemiesDatabase>.Instance.LoadInGameData(text);
			Enemy enemy = WeakGlobalInstance<WaveManager>.Instance.ConstructEnemy(text);
			if (enemy != null)
			{
				WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(enemy);
				GrConsole instance = Singleton<GrConsole>.Instance;
				instance.addMessage(GrConsole.eType.Message, "Spawned enemy \"" + text + "\"");
				return string.Empty;
			}
		}
		return "Usage: spawnenemy <enemy_name>";
	}

	private static string UseConsumable(string[] args)
	{
		if (args.Length == 2)
		{
			string text = args[1];
			bool flag = Singleton<PotionsDatabase>.Instance.Execute(text);
			GrConsole instance = Singleton<GrConsole>.Instance;
			instance.addMessage(GrConsole.eType.Message, "Activated consumable \"" + text + "\", wasUsed = " + flag);
			return string.Empty;
		}
		return "Usage: useconsumable <id>";
	}

	private static string EndWave(string[] args)
	{
		if (args.Length == 2)
		{
			switch (args[1])
			{
			case "suicide":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.ForceDeath();
				break;
			case "kill":
				WeakGlobalInstance<CharactersManager>.Instance.KillAllEnemies();
				break;
			case "win":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.Win();
				break;
			case "lose":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.Lose();
				break;
			case "restart":
				WeakGlobalMonoBehavior<InGameImpl>.Instance.Restart();
				break;
			}
			return string.Empty;
		}
		return "Usage: endwave <win/lose/restart>";
	}

	private static string GotoWave(string[] args)
	{
		if (args.Length == 2)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				Singleton<Profile>.Instance.wave_SinglePlayerGame = result;
				WaveManager.LoadSceneForWave();
			}
			return string.Empty;
		}
		return "Usage: wave <number>";
	}

	private static string ModifyProfile(string[] args)
	{
		if (args.Length >= 2)
		{
			string text = args[1];
			string text2 = ((args.Length < 3) ? string.Empty : args[2]);
			string text3 = ((args.Length < 3) ? string.Empty : args[3]);
			switch (text)
			{
			case "set":
				if (text2 == "heroID" && WeakGlobalMonoBehavior<InGameImpl>.Instance != null)
				{
					Singleton<Profile>.Instance.heroID = text3;
					WeakGlobalMonoBehavior<InGameImpl>.Instance.CreateHero(0, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.controlledObject.transform);
				}
				else
				{
					Singleton<Profile>.Instance[text2] = text3;
				}
				break;
			case "unlockallwaves":
			{
				for (int i = 0; i <= 999; i++)
				{
					Singleton<Profile>.Instance.SetWaveLevel(i, 2);
				}
				break;
			}
			case "resetachievements":
				Singleton<Achievements>.Instance.Reset();
				break;
			}
			return string.Empty;
		}
		return "Usage: profile [set/unlockallwaves] <key> <value>";
	}

	private static string TutorialPassive(string[] args)
	{
		if (args.Length >= 2)
		{
			switch (args[1])
			{
			case "clearall":
				SingletonMonoBehaviour<TutorialMain>.Instance.SetAllTutorialDoneFlags(false);
				return "All Passive Tutorial Flags Cleared";
			case "setall":
				SingletonMonoBehaviour<TutorialMain>.Instance.SetAllTutorialDoneFlags(true);
				return "All Passive Tutorial Flags Set";
			}
		}
		return string.Empty;
	}

	private static string Timers(string[] args)
	{
		if (args.Length >= 2)
		{
			GrConsole console = Singleton<GrConsole>.Instance;
			switch (args[1])
			{
			case "add":
			{
				int num = 1;
				if (args.Length == 3)
				{
					num = int.Parse(args[2]);
				}
				SingletonSpawningMonoBehaviour<DecaySystem>.Instance.FastForward((uint)(num * 60), true);
				break;
			}
			case "show":
				SingletonSpawningMonoBehaviour<DecaySystem>.Instance.Timers.ForEach(delegate(DecayTimer timer)
				{
					console.addMessage(GrConsole.eType.Message, "Timer [" + timer.Data.name + "] " + StringUtils.FormatTime(timer.TimeToNextTick(), StringUtils.TimeFormatType.HourMinuteSecond_Colons));
				});
				break;
			}
		}
		return string.Empty;
	}

	private static string QualityFilter(string[] args)
	{
		if (args.Length >= 2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 1; i < args.Length; i++)
			{
				stringBuilder.AppendFormat("{0} ", args[i]);
			}
			string b = stringBuilder.ToString().TrimEnd();
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(QualityFilter));
			for (int j = 0; j < array.Length; j++)
			{
				QualityFilter qualityFilter = (QualityFilter)array[j];
				QualityFilter.FilterSetting[] settings = qualityFilter.settings;
				foreach (QualityFilter.FilterSetting filterSetting in settings)
				{
					GameObject[] objects = filterSetting.objects;
					foreach (GameObject gameObject in objects)
					{
						if (gameObject != null && string.Equals(gameObject.name, b, StringComparison.OrdinalIgnoreCase))
						{
							gameObject.SetActive(!gameObject.activeSelf);
						}
					}
				}
			}
			return string.Empty;
		}
		return "Usage: qualityfilter <GameObject name>";
	}

	private static string SpawnLoot(string[] args)
	{
		if (args.Length == 2)
		{
			string dropType = args[1];
			WeakGlobalInstance<CollectableManager>.Instance.ForceSpawnResourceType(dropType, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.position);
			return string.Empty;
		}
		return "Usage: loot <drop name>";
	}

	private static string ToggleGoldenHelper(string[] args)
	{
		if (args.Length == 2)
		{
			string text = args[1];
			bool goldenHelperUnlocked = Singleton<Profile>.Instance.GetGoldenHelperUnlocked(text);
			Singleton<Profile>.Instance.SetGoldenHelperUnlocked(text, !goldenHelperUnlocked);
			Singleton<GrConsole>.Instance.addMessage(GrConsole.eType.Message, string.Format("GoldenHelper \"{0}\" unlocked = {1}", text, !goldenHelperUnlocked));
			return string.Empty;
		}
		return "Usage: goldenhelper <Helper ID>";
	}

	private static string Narrative(string[] args)
	{
		if (args.Length == 2)
		{
			string recordKey = args[1];
			GluiState_NarrativePanel.resourceToLoad = DataBundleRuntime.Instance.GetValue<string>(typeof(NarrativeSchema), NarrativeSchema.UdamanTableName, recordKey, "prefab", true);
			GluiState_NarrativePanel.ShowIfSet();
			return string.Empty;
		}
		return "Usage: narrative <id>";
	}

	private static string WaveStats(string[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("WAVE STATS\n------------------------\n");
		foreach (string item in DataBundleRuntime.Instance.EnumerateTables<WaveSchema>())
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (WaveSchema item2 in DataBundleRuntime.Instance.EnumerateRecords<WaveSchema>(item))
			{
				dictionary.Clear();
				WaveSchema waveSchema = WaveSchema.Initialize(item2);
				WaveCommandSchema[] commands = waveSchema.Commands;
				foreach (WaveCommandSchema waveCommandSchema in commands)
				{
					if (!DataBundleRecordKey.IsNullOrEmpty(waveCommandSchema.enemy))
					{
						dictionary[waveCommandSchema.enemy.ToString()] = true;
					}
					else
					{
						if (DataBundleRecordTable.IsNullOrEmpty(waveCommandSchema.enemyGroup))
						{
							continue;
						}
						foreach (EnemyGroupSchema item3 in DataBundleRuntime.Instance.EnumerateRecords<EnemyGroupSchema>(waveCommandSchema.enemyGroup))
						{
							if (!DataBundleRecordKey.IsNullOrEmpty(item3.enemy_1))
							{
								dictionary[item3.enemy_1.ToString()] = true;
							}
							if (!DataBundleRecordKey.IsNullOrEmpty(item3.enemy_2))
							{
								dictionary[item3.enemy_2.ToString()] = true;
							}
							if (!DataBundleRecordKey.IsNullOrEmpty(item3.enemy_3))
							{
								dictionary[item3.enemy_3.ToString()] = true;
							}
						}
					}
				}
				stringBuilder.AppendFormat("[{0}.{1}] unique enemies = {2}\n", item, waveSchema.index, dictionary.Count);
			}
		}
		string str = stringBuilder.ToString();
		Singleton<GrConsole>.Instance.addMessage(GrConsole.eType.Message, str);
		return string.Empty;
	}
}
