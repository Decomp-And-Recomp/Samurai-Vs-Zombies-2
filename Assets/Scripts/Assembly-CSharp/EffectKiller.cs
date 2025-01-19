using UnityEngine;

public class EffectKiller : MonoBehaviour
{
	private bool mHadChildren;

	private bool mHadEffects;

	private ParticleEmitter[] mEmitters;

	private ParticleSystem[] mParticleSystems;

	private Vector3 mOriginalScale = Vector3.one;

	public GameObjectPool effectPool { get; set; }

	public float maxLifetime { get; set; }

	private void Awake()
	{
		effectPool = GameObjectPool.DefaultObjectPool;
		mOriginalScale = base.gameObject.transform.localScale;
	}

	private void Start()
	{
		mParticleSystems = GetComponentsInChildren<ParticleSystem>();
		mEmitters = GetComponentsInChildren<ParticleEmitter>();
		mHadChildren = base.transform.GetChildCount() > 0;
		mHadEffects = mParticleSystems.Length > 0 || mEmitters.Length > 0;
	}

	public void Cleanup()
	{
		ParticleEmitter[] array = mEmitters;
		foreach (ParticleEmitter particleEmitter in array)
		{
			if (!(particleEmitter == null))
			{
				particleEmitter.emit = false;
			}
		}
		base.gameObject.transform.parent = null;
		base.gameObject.transform.localScale = mOriginalScale;
		if (effectPool != null)
		{
			effectPool.Release(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void StopEmitting()
	{
		ParticleEmitter[] array = mEmitters;
		foreach (ParticleEmitter particleEmitter in array)
		{
			if (!(particleEmitter == null))
			{
				particleEmitter.emit = false;
			}
		}
		ParticleSystem[] array2 = mParticleSystems;
		foreach (ParticleSystem particleSystem in array2)
		{
			if (!(particleSystem == null))
			{
				particleSystem.Stop();
			}
		}
	}

	private void Update()
	{
		if (maxLifetime > 0f)
		{
			maxLifetime -= Time.deltaTime;
			if (maxLifetime <= 0f)
			{
				if (mHadEffects)
				{
					StopEmitting();
					return;
				}
				Cleanup();
			}
		}
		if (mHadChildren && base.transform.GetChildCount() <= 0)
		{
			Cleanup();
		}
		else if (!mHadEffects)
		{
			return;
		}
		bool flag = false;
		ParticleSystem[] array = mParticleSystems;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem != null && particleSystem.IsAlive())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		bool flag2 = false;
		if (mEmitters != null)
		{
			ParticleEmitter[] array2 = mEmitters;
			foreach (ParticleEmitter particleEmitter in array2)
			{
				if (!(particleEmitter == null) && particleEmitter.particleCount > 0)
				{
					flag2 = true;
				}
			}
		}
		if (!flag2)
		{
			Cleanup();
		}
	}

	public static EffectKiller AddKiller(GameObject effect, GameObjectPool effectPool)
	{
		if (effect == null)
		{
			return null;
		}
		EffectKiller effectKiller = effect.GetComponent<EffectKiller>();
		if (effectKiller == null)
		{
			effectKiller = effect.AddComponent<EffectKiller>();
		}
		effectKiller.effectPool = effectPool;
		return effectKiller;
	}
}
