using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Animation Modifier")]
public class AnimationModifier : MonoBehaviour
{
	public float startTime;

	public bool randomStartTime;

	public float speed;

	public WrapMode wrapMode;

	public bool applyToChildAnims;

	private void Start()
	{
		List<Animation> list = new List<Animation>();
		if (applyToChildAnims)
		{
			list.AddRange(GetComponentsInChildren<Animation>(true));
		}
		else
		{
			list.Add(GetComponent<Animation>());
		}
		list.ForEach(delegate(Animation anim)
		{
			AnimationClip clip = anim.clip;
			AnimationState animationState = anim[clip.name];
			if (randomStartTime)
			{
				animationState.time = Random.Range(0f, animationState.length);
			}
			else if (startTime != 0f)
			{
				animationState.time = startTime;
			}
			if (speed != 0f)
			{
				animationState.speed = speed;
			}
			if (wrapMode != 0)
			{
				animationState.wrapMode = wrapMode;
			}
		});
	}
}
