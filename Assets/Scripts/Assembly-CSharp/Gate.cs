using UnityEngine;

public class Gate : Character
{
	private float damageReflectionRatio { get; set; }

	public bool NoDamage { get; set; }

	public Gate(GameObject baseObject, int playerId)
	{
		base.isBase = true;
		base.controlledObject = baseObject;
		base.controller.Idle();
		base.ownerId = playerId;
		ActivateHealthBar();
		SetupBaseAttributes();
		base.BlocksHeroMovement = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("GateBlockMovement", true);
		supportColorFlash = false;
		NoDamage = true;
	}

	public override void Update()
	{
		base.Update();
		if (base.health < base.maxHealth)
		{
			NoDamage = false;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public void Revive()
	{
		base.controller.startedDieAnim = false;
		if (base.health <= 0f)
		{
			base.controller.StopWalking();
			base.controller.Idle();
			base.controller.PlayHurtAnim("revive");
		}
		base.health = base.maxHealth;
	}

	private void SetupBaseAttributes()
	{
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Gate");
		int baseLevel = Singleton<Profile>.Instance.baseLevel;
		int num = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Flower");
		if (base.ownerId != 0)
		{
			baseLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.baseLevel;
			num = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.flowersCollected;
		}
		base.maxHealth = data.GetFloat(TextDBSchema.LevelKey("health", baseLevel));
		if (base.ownerId == 0)
		{
			damageReflectionRatio = (float)num * 0.1f;
		}
		else if (Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
		{
			base.maxHealth *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.gateHealth;
		}
		base.health = base.maxHealth;
	}

	public override void RecievedAttack(EAttackType attackType, float damage, Character attacker, bool canReflect)
	{
		base.RecievedAttack(attackType, damage, attacker, canReflect);
		if (canReflect && base.health > 0f && damageReflectionRatio > 0f && attacker != null)
		{
			attacker.RecievedAttack(attackType, damage * damageReflectionRatio, this, false);
		}
	}
}
