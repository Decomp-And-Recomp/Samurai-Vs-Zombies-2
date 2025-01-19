using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TaggedAnimPlayer))]
[RequireComponent(typeof(AutoPaperdoll))]
public class TaggedAnimPlayerController : MonoBehaviour
{
	[Serializable]
	public class RandomAnimSet
	{
		public string name;

		public AnimationClip[] clips;

		public bool onlyRandomizeOnce;

		[NonSerialized]
		internal int? mLastClipIndexChosen;
	}

	private TaggedAnimPlayer mAnimPlayer;

	public RandomAnimSet[] randomAnimSets = new RandomAnimSet[0];

	private Dictionary<string, RandomAnimSet> mRandomAnimSets = new Dictionary<string, RandomAnimSet>();

	private string mCurrentAnimGroup;

	private string mBaseAnimGroup;

	public TaggedAnimPlayer animPlayer
	{
		get
		{
			return mAnimPlayer;
		}
	}

	protected bool paused
	{
		get
		{
			return mAnimPlayer.paused;
		}
		set
		{
			mAnimPlayer.paused = value;
		}
	}

	protected AnimationState actionAnimState
	{
		get
		{
			return mAnimPlayer.actionAnimState;
		}
	}

	protected AnimationState currAnimState
	{
		get
		{
			return mAnimPlayer.currAnimState;
		}
	}

	protected string actionAnim
	{
		get
		{
			return mAnimPlayer.actionAnim;
		}
	}

	protected float baseAnimSpeed
	{
		get
		{
			return mAnimPlayer.baseAnimSpeed;
		}
		set
		{
			mAnimPlayer.baseAnimSpeed = value;
		}
	}

	protected float currAnimSpeed
	{
		get
		{
			return mAnimPlayer.currAnimSpeed;
		}
		set
		{
			mAnimPlayer.currAnimSpeed = value;
		}
	}

	protected float currAnimTime
	{
		get
		{
			return mAnimPlayer.currAnimTime;
		}
		set
		{
			mAnimPlayer.currAnimTime = value;
		}
	}

	protected Action currAnimChangedCallback
	{
		get
		{
			return mAnimPlayer.currAnimChangedCallback;
		}
		set
		{
			mAnimPlayer.currAnimChangedCallback = value;
		}
	}

	protected TaggedAnimPlayer.TaggedAnimCallback currAnimDoneCallback
	{
		get
		{
			return mAnimPlayer.currAnimDoneCallback;
		}
		set
		{
			mAnimPlayer.currAnimDoneCallback = value;
		}
	}

	protected string baseAnim
	{
		get
		{
			return mBaseAnimGroup;
		}
		private set
		{
			mBaseAnimGroup = value;
		}
	}

	protected string currAnim
	{
		get
		{
			return mCurrentAnimGroup;
		}
		private set
		{
			mCurrentAnimGroup = value;
		}
	}

	private AnimationState this[string name]
	{
		get
		{
			return mAnimPlayer[name];
		}
	}

	public string animNamePrefix { get; set; }

	public virtual float speedModifier
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	protected void Init()
	{
		if (mAnimPlayer == null)
		{
			mAnimPlayer = GetComponent<TaggedAnimPlayer>();
			if (mAnimPlayer == null)
			{
				mAnimPlayer = base.gameObject.AddComponent<TaggedAnimPlayer>();
			}
		}
	}

	protected bool IsDone()
	{
		return mAnimPlayer.IsDone();
	}

	protected void RevertToBaseAnim()
	{
		mAnimPlayer.RevertToBaseAnim();
	}

	protected void RevertToBaseAnim(float blendOutTime)
	{
		mAnimPlayer.RevertToBaseAnim(blendOutTime);
	}

	private void PlayAnimRaw(string theAnimName, float theBlendSpeed, WrapMode theWrapMode)
	{
		mAnimPlayer.PlayAnim(theAnimName, theBlendSpeed, theWrapMode, speedModifier);
	}

	private void PlayAnimRaw(string theAnimName, float theBlendSpeed, WrapMode theWrapMode, float thePlaybackSpeed)
	{
		mAnimPlayer.PlayAnim(theAnimName, theBlendSpeed, theWrapMode, speedModifier);
	}

	private float GetDefaultBlendSpeedRaw(string animName)
	{
		return mAnimPlayer.GetDefaultBlendSpeed(animName);
	}

	private string GetRandomAnimRaw(string anim)
	{
		if (mRandomAnimSets != null)
		{
			RandomAnimSet value = null;
			if (mRandomAnimSets.TryGetValue(anim, out value) && value != null && value.clips != null)
			{
				if (value.onlyRandomizeOnce)
				{
					int? mLastClipIndexChosen = value.mLastClipIndexChosen;
					if (mLastClipIndexChosen.HasValue)
					{
						return value.clips[value.mLastClipIndexChosen.Value].name;
					}
				}
				int num = RandomRangeInt.between(0, value.clips.Length - 1);
				int? mLastClipIndexChosen2 = value.mLastClipIndexChosen;
				if (mLastClipIndexChosen2.HasValue && value.clips.Length >= 3)
				{
					while (value.mLastClipIndexChosen == num)
					{
						num = RandomRangeInt.between(0, value.clips.Length - 1);
					}
				}
				value.mLastClipIndexChosen = num;
				AnimationClip animationClip = value.clips[num];
				if (animationClip != null)
				{
					return animationClip.name;
				}
			}
		}
		if ((bool)this[anim])
		{
			return anim;
		}
		if ((bool)this[anim + "01"])
		{
			return anim + "01";
		}
		return string.Empty;
	}

	private string LastRandomAnimRaw(string anim)
	{
		RandomAnimSet value;
		if (mRandomAnimSets != null && mRandomAnimSets.TryGetValue(anim, out value))
		{
			int? mLastClipIndexChosen = value.mLastClipIndexChosen;
			if (mLastClipIndexChosen.HasValue)
			{
				return value.clips[value.mLastClipIndexChosen.Value].name;
			}
			return string.Empty;
		}
		if ((bool)this[anim])
		{
			return anim;
		}
		if ((bool)this[anim + "01"])
		{
			return anim + "01";
		}
		return string.Empty;
	}

	protected void PlayAnimGroupRandom(string animName, WrapMode mode)
	{
		currAnim = animName;
		string text = animNamePrefix + animName;
		PlayAnimRaw(GetRandomAnimRaw(text), GetDefaultBlendSpeedRaw(text), mode);
	}

	protected void PlayAnimGroup(string animName, WrapMode mode)
	{
		currAnim = animName;
		string text = animNamePrefix + animName;
		PlayAnimRaw(text, GetDefaultBlendSpeedRaw(text), mode);
	}

	protected void ClearBaseAnim()
	{
		mAnimPlayer.ClearBaseAnim();
		baseAnim = string.Empty;
	}

	public virtual void PlayAnimation(string anim)
	{
		PlayAnimGroupRandom(anim, WrapMode.Once);
	}

	public void PlayAnimation(string anim, float inMaxTime)
	{
		PlayAnimation(anim);
		if (!(mAnimPlayer == null) && !(mAnimPlayer.currAnimState == null) && mAnimPlayer.currAnimState.length > inMaxTime)
		{
			mAnimPlayer.currAnimSpeed *= mAnimPlayer.currAnimState.length / inMaxTime;
		}
	}

	public void SetBaseAnim(string anim)
	{
		string anim2 = animNamePrefix + anim;
		if (!(mAnimPlayer.baseAnim == LastRandomAnimRaw(anim2)))
		{
			mAnimPlayer.SetBaseAnim(GetRandomAnimRaw(anim2));
			mAnimPlayer.baseAnimSpeed = speedModifier;
			baseAnim = anim;
		}
	}

	public bool HasAnim(string theAnimName)
	{
		return mRandomAnimSets.ContainsKey(theAnimName) || this[theAnimName] != null || this[theAnimName + "01"] != null;
	}

	public void UpdateRandomAnimSets()
	{
		mRandomAnimSets.Clear();
		if (randomAnimSets != null)
		{
			RandomAnimSet[] array = randomAnimSets;
			foreach (RandomAnimSet randomAnimSet in array)
			{
				mRandomAnimSets.Add(randomAnimSet.name, randomAnimSet);
			}
		}
	}
}
