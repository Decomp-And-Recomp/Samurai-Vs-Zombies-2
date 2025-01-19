using UnityEngine;

public class FXUItoWorld : MonoBehaviour
{
	public float depth;

	private Transform parent;

	private Camera orthoCamera;

	private Camera perspectiveCamera;

	private void Start()
	{
		parent = base.transform.parent;
		orthoCamera = ObjectUtils.FindFirstCamera(parent.gameObject.layer);
		perspectiveCamera = ObjectUtils.FindFirstCamera(base.gameObject.layer);
		base.transform.parent = perspectiveCamera.transform;
	}

	private void Update()
	{
		if (parent == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Vector3 position = orthoCamera.WorldToScreenPoint(new Vector3(parent.position.x, parent.position.y, depth));
		base.transform.position = perspectiveCamera.ScreenToWorldPoint(position);
	}
}
