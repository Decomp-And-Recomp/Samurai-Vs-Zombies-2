using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MultiplayerProfileLoadout
{
	public static readonly int kMaxHelpers = 6;

	public static readonly int kMaxAbilities = 5;

	private int mVersion;

	public string heroId = string.Empty;

	public int heroLevel;

	public int leadershipLevel;

	public int meleeLevel;

	public int bowLevel;

	public int armorLevel;

	public int baseLevel;

	public int archerLevel;

	public int bellLevel;

	public int pitLevel;

	public int flowersCollected;

	public int bannersCollected;

	public int swordsCollected;

	public int bowsCollected;

	public int armorCollected;

	public int horsesCollected;

	public string playerName = string.Empty;

	public HelperSelectionInfo[] selectedHelpers = new HelperSelectionInfo[kMaxHelpers];

	public List<string> abilityIdList = new List<string>();

	public int[] abilityLevel = new int[kMaxAbilities];

	public bool isDirty;

	public int defenseRating;

	public int heroRating;

	public int helperRating;

	public int abilityRating;

	public int gateRating;

	public int archerRating;

	public int bellRating;

	public int pitRating;

	private Dictionary<string, int> additionalHelperLevels = new Dictionary<string, int>();

	public MultiplayerProfileLoadout()
	{
		isDirty = false;
		Unpack(null);
	}

	public MultiplayerProfileLoadout(byte[] bytes)
	{
		isDirty = false;
		Unpack(bytes);
	}

	public void UpdateLocalProfile()
	{
		if (!Singleton<Profile>.Instance.HasSetupDefenses)
		{
			Singleton<Profile>.Instance.SelectedDefendHero = Singleton<Profile>.Instance.heroID;
			Singleton<Profile>.Instance.SetSelectedDefendHelpers(Singleton<Profile>.Instance.GetSelectedHelpers());
			Singleton<Profile>.Instance.SetSelectedDefendAbilities(Singleton<Profile>.Instance.GetSelectedAbilities());
		}
		playerName = string.Empty;
		mVersion = CollectionStatusRecord.kCollectionVersion;
		UpdateString(ref heroId, Singleton<Profile>.Instance.SelectedDefendHero);
		UpdateInt(ref heroLevel, Singleton<Profile>.Instance.heroLevel);
		UpdateInt(ref leadershipLevel, Singleton<Profile>.Instance.initialLeadershipLevel);
		UpdateInt(ref meleeLevel, Singleton<Profile>.Instance.GetMeleeWeaponLevel(heroId));
		UpdateInt(ref bowLevel, Singleton<Profile>.Instance.GetRangedWeaponLevel(heroId));
		UpdateInt(ref armorLevel, Singleton<Profile>.Instance.GetArmorLevel(heroId));
		UpdateInt(ref baseLevel, Singleton<Profile>.Instance.baseLevel);
		UpdateInt(ref archerLevel, Singleton<Profile>.Instance.archerLevel);
		UpdateInt(ref bellLevel, Singleton<Profile>.Instance.bellLevel);
		UpdateInt(ref pitLevel, Singleton<Profile>.Instance.pitLevel);
		UpdateInt(ref flowersCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Flower"));
		UpdateInt(ref bannersCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Banner"));
		UpdateInt(ref swordsCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Sword"));
		UpdateInt(ref bowsCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Bow"));
		UpdateInt(ref armorCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Armor"));
		UpdateInt(ref horsesCollected, Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Horse"));
		List<string> selectedDefendHelpers = Singleton<Profile>.Instance.GetSelectedDefendHelpers();
		HelperSelectionInfo[] array = selectedHelpers;
		selectedHelpers = new HelperSelectionInfo[kMaxHelpers];
		int num = 0;
		foreach (string item in selectedDefendHelpers)
		{
			HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[item];
			if (helperSchema != null && helperSchema.summonIndex > 0)
			{
				HelperSelectionInfo helperSelectionInfo = new HelperSelectionInfo();
				helperSelectionInfo.helperId = helperSchema.id;
				helperSelectionInfo.summonId = helperSchema.summonIndex;
				helperSelectionInfo.level = Singleton<Profile>.Instance.GetHelperLevel(helperSchema.id);
				helperSelectionInfo.golden = Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helperSchema.id);
				selectedHelpers[num] = helperSelectionInfo;
				num++;
			}
		}
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null || array[i] != null)
			{
				if ((selectedHelpers[i] == null && array[i] != null) || (selectedHelpers[i] != null && array[i] == null))
				{
					isDirty = true;
					break;
				}
				if (selectedHelpers[i].level != array[i].level || selectedHelpers[i].golden != array[i].golden || selectedHelpers[i].helperId != array[i].helperId)
				{
					isDirty = true;
					break;
				}
			}
		}
		List<string> selectedDefendAbilities = Singleton<Profile>.Instance.GetSelectedDefendAbilities();
		for (int j = 0; j < kMaxAbilities; j++)
		{
			if (j < selectedDefendAbilities.Count)
			{
				UpdateStringList(ref abilityIdList, j, selectedDefendAbilities[j]);
				UpdateInt(ref abilityLevel[j], Singleton<Profile>.Instance.GetAbilityLevel(selectedDefendAbilities[j]));
			}
			else
			{
				UpdateStringList(ref abilityIdList, j, string.Empty);
				UpdateInt(ref abilityLevel[j], 0);
			}
		}
		CalculateDefenseRating();
		if (defenseRating > Singleton<Profile>.Instance.MaxDefensiveRating)
		{
			Singleton<Profile>.Instance.MaxDefensiveRating = defenseRating;
		}
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("DEFENSE_RATING", StringUtils.FormatAmountString(defenseRating));
	}

	private void UpdateString(ref string target, string src)
	{
		if (target == null || target != src)
		{
			target = src;
			isDirty = true;
		}
	}

	private void UpdateStringList(ref List<string> stList, int index, string src)
	{
		if (index >= stList.Count)
		{
			stList.Add(src);
			isDirty = true;
		}
		else if (stList[index] != src)
		{
			stList[index] = src;
			isDirty = true;
		}
	}

	private void UpdateInt(ref int target, int src)
	{
		if (target != src)
		{
			target = src;
			isDirty = true;
		}
	}

	private void UpdateBool(ref bool target, bool src)
	{
		if (target != src)
		{
			target = src;
			isDirty = true;
		}
	}

	public void CalculateDefenseRating()
	{
		defenseRating = 0;
		heroRating = 0;
		HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroId];
		if (heroSchema != null)
		{
			heroRating += heroSchema.defenseRating * heroLevel;
			WeaponSchema meleeWeapon = heroSchema.MeleeWeapon;
			if (meleeWeapon != null)
			{
				heroRating += meleeWeapon.defenseRating * meleeLevel;
			}
			meleeWeapon = heroSchema.RangedWeapon;
			if (meleeWeapon != null)
			{
				heroRating += meleeWeapon.defenseRating * bowLevel;
			}
			if (armorLevel > 0 && heroSchema.ArmorLevels != null)
			{
				int num = Mathf.Clamp(armorLevel - 1, 0, heroSchema.ArmorLevels.Length - 1);
				heroRating += heroSchema.ArmorLevels[num].defenseRating;
			}
		}
		abilityRating = 0;
		int num2 = 0;
		foreach (string abilityId in abilityIdList)
		{
			AbilitySchema abilitySchema = Singleton<AbilitiesDatabase>.Instance[abilityId];
			if (abilitySchema != null)
			{
				abilityRating += abilitySchema.defenseRating * abilityLevel[num2];
			}
			num2++;
		}
		helperRating = 0;
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null)
			{
				HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[selectedHelpers[i].helperId];
				if (helperSchema != null)
				{
					helperRating += helperSchema.defenseRating * selectedHelpers[i].level;
				}
			}
		}
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Gate");
		float @float = data.GetFloat(TextDBSchema.LevelKey("health", baseLevel));
		gateRating = (int)(@float / 10f);
		bellRating = bellLevel * 100;
		archerRating = archerLevel * 100;
		pitRating = pitLevel * 100;
		defenseRating = heroRating + abilityRating + helperRating + archerRating + bellRating + gateRating + pitRating;
	}

	public void UpdateFromAISchema(AIEnemySchema aiSchema)
	{
		mVersion = CollectionStatusRecord.kCollectionVersion;
		heroId = aiSchema.heroId.Key;
		heroLevel = aiSchema.heroLevel;
		bowLevel = aiSchema.bowLevel;
		armorLevel = aiSchema.armorLevel;
		meleeLevel = aiSchema.swordLevel;
		baseLevel = aiSchema.gateLevel;
		archerLevel = aiSchema.archerLevel;
		bellLevel = aiSchema.bellLevel;
		pitLevel = aiSchema.pitLevel;
		flowersCollected = aiSchema.flowerSets;
		bannersCollected = aiSchema.bannerSets;
		swordsCollected = aiSchema.swordSets;
		bowsCollected = aiSchema.bowSets;
		armorCollected = aiSchema.armorSets;
		horsesCollected = aiSchema.horseSets;
		abilityIdList.Clear();
		string[] array = new string[6]
		{
			aiSchema.helper1.Key,
			aiSchema.helper2.Key,
			aiSchema.helper3.Key,
			aiSchema.helper4.Key,
			aiSchema.helper5.Key,
			aiSchema.helper6.Key
		};
		int[] array2 = new int[6] { aiSchema.helperLevel1, aiSchema.helperLevel2, aiSchema.helperLevel3, aiSchema.helperLevel4, aiSchema.helperLevel5, aiSchema.helperLevel6 };
		string[] array3 = new string[5]
		{
			aiSchema.ability1.Key,
			aiSchema.ability2.Key,
			aiSchema.ability3.Key,
			string.Empty,
			string.Empty
		};
		int[] array4 = new int[5] { aiSchema.abilityLevel1, aiSchema.abilityLevel2, aiSchema.abilityLevel3, 0, 0 };
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (!string.IsNullOrEmpty(array[i]))
			{
				HelperSelectionInfo helperSelectionInfo = new HelperSelectionInfo();
				helperSelectionInfo.level = array2[i];
				helperSelectionInfo.helperId = array[i];
				helperSelectionInfo.golden = false;
				selectedHelpers[i] = helperSelectionInfo;
			}
		}
		for (int j = 0; j < kMaxAbilities; j++)
		{
			if (!string.IsNullOrEmpty(array3[j]))
			{
				abilityLevel[abilityIdList.Count] = array4[j];
				abilityIdList.Add(array3[j]);
			}
		}
		CalculateDefenseRating();
	}

	private void SelectHelper(ref int selectionIndex, List<HelperSelectionInfo> availableHelpers, HelperSelectionInfo selection)
	{
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] == selection)
			{
				return;
			}
		}
		selectedHelpers[selectionIndex] = selection;
		selectionIndex++;
		HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[selection.helperId];
		if (string.IsNullOrEmpty(helperSchema.GetLevel(selection.level).upgradeAlliesFrom))
		{
			return;
		}
		string key = helperSchema.GetLevel(selection.level).upgradeAlliesFrom.Key;
		foreach (HelperSelectionInfo availableHelper in availableHelpers)
		{
			if (availableHelper.helperId == key)
			{
				SelectHelper(ref selectionIndex, availableHelpers, availableHelper);
			}
		}
	}

	public void Unpack(byte[] bytes)
	{
		abilityIdList.Clear();
		if (bytes == null || bytes.Length < 8)
		{
			WaveSchema waveData = WaveManager.GetWaveData(Singleton<Profile>.Instance.waveToPlay, WaveManager.WaveType.Wave_Multiplayer);
			AIEnemySchema aIEnemySchema = null;
			if (waveData != null)
			{
				aIEnemySchema = waveData.VsModeAIOpponent.InitializeRecord<AIEnemySchema>();
			}
			if (aIEnemySchema != null)
			{
				UpdateFromAISchema(aIEnemySchema);
			}
			else
			{
				UpdateLocalProfile();
			}
		}
		else
		{
			MemoryStream input = new MemoryStream(bytes);
			BinaryReader binaryReader = new BinaryReader(input);
			mVersion = (int)binaryReader.ReadUInt64();
			if (mVersion == CollectionStatusRecord.kCollectionVersion)
			{
				heroId = binaryReader.ReadString();
				heroLevel = binaryReader.ReadByte();
				leadershipLevel = binaryReader.ReadByte();
				meleeLevel = binaryReader.ReadByte();
				bowLevel = binaryReader.ReadByte();
				armorLevel = binaryReader.ReadByte();
				baseLevel = binaryReader.ReadByte();
				archerLevel = binaryReader.ReadByte();
				bellLevel = binaryReader.ReadByte();
				pitLevel = binaryReader.ReadByte();
				flowersCollected = binaryReader.ReadByte();
				bannersCollected = binaryReader.ReadByte();
				int num = binaryReader.ReadByte();
				for (int i = 0; i < num; i++)
				{
					HelperSelectionInfo helperSelectionInfo = new HelperSelectionInfo();
					helperSelectionInfo.summonId = binaryReader.ReadByte();
					helperSelectionInfo.level = binaryReader.ReadByte();
					helperSelectionInfo.golden = binaryReader.ReadByte() != 0;
					selectedHelpers[i] = helperSelectionInfo;
				}
				for (int j = num; j < kMaxHelpers; j++)
				{
					selectedHelpers[j] = null;
				}
				HelperSchema[] allHelpers = Singleton<HelpersDatabase>.Instance.AllHelpers;
				foreach (HelperSchema helperSchema in allHelpers)
				{
					if (helperSchema == null || helperSchema.summonIndex <= 0)
					{
						continue;
					}
					HelperSelectionInfo[] array = selectedHelpers;
					foreach (HelperSelectionInfo helperSelectionInfo2 in array)
					{
						if (helperSelectionInfo2 != null && helperSelectionInfo2.summonId == helperSchema.summonIndex)
						{
							helperSelectionInfo2.helperId = helperSchema.id;
						}
					}
				}
				for (int m = 0; m < kMaxAbilities; m++)
				{
					string text = binaryReader.ReadString();
					abilityLevel[m] = binaryReader.ReadByte();
					if (!string.IsNullOrEmpty(text))
					{
						abilityIdList.Add(text);
					}
				}
				if (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length - 4)
				{
					swordsCollected = binaryReader.ReadByte();
					bowsCollected = binaryReader.ReadByte();
					armorCollected = binaryReader.ReadByte();
					horsesCollected = binaryReader.ReadByte();
				}
			}
			else
			{
				WaveSchema waveData2 = WaveManager.GetWaveData(Singleton<Profile>.Instance.waveToPlay, WaveManager.WaveType.Wave_Multiplayer);
				AIEnemySchema aIEnemySchema2 = null;
				if (waveData2 != null)
				{
					aIEnemySchema2 = waveData2.VsModeAIOpponent.InitializeRecord<AIEnemySchema>();
				}
				if (aIEnemySchema2 != null)
				{
					UpdateFromAISchema(aIEnemySchema2);
				}
				else
				{
					UpdateLocalProfile();
				}
			}
		}
		CalculateDefenseRating();
	}

	public byte[] Pack()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ulong)mVersion);
		binaryWriter.Write(heroId);
		binaryWriter.Write((byte)heroLevel);
		binaryWriter.Write((byte)leadershipLevel);
		binaryWriter.Write((byte)meleeLevel);
		binaryWriter.Write((byte)bowLevel);
		binaryWriter.Write((byte)armorLevel);
		binaryWriter.Write((byte)baseLevel);
		binaryWriter.Write((byte)archerLevel);
		binaryWriter.Write((byte)bellLevel);
		binaryWriter.Write((byte)pitLevel);
		binaryWriter.Write((byte)flowersCollected);
		binaryWriter.Write((byte)bannersCollected);
		int num = 0;
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null)
			{
				num++;
			}
		}
		binaryWriter.Write((byte)num);
		for (int j = 0; j < num; j++)
		{
			if (selectedHelpers[j] != null)
			{
				binaryWriter.Write((byte)selectedHelpers[j].summonId);
				binaryWriter.Write((byte)selectedHelpers[j].level);
				binaryWriter.Write((byte)(selectedHelpers[j].golden ? 1u : 0u));
			}
		}
		for (int k = 0; k < kMaxAbilities; k++)
		{
			if (k < abilityIdList.Count)
			{
				binaryWriter.Write(abilityIdList[k]);
			}
			else
			{
				binaryWriter.Write(string.Empty);
			}
			binaryWriter.Write((byte)abilityLevel[k]);
		}
		binaryWriter.Write((byte)swordsCollected);
		binaryWriter.Write((byte)bowsCollected);
		binaryWriter.Write((byte)armorCollected);
		binaryWriter.Write((byte)horsesCollected);
		return memoryStream.GetBuffer();
	}

	public int GetHelperLevel(string helper)
	{
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null && selectedHelpers[i].helperId != null && selectedHelpers[i].helperId == helper)
			{
				return selectedHelpers[i].level;
			}
		}
		int value = 1;
		additionalHelperLevels.TryGetValue(helper, out value);
		return 1;
	}

	public void SetHelperLevel(string helper, int level)
	{
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null && selectedHelpers[i].helperId != null && selectedHelpers[i].helperId == helper)
			{
				selectedHelpers[i].level = level;
				return;
			}
		}
		additionalHelperLevels[helper] = level;
	}

	public bool IsGoldenHelperUnlocked(string helper)
	{
		for (int i = 0; i < kMaxHelpers; i++)
		{
			if (selectedHelpers[i] != null && selectedHelpers[i].helperId != null && selectedHelpers[i].helperId == helper)
			{
				return selectedHelpers[i].golden;
			}
		}
		return false;
	}

	public string[] GetSelectedHelperIDs()
	{
		List<string> list = new List<string>();
		HelperSelectionInfo[] array = selectedHelpers;
		foreach (HelperSelectionInfo helperSelectionInfo in array)
		{
			if (helperSelectionInfo != null && !string.IsNullOrEmpty(helperSelectionInfo.helperId))
			{
				list.Add(helperSelectionInfo.helperId);
			}
		}
		return list.ToArray();
	}

	public int GetAbilityLevel(string ability)
	{
		for (int i = 0; i < Mathf.Min(kMaxAbilities, abilityIdList.Count); i++)
		{
			if (abilityIdList[i] != null && abilityIdList[i] == ability)
			{
				return abilityLevel[i];
			}
		}
		return 1;
	}

	public bool HasAbilitiy(string ability)
	{
		for (int i = 0; i < Mathf.Min(kMaxAbilities, abilityIdList.Count); i++)
		{
			if (abilityIdList[i] != null && abilityIdList[i] == ability)
			{
				return true;
			}
		}
		return false;
	}
}
