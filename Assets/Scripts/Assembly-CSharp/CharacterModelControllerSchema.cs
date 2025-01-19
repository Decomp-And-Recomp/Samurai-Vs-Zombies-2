using UnityEngine;

[DataBundleClass(Category = "Character")]
public class CharacterModelControllerSchema
{
	[DataBundleKey]
	public string id;

	public float animatedWalkSpeed = 1f;

	[DataBundleSchemaFilter(typeof(RandomAnimationClipSetSchema), false)]
	public DataBundleRecordTable randomAnimSets;

	[DataBundleField(StaticResource = true)]
	public GameObject arrowImpactEffect;

	[DataBundleField(StaticResource = true)]
	public GameObject bladeImpactEffect;

	[DataBundleField(StaticResource = true)]
	public GameObject bladeCriticalImpactEffect;

	[DataBundleField(StaticResource = true)]
	public GameObject bluntImpactEffect;

	[DataBundleField(StaticResource = true)]
	public GameObject bluntCriticalImpactEffect;

	public bool snapToGround = true;

	public DataBundleRecordTable Table { get; set; }

	public RandomAnimationClipSetSchema[] RandomAnimSets { get; set; }

	public static CharacterModelControllerSchema Initialize(DataBundleRecordKey record)
	{
		CharacterModelControllerSchema characterModelControllerSchema = null;
		if (!DataBundleRecordKey.IsNullOrEmpty(record))
		{
			characterModelControllerSchema = record.InitializeRecord<CharacterModelControllerSchema>();
			characterModelControllerSchema.Table = record.Table;
			if (!DataBundleRecordTable.IsNullOrEmpty(characterModelControllerSchema.randomAnimSets))
			{
				characterModelControllerSchema.RandomAnimSets = characterModelControllerSchema.randomAnimSets.InitializeRecords<RandomAnimationClipSetSchema>();
				RandomAnimationClipSetSchema[] array = characterModelControllerSchema.RandomAnimSets;
				foreach (RandomAnimationClipSetSchema randomAnimationClipSetSchema in array)
				{
					if (!DataBundleRecordTable.IsNullOrEmpty(randomAnimationClipSetSchema.clips))
					{
						randomAnimationClipSetSchema.Clips = randomAnimationClipSetSchema.clips.InitializeRecords<AnimationClipSchema>();
					}
				}
			}
		}
		return characterModelControllerSchema;
	}
}
