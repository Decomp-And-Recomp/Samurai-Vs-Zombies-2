using UnityEngine;

[AddComponentMenu("Glui/Anchor")]
public class GluiAnchor : MonoBehaviour
{
	private static Camera staticCamera;

	public Camera renderCamera;

	public Vector2 ScreenPos = default(Vector3);

	public HorizontalAnchor horizontalAnchor;

	public VerticalAnchor verticalAnchor;

	private void Start()
	{
		UpdatePosition();
	}

	public void UpdatePosition()
	{
		Camera camera = null;
		if (renderCamera != null)
		{
			camera = renderCamera;
		}
		else if (staticCamera != null)
		{
			camera = staticCamera;
		}
		else
		{
			camera = ObjectUtils.FindFirstCamera(base.gameObject.layer);
			if (camera != null)
			{
				staticCamera = camera;
			}
		}
		if (camera == null)
		{
			return;
		}
		float x = 0f;
		if (horizontalAnchor != 0)
		{
			float num = camera.pixelRect.width * ScreenPos.x;
			if (horizontalAnchor == HorizontalAnchor.Left)
			{
				x = camera.pixelRect.xMin + num;
			}
			else if (horizontalAnchor == HorizontalAnchor.Center)
			{
				x = camera.pixelRect.xMin + camera.pixelRect.width / 2f + num;
			}
			else if (horizontalAnchor == HorizontalAnchor.Right)
			{
				x = camera.pixelRect.xMax - num;
			}
		}
		float y = 0f;
		if (verticalAnchor != 0)
		{
			float num2 = camera.pixelRect.height * ScreenPos.y;
			if (verticalAnchor == VerticalAnchor.Top)
			{
				y = camera.pixelRect.yMax - num2;
			}
			else if (verticalAnchor == VerticalAnchor.Center)
			{
				y = camera.pixelRect.yMin + camera.pixelRect.height / 2f + num2;
			}
			else if (verticalAnchor == VerticalAnchor.Bottom)
			{
				y = camera.pixelRect.yMin + num2;
			}
		}
		Vector3 position = camera.ScreenToWorldPoint(new Vector3(x, y, 0f));
		if (horizontalAnchor == HorizontalAnchor.None)
		{
			position.x = base.gameObject.transform.position.x;
		}
		if (verticalAnchor == VerticalAnchor.None)
		{
			position.y = base.gameObject.transform.position.y;
		}
		position.z = base.gameObject.transform.position.z;
		base.gameObject.transform.position = position;
	}
}
