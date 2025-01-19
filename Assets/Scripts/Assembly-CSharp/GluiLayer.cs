using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Layer")]
public class GluiLayer : GluiWidget
{
	public bool modal;

	public int depth;

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.gray;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(base.Size.x, base.Size.y, 0f));
	}
}
