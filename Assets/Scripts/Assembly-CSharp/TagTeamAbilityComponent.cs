using UnityEngine;

public class TagTeamAbilityComponent : MonoBehaviour
{
	public float DelayTime = 1f;

	private float mDelayTimer;

	public Hero TagHero;

	private bool mAbilityTriggered;

	public AbilitySchema schema;

	private void Start()
	{
		mDelayTimer = DelayTime;
	}

	private void Update()
	{
		if (!(mDelayTimer >= 0f))
		{
			return;
		}
		if (!mAbilityTriggered)
		{
			mDelayTimer -= Time.deltaTime;
			if (mDelayTimer <= 0f)
			{
				mAbilityTriggered = true;
				TagHero.DoAbility(WeakGlobalMonoBehavior<InGameImpl>.Instance.TagAbilityID, true);
				mDelayTimer = DelayTime;
			}
		}
		else if (TagHero.isEffectivelyIdle)
		{
			mDelayTimer -= Time.deltaTime;
			if (mDelayTimer <= 0f)
			{
				GameObject obj = GameObjectPool.DefaultObjectPool.Acquire(schema.prop, TagHero.transform.position, Quaternion.identity);
				GameObjectPool.DefaultObjectPool.Release(obj, 1.5f);
				Object.Destroy(this);
				Object.Destroy(TagHero.controlledObject);
				TagHero.Destroy();
			}
		}
	}
}
