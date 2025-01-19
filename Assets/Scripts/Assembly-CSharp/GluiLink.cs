using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Link")]
public class GluiLink : GluiHotspot
{
	public string target = string.Empty;

	public bool asynchronous = true;

	public bool showLoadingAnim;

	protected override void Trigger(bool pressed)
	{
		if (!pressed)
		{
			Application.LoadLevel(target);
		}
	}
}
