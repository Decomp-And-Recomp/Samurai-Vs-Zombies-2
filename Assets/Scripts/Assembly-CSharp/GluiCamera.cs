using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Camera")]
public class GluiCamera : GluiBase
{
	public bool reference = true;

	protected override void OnReset()
	{
		Camera component = GetComponent<Camera>();
		if (!(component == null))
		{
			component.clearFlags = CameraClearFlags.Depth;
			component.backgroundColor = new Color(0f, 0f, 0f, 0f);
			component.cullingMask = 1 << GluiSettings.MainLayer;
			component.orthographic = true;
			component.near = 0f;
			component.far = 1000f;
			GluiScreen gluiScreen = Object.FindObjectOfType(typeof(GluiScreen)) as GluiScreen;
			if (gluiScreen != null)
			{
				component.orthographicSize = gluiScreen.nativeHeight / 2f;
			}
			else
			{
				component.orthographicSize = 384f;
			}
		}
	}

#if UNITY_STANDALONE || UNITY_EDITOR
    void Update()
	{
		if (Input.mouseScrollDelta == Vector2.zero) return;

		Camera cam = GetComponent<Camera>();

		cam.ScreenPointToRay(Input.mousePosition);

		RaycastHit hitInfo;

		if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo, float.PositiveInfinity)) return;

		GluiBouncyScrollList component = hitInfo.collider.GetComponent<GluiBouncyScrollList>();

		if (!component) return;

		component.Force(Input.mouseScrollDelta.y * (Screen.height / 128f));
	}
#endif
}
