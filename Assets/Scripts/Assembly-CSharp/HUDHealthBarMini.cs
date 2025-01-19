using System.Collections.Generic;
using UnityEngine;

public class HUDHealthBarMini
{
	private static GameObjectPool sObjectPool = new GameObjectPool();

	private Camera mOwnerCamera;

	private Camera mHealthBarCamera;

	private Transform mHealthBarObjXForm;

	private Transform mObservedCharXForm;

	private Vector3 mJointOffset;

	private List<Renderer> mHealthBarRenderers = new List<Renderer>();

	private GluiMeter mMeter;

	private Character mObservedChar;

	private GameObject mObject;

	private GameObject mGateIconObject;

	private static Vector3 kScreenOffset = new Vector3(0f, 0f, 0f);

	private static Vector3 kWorldOffset = new Vector3(0f, -0.5f, 0f);

	private static Vector3 kWorldOffsetBase = new Vector3(0f, 2f, 0f);

	private bool mIsBase;

	private static Vector3 screenPush = new Vector3(0f, 0f, 250f);

	public GameObject gameObject
	{
		get
		{
			return mObject;
		}
		private set
		{
		}
	}

	public HUDHealthBarMini(Character charToObserve)
	{
		mObservedChar = charToObserve;
		mObservedCharXForm = charToObserve.controller.transform;
		mIsBase = charToObserve.isBase;
		GameObject objPrefab = ((!mIsBase) ? ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/LifeMeter_Standard.prefab", 1) : ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/LifeMeter_Special.prefab", 1)).Resource as GameObject;
		mObject = sObjectPool.Acquire(objPrefab);
		mHealthBarObjXForm = mObject.transform;
		mHealthBarObjXForm.position = mObservedChar.position;
		mHealthBarObjXForm.localScale = Vector3.one * ((!mIsBase) ? 0.5f : 1f);
		mJointOffset = mObservedChar.controller.GetRelativeJointPosition("buff_icon");
		if (mJointOffset == Vector3.zero && !mObservedChar.isBase)
		{
			mJointOffset = new Vector3(0f, 2.1f, 0f);
		}
		mMeter = mObject.FindChildComponent<GluiMeter>("Meter_Life");
		mHealthBarRenderers = new List<Renderer>(mObject.GetComponentsInChildren<Renderer>());
		if (charToObserve.ownerId != 0)
		{
			mMeter.Color = Color.red;
		}
		else
		{
			mMeter.Color = Color.green;
		}
		mOwnerCamera = ObjectUtils.FindFirstCamera(charToObserve.controller.gameObject.layer);
		mHealthBarCamera = ObjectUtils.FindFirstCamera(mObject.layer);
		Update();
	}

	public virtual void Destroy()
	{
		sObjectPool.Release(mObject);
		gameObject = null;
	}

	public void Update()
	{
		if (mObservedChar == null || !(mObject != null))
		{
			return;
		}
		float mountedHealth = mObservedChar.mountedHealth;
		float value = ((!(mountedHealth > 0f)) ? (mObservedChar.health / mObservedChar.maxHealth) : (mountedHealth / mObservedChar.mountedHealthMax));
		mMeter.Value = value;
		if (mMeter.Value >= 1f || mMeter.Value <= 0f || (mIsBase && mObservedChar.timeSinceDamaged > 5f))
		{
			foreach (Renderer mHealthBarRenderer in mHealthBarRenderers)
			{
				mHealthBarRenderer.enabled = false;
			}
			return;
		}
		foreach (Renderer mHealthBarRenderer2 in mHealthBarRenderers)
		{
			mHealthBarRenderer2.enabled = true;
		}
		Vector3 position = mObservedCharXForm.position;
		if (mIsBase)
		{
			Hero hero = WeakGlobalMonoBehavior<InGameImpl>.Instance.hero;
			if (hero != null)
			{
				Vector3 position2 = WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.transform.position;
				position += kWorldOffsetBase;
				float num = position2.z - 3.3f;
				if (position.z < num)
				{
					position.z = num;
				}
			}
		}
		else
		{
			position += mJointOffset + kWorldOffset;
		}
		mHealthBarObjXForm.position = mOwnerCamera.WorldToScreenPoint(position) + kScreenOffset;
		mHealthBarObjXForm.position = mHealthBarCamera.ScreenToWorldPoint(mHealthBarObjXForm.position);
		mHealthBarObjXForm.position += screenPush;
		if (mountedHealth > 0f)
		{
			mMeter.Color = Color.cyan;
		}
		else if (mObservedChar.ownerId != 0)
		{
			mMeter.Color = Color.red;
		}
		else
		{
			mMeter.Color = Color.green;
		}
	}

	public bool OnUIEvent(string eventID)
	{
		return false;
	}
}
