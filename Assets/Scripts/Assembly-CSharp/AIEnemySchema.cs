[DataBundleClass(Category = "Design")]
public class AIEnemySchema
{
	[DataBundleKey]
	public string id;

	[DataBundleSchemaFilter(typeof(HeroSchema), false)]
	public DataBundleRecordKey heroId;

	public int heroLevel;

	public int bowLevel;

	public int swordLevel;

	public int armorLevel;

	public int leadershipLevel;

	public int gateLevel;

	public int bellLevel;

	public int pitLevel;

	public int archerLevel;

	public int flowerSets;

	public int bannerSets;

	public int swordSets;

	public int bowSets;

	public int armorSets;

	public int horseSets;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper1;

	public int helperLevel1;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper2;

	public int helperLevel2;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper3;

	public int helperLevel3;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper4;

	public int helperLevel4;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper5;

	public int helperLevel5;

	[DataBundleSchemaFilter(typeof(HelperSchema), false)]
	public DataBundleRecordKey helper6;

	public int helperLevel6;

	[DataBundleSchemaFilter(typeof(AbilitySchema), false)]
	public DataBundleRecordKey ability1;

	public int abilityLevel1;

	[DataBundleSchemaFilter(typeof(AbilitySchema), false)]
	public DataBundleRecordKey ability2;

	public int abilityLevel2;

	[DataBundleSchemaFilter(typeof(AbilitySchema), false)]
	public DataBundleRecordKey ability3;

	public int abilityLevel3;
}
