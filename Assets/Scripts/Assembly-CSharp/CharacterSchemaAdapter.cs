using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Game/Character Schema Adapter")]
public class CharacterSchemaAdapter : SchemaAdapter<CharacterSchema>
{
	private bool newRecord;

	private string modelPath = string.Empty;

	protected override void Start()
	{
		base.Start();
		if (DataBundleRecordKey.IsNullOrEmpty(record))
		{
			record = DataBundleRuntime.TableRecordKey("Character", base.name.Substring(2));
			newRecord = true;
		}
		if (record != null)
		{
			base.Schema = CharacterSchema.Initialize(record);
		}
	}

	public override GameObject Deserialize(DataBundleRecordKey record)
	{
		return null;
	}

	public override void Serialize(GameObject obj)
	{
	}
}
