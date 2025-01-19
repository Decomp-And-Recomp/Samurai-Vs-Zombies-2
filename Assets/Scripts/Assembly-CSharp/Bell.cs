using System.Collections.Generic;
using UnityEngine;

public class Bell
{
	private DataBundleRecordHandle<CharacterSchema> resourceHandle;

	private GameObject mBellRinger;

	private CharacterModelController mBellRingerController;

	private GameObject mBell;

	private int mBellLevel;

	private GameRange mRange;

	private float mDamage;

	private float mAttackFrequency = 5f;

	private string mBellResourcePath;

	private float mTimerSinceBeginSwing;

	private bool mAgainstPlayer;

	private int OwnerId
	{
		get
		{
			return mAgainstPlayer ? 1 : 0;
		}
	}

	public Bell(GameObject bellRingerTransform, GameObject bellLocationObject, int owner)
	{
		if (owner == 0)
		{
			mBellLevel = Singleton<Profile>.Instance.bellLevel;
		}
		else
		{
			mBellLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bellLevel;
		}
		if (owner != 0)
		{
			mAgainstPlayer = true;
		}
		if (bellRingerTransform == null)
		{
			mBellLevel = 0;
		}
		if (mBellLevel == 0)
		{
			if (mBellRinger != null)
			{
				mBellRinger.SetActive(false);
			}
			return;
		}
		Transform transform = bellLocationObject.transform;
		Vector3 position = transform.position;
		GetBellStats(position);
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(mBellResourcePath, 1);
		GameObject objPrefab = cachedResource.Resource as GameObject;
		mBell = GameObjectPool.DefaultObjectPool.Acquire(objPrefab, position, transform.rotation);
		mBell.transform.parent = transform;
		mBell.transform.localPosition = Vector3.zero;
		InitializeModel("Bellringer");
		mBellRinger.transform.parent = bellRingerTransform.transform;
		mBellRinger.transform.localPosition = Vector3.zero;
		mBellRingerController = mBellRinger.GetComponent<CharacterModelController>();
		if (mBellRingerController == null)
		{
			mBellRingerController = mBellRinger.AddComponent<CharacterModelController>();
		}
		mBellRingerController.Idle();
	}

	private void InitializeModel(string characterRecordName)
	{
		resourceHandle = new DataBundleRecordHandle<CharacterSchema>("Character", characterRecordName);
		resourceHandle.Load(DataBundleResourceGroup.InGame, true, delegate(CharacterSchema schema)
		{
			schema.Initialize("Character");
			mBellRinger = CharacterSchema.Deserialize(schema);
		});
	}

	public void Update()
	{
		if (mBellRinger == null || mBellLevel == 0)
		{
			return;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			if (WeakGlobalMonoBehavior<InGameImpl>.Instance.playerWon != Singleton<PlayModesManager>.Instance.Attacking)
			{
				mBellRingerController.PlayVictoryAnim();
				return;
			}
			if (Singleton<Profile>.Instance.inVSMultiplayerWave)
			{
				mBellRingerController.Die();
				return;
			}
		}
		mTimerSinceBeginSwing += Time.deltaTime;
		if (!(mTimerSinceBeginSwing < mAttackFrequency) && mBellRingerController.isEffectivelyIdle && WeakGlobalInstance<CharactersManager>.Instance.IsCharacterInRange(mRange, 1 - OwnerId, false, true, false, true, false))
		{
			PlayRingerSwing();
		}
	}

	public void UnloadData()
	{
		if (resourceHandle != null)
		{
			resourceHandle.Unload();
		}
	}

	private void GetBellStats(Vector3 bellLoc)
	{
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Bell");
		float @float = data.GetFloat(TextDBSchema.LevelKey("range", mBellLevel));
		mDamage = data.GetFloat(TextDBSchema.LevelKey("damage", mBellLevel));
		mAttackFrequency = data.GetFloat("attackFrequency");
		float z = bellLoc.z;
		mRange = new GameRange(z - @float, z + @float);
		string key = TextDBSchema.LevelKey("Prefab", mBellLevel);
		mBellResourcePath = data.GetString(key);
	}

	private void PlayRingerSwing()
	{
		mBellRingerController.PerformSpecialAction("ring", OnBellDamage, mAttackFrequency);
		mBell.GetComponent<Animation>().Play("ring");
		mTimerSinceBeginSwing = 0f;
	}

	private void OnBellDamage()
	{
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(mRange, 1 - OwnerId);
		foreach (Character item in charactersInRange)
		{
			item.controller.stunnedTimer = 2f;
			item.RecievedAttack(EAttackType.Sonic, mDamage, WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(OwnerId));
		}
	}
}
