using System;
using UnityEngine;

public class ModelMelter : MonoBehaviour
{
	public bool ascendToTheHeavens;

	private Action mDoneEvent;

	private float mTimeLeft;

	private float mShaderTimeLeft;

	private GameObject mGameObject;

	private GameObject mMelterGameObjectParent;

	private GameObject mMelterGameObjectChild;

	private GameObject mDieEffectObject;

	private static GameObjectPool sObjectPool = new GameObjectPool();

	private static void InitializeNewObjectMelt()
	{
	}

	public static void MeltGameObject(GameObject theObject, Action onDone)
	{
		ModelMelter modelMelter = Initialize(theObject, false);
		modelMelter.mDoneEvent = onDone;
	}

	public static void AscendGameObject(GameObject theObject, Action onDone)
	{
		ModelMelter modelMelter = Initialize(theObject, true);
		modelMelter.mDoneEvent = onDone;
	}

	private static ModelMelter Initialize(GameObject theObject, bool ascendToHeavens)
	{
		Vector3 position = theObject.transform.position;
		GameObject gameObject = sObjectPool.Acquire("ModelMelterParent");
		GameObject gameObject2 = sObjectPool.Acquire("ModelMelter");
		ModelMelter modelMelter = gameObject2.GetComponent<ModelMelter>();
		if (modelMelter == null)
		{
			modelMelter = gameObject2.AddComponent<ModelMelter>();
		}
		else
		{
			modelMelter.enabled = true;
		}
		Renderer componentInChildren = theObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			componentInChildren = theObject.GetComponentInChildren<MeshRenderer>();
		}
		if (componentInChildren != null)
		{
			position = componentInChildren.bounds.center;
			position.y = theObject.transform.position.y;
		}
		gameObject.transform.position = position;
		gameObject.transform.localScale = Vector3.one;
		gameObject2.transform.position = position;
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localScale = Vector3.one;
		theObject.transform.parent = gameObject2.transform;
		theObject.transform.localScale = Vector3.one;
		modelMelter.mGameObject = theObject;
		modelMelter.mMelterGameObjectParent = gameObject;
		modelMelter.mMelterGameObjectChild = gameObject2;
		modelMelter.PostStartInitialize(ascendToHeavens);
		return modelMelter;
	}

	private void PostStartInitialize(bool ascToHeavens)
	{
		ParticleEmitter[] componentsInChildren = GetComponentsInChildren<ParticleEmitter>();
		ParticleEmitter[] array = componentsInChildren;
		foreach (ParticleEmitter particleEmitter in array)
		{
			if (particleEmitter != null)
			{
				particleEmitter.emit = false;
			}
		}
		Animation[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Animation>();
		Animation[] array2 = componentsInChildren2;
		foreach (Animation animation in array2)
		{
			if (animation != null)
			{
				animation.Stop();
			}
		}
		Animation animation2 = base.gameObject.GetComponent<Animation>();
		if (animation2 == null)
		{
			animation2 = base.gameObject.AddComponent<Animation>();
		}
		AnimationClip animationClip = null;
		ascendToTheHeavens = ascToHeavens;
		if (ascendToTheHeavens)
		{
			animationClip = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/ModelMeltUp.anim", 1).Resource as AnimationClip;
			GameObject gameObject = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/AllyDie.prefab", 1).Resource as GameObject;
			if (gameObject != null)
			{
				mDieEffectObject = sObjectPool.Acquire(gameObject);
			}
		}
		else
		{
			animationClip = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/ModelMeltDown.anim", 1).Resource as AnimationClip;
			GameObject gameObject2 = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/EnemyDie.prefab", 1).Resource as GameObject;
			if (gameObject2 != null)
			{
				mDieEffectObject = sObjectPool.Acquire(gameObject2);
			}
		}
		mDieEffectObject.transform.position = base.transform.position;
		if (animationClip != null)
		{
			animation2.AddClip(animationClip, "melt");
			animation2.Play("melt");
			mTimeLeft = animationClip.length;
			mShaderTimeLeft = mTimeLeft;
			if (ascendToTheHeavens)
			{
				mShaderTimeLeft *= 0.66f;
			}
		}
		ProceduralShaderManager.postShaderEvent(new DiffuseFadeInOutShaderEvent(mGameObject, (!ascendToTheHeavens) ? Color.black : Color.white, mShaderTimeLeft, 3f, 5f));
	}

	private void Start()
	{
	}

	private void Update()
	{
		mTimeLeft -= Time.deltaTime;
		if (mTimeLeft <= 0f)
		{
			base.enabled = false;
			while (base.transform.childCount > 0)
			{
				base.transform.GetChild(0).parent = null;
			}
			if (mDoneEvent != null)
			{
				mDoneEvent();
			}
			mGameObject.transform.parent = null;
			mGameObject = null;
			mMelterGameObjectChild.transform.parent = null;
			sObjectPool.Release(mMelterGameObjectParent);
			sObjectPool.Release(mMelterGameObjectChild);
			sObjectPool.Release(mDieEffectObject);
			mMelterGameObjectParent = null;
			mMelterGameObjectChild = null;
			mDieEffectObject = null;
		}
	}
}
