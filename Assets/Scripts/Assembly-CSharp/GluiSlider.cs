using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Glui/Slider")]
public class GluiSlider : GluiSprite
{
	public string onChange = string.Empty;

	public GluiSendMessageSupport.Argument onChangeArg;

	public string onRelease = string.Empty;

	public GluiSendMessageSupport.Argument onReleaseArg;

	public float previousValue = 1f;

	public float currentValue = 1f;

	public int numSteps = -1;

	public string actionOnPress = string.Empty;

	public string actionOnRelease = string.Empty;

	public string actionOnChange = string.Empty;

	public GameObject cursorSprite;

	public GameObject previewWindow;

	public bool firstInputRecieved;

	protected bool ignoringCurrentInput = true;

	protected bool handlingInput;

	public float Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (value != currentValue)
			{
				setValue(value);
				sendOnChangeAction();
			}
		}
	}

	public int intValue
	{
		get
		{
			return (int)(currentValue * (float)(numSteps - 1));
		}
	}

	protected override ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_Box;
		}
	}

	private new void Start()
	{
		base.Start();
		if (cursorSprite != null)
		{
			cursorSprite.transform.parent = base.transform;
		}
		if (previewWindow != null && previewWindow.GetComponent<Renderer>() != null)
		{
			previewWindow.transform.parent = base.transform;
			previewWindow.GetComponent<Renderer>().enabled = false;
			Renderer[] componentsInChildren = previewWindow.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = false;
			}
		}
		setValue(currentValue);
	}

	protected float CalculateValue(float screenX)
	{
		BoxCollider component = GetComponent<BoxCollider>();
		float num = ApplicationUtilities.DeviceXSize();
		screenX = (screenX / (float)Screen.width - 0.5f) * num;
		float num2 = screenX - (base.transform.position.x + component.extents.x);
		float num3 = num2 / component.size.x + 1f;
		if (numSteps > 1)
		{
			float num4 = numSteps;
			num3 = (float)(int)(num3 * (num4 - 1f) + 0.5f) / (num4 - 1f);
		}
		return num3;
	}

	public void setValue(float v)
	{
		currentValue = v;
		if (cursorSprite != null)
		{
			BoxCollider component = GetComponent<BoxCollider>();
			float num = Mathf.Lerp(0f - component.extents.x, component.extents.x, Value);
			cursorSprite.transform.position = new Vector3(base.transform.position.x + num, cursorSprite.transform.position.y, cursorSprite.transform.position.z);
		}
		if (previewWindow != null)
		{
			previewWindow.transform.localPosition = new Vector3(cursorSprite.transform.localPosition.x, previewWindow.transform.localPosition.y, previewWindow.transform.localPosition.z);
		}
	}

	private float CalculateValueByPercent(float percent)
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			float num = Mathf.Abs(component.size.x);
			float num2 = num * percent;
			float screenX = 0f - component.extents.x + num2;
			return CalculateValue(screenX);
		}
		return 0f;
	}

	public override void HandleInput(InputCrawl inputCrawl, out InputRouter.InputResponse response)
	{
		if (!base.AllowInput)
		{
			if (base.Visible)
			{
				response = InputRouter.InputResponse.Blocked;
			}
			else
			{
				response = InputRouter.InputResponse.Passthrough;
			}
			return;
		}
		switch (inputCrawl.inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
		{
			firstInputRecieved = true;
			ignoringCurrentInput = false;
			handlingInput = true;
			Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			GluiActionSender.SendGluiAction(actionOnPress, base.gameObject, null);
			SingletonMonoBehaviour<InputManager>.Instance.SetFocusedObject(base.gameObject);
			if (!(previewWindow != null) || !(previewWindow.GetComponent<Renderer>() != null))
			{
				break;
			}
			previewWindow.GetComponent<Renderer>().enabled = true;
			Renderer[] componentsInChildren4 = previewWindow.GetComponentsInChildren<Renderer>();
			Renderer[] array4 = componentsInChildren4;
			foreach (Renderer renderer4 in array4)
			{
				renderer4.enabled = true;
			}
			previewWindow.transform.localPosition = new Vector3(cursorSprite.transform.localPosition.x, previewWindow.transform.localPosition.y, previewWindow.transform.localPosition.z);
			GluiText component2 = previewWindow.GetComponent<GluiText>();
			if (component2 != null)
			{
				if (numSteps > 1)
				{
					component2.Text = string.Format("{0}", (int)(Value * (float)(numSteps - 1) + 1f));
				}
				else
				{
					component2.Text = string.Format("{0}", Value);
				}
			}
			break;
		}
		case InputEvent.EEventType.OnCursorUp:
			if (!handlingInput)
			{
				break;
			}
			ignoringCurrentInput = true;
			Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			previousValue = Value;
			GluiSendMessageSupport.CallHandler(base.Handler, onRelease, onReleaseArg);
			GluiActionSender.SendGluiAction(actionOnRelease, base.gameObject, null);
			SingletonMonoBehaviour<InputManager>.Instance.ClearFocusedObject(base.gameObject);
			if (previewWindow != null && previewWindow.GetComponent<Renderer>() != null)
			{
				previewWindow.GetComponent<Renderer>().enabled = false;
				Renderer[] componentsInChildren3 = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array3 = componentsInChildren3;
				foreach (Renderer renderer3 in array3)
				{
					renderer3.enabled = false;
				}
			}
			break;
		case InputEvent.EEventType.OnCursorMove:
		{
			if (ignoringCurrentInput)
			{
				break;
			}
			Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			GluiSendMessageSupport.CallHandler(base.Handler, onChange, onChangeArg);
			if (!(previewWindow != null) || !(previewWindow.GetComponent<Renderer>() != null))
			{
				break;
			}
			previewWindow.GetComponent<Renderer>().enabled = true;
			Renderer[] componentsInChildren2 = previewWindow.GetComponentsInChildren<Renderer>();
			Renderer[] array2 = componentsInChildren2;
			foreach (Renderer renderer2 in array2)
			{
				renderer2.enabled = true;
			}
			previewWindow.transform.localPosition = new Vector3(cursorSprite.transform.localPosition.x, previewWindow.transform.localPosition.y, previewWindow.transform.localPosition.z);
			GluiText component = previewWindow.GetComponent<GluiText>();
			if (component != null)
			{
				if (numSteps > 1)
				{
					component.Text = string.Format("{0}", (int)(Value * (float)(numSteps - 1) + 1f));
				}
				else
				{
					component.Text = string.Format("{0}", Value);
				}
			}
			break;
		}
		case InputEvent.EEventType.OnCursorExit:
			ignoringCurrentInput = true;
			if (previewWindow != null && previewWindow.GetComponent<Renderer>() != null)
			{
				previewWindow.GetComponent<Renderer>().enabled = false;
				Renderer[] componentsInChildren = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = false;
				}
			}
			break;
		}
		response = InputRouter.InputResponse.Handled;
	}

	public virtual void Update()
	{
		if (!firstInputRecieved && previewWindow != null && previewWindow.GetComponent<Renderer>() != null && previewWindow.GetComponent<Renderer>().enabled)
		{
			previewWindow.GetComponent<Renderer>().enabled = false;
			Renderer[] componentsInChildren = previewWindow.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = false;
			}
		}
	}

	public virtual void sendOnChangeAction()
	{
		GluiActionSender.SendGluiAction(actionOnChange, base.gameObject, null);
	}
}
