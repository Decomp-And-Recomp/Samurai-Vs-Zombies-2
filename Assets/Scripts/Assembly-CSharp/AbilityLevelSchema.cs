[DataBundleClass(Category = "Design")]
public class AbilityLevelSchema
{
	[DataBundleKey]
	public int level;

	[DataBundleField]
	public int costCoins;

	[DataBundleField]
	public int costGems;

	[DataBundleField]
	public float damage;

	[DataBundleField]
	public float damageMultEachTarget;

	[DataBundleField]
	public float DOTDamage;

	[DataBundleField]
	public float DOTFrequency;

	[DataBundleField]
	public float DOTDuration;

	[DataBundleField]
	public float radius;

	[DataBundleField]
	public float speed;

	[DataBundleField]
	public float distance;

	[DataBundleField]
	public float duration;

	[DataBundleField]
	public float effectDuration;

	[DataBundleField]
	public float effectModifier;

	[DataBundleField]
	public float lifeSteal;

	[DataBundleField]
	public float flyerDamageMultiplier;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey upgradeDescription;

	public static AbilityLevelSchema Initialize(DataBundleRecordKey record)
	{
		return DataBundleUtils.InitializeRecord<AbilityLevelSchema>(record);
	}

	public AbilitySchema ShallowCopy()
	{
		return (AbilitySchema)MemberwiseClone();
	}
}
