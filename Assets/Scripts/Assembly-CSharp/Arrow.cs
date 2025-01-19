using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
	private const float kVelocity = 10f;

	private const float kArrowStickTime = 2.5f;

	private const float kArrowHalfLength = 0.2f;

	private const float kBigExplosionRadius = 1.6f;

	private const float kExplosionRadius = 0.8f;

	private static GameObjectPool sObjectPool = new GameObjectPool();

	private static Vector3 defaultTargetOffset = new Vector3(0f, 0.5f, 0f);

	private WeakPtr<Character> mTargetRef;

	private float mDamage;

	private bool mUsingArcShot;

	private float mOriginalDistance;

	private float mOriginalHeight;

	private float mArcHeightLastFrame;

	private float mVelocityModifier = 1f;

	private UdamanSoundThemePlayer uSoundPlayer;

	private Quaternion? rotationOffset { get; set; }

	public override bool isDone
	{
		get
		{
			return base.gameObject == null;
		}
	}

	public Vector3 targetPosition
	{
		get
		{
			if (mTargetRef.ptr != null)
			{
				base.cachedTargetPos = mTargetRef.ptr.controller.autoPaperdoll.GetJointPosition("impact_target", defaultTargetOffset);
			}
			return base.cachedTargetPos;
		}
	}

	public Arrow(string type, Character shooter, Character target, float damage, Vector3 spawnPos)
	{
		base.type = type;
		base.shooter = shooter;
		mTargetRef = new WeakPtr<Character>(target);
		mDamage = damage;
		ProjectileSchema projectileSchema = WeakGlobalInstance<ProjectileManager>.Instance[type];
		base.gameObject = sObjectPool.Acquire(projectileSchema.prefab);
		if (projectileSchema.material != null)
		{
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.sharedMaterial = projectileSchema.material;
			}
		}
		base.transform = base.gameObject.transform;
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
		base.transform.position = spawnPos;
		SoundThemePlayer component = base.gameObject.GetComponent<SoundThemePlayer>();
		if (component != null && component.autoPlayEvent != "Spawn")
		{
			component.PlaySoundEvent("Spawn");
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(projectileSchema.soundTheme))
		{
			uSoundPlayer = base.gameObject.GetComponent<UdamanSoundThemePlayer>();
			if (uSoundPlayer == null)
			{
				uSoundPlayer = base.gameObject.AddComponent<UdamanSoundThemePlayer>();
			}
			uSoundPlayer.soundThemeKey = projectileSchema.soundTheme;
			uSoundPlayer.onLoadedSoundEvent = projectileSchema.onLoadedSoundEvent;
			uSoundPlayer.PlayOnLoadedSoundEvent();
		}
		mOriginalDistance = Vector3.Distance(spawnPos, targetPosition) - 0.2f;
		mOriginalHeight = Mathf.Abs(spawnPos.y - targetPosition.y);
		mUsingArcShot = WeakGlobalInstance<ProjectileManager>.Instance.ProjectileArcs(type) && (Mathf.Abs(spawnPos.z - targetPosition.z) > 3f || Mathf.Abs(spawnPos.y - targetPosition.y) > 0.5f);
		mVelocityModifier = projectileSchema.velocityModifier;
		MoveArrow();
		base.transform.position = spawnPos;
		mArcHeightLastFrame = 0f;
		if (type == "Rocket")
		{
			rotationOffset = Quaternion.Euler(90f, 0f, 0f);
		}
	}

	private void Restore()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = true;
		}
		ParticleEmitter[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ParticleEmitter>();
		foreach (ParticleEmitter particleEmitter in componentsInChildren2)
		{
			particleEmitter.emit = true;
		}
		ParticleSystem[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren3)
		{
			particleSystem.Play();
		}
	}

	private void DestroyArrow()
	{
		if (base.gameObject != null)
		{
			base.transform.parent = null;
			if (uSoundPlayer != null)
			{
				uSoundPlayer.soundThemeKey = string.Empty;
			}
			sObjectPool.Release(base.gameObject);
			base.gameObject = null;
			base.transform = null;
		}
	}

	private void DestroyBullet()
	{
		if (base.gameObject != null)
		{
			base.transform.parent = null;
			if (uSoundPlayer != null)
			{
				uSoundPlayer.soundThemeKey = string.Empty;
			}
			MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				meshRenderer.enabled = false;
			}
			ParticleEmitter[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ParticleEmitter>();
			foreach (ParticleEmitter particleEmitter in componentsInChildren2)
			{
				particleEmitter.emit = false;
			}
			ParticleSystem[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren3)
			{
				particleSystem.Stop();
			}
			sObjectPool.Release(base.gameObject);
			base.gameObject = null;
			base.transform = null;
		}
	}

	private void DestroyArrow(GameObject arrow)
	{
		if (arrow != null)
		{
			arrow.transform.parent = null;
			sObjectPool.Release(arrow);
		}
	}

	private void DestroyArrowTimed(float time)
	{
		GameObject arrow = base.gameObject;
		base.gameObject = null;
		base.transform = null;
		if (uSoundPlayer != null)
		{
			uSoundPlayer.soundThemeKey = string.Empty;
		}
		WeakGlobalMonoBehavior<InGameImpl>.Instance.StartCoroutine(DestroyArrowTimedCoroutine(arrow, time));
	}

	private IEnumerator DestroyArrowTimedCoroutine(GameObject arrow, float time)
	{
		yield return new WaitForSeconds(time);
		DestroyArrow(arrow);
	}

	public override void Update()
	{
		if (base.gameObject == null)
		{
			return;
		}
		base.Update();
		if (type == "Bullet")
		{
			mTargetRef.ptr.RecievedAttack(EAttackType.Bullet, mDamage, shooter);
			DestroyArrow();
		}
		else
		{
			if (!MoveArrow())
			{
				return;
			}
			if (mTargetRef.ptr != null)
			{
				if (mTargetRef.ptr.health <= 0f && type != "HealBolt" && type != "EvilHealBolt")
				{
					DestroyArrow();
					return;
				}
				EAttackType attackType = EAttackType.Arrow;
				switch (type)
				{
				case "HealBolt":
					if (shooter != null)
					{
						if (mTargetRef.ptr.ownerId == shooter.ownerId)
						{
							mTargetRef.ptr.RecievedHealing(mDamage);
						}
						else
						{
							mTargetRef.ptr.RecievedHealing((0f - mDamage) * 0.25f);
						}
					}
					DestroyArrow();
					return;
				case "EvilHealBolt":
					if (shooter != null)
					{
						if (mTargetRef.ptr.ownerId == shooter.ownerId)
						{
							mTargetRef.ptr.RecievedHealing(mDamage);
						}
						else
						{
							mTargetRef.ptr.RecievedHealing((0f - mDamage) * 0.25f);
						}
					}
					DestroyArrow();
					return;
				case "HeroExplodingArrow":
				case "ExplodingArrow":
					ApplyExplosion(mTargetRef.ptr, false);
					DestroyArrow();
					return;
				case "HeroBigExplodingArrow":
				case "BigExplodingArrow":
				case "Rocket":
					ApplyExplosion(mTargetRef.ptr, true);
					DestroyArrow();
					return;
				case "IceArrow":
					ApplyIceEffect(shooter, mTargetRef.ptr);
					break;
				case "Shuriken":
					attackType = EAttackType.Shuriken;
					mTargetRef.ptr.RecievedAttack(attackType, mDamage, shooter);
					DestroyArrow();
					return;
				case "FireArrow":
				case "BlueFireArrow":
				case "HeroFireArrow":
				case "HeroBlueFireArrow":
					attackType = EAttackType.FireArrow;
					break;
				case "Kunoichi_Dagger_Poison":
				case "Kunoichi_Dagger_RedPoison":
					ApplyPoisonDOT(mTargetRef.ptr, mDamage, shooter, 0f, false);
					return;
				case "Kunoichi_Dagger_ExplodePoison":
					ApplyPoisonDOT(mTargetRef.ptr, mDamage, shooter, 0.8f, true);
					mTargetRef.ptr.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/KunaiPoisonCloud.prefab", 1).Resource as GameObject, "impact_target", false);
					return;
				case "Kunoichi_Dagger_BigExplodePoison":
					ApplyPoisonDOT(mTargetRef.ptr, mDamage, shooter, 1.6f, true);
					mTargetRef.ptr.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/KunaiPoisonCloudBig.prefab", 1).Resource as GameObject, "impact_target", false);
					return;
				case "Corruption":
					ApplyCorruptionSwap(mTargetRef.ptr, shooter);
					return;
				case "SorceressBolt":
				case "SorceressBolt_Upgrade":
				case "SorceressBolt_Gold":
					mTargetRef.ptr.RecievedAttack(attackType, mDamage, shooter);
					DestroyArrow();
					return;
				case "SorceressBolt_Explode":
					ApplyMagicExplosion(mTargetRef.ptr, false);
					DestroyArrow();
					return;
				case "SorceressBolt_BigExplode":
					ApplyMagicExplosion(mTargetRef.ptr, true);
					DestroyArrow();
					return;
				case "Daimyo_Bullet":
					attackType = EAttackType.Bullet;
					shooter.PerformKnockback(mTargetRef.ptr, shooter.knockbackPowerRanged, false, Vector3.zero);
					mTargetRef.ptr.RecievedAttack(attackType, mDamage, shooter);
					DestroyBullet();
					return;
				case "Daimyo_Bullet_Explode":
					shooter.PerformKnockback(mTargetRef.ptr, shooter.knockbackPowerRanged, false, Vector3.zero);
					ApplyExplosion(mTargetRef.ptr, false);
					DestroyBullet();
					return;
				case "Daimyo_Bullet_Explode_Big":
					shooter.PerformKnockback(mTargetRef.ptr, shooter.knockbackPowerRanged, false, Vector3.zero);
					ApplyExplosion(mTargetRef.ptr, true);
					DestroyBullet();
					return;
				}
				mTargetRef.ptr.RecievedAttack(attackType, mDamage, shooter);
				mTargetRef.ptr.controller.autoPaperdoll.AttachObjectToJoint(base.gameObject, "impact_target", false);
				ParticleRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleRenderer>();
				ParticleRenderer[] array = componentsInChildren;
				foreach (ParticleRenderer particleRenderer in array)
				{
					if (particleRenderer != null)
					{
						particleRenderer.enabled = false;
					}
				}
				DestroyArrowTimed(2.5f);
			}
			else
			{
				DestroyArrow();
			}
		}
	}

	public override void Destroy()
	{
		DestroyArrow();
		base.Destroy();
	}

	private bool MoveArrow()
	{
		bool flag = false;
		Vector3 vector = targetPosition;
		Vector3 position = base.transform.position;
		if (mUsingArcShot)
		{
			position.y -= mArcHeightLastFrame;
		}
		float num = 10f * Time.deltaTime * mVelocityModifier;
		float num2 = Vector3.Distance(position, vector) - 0.2f;
		if (num > num2)
		{
			flag = true;
			num = num2;
		}
		position = Vector3.MoveTowards(position, vector, num);
		mArcHeightLastFrame = 0f;
		if (mUsingArcShot && !flag)
		{
			num2 -= num;
			if (num2 < mOriginalDistance)
			{
				float num3 = num2 / mOriginalDistance - 0.5f;
				float num4 = Mathf.Max(mOriginalDistance * 0.2f, mOriginalHeight + 0.1f * mOriginalHeight);
				mArcHeightLastFrame = num4 - 4f * num4 * (num3 * num3);
				position.y += mArcHeightLastFrame;
			}
		}
		if (!flag)
		{
			FaceTowardTarget(base.transform, position);
		}
		base.transform.position = position;
		return flag;
	}

	private void FaceTowardTarget(Transform arrowTransform, Vector3 targetPos)
	{
		Quaternion quaternion = Quaternion.LookRotation(targetPos - arrowTransform.position);
		arrowTransform.rotation = ((!rotationOffset.HasValue) ? quaternion : (quaternion * rotationOffset.Value));
	}

	private void ApplyExplosion(Character target, bool bigVersion)
	{
		if (bigVersion)
		{
			target.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/ExplosionBig.prefab", 1).Resource as GameObject, "impact_target", false);
		}
		else
		{
			target.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/Explosion.prefab", 1).Resource as GameObject, "impact_target", false);
		}
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(target.position.z - ((!bigVersion) ? 0.8f : 1.6f), target.position.z + ((!bigVersion) ? 0.8f : 1.6f), target.ownerId);
		foreach (Character item in charactersInRange)
		{
			item.RecievedAttack(EAttackType.Explosion, mDamage, shooter);
		}
	}

	private void ApplyIceEffect(Character shooter, Character target)
	{
		target.ApplyIceEffect(shooter.bowAttackFrequency, shooter);
	}

	private void ApplyPoisonDOT(Character target, float damage, Character shooter, float dotRadius, bool destroyArrow)
	{
		target.RecievedAttack(EAttackType.Arrow, damage, shooter);
		DOTInfo dotInfo = shooter.dotInfo;
		float num = 0.15f;
		float damagePerTick = damage * dotInfo.ratio * dotInfo.interval / dotInfo.duration;
		if (dotRadius > 0f)
		{
			float z = base.transform.position.z;
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(z - dotRadius, z + dotRadius, target.ownerId);
			foreach (Character item in charactersInRange)
			{
				item.SetDamageOverTime(damagePerTick, dotInfo.interval, dotInfo.duration, shooter);
				item.MaterialColorFadeInOut(Color.green, num, dotInfo.duration - num * 2f, num);
			}
		}
		else
		{
			target.SetDamageOverTime(damagePerTick, dotInfo.interval, dotInfo.duration, shooter);
			target.MaterialColorFadeInOut(Color.green, num, dotInfo.duration - num * 2f, num);
		}
		if (destroyArrow)
		{
			DestroyArrow();
			return;
		}
		mTargetRef.ptr.controller.autoPaperdoll.AttachObjectToJoint(base.gameObject, "impact_target", false);
		ParticleRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleRenderer>();
		ParticleRenderer[] array = componentsInChildren;
		foreach (ParticleRenderer particleRenderer in array)
		{
			if (particleRenderer != null)
			{
				particleRenderer.enabled = false;
			}
		}
		DestroyArrowTimed(2.5f);
	}

	private void ApplyMagicExplosion(Character target, bool bigVersion)
	{
		if (bigVersion)
		{
			target.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/SorceressExplosiveCloudBig.prefab", 1).Resource as GameObject, "impact_target", false);
		}
		else
		{
			target.controller.SpawnEffectAtJoint(ResourceCache.GetCachedResource("Assets/Game/Resources/FX/SorceressExplosiveCloud.prefab", 1).Resource as GameObject, "impact_target", false);
		}
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(target.position.z - ((!bigVersion) ? 0.8f : 1.6f), target.position.z + ((!bigVersion) ? 0.8f : 1.6f), target.ownerId);
		foreach (Character item in charactersInRange)
		{
			item.RecievedAttack(EAttackType.Explosion, mDamage, shooter);
		}
	}

	private void ApplyCorruptionSwap(Character target, Character shooter)
	{
		Helper helper = target as Helper;
		if (helper != null)
		{
			WeakGlobalInstance<WaveManager>.Instance.ReplaceHelperWithEnemy(helper);
		}
	}
}
