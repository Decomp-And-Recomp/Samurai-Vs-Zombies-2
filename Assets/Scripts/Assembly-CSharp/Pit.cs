using System.Collections;
using UnityEngine;

public class Pit
{
	public const string kPitTableName = "Pit";

	private const float kPostFallTime = 2f;

	private float LuckBender;

	private float mLifetime;

	private int mLevel;

	private GameRange mRange;

	private float mChanceToEnact;

	private BoxCollider mPitArea;

	private GameObject ambientPrefab;

	private GameObject dynamicPrefab;

	private bool mAgainstPlayer;

	private GameObject AmbientEffect { get; set; }

	public int Level
	{
		get
		{
			return mLevel;
		}
		set
		{
		}
	}

	public float ChanceToEnact
	{
		get
		{
			return mChanceToEnact;
		}
		set
		{
		}
	}

	public BoxCollider Area
	{
		get
		{
			return mPitArea;
		}
		set
		{
		}
	}

	public GameRange Range
	{
		get
		{
			return mRange;
		}
		set
		{
		}
	}

	private int OwnerId
	{
		get
		{
			return mAgainstPlayer ? 1 : 0;
		}
	}

	public Pit(BoxCollider pitArea, int owner)
	{
		if (owner == 0)
		{
			mLevel = Singleton<Profile>.Instance.pitLevel;
		}
		else
		{
			mLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.pitLevel;
		}
		if (owner != 0)
		{
			mAgainstPlayer = true;
		}
		if (mLevel > 0)
		{
			GetStats(pitArea);
			AmbientEffect = GameObjectPool.DefaultObjectPool.Acquire(ambientPrefab, pitArea.transform.position, Quaternion.identity);
		}
	}

	public bool TryTaking(Character target)
	{
		float z = target.transform.position.z;
		if (mPitArea != null && Contains(z))
		{
			float value = Random.value;
			if (value <= ChanceToEnact * (1f + LuckBender))
			{
				LuckBender = 0f;
				return true;
			}
			LuckBender += 0.1f;
		}
		return false;
	}

	public void CatchTarget(Character target)
	{
		target.enemyIgnoresMe = true;
		WeakGlobalMonoBehavior<InGameImpl>.Instance.StartCoroutine(CheckCatchTarget(target));
	}

	private void RotateTargetUsing(Character target, Vector3 rotVelocity)
	{
		target.controlledObject.transform.RotateAroundLocal(Vector3.right, rotVelocity.x * Time.deltaTime);
		target.controlledObject.transform.RotateAroundLocal(Vector3.up, rotVelocity.y * Time.deltaTime);
	}

	private IEnumerator CheckCatchTarget(Character target)
	{
		float countDown = target.controller.KnockbackTime;
		GameObject dynamicObj = null;
		target.controller.stunnedTimer = countDown;
		Vector3 rotVelocity = new Vector3(Random.Range(0f, 4f), Random.Range(-5f, 5f), 0f);
		target.controller.isPitThrown = true;
		target.controller.SetBaseAnim("stun");
		target.controller.Idle();
		do
		{
			yield return new WaitForSeconds(0f);
			RotateTargetUsing(target, rotVelocity);
			countDown -= Time.deltaTime;
			if (dynamicObj != null)
			{
				dynamicObj.transform.rotation = Quaternion.identity;
				continue;
			}
			dynamicObj = GameObjectPool.DefaultObjectPool.Acquire(dynamicPrefab);
			dynamicObj.transform.parent = target.transform;
			dynamicObj.transform.localPosition = Vector3.zero;
		}
		while (!(countDown <= 0f));
		yield return WeakGlobalMonoBehavior<InGameImpl>.Instance.StartCoroutine(MoveTargetDown(target, dynamicObj, rotVelocity));
	}

	private IEnumerator MoveTargetDown(Character target, GameObject dynamicObj, Vector3 rotVelocity)
	{
		float countDown = 2f;
		target.controller.stunnedTimer = countDown;
		while (true)
		{
			RotateTargetUsing(target, rotVelocity);
			target.transform.Translate(target.controller.KnockbackVelocity * Time.deltaTime, Space.World);
			if (dynamicObj != null)
			{
				dynamicObj.transform.rotation = Quaternion.identity;
			}
			countDown -= Time.deltaTime;
			if (countDown <= 0f)
			{
				break;
			}
			yield return new WaitForSeconds(0f);
		}
		if (target.controller != null)
		{
			target.health = 0f;
			target.controlledObject.transform.parent = null;
		}
		GameObjectPool.DefaultObjectPool.Release(dynamicObj);
	}

	public void Update()
	{
		if (!(mLifetime >= 0f))
		{
			return;
		}
		mLifetime += Time.deltaTime;
		if (!(mLifetime >= 1f))
		{
			return;
		}
		if (AmbientEffect != null)
		{
			ParticleEmitter[] componentsInChildren = AmbientEffect.GetComponentsInChildren<ParticleEmitter>();
			foreach (ParticleEmitter particleEmitter in componentsInChildren)
			{
				particleEmitter.emit = true;
			}
			ParticleSystem[] componentsInChildren2 = AmbientEffect.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren2)
			{
				particleSystem.Play();
			}
		}
		mLifetime = -1f;
	}

	public bool Contains(float zLoc)
	{
		return mRange.Contains(zLoc);
	}

	private void GetStats(BoxCollider pitArea)
	{
		mPitArea = pitArea;
		float z = pitArea.transform.position.z;
		float num = pitArea.bounds.size.z * 0.5f;
		mRange = new GameRange(z - num, z + num);
		DataBundleRecordHandle<PitSchema> dataBundleRecordHandle = new DataBundleRecordHandle<PitSchema>("Pit", mLevel.ToString());
		dataBundleRecordHandle.Load(DataBundleResourceGroup.InGame, true, null);
		dynamicPrefab = dataBundleRecordHandle.Data.dynamicPrefab;
		ambientPrefab = dataBundleRecordHandle.Data.ambientPrefab;
		mChanceToEnact = dataBundleRecordHandle.Data.chanceToEnact * 0.01f;
	}
}
