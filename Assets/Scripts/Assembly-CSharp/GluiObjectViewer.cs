using System;
using UnityEngine;

[AddComponentMenu("Glui/ObjectViewer")]
[ExecuteInEditMode]
public class GluiObjectViewer : GluiRenderTarget
{
	public static readonly Vector3 BaseObjectLocation = new Vector3(0f, 0f, -1000f);

	public static readonly Vector3 ObjectLocationDelta = new Vector3(100f, 0f, 0f);

	[SerializeField]
	private GameObject focusPrefab;

	[SerializeField]
	private float focusDistance = 10f;

	[SerializeField]
	private float focusFieldOfView = 60f;

	[SerializeField]
	private float zoomRate = 50f;

	[SerializeField]
	private float cropFactor = 0.5f;

	private GameObject focusObject;

	private GameObject focusCamera;

	public GameObject Focus
	{
		get
		{
			return focusObject;
		}
		set
		{
			if (focusObject != value)
			{
				focusObject = value;
				Reframe();
			}
		}
	}

	public GameObject FocusPrefab
	{
		get
		{
			return focusPrefab;
		}
		set
		{
			focusPrefab = value;
		}
	}

	public float FocusDistance
	{
		get
		{
			return focusDistance;
		}
		set
		{
			if (value != focusDistance)
			{
				focusDistance = value;
				Reframe();
			}
		}
	}

	public float FocusFieldOfView
	{
		get
		{
			return focusFieldOfView;
		}
		set
		{
			if (value != focusFieldOfView)
			{
				focusFieldOfView = value;
				Reframe();
			}
		}
	}

	public float ZoomRate
	{
		get
		{
			return zoomRate;
		}
		set
		{
			zoomRate = value;
		}
	}

	public float CropFactor
	{
		get
		{
			return cropFactor;
		}
		set
		{
			cropFactor = value;
		}
	}

	protected bool IsCreated
	{
		get
		{
			return focusCamera != null;
		}
	}

	private Vector3 FindLocation()
	{
		Vector3 baseObjectLocation = BaseObjectLocation;
		GluiObjectViewer[] array = UnityEngine.Object.FindObjectsOfType(typeof(GluiObjectViewer)) as GluiObjectViewer[];
		GluiObjectViewer[] array2 = array;
		foreach (GluiObjectViewer gluiObjectViewer in array2)
		{
			if (!(gluiObjectViewer == this) && gluiObjectViewer.IsCreated)
			{
				baseObjectLocation += ObjectLocationDelta;
			}
		}
		return baseObjectLocation;
	}

	private void Reframe()
	{
		if (focusObject != null)
		{
			focusObject.transform.parent = focusCamera.transform;
			focusObject.transform.localPosition = new Vector3(0f, 0f, focusDistance);
		}
		if (focusCamera != null)
		{
			Camera component = focusCamera.GetComponent<Camera>();
			if (component != null)
			{
				component.fieldOfView = focusFieldOfView;
				base.SourceCamera = component;
			}
		}
	}

	protected override void OnCreate()
	{
		if (Application.isPlaying)
		{
			focusCamera = new GameObject(base.name + "_focus_camera");
			focusCamera.transform.position = FindLocation();
			focusCamera.SetActive(false);
			Camera camera = focusCamera.AddComponent<Camera>();
			if (focusPrefab != null)
			{
				Focus = UnityEngine.Object.Instantiate(focusPrefab) as GameObject;
			}
			camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			camera.clearFlags = CameraClearFlags.Color;
			camera.cullingMask = 1 << Focus.layer;
			Reframe();
		}
		base.OnCreate();
	}

	public virtual void Update()
	{
		if (!(focusObject == null) && !(focusCamera == null) && base.Enabled && base.Visible)
		{
			UpdateZoom();
		}
	}

	private void UpdateZoom()
	{
		if (zoomRate <= 0f)
		{
			return;
		}
		Camera component = focusCamera.GetComponent<Camera>();
		if (component == null)
		{
			return;
		}
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Collider[] componentsInChildren = focusObject.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			Bounds bounds = collider.bounds;
			Vector3 vector3 = component.WorldToViewportPoint(bounds.min);
			Vector3 vector4 = component.WorldToViewportPoint(bounds.max);
			if (vector3.x < vector.x)
			{
				vector.x = vector3.x;
			}
			if (vector3.y < vector.y)
			{
				vector.y = vector3.y;
			}
			if (vector4.x > vector2.x)
			{
				vector2.x = vector4.x;
			}
			if (vector4.y > vector2.y)
			{
				vector2.y = vector4.y;
			}
		}
		float num = Mathf.Sin(component.fieldOfView * ((float)Math.PI / 180f) * cropFactor);
		float num2 = vector2.x - vector.x - num;
		float num3 = vector2.y - vector.y - num;
		float num4 = num2;
		if (Mathf.Abs(num3) > Mathf.Abs(num2))
		{
			num4 = num3;
		}
		float x = 1f - Mathf.Abs(vector2.x - vector.x);
		float y = 1f - Mathf.Abs(vector2.y - vector.y);
		float magnitude = new Vector3(x, y, 0f).magnitude;
		FocusDistance += num4 * Time.deltaTime * zoomRate * magnitude;
	}
}
