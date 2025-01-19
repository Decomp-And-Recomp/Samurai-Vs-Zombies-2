using UnityEngine;

[AddComponentMenu("Glui/Sprite")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class GluiSprite : GluiWidget
{
	protected override void OnResize()
	{
		UpdateQuadMesh(base.Size);
	}
}
