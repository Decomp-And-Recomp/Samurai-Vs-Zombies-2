using System.Collections.Generic;

[DataBundleClass(Category = "Character", Comment = "")]
public class CharacterDisplaySchema
{
	[DataBundleKey]
	public string propertyName;

	[DataBundleSchemaFilter(typeof(TaggedString), false)]
	[DataBundleRecordTableFilter("LocalizedStrings")]
	public DataBundleRecordKey displayText;

	private static Dictionary<string, string> mPropertyLookups;

	public static Dictionary<string, string> PropertyLookups()
	{
		if (mPropertyLookups == null)
		{
			mPropertyLookups = new Dictionary<string, string>();
			DataBundleTableHandle<CharacterDisplaySchema> dataBundleTableHandle = new DataBundleTableHandle<CharacterDisplaySchema>("SpecialAbilities");
			CharacterDisplaySchema[] data = dataBundleTableHandle.Data;
			foreach (CharacterDisplaySchema characterDisplaySchema in data)
			{
				mPropertyLookups.Add(characterDisplaySchema.propertyName, StringUtils.GetStringFromStringRef(characterDisplaySchema.displayText));
			}
		}
		return mPropertyLookups;
	}
}
