using UnityEngine;

public class GluiDebugRenderSupport
{
	public static void DrawDebugRegion(Renderer renderer, Matrix4x4 worldMatrix, Color color, Vector2 size)
	{
		Gizmos.matrix = worldMatrix;
		Gizmos.color = color;
		if (renderer != null)
		{
			Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(size.x, size.y, 0f));
			return;
		}
		Gizmos.DrawLine(new Vector3((0f - size.x) / 2f, (0f - size.y) / 2f, 0f), new Vector3(size.x / 2f, size.y / 2f, 0f));
		Gizmos.DrawLine(new Vector3((0f - size.x) / 2f, size.y / 2f, 0f), new Vector3(size.x / 2f, (0f - size.y) / 2f, 0f));
	}
}
