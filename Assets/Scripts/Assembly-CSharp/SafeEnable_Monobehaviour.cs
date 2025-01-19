using UnityEngine;

public class SafeEnable_Monobehaviour : MonoBehaviour
{
	private bool readyToEnable;

	private void Start()
	{
		readyToEnable = true;
		if (base.enabled)
		{
			OnSafeEnable();
		}
	}

	private void OnEnable()
	{
		if (readyToEnable)
		{
			OnSafeEnable();
		}
	}

	public virtual void OnSafeEnable()
	{
	}
}
