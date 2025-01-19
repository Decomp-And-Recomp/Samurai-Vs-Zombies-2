using UnityEngine;

public class InputManagerTestScript : MonoBehaviour
{
	private Camera camera;

	private GameObject objItem;

	private float fAutoRotateWaitCurrentTime;

	private float fAutoRotateWaitMaxTime;

	private float fRotationDelta;

	private Vector3 vecMaxZoomOut;

	private Vector3 vecMaxZoomIn;

	private float fZoomT;

	private float fZoomMinT;

	private float fZoomMaxT;

	private void Awake()
	{
		camera = null;
		objItem = null;
	}

	private void Start()
	{
		GameObject gameObject = GameObject.Find("ItemObject");
		Camera mainCamera = Camera.main;
		Setup(gameObject, mainCamera, 0f, 1f, 0f);
	}

	private void Update()
	{
		UpdateItemRotation();
	}

	public void Setup(GameObject objItem, Camera camera, float fT, float fMinT, float fMaxT)
	{
		this.objItem = objItem;
		this.camera = camera;
		if (this.objItem == null)
		{
		}
		if (this.camera == null)
		{
		}
		SetupZoom(fT, fMinT, fMaxT);
		StartAutoRotation(true);
	}

	private void SetupZoom(float fT, float fMinT, float fMaxT)
	{
		if (!(objItem == null) && !(camera == null))
		{
			Vector3 localPosition = camera.transform.localPosition;
			Vector3 forward = camera.transform.forward;
			vecMaxZoomOut = localPosition + -forward * 2f;
			vecMaxZoomIn = localPosition + forward * 2f;
			if (fT == 0f && fMinT == 0f && fMaxT == 0f)
			{
				fZoomMinT = 0.5f;
				fZoomMaxT = 0.3f;
			}
			else
			{
				fZoomMinT = fMinT;
				fZoomMaxT = fMaxT;
			}
			SetZoomT(fT);
		}
	}

	public void IncrementZoomT(float t)
	{
		SetZoomT(fZoomT + t);
	}

	public void SetZoomT(float t)
	{
		fZoomT = t;
		if (fZoomT > fZoomMinT)
		{
			fZoomT = fZoomMinT;
		}
		else if (fZoomT < fZoomMaxT)
		{
			fZoomT = fZoomMaxT;
		}
		if (!(objItem == null) && !(camera == null))
		{
			Vector3 localPosition = (1f - fZoomT) * vecMaxZoomOut + fZoomT * vecMaxZoomIn;
			camera.transform.localPosition = localPosition;
		}
	}

	public void StartAutoRotation(bool bForceStart)
	{
		fRotationDelta = 0f;
		fAutoRotateWaitCurrentTime = 0f;
		fAutoRotateWaitMaxTime = 1.5f;
		if (bForceStart)
		{
			fAutoRotateWaitCurrentTime = fAutoRotateWaitMaxTime + 1f;
		}
	}

	public void StopAutoRotation()
	{
		fAutoRotateWaitCurrentTime = -1f;
	}

	public void UpdateItemRotation()
	{
		if (!(fAutoRotateWaitCurrentTime < 0f))
		{
			if (fAutoRotateWaitCurrentTime >= fAutoRotateWaitMaxTime)
			{
				fRotationDelta = 50f;
				RotateItem(fRotationDelta);
			}
			else
			{
				fAutoRotateWaitCurrentTime += Time.deltaTime;
			}
		}
	}

	public void RotateItem(float fDelta)
	{
		if (!(objItem == null) && !(camera == null))
		{
			Quaternion rotation = objItem.transform.rotation;
			Quaternion quaternion = Quaternion.AngleAxis(fDelta * Time.deltaTime, Vector3.up);
			Quaternion rotation2 = rotation * quaternion;
			objItem.transform.rotation = rotation2;
		}
	}

	public void OnCursorMove(InputEvent e)
	{
		InputGesture_Pinch component = SingletonMonoBehaviour<InputManager>.Instance.gameObject.GetComponent<InputGesture_Pinch>();
		if (!component.IsPinching)
		{
			StopAutoRotation();
		}
		if (!(objItem == null) && !component.IsPinching)
		{
			Vector2 cursorDeltaMovement = SingletonMonoBehaviour<InputManager>.Instance.Hand.fingers[e.CursorIndex].CursorDeltaMovement;
			RotateItem((0f - cursorDeltaMovement.x) * 20f);
			IncrementZoomT(cursorDeltaMovement.y * 0.001f);
		}
	}

	public void OnCursorUp(InputEvent e)
	{
		StartAutoRotation(false);
	}

	public void OnCursorExit(InputEvent e)
	{
		StartAutoRotation(false);
	}
}
