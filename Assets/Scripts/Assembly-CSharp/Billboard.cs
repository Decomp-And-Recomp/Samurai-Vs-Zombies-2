using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Transform myTransform;

	private Transform cameraTransform;

	private void Start()
	{
		myTransform = base.transform;
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			cameraTransform = WeakGlobalMonoBehavior<InGameImpl>.Instance.gameCamera.transform;
		}
	}

	private void LateUpdate()
	{
		if (cameraTransform == null && WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			cameraTransform = WeakGlobalMonoBehavior<InGameImpl>.Instance.gameCamera.transform;
		}
		myTransform.LookAt(myTransform.position + cameraTransform.rotation * new Vector3(0f, 0f, 1f));
	}
}
