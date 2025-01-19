[DataBundleClass(Category = "Character")]
public class TaggedAnimSettingsSchema
{
	[DataBundleKey]
	public string clipName;

	public bool overrideBlendSpeed;

	public float blendSpeed;

	public string jointMaskRootName;

	public bool onlyUseSingleJoint;
}
