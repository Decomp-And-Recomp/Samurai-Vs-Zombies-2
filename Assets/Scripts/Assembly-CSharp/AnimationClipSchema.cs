using UnityEngine;

[DataBundleClass(Category = "Character")]
public class AnimationClipSchema
{
	[DataBundleKey]
	public string name;

	[DataBundleField(StaticResource = true)]
	public AnimationClip clip;
}
