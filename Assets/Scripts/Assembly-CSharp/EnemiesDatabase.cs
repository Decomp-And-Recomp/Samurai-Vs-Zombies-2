using System.Collections.Generic;
using UnityEngine;

public class EnemiesDatabase : Singleton<EnemiesDatabase>
{
	private Dictionary<string, DataBundleRecordHandle<EnemySchema>> mData = new Dictionary<string, DataBundleRecordHandle<EnemySchema>>();

	private string[] mAllIDs;

	private Dictionary<string, GameObject> characterObjects = new Dictionary<string, GameObject>();

	public string[] allIDs
	{
		get
		{
			return mAllIDs;
		}
	}

	public EnemySchema this[string id]
	{
		get
		{
			DataBundleRecordHandle<EnemySchema> value;
			if (mData.TryGetValue(id, out value))
			{
				return value.Data;
			}
			return null;
		}
	}

	public EnemiesDatabase()
	{
		ResetCachedData();
		CacheSimpleIDList();
	}

	public void ResetCachedData()
	{
		mData.Clear();
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return;
		}
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys(typeof(EnemySchema), "Enemies"))
		{
			DataBundleRecordHandle<EnemySchema> dataBundleRecordHandle = new DataBundleRecordHandle<EnemySchema>("Enemies", item);
			mData[dataBundleRecordHandle.Data.id] = dataBundleRecordHandle;
		}
		CacheSimpleIDList();
	}

	public void LoadFrontEndData()
	{
		foreach (DataBundleRecordHandle<EnemySchema> value in mData.Values)
		{
			value.Load(DataBundleResourceGroup.FrontEnd, false, null);
		}
	}

	public void LoadInGameData(string id)
	{
		DataBundleResourceGroup dataBundleResourceGroup = ((!WeakGlobalMonoBehavior<InGameImpl>.Exists) ? DataBundleResourceGroup.Preview : DataBundleResourceGroup.InGame);
		if ((mData[id].LoadedGroup & dataBundleResourceGroup) == 0)
		{
			mData[id].Load(dataBundleResourceGroup, true, null);
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists && !characterObjects.ContainsKey(id))
		{
			GameObject gameObject = CharacterSchema.Deserialize(mData[id].Data.resources);
			gameObject.SetActive(false);
			characterObjects[id] = gameObject;
		}
	}

	public void LoadInGameData(List<string> ids)
	{
		DataBundleResourceGroup groupToLoad = ((!WeakGlobalMonoBehavior<InGameImpl>.Exists) ? DataBundleResourceGroup.Preview : DataBundleResourceGroup.InGame);
		foreach (DataBundleRecordHandle<EnemySchema> value in mData.Values)
		{
			if (ids.Contains(value.Data.id))
			{
				value.Load(groupToLoad, true, null);
			}
			else
			{
				value.UnloadExcept(DataBundleResourceGroup.FrontEnd);
			}
		}
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return;
		}
		foreach (string id in ids)
		{
			GameObject gameObject = CharacterSchema.Deserialize(mData[id].Data.resources);
			gameObject.SetActive(false);
			characterObjects[id] = gameObject;
		}
	}

	public void UnloadData()
	{
		foreach (DataBundleRecordHandle<EnemySchema> value in mData.Values)
		{
			CharacterSchema characterSchema = CharacterSchema.Initialize(value.Data.resources);
			if (characterSchema != null)
			{
				SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.UnloadSoundTheme(characterSchema.soundTheme);
			}
			value.Unload();
		}
		characterObjects.Clear();
	}

	public bool Contains(string id)
	{
		return mData.ContainsKey(id);
	}

	public GameObject GetCharacterObject(string id)
	{
		GameObject value = null;
		characterObjects.TryGetValue(id, out value);
		return value;
	}

	public string ModelName(string id)
	{
		GameObject characterObject = GetCharacterObject(id);
		if (characterObject != null)
		{
			return characterObject.transform.GetChild(0).name;
		}
		return null;
	}

	private void CacheSimpleIDList()
	{
		mAllIDs = new string[mData.Count];
		mData.Keys.CopyTo(mAllIDs, 0);
	}
}
