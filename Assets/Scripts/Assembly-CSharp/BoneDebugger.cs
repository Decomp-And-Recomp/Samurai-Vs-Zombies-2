using UnityEngine;

public class BoneDebugger : MonoBehaviour
{
	private void drawbone(Transform t)
	{
		foreach (Transform item in t)
		{
			float num = 0.05f;
			Vector3 vector = new Vector3(num, 0f, 0f);
			Vector3 vector2 = new Vector3(0f, num, 0f);
			Vector3 vector3 = new Vector3(0f, 0f, num);
			vector = item.rotation * vector;
			vector2 = item.rotation * vector2;
			vector3 = item.rotation * vector3;
			drawbone(item);
		}
	}

	private void Update()
	{
		drawbone(base.transform);
	}
}
