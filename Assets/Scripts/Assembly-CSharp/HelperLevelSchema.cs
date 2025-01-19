[DataBundleClass(Category = "Design")]
public class HelperLevelSchema
{
	[DataBundleKey]
	public string id;

	public string cost;

	public int health;

	public float speedMin;

	public float speedMax;

	public float meleeRange;

	public float bowRange;

	public int meleeDamage;

	public int bowDamage;

	public int knockbackPower;

	public int knockbackResistance;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("Projectile")]
	public DataBundleRecordKey projectile;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey upgradeAlliesFrom;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey upgradeAlliesTo;

	[DataBundleRecordTableFilter("LocalizedStrings")]
	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	public DataBundleRecordKey specialUnlockText;

	[DataBundleSchemaFilter(typeof(BuffSchema), false)]
	public DataBundleRecordKey buffRecordKey;

	public BuffSchema buffSchema { get; private set; }

	public void Initialize(string tableName)
	{
		if (!string.IsNullOrEmpty(buffRecordKey.Key))
		{
			buffSchema = DataBundleRuntime.Instance.InitializeRecord<BuffSchema>(buffRecordKey);
		}
	}
}
