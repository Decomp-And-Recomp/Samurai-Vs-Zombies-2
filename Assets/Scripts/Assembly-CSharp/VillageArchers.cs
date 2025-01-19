using System.Collections.Generic;
using UnityEngine;

public class VillageArchers
{
	private const float kTimeFireRate = 2.5f;

	private const float kTimerTriggerDamages = 3.2f;

	private const int kNumArchers = 2;

	private string[] mArcherCharacterRecordName = new string[2];

	private string[] mArrowType = new string[2];

	private GameObject[] mRangedWeaponPrefab = new GameObject[2];

	private float[] mRange = new float[2];

	private float[] mDamage = new float[2];

	private float[] mAttackSpeed = new float[2];

	private List<TowerArcher> mArcherList = new List<TowerArcher>();

	private int mArcherLevel;

	private bool mAgainstPlayer;

	public VillageArchers()
	{
		mArcherLevel = Singleton<Profile>.Instance.archerLevel;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			mArcherLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.archerLevel;
		}
		if (mArcherLevel == 0)
		{
			return;
		}
		GetVillageArcherStats();
		for (int i = 0; i < 2; i++)
		{
			if (!string.IsNullOrEmpty(mArcherCharacterRecordName[i]))
			{
				mArcherList.Add(new TowerArcher(mArcherCharacterRecordName[i], WeakGlobalMonoBehavior<InGameImpl>.Instance.villageArcher[i].position, mRangedWeaponPrefab[i], mArrowType[i], mDamage[i], mRange[i], mAttackSpeed[i], mAgainstPlayer));
			}
		}
	}

	public void Update()
	{
		if (mArcherLevel == 0)
		{
			return;
		}
		foreach (TowerArcher mArcher in mArcherList)
		{
			mArcher.Update();
		}
	}

	public void UnloadData()
	{
		if (mArcherList == null)
		{
			return;
		}
		foreach (TowerArcher mArcher in mArcherList)
		{
			mArcher.UnloadData();
		}
	}

	private void GetVillageArcherStats()
	{
		DataBundleRecordHandle<VillageArcherSchema> dataBundleRecordHandle = new DataBundleRecordHandle<VillageArcherSchema>("VillageArchers", mArcherLevel.ToString());
		dataBundleRecordHandle.Load(null);
		VillageArcherSchema data = dataBundleRecordHandle.Data;
		mRange[0] = data.bowRange_1;
		mRange[1] = data.bowRange_2;
		mDamage[0] = data.bowDamage_1;
		mDamage[1] = data.bowDamage_2;
		mAttackSpeed[0] = data.attackFrequency_1;
		mAttackSpeed[1] = data.attackFrequency_2;
		mArcherCharacterRecordName[0] = data.character_1;
		mArcherCharacterRecordName[1] = data.character_2;
		mRangedWeaponPrefab[0] = data.rangedWeaponPrefab_1;
		mRangedWeaponPrefab[1] = data.rangedWeaponPrefab_2;
		mArrowType[0] = DataBundleRuntime.RecordKey(data.projectile_1);
		mArrowType[1] = DataBundleRuntime.RecordKey(data.projectile_2);
		mAgainstPlayer = !string.IsNullOrEmpty(Singleton<Profile>.Instance.playModeSubSection);
	}
}
