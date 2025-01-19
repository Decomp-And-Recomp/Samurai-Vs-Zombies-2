using System.Collections.Generic;
using UnityEngine;

public class EnemiesShowCase : WeakGlobalInstance<EnemiesShowCase>
{
	private class Card
	{
		public string id;

		public Enemy actor;

		public Card(string id)
		{
			this.id = id;
		}
	}

	public delegate void OnCharacterTouched(Character c);

	private const float kDistanceMinX = 1.1f;

	private const float kDistanceMaxX = 2.2f;

	private const float kAreaToSplit = 6f;

	private const float kEnemyCountTreshold = 8f;

	private const float kScaleAdjustPerExtraEnemy = 0.08f;

	private const float kNewTagZOffset = 5f;

	private const float kDefaultTouchDistance = 200f;

	private float kMaxHeightTouchDistance = (float)Screen.height / 3f;

	public OnCharacterTouched onCharacterTouched;

	private Transform mOrigin;

	private Quaternion mRotationOffset;

	private GameObject mDimmer;

	private GameObject mTagPrefab;

	private WaveManager mWaveManager;

	private bool mIsHighlight = true;

	private List<Character> mEnemies = new List<Character>();

	private float mTouchDistance;

	private List<string> mPreviouslySeenEnemies;

	private List<GameObject> mNewTags = new List<GameObject>();

	private WaveManager.WaveType mCurrentWaveType;

	private int mCurrentWaveNum = -1;

	public bool highlight
	{
		get
		{
			return mIsHighlight;
		}
		set
		{
			mIsHighlight = value;
			if (mDimmer != null)
			{
				mDimmer.SetActive(!mIsHighlight);
			}
			foreach (GameObject mNewTag in mNewTags)
			{
				mNewTag.SetActive(mIsHighlight);
			}
		}
	}

	public EnemiesShowCase(Transform centerPos, GameObject dimmer, WaveManager.WaveType waveType, int waveNum)
	{
		SetUniqueInstance(this);
		mDimmer = dimmer;
		mOrigin = centerPos;
		mRotationOffset = new Quaternion(mOrigin.localRotation.x, mOrigin.localRotation.y - 1f, mOrigin.localRotation.z, mOrigin.localRotation.w);
		mOrigin.localRotation = new Quaternion(0f, 1f, 0f, 0f);
		if (!Singleton<Profile>.Instance.inVSMultiplayerWave || Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null)
		{
			Reload(waveType, waveNum);
		}
		else
		{
			Clear();
		}
	}

	public void Update()
	{
		UpdateInput();
	}

	public void Reload(WaveManager.WaveType waveType, int waveNum)
	{
		if (mCurrentWaveType == waveType && mCurrentWaveNum == waveNum)
		{
			return;
		}
		mCurrentWaveType = waveType;
		mCurrentWaveNum = waveNum;
		Clear();
		if (mOrigin == null)
		{
			return;
		}
		mWaveManager = new WaveManager(waveType, waveNum, null, 0f);
		List<string> list;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			if (Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent == null || Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout == null)
			{
				return;
			}
			list = new List<string>();
			list.Add(Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroId);
			string text = WaveManager.SpecialBossName(Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.defensiveBuffs[0]);
			if (text != string.Empty)
			{
				Singleton<EnemiesDatabase>.Instance.LoadInGameData(text);
				list.Add(text);
			}
			string[] selectedHelperIDs = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetSelectedHelperIDs();
			string[] array = selectedHelperIDs;
			foreach (string text2 in array)
			{
				Singleton<HelpersDatabase>.Instance.LoadInGameData(text2);
				list.Add(text2);
			}
		}
		else
		{
			list = Prioritize(mWaveManager.allDifferentEnemies);
			Singleton<EnemiesDatabase>.Instance.LoadInGameData(list);
			mPreviouslySeenEnemies = Singleton<Profile>.Instance.alreadySeenEnemies;
		}
		CreateAllEnemies(list);
	}

	public void Clear()
	{
		foreach (Character mEnemy in mEnemies)
		{
			if (mEnemy.rootObject != null)
			{
				mEnemy.rootObject.transform.parent = null;
				mEnemy.Destroy();
			}
		}
		mEnemies.Clear();
		foreach (GameObject mNewTag in mNewTags)
		{
			mNewTag.transform.parent = null;
			mNewTag.SetActive(false);
			Object.DestroyImmediate(mNewTag);
		}
		mNewTags.Clear();
	}

	private void CreateAllEnemies(List<string> enemies)
	{
		int num = Mathf.Max(1, enemies.Count - 1);
		float scaling = GetScaling(enemies);
		float num2 = Mathf.Clamp(6f / (float)num, 1.1f, 2.2f) * scaling;
		Vector3 vector = new Vector3(0f - num2 * (float)num / 2f, 0f, 0f);
		bool flag = false;
		if (Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.RightToLeft)
		{
			vector.x *= -1f;
			num2 *= -1f;
			flag = true;
			mRotationOffset *= Quaternion.AngleAxis(180f, Vector3.up);
		}
		if (enemies.Count == 1)
		{
			vector.x = 0f;
		}
		string text = string.Empty;
		foreach (string enemy in enemies)
		{
			text = text + enemy + ", ";
			Character character = null;
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[enemy];
			if (heroSchema != null)
			{
				character = new Hero(mOrigin.transform, 1);
				if (!flag)
				{
					character.SetMirroredSkeleton(true);
				}
			}
			else
			{
				CharacterData characterData = new CharacterData(enemy, 1);
				character = new Enemy(characterData, 0f, vector, 1);
				characterData.Setup(character);
				if (characterData.rangedWeaponPrefab != null)
				{
					character.rangedWeaponPrefab = characterData.rangedWeaponPrefab;
				}
				if (characterData.meleeWeaponPrefab != null)
				{
					character.meleeWeaponPrefab = characterData.meleeWeaponPrefab;
				}
			}
			character.controller.ShowRoomWalk();
			ObjectUtils.SetLayerRecursively(character.rootObject, LayerMask.NameToLayer("GLUI"));
			character.rootObject.transform.parent = mOrigin.transform;
			character.rootObject.transform.localPosition = vector;
			character.rootObject.transform.localScale = new Vector3(scaling, scaling, scaling);
			character.rootObject.transform.localRotation = mRotationOffset;
			mEnemies.Add(character);
			if (mPreviouslySeenEnemies != null && !mPreviouslySeenEnemies.Contains(enemy))
			{
				AddNewEnemyTag(vector);
			}
			vector.x += num2;
		}
		if (mEnemies.Count > 2)
		{
			mTouchDistance = Mathf.Abs(ObjectUtils.GetObjectScreenPosition(mEnemies[0].rootObject).x - ObjectUtils.GetObjectScreenPosition(mEnemies[1].rootObject).x) / 2f;
		}
		else
		{
			mTouchDistance = 200f;
		}
	}

	private void AddNewEnemyTag(Vector3 pos)
	{
		pos.z += 5f;
		if (mTagPrefab == null)
		{
			mTagPrefab = Resources.Load("UI/Prefabs/SelectSuite/Widget_NewEnemyBadge") as GameObject;
		}
		GameObject gameObject = Object.Instantiate(mTagPrefab) as GameObject;
		gameObject.transform.parent = mOrigin.transform;
		gameObject.transform.localPosition = pos;
		if ((mNewTags.Count & 1) == 1)
		{
			Animation animation = gameObject.FindChildComponent<Animation>("NewBadge");
			if (!(animation != null))
			{
			}
		}
		mNewTags.Add(gameObject);
	}

	private List<string> Prioritize(List<string> source)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string item in source)
		{
			CharacterData characterData = new CharacterData(item, 1);
			if (characterData.isBoss)
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			list2.Insert(list2.Count / 2, item2);
		}
		return list2;
	}

	private float GetScaling(List<string> enemies)
	{
		int count = enemies.Count;
		float num = 1f;
		foreach (string enemy in enemies)
		{
			EnemySchema enemySchema = Singleton<EnemiesDatabase>.Instance[enemy];
			if (enemySchema != null)
			{
				float previewScale = enemySchema.previewScale;
				if (previewScale != 0f && previewScale < num)
				{
					num = previewScale;
				}
			}
		}
		float num2 = 4f * (float)Screen.height / (3f * (float)Screen.width);
		float num3 = 1f;
		if (count > (int)(8f / num2))
		{
			num3 -= 0.08f * num2 * (float)(count - (int)(8f / num2));
		}
		return Mathf.Min(num3, num);
	}

	private void UpdateInput()
	{
		if (onCharacterTouched == null || SingletonMonoBehaviour<InputManager>.Instance.Hand.fingers.Length == 0)
		{
			return;
		}
		FingerInfo fingerInfo = SingletonMonoBehaviour<InputManager>.Instance.Hand.fingers[0];
		if (!fingerInfo.IsFingerDown)
		{
			return;
		}
		Vector2 cursorPosition = fingerInfo.CursorPosition;
		int num = 0;
		foreach (Character mEnemy in mEnemies)
		{
			Vector2 objectScreenPosition = ObjectUtils.GetObjectScreenPosition(mEnemy.rootObject);
			float num2 = Mathf.Abs(cursorPosition.x - objectScreenPosition.x);
			if (num2 < mTouchDistance)
			{
				if (cursorPosition.y > objectScreenPosition.y && cursorPosition.y < objectScreenPosition.y + kMaxHeightTouchDistance)
				{
					onCharacterTouched(mEnemies[num]);
				}
				break;
			}
			num++;
		}
	}
}
