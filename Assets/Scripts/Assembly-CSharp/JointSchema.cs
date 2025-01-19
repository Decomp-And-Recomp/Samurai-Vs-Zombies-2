using UnityEngine;

[DataBundleClass(Category = "Character")]
public class JointSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "JointLabel")]
	public DataBundleRecordKey label;

	public string jointName;

	[DataBundleField(StaticResource = true, Group = (DataBundleResourceGroup.InGame | DataBundleResourceGroup.Preview))]
	public GameObject prefab;

	public float offsetPositionX;

	public float offsetPositionY;

	public float offsetPositionZ;

	public float offsetRotationX;

	public float offsetRotationY;

	public float offsetRotationZ;

	public float offsetScaleX;

	public float offsetScaleY;

	public float offsetScaleZ;

	public static string kEnumTable
	{
		get
		{
			return "JointLabel";
		}
	}
}
