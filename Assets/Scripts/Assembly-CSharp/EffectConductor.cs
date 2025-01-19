using UnityEngine;

[AddComponentMenu("")]
public class EffectConductor : MonoBehaviour
{
	protected EffectContainer effectContainer;

	public virtual void Activity_Start()
	{
	}

	public virtual void Activity_Stop()
	{
	}

	public virtual void Activity_Reverse()
	{
	}

	public virtual void SetStartPosition(Vector2 position)
	{
	}

	public virtual void OnEffectKilled(bool destroyingContainer = false)
	{
		effectContainer = null;
	}

	public virtual void Awake()
	{
		if (effectContainer == null)
		{
			effectContainer = GetComponent(typeof(EffectContainer)) as EffectContainer;
			effectContainer.AddConductor(this);
		}
	}

	public virtual void Start()
	{
	}
}
