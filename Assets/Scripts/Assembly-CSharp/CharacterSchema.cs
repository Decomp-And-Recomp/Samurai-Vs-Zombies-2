using System.IO;
using UnityEngine;

[DataBundleClass(Category = "Character", Comment = "Spawns Character GameObject")]
public class CharacterSchema
{
	[DataBundleKey]
	public string id;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject prefab;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject model;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public Material material;

	[DataBundleDefaultValue(1f)]
	public float scaleX = 1f;

	[DataBundleDefaultValue(1f)]
	public float scaleY = 1f;

	[DataBundleDefaultValue(1f)]
	public float scaleZ = 1f;

	public string animTagRoot;

	[DataBundleSchemaFilter(typeof(TaggedAnimPlayerSchema), false)]
	public DataBundleRecordKey taggedAnimPlayerData;

	[DataBundleSchemaFilter(typeof(JointSchema), false)]
	public DataBundleRecordTable paperdollData;

	[DataBundleSchemaFilter(typeof(CharacterModelControllerSchema), false)]
	public DataBundleRecordKey characterModelControllerData;

	[DataBundleSchemaFilter(typeof(USoundThemeSetSchema), false, DontFollowRecordLink = true)]
	public DataBundleRecordKey soundTheme;

	public DataBundleRecordTable Table { get; set; }

	public TaggedAnimPlayerSchema TaggedAnimPlayerData { get; set; }

	public JointSchema[] PaperdollData { get; set; }

	public CharacterModelControllerSchema CharacterModelControllerData { get; set; }

	public static CharacterSchema Initialize(DataBundleRecordKey record)
	{
		CharacterSchema characterSchema = null;
		if (!DataBundleRecordKey.IsNullOrEmpty(record))
		{
			characterSchema = DataBundleUtils.InitializeRecord<CharacterSchema>(record);
			if (characterSchema != null)
			{
				characterSchema.Initialize(record.Table);
			}
		}
		return characterSchema;
	}

	public void Initialize(string tableName)
	{
		Table = tableName;
		TaggedAnimPlayerData = TaggedAnimPlayerSchema.Initialize(taggedAnimPlayerData);
		if (!DataBundleRecordTable.IsNullOrEmpty(paperdollData))
		{
			PaperdollData = paperdollData.InitializeRecords<JointSchema>();
		}
		CharacterModelControllerData = CharacterModelControllerSchema.Initialize(characterModelControllerData);
	}

	public static GameObject Deserialize(DataBundleRecordKey record)
	{
		CharacterSchema schema = Initialize(record);
		return Deserialize(schema, true);
	}

	public static GameObject Deserialize(DataBundleRecordKey record, bool includeSounds)
	{
		CharacterSchema schema = Initialize(record);
		return Deserialize(schema, includeSounds);
	}

	public static GameObject Deserialize(CharacterSchema schema)
	{
		return Deserialize(schema, true);
	}

	public static GameObject Deserialize(CharacterSchema schema, bool includeSounds)
	{
		if (schema == null)
		{
			return null;
		}
		GameObject gameObject = SchemaFieldAdapter.Deserialize(schema.prefab);
		if (gameObject != null)
		{
			Transform transform = gameObject.transform;
			gameObject.name = schema.id;
			GameObject gameObject2 = SchemaFieldAdapter.Deserialize(schema.model);
			if (gameObject2 != null)
			{
				gameObject2.name = schema.model.name;
				Transform transform2 = gameObject2.transform;
				transform2.parent = transform;
				transform2.localScale = new Vector3(schema.scaleX, schema.scaleY, schema.scaleZ);
				Renderer componentInChildren = gameObject2.GetComponentInChildren<Renderer>();
				if (componentInChildren != null)
				{
					Material sharedMaterial = schema.material;
					componentInChildren.sharedMaterial = sharedMaterial;
				}
			}
			TaggedAnimPlayer component = gameObject.GetComponent<TaggedAnimPlayer>();
			string resourcePath;
			if (string.IsNullOrEmpty(schema.animTagRoot))
			{
				string assetPath = SchemaFieldAdapter.GetAssetPath(typeof(CharacterSchema), schema.Table, schema.id, "model");
				string text = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + "_anim_tags";
				resourcePath = FileUtil.GetResourcePath(assetPath);
			}
			else
			{
				resourcePath = schema.animTagRoot;
			}
			component.overrideAnimFolder = null;
			TaggedAnimPlayerAdapter.Deserialize(component, gameObject2, resourcePath, schema.TaggedAnimPlayerData);
			component.alsoPlayOnAllChildren = true;
			if (gameObject2 != null)
			{
				Animation[] componentsInChildren = gameObject2.GetComponentsInChildren<Animation>(true);
				Animation[] array = componentsInChildren;
				foreach (Animation animation in array)
				{
					if (animation != null)
					{
						animation.Stop();
						component.RegisterNewChildAnimationPlayer(animation);
					}
				}
			}
			AutoPaperdoll component2 = gameObject.GetComponent<AutoPaperdoll>();
			AutoPaperdollAdapter.Deserialize(component2, gameObject2, schema.PaperdollData);
			CharacterModelController component3 = gameObject.GetComponent<CharacterModelController>();
			CharacterModelControllerSchemaAdapter.Deserialize(component3, schema.CharacterModelControllerData);
			if (includeSounds && WeakGlobalMonoBehavior<InGameImpl>.Instance != null)
			{
				UdamanSoundThemePlayer component4 = gameObject.GetComponent<UdamanSoundThemePlayer>();
				if ((bool)component4)
				{
					component4.soundThemeKey = schema.soundTheme;
				}
				if (SingletonSpawningMonoBehaviour<USoundThemeManager>.Exists)
				{
					SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.GetSoundTheme(schema.soundTheme);
				}
			}
		}
		return gameObject;
	}
}
