using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Lifetime")]
public class EffectConductor_Lifetime : EffectConductor
{
	public float lifetime = 1f;

	private float timeToDestroy;

	private bool destroying;

	public override void Start()
	{
		base.Start();
		if (lifetime > 0f)
		{
			timeToDestroy = Time.time + lifetime;
			destroying = true;
		}
		else
		{
			effectContainer.EffectKill();
		}
	}

	public void Update()
	{
		if (Time.time >= timeToDestroy && destroying)
		{
			effectContainer.EffectKill();
			destroying = false;
		}
	}
}
