[DataBundleClass(Category = "Character")]
public class RandomAnimationClipSetSchema
{
	[DataBundleKey]
	public string name;

	[DataBundleSchemaFilter(typeof(AnimationClipSchema), false)]
	public DataBundleRecordTable clips;

	public bool onlyRandomizeOnce;

	public AnimationClipSchema[] Clips { get; set; }
}
