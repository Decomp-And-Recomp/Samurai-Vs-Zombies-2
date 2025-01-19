using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/SoulBurnHandler")]
public class SoulBurnHandler : AbilityHandlerComponent
{
	private enum State
	{
		ES_Projectile = 0,
		ES_Stunning = 1,
		ES_Burning = 2,
		ES_Exploding = 3
	}

	private const float kProjectileRadius = 0.5f;

	private State mState;

	private float mDistanceRemaining;

	private float mRemainingBurnTime;

	private float mRemainingStunTime;

	private float mDPS;

	private float mSpeed;

	private Character mTarget;

	private Transform mTargetXForm;

	private Hero mHeroExecutor;

	private void Start()
	{
		mTarget = null;
		mTargetXForm = null;
		mState = State.ES_Projectile;
		mHeroExecutor = mExecutor as Hero;
		float num = Extrapolate((AbilityLevelSchema als) => als.distance);
		mDistanceRemaining = num - schema.spawnOffsetHorizontal;
		mSpeed = Extrapolate((AbilityLevelSchema als) => als.speed);
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = true;
		}
	}

	private void AssignTarget()
	{
		float z = base.transform.position.z;
		GameRange gameRange = new GameRange(z, z + mDistanceRemaining);
		if (!mHeroExecutor.LeftToRight)
		{
			gameRange.PivotAboutLeft();
		}
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(gameRange, 1 - mHeroExecutor.ownerId);
		if (charactersInRange.Count <= 0)
		{
			return;
		}
		float num = float.MaxValue;
		Character character = null;
		foreach (Character item in charactersInRange)
		{
			float num2 = Mathf.Abs(item.transform.position.z - z);
			if (num2 < num)
			{
				character = item;
				num = num2;
			}
		}
		mTarget = character;
		if (mTarget != null)
		{
			mTargetXForm = mTarget.transform;
		}
		else
		{
			mTargetXForm = null;
		}
	}

	private void Update()
	{
		switch (mState)
		{
		case State.ES_Projectile:
			UpdateProjectile();
			break;
		case State.ES_Stunning:
			UpdateStunning();
			break;
		case State.ES_Burning:
			UpdateBurning();
			break;
		}
	}

	private void UpdateProjectile()
	{
		float z = base.transform.position.z;
		float num = mSpeed * Time.deltaTime;
		if (num > mDistanceRemaining)
		{
			num = mDistanceRemaining;
		}
		if (mTarget == null)
		{
			AssignTarget();
		}
		if (mTarget != null && mTarget.health > 0f && ((!mHeroExecutor.LeftToRight) ? (z - num < mTargetXForm.position.z) : (z + num > mTargetXForm.position.z)))
		{
			mState = State.ES_Stunning;
			mRemainingBurnTime = Extrapolate((AbilityLevelSchema als) => als.duration);
			mRemainingStunTime = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
			mTarget.controller.impactPauseTime = 0f;
			mTarget.controller.stunnedTimer = mRemainingBurnTime + mRemainingStunTime;
			mTarget.controller.LoopStunAnim("stun");
			mTarget.controller.SpawnEffectAtJoint(schema.activateFX, "body_effect", true, mRemainingBurnTime + mRemainingStunTime);
			mTarget.MaterialColorFlicker(Color.green, 0.1f, 0.25f, 0.1f, mRemainingStunTime);
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = false;
			}
		}
		else
		{
			mDistanceRemaining -= num;
			if (mDistanceRemaining <= 0.01f)
			{
				GameObjectPool.DefaultObjectPool.Release(base.gameObject);
				return;
			}
			float num2 = ((mTarget == null) ? 0f : (mTarget.controller.autoPaperdoll.GetJointPosition("impact_target").y - base.transform.position.y));
			base.gameObject.transform.Translate(0f, Mathf.Min(num2 * 5f * Time.deltaTime, num2), (!mHeroExecutor.LeftToRight) ? (0f - num) : num, Space.World);
		}
	}

	private void UpdateStunning()
	{
		mRemainingStunTime -= Time.deltaTime;
		if (mRemainingStunTime <= 0f)
		{
			mState = State.ES_Burning;
			mDPS = Extrapolate((AbilityLevelSchema als) => als.DOTDamage);
			mTarget.SetDamageOverTimeSpecialType(mDPS * 0.15f, 0.15f, mRemainingBurnTime, mHeroExecutor, 0f, EAttackType.SoulBurn);
		}
	}

	private void UpdateBurning()
	{
		if (mRemainingBurnTime <= 0f || mTarget == null)
		{
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
			return;
		}
		float deltaTime = Time.deltaTime;
		if (mTarget.health <= 0f)
		{
			float num = Extrapolate((AbilityLevelSchema als) => als.radius);
			GameObject obj = GameObjectPool.DefaultObjectPool.Acquire(schema.resultFX, mTargetXForm.position, Quaternion.identity);
			GameObjectPool.DefaultObjectPool.Release(obj, 2f);
			float z = mTargetXForm.position.z;
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(z - num, z + num, 1 - mExecutor.ownerId);
			float damage = Extrapolate((AbilityLevelSchema als) => als.damage);
			foreach (Character item in charactersInRange)
			{
				if (item.ownerId != mHeroExecutor.ownerId && item != mTarget)
				{
					item.RecievedAttack(EAttackType.Explosion, damage, mHeroExecutor);
				}
			}
			mTarget.DoExplosionBodyFX(mTarget.controlledObject);
			mTarget = null;
			mTargetXForm = null;
		}
		mRemainingBurnTime -= deltaTime;
	}
}
