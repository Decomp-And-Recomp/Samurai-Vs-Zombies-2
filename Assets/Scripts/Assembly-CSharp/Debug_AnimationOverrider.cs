using UnityEngine;

public class Debug_AnimationOverrider : MonoBehaviour
{
	public string animationPlaying = string.Empty;

	public string AnimationPlaying
	{
		get
		{
			return animationPlaying;
		}
		set
		{
			animationPlaying = value;
			if (Application.isPlaying)
			{
				PlayAnimation(animationPlaying);
			}
		}
	}

	public void Start()
	{
		PlayAnimation(animationPlaying);
	}

	public virtual void PlayAnimation(string newAnimation)
	{
		if (!(newAnimation == "None") && !(newAnimation == string.Empty))
		{
			base.gameObject.GetComponent<Animation>().Play(newAnimation);
		}
	}
}
