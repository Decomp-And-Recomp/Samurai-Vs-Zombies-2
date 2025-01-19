using UnityEngine;

[AddComponentMenu("Glui/Screen")]
[ExecuteInEditMode]
public class GluiScreen : GluiBase
{
	public enum ResizeAxis
	{
		Width = 0,
		Height = 1
	}

	public float nativeWidth = 1024f;

	public float nativeHeight = 768f;

	public ResizeAxis resizeConstant = ResizeAxis.Height;

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(nativeWidth, nativeHeight, 0f));
	}
}
