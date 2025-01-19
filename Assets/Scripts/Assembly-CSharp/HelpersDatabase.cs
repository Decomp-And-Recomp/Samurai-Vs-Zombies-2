using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpersDatabase : Singleton<HelpersDatabase>
{
	private class CharacterObjectRedirect
	{
		public GameObject[] gObj = new GameObject[2];

		public DataBundleRecordHandle<CharacterSchema>[] recordHandles = new DataBundleRecordHandle<CharacterSchema>[2];
	}

	private delegate bool ShouldSwap(int ownerID);

	private const string goldenHelperPrefix = "GoldenHelper.";

	private DataBundleTableHandle<HelperSchema> mData;

	private string[] mAllIDs;

	private Dictionary<string, CharacterObjectRedirect> characterObjects = new Dictionary<string, CharacterObjectRedirect>();

	private DataBundleTableHandle<HelperSwapSchema> mMountedSwapData;

	public static string UdamanTableName
	{
		get
		{
			return "Helpers";
		}
	}

	public string[] allIDs
	{
		get
		{
			return mAllIDs;
		}
	}

	public HelperSchema[] AllHelpers
	{
		get
		{
			return mData.Data;
		}
	}

	public HelperSchema this[string id]
	{
		get
		{
			return Seek(id);
		}
	}

	public HelpersDatabase()
	{
		ResetCachedData();
	}

	public static string GoldenHelperID(string helperID)
	{
		return "GoldenHelper." + helperID;
	}

	public static bool IsGoldenHelperID(string id)
	{
		return id.StartsWith("GoldenHelper.");
	}

	public static string HelperIDFromGoldenHelperID(string goldenHelperID)
	{
		if (IsGoldenHelperID(goldenHelperID))
		{
			return goldenHelperID.Substring("GoldenHelper.".Length);
		}
		return goldenHelperID;
	}

	public void ResetCachedData()
	{
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			mData = new DataBundleTableHandle<HelperSchema>(UdamanTableName);
			HelperSchema[] data = mData.Data;
			foreach (HelperSchema helperSchema in data)
			{
				helperSchema.Initialize(UdamanTableName);
			}
			CacheSimpleIDList();
		}
	}

	public bool Contains(string id)
	{
		string[] array = mAllIDs;
		foreach (string strA in array)
		{
			if (string.Compare(strA, id, true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMaxLevel(string helperID)
	{
		HelperSchema helperSchema = Seek(helperID);
		if (helperSchema != null)
		{
			if (helperSchema.Levels == null)
			{
				return 1;
			}
			return helperSchema.Levels.Length;
		}
		return -1;
	}

	public HelperLevelSchema GetHelperLevelData(HelperSchema data)
	{
		if (data == null)
		{
			return null;
		}
		string key = data.levelMatchOtherHelper.Key;
		int num = 0;
		num = ((!(key != string.Empty)) ? Singleton<Profile>.Instance.GetHelperLevel(data.id) : Singleton<Profile>.Instance.GetHelperLevel(key));
		if (num == 0)
		{
			if (data.Levels != null && data.Levels.Length > 0)
			{
				return data.Levels[0];
			}
			return null;
		}
		return data.Levels[num - 1];
	}

	public int EnsureProperInitialHelperLevel(string helperID)
	{
		int num = Singleton<Profile>.Instance.GetHelperLevel(helperID);
		if (num == 0)
		{
			num++;
			Singleton<Profile>.Instance.SetHelperLevel(helperID, num);
			Singleton<Profile>.Instance.Save();
		}
		return num;
	}

	public void LoadFrontEndData()
	{
		mData.Load(DataBundleResourceGroup.FrontEnd, false, null);
	}

	private DataBundleRecordHandle<CharacterSchema> BuildCharacter(string newCharacterName, out GameObject newCharacterObject, string baseCharacterName)
	{
		return BuildCharacter(new DataBundleRecordKey(newCharacterName), out newCharacterObject, baseCharacterName);
	}

	private DataBundleRecordHandle<CharacterSchema> BuildCharacter(DataBundleRecordKey newCharacterKey, out GameObject newCharacterObject, string baseCharacterName)
	{
		DataBundleRecordHandle<CharacterSchema> dataBundleRecordHandle = new DataBundleRecordHandle<CharacterSchema>(newCharacterKey);
		if (dataBundleRecordHandle.Data != null)
		{
			dataBundleRecordHandle.Load(DataBundleResourceGroup.InGame, true, null);
			dataBundleRecordHandle.Data.Initialize("Character");
			newCharacterObject = CharacterSchema.Deserialize(dataBundleRecordHandle.Data);
			AddExtraAnims(baseCharacterName, newCharacterObject);
			newCharacterObject.SetActive(false);
		}
		else
		{
			dataBundleRecordHandle = null;
			newCharacterObject = null;
		}
		return dataBundleRecordHandle;
	}

	private void AddCharacterObjects(string id, string upgradedFrom)
	{
		CharacterObjectRedirect value = null;
		if (characterObjects.TryGetValue(id, out value))
		{
			return;
		}
		value = new CharacterObjectRedirect();
		characterObjects[id] = value;
		DataBundleRecordHandle<CharacterSchema> dataBundleRecordHandle = null;
		GameObject newCharacterObject = null;
		HelperSchema helperSchema = Seek(id);
		string text = (string.IsNullOrEmpty(upgradedFrom) ? id : upgradedFrom);
		HelperSchema helperSchema2 = this[text];
		bool flag = Singleton<Profile>.Instance.GetHelperLevel(text) > Helper.kPlatinumLevel;
		bool flag2 = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetHelperLevel(text) > Helper.kPlatinumLevel;
		bool flag3 = helperSchema2.isMount || helperSchema2.isMounted;
		bool flag4 = flag3 && Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Horse") > 0;
		bool flag5 = flag3 && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.horsesCollected > 0;
		GameObject newCharacterObject2 = null;
		string newCharacterName = string.Concat(helperSchema.resources, (!flag) ? string.Empty : "_Platinum", (!flag4) ? string.Empty : "_Nightmare");
		DataBundleRecordHandle<CharacterSchema> dataBundleRecordHandle2 = BuildCharacter(newCharacterName, out newCharacterObject2, helperSchema.resources.Key);
		if (newCharacterObject2 != null)
		{
			value.gObj[0] = newCharacterObject2;
			value.recordHandles[0] = dataBundleRecordHandle2;
		}
		else
		{
			dataBundleRecordHandle = BuildCharacter(helperSchema.resources, out newCharacterObject, helperSchema.resources.Key);
			value.gObj[0] = newCharacterObject;
			value.recordHandles[0] = dataBundleRecordHandle;
		}
		bool flag6 = false;
		if (Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null)
		{
			HelperSelectionInfo[] selectedHelpers = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.selectedHelpers;
			foreach (HelperSelectionInfo helperSelectionInfo in selectedHelpers)
			{
				if (helperSelectionInfo != null && string.Equals(helperSelectionInfo.helperId, id))
				{
					flag6 = true;
					break;
				}
			}
			if (!flag6)
			{
				string upgradeHelper = GetUpgradeHelper(this[id], new List<string>(Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetSelectedHelperIDs()));
				flag6 = !string.IsNullOrEmpty(upgradeHelper);
			}
		}
		if (!flag6)
		{
			return;
		}
		string text2 = string.Concat(helperSchema.resources, (!flag2) ? string.Empty : "_Platinum", (!flag5) ? string.Empty : "_Nightmare");
		if (dataBundleRecordHandle2 == null || text2 != dataBundleRecordHandle2.TableRecordKey)
		{
			dataBundleRecordHandle2 = BuildCharacter(text2, out newCharacterObject2, helperSchema.resources.Key);
		}
		if (newCharacterObject2 != null)
		{
			value.gObj[1] = newCharacterObject2;
			value.recordHandles[1] = dataBundleRecordHandle2;
			return;
		}
		if (newCharacterObject == null)
		{
			dataBundleRecordHandle = BuildCharacter(helperSchema.resources, out newCharacterObject, helperSchema.resources.Key);
		}
		value.gObj[1] = newCharacterObject;
		value.recordHandles[1] = dataBundleRecordHandle;
	}

	public void LoadInGameData(string id)
	{
		DataBundleResourceGroup groupToLoad = ((!WeakGlobalMonoBehavior<InGameImpl>.Exists) ? DataBundleResourceGroup.Preview : DataBundleResourceGroup.InGame);
		mData.LoadWithFilter(groupToLoad, true, null, (HelperSchema s) => id.Equals(s.id));
		if (!characterObjects.ContainsKey(id))
		{
			AddCharacterObjects(id, null);
		}
	}

	private string GetUpgradeHelper(HelperSchema hSchema, List<string> selectedHelpers)
	{
		foreach (string selectedHelper in selectedHelpers)
		{
			HelperSchema helperSchema = Seek(selectedHelper);
			if (helperSchema == null)
			{
				continue;
			}
			HelperLevelSchema curLevel = helperSchema.CurLevel;
			if (curLevel != null)
			{
				DataBundleRecordKey upgradeAlliesFrom = curLevel.upgradeAlliesFrom;
				if (upgradeAlliesFrom != null && !string.IsNullOrEmpty(upgradeAlliesFrom.Key) && selectedHelpers.Contains(upgradeAlliesFrom.Key) && hSchema.id == helperSchema.CurLevel.upgradeAlliesTo.Key)
				{
					return upgradeAlliesFrom.Key;
				}
			}
		}
		return string.Empty;
	}

	private void AddExtraAnims(string id, GameObject character)
	{
		if (!Singleton<Profile>.Instance.inMultiplayerWave)
		{
			return;
		}
		GameObject gameObject = Resources.Load("Characters/" + id + "/" + id + "@stun") as GameObject;
		if (!(gameObject != null))
		{
			return;
		}
		Animation component = gameObject.GetComponent<Animation>();
		IEnumerator enumerator = component.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AnimationState animationState = (AnimationState)enumerator.Current;
			AnimationClip clip = animationState.clip;
			if (!(clip.name == "stun"))
			{
				continue;
			}
			Animation[] componentsInChildren = character.GetComponentsInChildren<Animation>();
			Animation[] array = componentsInChildren;
			foreach (Animation animation in array)
			{
				if (animation.GetClip("stun") == null)
				{
					animation.AddClip(clip, "stun");
				}
			}
		}
	}

	public void LoadInGameData(List<string> ids, List<string> adhocHelpers)
	{
		if (Singleton<Profile>.Instance.GetSelectedAbilities().Contains("ExplosiveCart") || (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.HasAbilitiy("ExplosiveCart")))
		{
			ids.Add("CartOfDoom");
		}
		if (Singleton<Profile>.Instance.GetSelectedAbilities().Contains("DragonDamage") || (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.HasAbilitiy("DragonDamage")))
		{
			ids.Add("DragonDamageHelper");
		}
		mData.LoadWithFilter(DataBundleResourceGroup.InGame, true, null, delegate(HelperSchema s)
		{
			if (ids.Contains(s.id))
			{
				AddCharacterObjects(s.id, null);
				return true;
			}
			string upgradeHelper = GetUpgradeHelper(s, ids);
			if (!string.IsNullOrEmpty(upgradeHelper))
			{
				adhocHelpers.Add(s.id);
				AddCharacterObjects(s.id, upgradeHelper);
				return true;
			}
			return false;
		});
	}

	public void UnloadData()
	{
		foreach (KeyValuePair<string, CharacterObjectRedirect> characterObject in characterObjects)
		{
			CharacterObjectRedirect value = characterObject.Value;
			if (value.recordHandles[0] != null && value.recordHandles[0].Data != null)
			{
				if (value.recordHandles[0].Data.soundTheme != null)
				{
					SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.UnloadSoundTheme(value.recordHandles[0].Data.soundTheme);
				}
				value.recordHandles[0].Unload();
			}
			if (value.recordHandles[1] != null && value.recordHandles[1].Data != null)
			{
				if (value.recordHandles[1].Data.soundTheme != null)
				{
					SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.UnloadSoundTheme(value.recordHandles[1].Data.soundTheme);
				}
				if (value.recordHandles[1] != value.recordHandles[0])
				{
					value.recordHandles[1].Unload();
				}
			}
		}
		characterObjects.Clear();
		mData.Unload();
	}

	public GameObject GetCharacterObject(string id, int ownerID)
	{
		CharacterObjectRedirect value = null;
		characterObjects.TryGetValue(id, out value);
		if (value != null)
		{
			return value.gObj[ownerID];
		}
		return null;
	}

	public string ModelName(string id, int ownerID)
	{
		GameObject characterObject = GetCharacterObject(id, ownerID);
		if (characterObject != null)
		{
			return characterObject.transform.GetChild(0).name;
		}
		return null;
	}

	public string GetRandomAvailableGoldenHelper(bool includeUnlocked = false)
	{
		List<HelperSchema> list = new List<HelperSchema>();
		int num = 0;
		HelperSchema[] data = mData.Data;
		foreach (HelperSchema helperSchema in data)
		{
			if ((includeUnlocked || !Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helperSchema.id)) && !helperSchema.Locked && helperSchema.goldenHelperProbability > 0)
			{
				list.Add(helperSchema);
				num += helperSchema.goldenHelperProbability;
			}
		}
		if (list.Count > 0 && num > 0)
		{
			int num2 = UnityEngine.Random.Range(1, num);
			for (int j = 0; j < list.Count; j++)
			{
				num2 -= list[j].goldenHelperProbability;
				if (num2 <= 0)
				{
					return GoldenHelperID(list[j].id);
				}
			}
		}
		return null;
	}

	public int GetTotalGoldenHelperUnlocks()
	{
		int num = 0;
		HelperSchema[] data = mData.Data;
		foreach (HelperSchema helperSchema in data)
		{
			if (Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helperSchema.id))
			{
				num++;
			}
		}
		return num;
	}

	private void CacheSimpleIDList()
	{
		mAllIDs = new string[mData.Data.Length];
		int num = 0;
		HelperSchema[] data = mData.Data;
		foreach (HelperSchema helperSchema in data)
		{
			mAllIDs[num++] = helperSchema.id;
		}
	}

	private HelperSchema Seek(string helperID)
	{
		HelperSchema[] data = mData.Data;
		foreach (HelperSchema helperSchema in data)
		{
			if (helperSchema.id.Equals(helperID, StringComparison.OrdinalIgnoreCase))
			{
				return helperSchema;
			}
		}
		return null;
	}

	public int SeekIndex(string helperID)
	{
		int num = 0;
		HelperSchema[] data = mData.Data;
		foreach (HelperSchema helperSchema in data)
		{
			if (helperSchema.id.Equals(helperID, StringComparison.OrdinalIgnoreCase))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public DataBundleTableHandle<HelperSwapSchema> GetMountSwapData()
	{
		if (mMountedSwapData == null)
		{
			mMountedSwapData = new DataBundleTableHandle<HelperSwapSchema>("HorseCollectionSwaps");
		}
		return mMountedSwapData;
	}
}
