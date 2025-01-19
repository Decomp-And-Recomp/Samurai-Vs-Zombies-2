using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AutoPaperdoll))]
public class AnimationEffects : MonoBehaviour
{
	private class AnimEndedFunc
	{
		internal AnimationState parentState;

		internal Action funcToCall;
	}

	public bool playRandomAnimAtStartup;

	public float cameraShakeIntensityMult = 1f;

	private List<Transform> mHiddenBodyParts;

	private List<AnimEndedFunc> mAnimEndedFunctions;

	private AutoPaperdoll mAutoPaperdoll;

	private void Awake()
	{
		mHiddenBodyParts = null;
		mAnimEndedFunctions = null;
		mAutoPaperdoll = null;
	}

	private void Start()
	{
		mAutoPaperdoll = GetComponent<AutoPaperdoll>();
		if ((mHiddenBodyParts == null || mHiddenBodyParts.Count == 0) && (mAnimEndedFunctions == null || mAnimEndedFunctions.Count == 0))
		{
			base.enabled = false;
		}
		if (!playRandomAnimAtStartup)
		{
			return;
		}
		Animation animation = base.GetComponent<Animation>();
		if (!(animation != null))
		{
			return;
		}
		animation.playAutomatically = false;
		int num = UnityEngine.Random.Range(0, animation.GetClipCount());
		foreach (AnimationState item in animation)
		{
			if (num == 0 && item != null && item.clip != null)
			{
				animation.Play(item.clip.name);
				break;
			}
			num--;
		}
	}

	public void HideBodyPart(string theJointLabel)
	{
		if (mHiddenBodyParts == null)
		{
			mHiddenBodyParts = new List<Transform>();
		}
		AutoPaperdoll.LabeledJoint jointData = mAutoPaperdoll.GetJointData(theJointLabel);
		Transform joint = jointData.joint;
		if (!mHiddenBodyParts.Contains(joint))
		{
			mHiddenBodyParts.Add(joint);
		}
		joint.localScale = Vector3.zero;
		base.enabled = true;
	}

	public void HideBodyPartThisAnimOnly(AnimationEvent animEvent)
	{
		if (animEvent != null && !(animEvent.animationState == null))
		{
			HideBodyPart(animEvent.stringParameter);
			AnimEndedFunc animEndedFunc = new AnimEndedFunc();
			animEndedFunc.parentState = animEvent.animationState;
			string bodyPartName = animEvent.stringParameter;
			animEndedFunc.funcToCall = delegate
			{
				RestoreHiddenBodyPart(bodyPartName);
			};
			if (mAnimEndedFunctions == null)
			{
				mAnimEndedFunctions = new List<AnimEndedFunc>();
			}
			mAnimEndedFunctions.Add(animEndedFunc);
			base.enabled = true;
		}
	}

	public void RestoreHiddenBodyPart(string theJointLabel)
	{
		if (mHiddenBodyParts != null)
		{
			AutoPaperdoll.LabeledJoint jointData = mAutoPaperdoll.GetJointData(theJointLabel);
			Transform joint = jointData.joint;
			mHiddenBodyParts.Remove(joint);
		}
	}

	private GameObject SpawnEffectHelper(AnimationEvent theAnimEvent, bool keepAttached, bool ignoreRotation)
	{
		GameObject gameObject = theAnimEvent.objectReferenceParameter as GameObject;
		if (!gameObject)
		{
			return null;
		}
		GameObject gameObject2 = GameObjectPool.DefaultObjectPool.Acquire(gameObject);
		string stringParameter = theAnimEvent.stringParameter;
		if (stringParameter != null)
		{
			mAutoPaperdoll.ApplyObjectToJoint(gameObject2, null, stringParameter, keepAttached, false, ignoreRotation);
		}
		internal_killEffect(gameObject2);
		return gameObject2;
	}

	public void SpawnEffectPrefab(AnimationEvent theAnimEvent)
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject gameObject = SpawnEffectHelper(theAnimEvent, false, true);
			if (gameObject != null)
			{
				gameObject.SendMessage("SpawnedFrom", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void SpawnEffectPrefabIgnoreRotation(AnimationEvent theAnimEvent)
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject gameObject = SpawnEffectHelper(theAnimEvent, false, false);
			if (gameObject != null)
			{
				gameObject.SendMessage("SpawnedFrom", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void AttachTargetToJoint(AnimationEvent theAnimEvent)
	{
		CharacterModelController component = base.gameObject.GetComponent<CharacterModelController>();
		if (component != null && component.target != null)
		{
			AutoPaperdoll.LabeledJoint jointData = mAutoPaperdoll.GetJointData(theAnimEvent.stringParameter);
			component.AttachCharacterToJoint(component.target, jointData);
		}
	}

	public void ShrinkTargetOverTime(AnimationEvent theAnimEvent)
	{
		CharacterModelController component = base.gameObject.GetComponent<CharacterModelController>();
		if (component != null)
		{
			component.target.SetScaleOverTime(0f, theAnimEvent.floatParameter);
		}
	}

	public void AttachEffectPrefabToJoint(AnimationEvent theAnimEvent)
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject gameObject = SpawnEffectHelper(theAnimEvent, true, true);
			if (gameObject != null)
			{
				gameObject.SendMessage("SpawnedFrom", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void AttachEffectPrefabToJointThisAnimOnly(AnimationEvent theAnimEvent)
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return;
		}
		AutoPaperdoll.LabeledJoint jointData = mAutoPaperdoll.GetJointData(theAnimEvent.stringParameter);
		if (jointData != null && jointData.joint != null)
		{
			for (int i = 0; i < jointData.joint.childCount; i++)
			{
				if (jointData.joint.GetChild(i).name.StartsWith(theAnimEvent.objectReferenceParameter.name))
				{
					return;
				}
			}
		}
		GameObject newObject = SpawnEffectHelper(theAnimEvent, true, true);
		if (newObject != null)
		{
			newObject.SendMessage("SpawnedFrom", base.gameObject, SendMessageOptions.DontRequireReceiver);
			AnimEndedFunc animEndedFunc = new AnimEndedFunc();
			animEndedFunc.parentState = theAnimEvent.animationState;
			animEndedFunc.funcToCall = delegate
			{
				GameObjectPool.DefaultObjectPool.Release(newObject);
			};
			if (mAnimEndedFunctions == null)
			{
				mAnimEndedFunctions = new List<AnimEndedFunc>();
			}
			mAnimEndedFunctions.Add(animEndedFunc);
			base.enabled = true;
		}
	}

	public void ShakeCamera(float theShakeIntensity)
	{
		CameraShaker.RequestShake(base.transform.position, theShakeIntensity * cameraShakeIntensityMult);
	}

	public void MeltThisGameObject()
	{
		ModelMelter.MeltGameObject(base.gameObject, DeleteThisGameObject);
	}

	public void DeleteThisGameObject()
	{
		GameObjectPool.DefaultObjectPool.Release(base.gameObject);
	}

	public void StopAllParticleEmitters()
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
		ParticleSystem[] componentsInChildren2 = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array2 = componentsInChildren2;
		foreach (ParticleSystem particleSystem in array2)
		{
			if (particleSystem != null)
			{
				particleSystem.Stop();
			}
		}
	}

	private void LateUpdate()
	{
		bool flag = mHiddenBodyParts != null && mHiddenBodyParts.Count > 0;
		bool flag2 = mAnimEndedFunctions != null && mAnimEndedFunctions.Count > 0;
		if (!flag && !flag2)
		{
			mHiddenBodyParts = null;
			mAnimEndedFunctions = null;
			base.enabled = false;
			return;
		}
		if (flag)
		{
			foreach (Transform mHiddenBodyPart in mHiddenBodyParts)
			{
				mHiddenBodyPart.localScale = Vector3.zero;
			}
		}
		if (!flag2)
		{
			return;
		}
		for (int num = mAnimEndedFunctions.Count - 1; num >= 0; num--)
		{
			AnimEndedFunc animEndedFunc = mAnimEndedFunctions[num];
			if (animEndedFunc == null)
			{
				mAnimEndedFunctions.RemoveAt(num);
			}
			else if (animEndedFunc.parentState == null || !animEndedFunc.parentState.enabled)
			{
				if (animEndedFunc.funcToCall != null)
				{
					animEndedFunc.funcToCall();
				}
				mAnimEndedFunctions.RemoveAt(num);
			}
		}
	}

	private void internal_killEffect(GameObject effect)
	{
		EffectKiller.AddKiller(effect, GameObjectPool.DefaultObjectPool);
	}
}
