using UnityEngine;

[DataBundleClass(Category = "Design")]
public class PlayModeSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "PlayMode")]
	public DataBundleRecordKey id;

	public bool useBell;

	public bool useVillageArchers;

	public bool allowBowOnFirstWave1;

	public int maxBaseWave;

	public int bonusWaveInterval;

	public bool rightToLeft;

	[DataBundleSchemaFilter(typeof(PotionSchema), false)]
	public DataBundleRecordKey revivePotion;

	[DataBundleSchemaFilter(typeof(WaveSchema), false)]
	public DataBundleRecordTable waves;

	[DataBundleSchemaFilter(typeof(WaveSchema), false)]
	public DataBundleRecordTable endlessWaves;

	[DataBundleSchemaFilter(typeof(WaveSchema), false)]
	public DataBundleRecordTable endlessBonusWaves;

	public string defaultHeroID;

	public string defaultMeleeWeaponID;

	public string defaultRangeWeaponID;

	public int minAIAttackRating;

	public int maxAIAttackRating;

	[DataBundleField(StaticResource = true, Group = DataBundleResourceGroup.None)]
	public Texture2D icon;

	public string profileSubSection;

	public string IconPath { get; private set; }

	public void Initialize(string tableName)
	{
		IconPath = DataBundleRuntime.Instance.GetValue<string>(typeof(PlayModeSchema), tableName, id, "icon", true);
	}
}
