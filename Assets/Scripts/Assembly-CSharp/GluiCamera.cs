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
}
