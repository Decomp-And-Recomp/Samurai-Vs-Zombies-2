[DataBundleClass(Category = "Design")]
public class MultiplayerTweakSchema
{
	[DataBundleKey]
	public int soulCost;

	[DataBundleField]
	public float heroMoveSpeed;

	[DataBundleField]
	public float heroAttackSpeed;

	[DataBundleField]
	public float heroHealth;

	[DataBundleField]
	public float heroHealthRegen;

	[DataBundleField]
	public float heroMeleeDamage;

	[DataBundleField]
	public float heroRangedDamage;

	[DataBundleField]
	public float heroAbilityDamage;

	[DataBundleField]
	public float heroAbilityFrequency;

	[DataBundleField]
	public float leadershipRate;

	[DataBundleField]
	public float helperMoveSpeed;

	[DataBundleField]
	public float helperDamage;

	[DataBundleField]
	public float helperHealth;

	[DataBundleField]
	public float startingLeadership;

	[DataBundleField]
	public float gateHealth;

	public float attackLeadershipPool;
}
