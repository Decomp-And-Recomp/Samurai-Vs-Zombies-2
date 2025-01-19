[DataBundleClass(Category = "Character")]
public class TaggedAnimPlayerSchema
{
	[DataBundleKey]
	public string id;

	public float defaultBlendSpeed;

	[DataBundleSchemaFilter(typeof(TaggedAnimSettingsSchema), false)]
	public DataBundleRecordTable specificAnimSettings;

	public bool alsoPlayOnAllChildren;

	public string overrideAnimFolder;

	public DataBundleRecordTable Table { get; set; }

	public TaggedAnimSettingsSchema[] SpecificAnimSettings { get; set; }

	public static TaggedAnimPlayerSchema Initialize(DataBundleRecordKey record)
	{
		TaggedAnimPlayerSchema taggedAnimPlayerSchema = null;
		if (!DataBundleRecordKey.IsNullOrEmpty(record))
		{
			taggedAnimPlayerSchema = record.InitializeRecord<TaggedAnimPlayerSchema>();
			taggedAnimPlayerSchema.Table = record.Table;
			if (!DataBundleRecordTable.IsNullOrEmpty(taggedAnimPlayerSchema.specificAnimSettings))
			{
				taggedAnimPlayerSchema.SpecificAnimSettings = taggedAnimPlayerSchema.specificAnimSettings.InitializeRecords<TaggedAnimSettingsSchema>();
			}
		}
		return taggedAnimPlayerSchema;
	}
}
