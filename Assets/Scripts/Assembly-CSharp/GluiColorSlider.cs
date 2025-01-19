using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Color Slider")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GluiColorSlider : GluiSlider
{
	public string colorTableName = string.Format("Udaman table name goes here");

	private ColorTable colorTable = new ColorTable();

	public Color ColorValue
	{
		get
		{
			return colorTable.sectionalLerp(base.Value);
		}
	}

	public bool ColorTableIsInitialized
	{
		get
		{
			return colorTable.IsInitialized;
		}
	}

	private new void Start()
	{
		base.Start();
		if ((bool)previewWindow)
		{
			previewWindow.GetComponent<Renderer>().enabled = false;
			Renderer[] componentsInChildren = previewWindow.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = false;
			}
			Light[] componentsInChildren2 = previewWindow.GetComponentsInChildren<Light>();
			Light[] array2 = componentsInChildren2;
			foreach (Light light in array2)
			{
				light.enabled = false;
			}
			previewWindow.transform.parent = base.transform;
			firstInputRecieved = true;
		}
	}

	public void setColorTable(string tableName)
	{
		InitWidget();
		colorTableName = tableName;
		UpdateColorBandMesh();
	}

	public Color? SetColorFromColorKeyIndex(int iColorIndex)
	{
		float? num = colorTable.ColorKeyToT(iColorIndex);
		if (num.HasValue)
		{
			setValue(num.Value);
			return ColorValue;
		}
		return null;
	}

	public int GetNumberOfColorKeys()
	{
		return colorTable.Count;
	}

	protected void UpdateColorBandMesh()
	{
		if (string.IsNullOrEmpty(colorTableName))
		{
			return;
		}
		colorTable.initializeColorTable(colorTableName);
		if (colorTable.Count < 2)
		{
			return;
		}
		int num = colorTable.Count - 1;
		float num2 = base.Size.x / 2f;
		float num3 = base.Size.y / 2f;
		float num4 = base.Size.x / (float)num;
		int[] array = new int[num * 6];
		Vector3[] array2 = new Vector3[colorTable.Count * 2];
		Vector2[] array3 = new Vector2[colorTable.Count * 2];
		Color[] array4 = new Color[colorTable.Count * 2];
		for (int i = 0; i < colorTable.Count; i++)
		{
			if (i < num)
			{
				int num5 = i * 2;
				int num6 = i * 6;
				array[num6] = num5;
				array[num6 + 1] = num5 + 1;
				array[num6 + 2] = num5 + 2;
				array[num6 + 3] = num5 + 1;
				array[num6 + 4] = num5 + 2;
				array[num6 + 5] = num5 + 3;
			}
			int num7 = i * 2;
			array2[num7] = new Vector3(0f - num2 + (float)i * num4, 0f - num3, 0f);
			array2[num7 + 1] = new Vector3(0f - num2 + (float)i * num4, num3, 0f);
			int num8 = i * 2;
			array3[num8] = Vector2.zero;
			array3[num8 + 1] = Vector2.zero;
			int num9 = i * 2;
			array4[num9] = colorTable.GetColorKey(i);
			array4[num9 + 1] = colorTable.GetColorKey(i);
		}
		UpdateMesh(array2, array3, array, array4);
		base.GetComponent<Renderer>().sharedMaterial.shader = GluiCore.VertColorShader;
	}

	public void PullColorsFromColorTable()
	{
		colorTable.initializeColorTable(colorTableName);
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
			ignoringCurrentInput = false;
			base.Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			if ((bool)previewWindow)
			{
				previewWindow.GetComponent<Renderer>().enabled = true;
				Renderer[] componentsInChildren7 = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array7 = componentsInChildren7;
				foreach (Renderer renderer4 in array7)
				{
					renderer4.enabled = true;
				}
				Light[] componentsInChildren8 = previewWindow.GetComponentsInChildren<Light>();
				Light[] array8 = componentsInChildren8;
				foreach (Light light4 in array8)
				{
					light4.enabled = true;
				}
				previewWindow.transform.localPosition = new Vector3(cursorSprite.transform.localPosition.x, previewWindow.transform.localPosition.y, previewWindow.transform.localPosition.z);
				previewWindow.GetComponent<Renderer>().material.SetColor("_Color", ColorValue);
			}
			GluiActionSender.SendGluiAction(actionOnPress, base.gameObject, ColorValue);
			break;
		case InputEvent.EEventType.OnCursorUp:
			ignoringCurrentInput = true;
			base.Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			previousValue = base.Value;
			if ((bool)previewWindow)
			{
				previewWindow.GetComponent<Renderer>().enabled = false;
				Renderer[] componentsInChildren3 = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array3 = componentsInChildren3;
				foreach (Renderer renderer2 in array3)
				{
					renderer2.enabled = false;
				}
				Light[] componentsInChildren4 = previewWindow.GetComponentsInChildren<Light>();
				Light[] array4 = componentsInChildren4;
				foreach (Light light2 in array4)
				{
					light2.enabled = false;
				}
			}
			GluiSendMessageSupport.CallHandler(base.Handler, onRelease, onReleaseArg);
			GluiActionSender.SendGluiAction(actionOnRelease, base.gameObject, ColorValue);
			break;
		case InputEvent.EEventType.OnCursorMove:
			if (ignoringCurrentInput)
			{
				break;
			}
			base.Value = CalculateValue(inputCrawl.inputEvent.Position.x);
			if ((bool)previewWindow)
			{
				previewWindow.GetComponent<Renderer>().enabled = true;
				Renderer[] componentsInChildren5 = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array5 = componentsInChildren5;
				foreach (Renderer renderer3 in array5)
				{
					renderer3.enabled = true;
				}
				Light[] componentsInChildren6 = previewWindow.GetComponentsInChildren<Light>();
				Light[] array6 = componentsInChildren6;
				foreach (Light light3 in array6)
				{
					light3.enabled = true;
				}
				previewWindow.transform.localPosition = new Vector3(cursorSprite.transform.localPosition.x, previewWindow.transform.localPosition.y, previewWindow.transform.localPosition.z);
				previewWindow.GetComponent<Renderer>().material.SetColor("_Color", ColorValue);
			}
			GluiSendMessageSupport.CallHandler(base.Handler, onChange, onChangeArg);
			GluiActionSender.SendGluiAction(actionOnChange, base.gameObject, ColorValue);
			break;
		case InputEvent.EEventType.OnCursorExit:
			ignoringCurrentInput = true;
			if ((bool)previewWindow)
			{
				previewWindow.GetComponent<Renderer>().enabled = false;
				Renderer[] componentsInChildren = previewWindow.GetComponentsInChildren<Renderer>();
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = false;
				}
				Light[] componentsInChildren2 = previewWindow.GetComponentsInChildren<Light>();
				Light[] array2 = componentsInChildren2;
				foreach (Light light in array2)
				{
					light.enabled = false;
				}
			}
			break;
		}
		response = InputRouter.InputResponse.Handled;
	}

	protected override void OnResize()
	{
	}

	public override void sendOnChangeAction()
	{
		GluiActionSender.SendGluiAction(actionOnChange, base.gameObject, ColorValue);
	}

	protected override void OnCreate()
	{
		if (Application.isPlaying)
		{
			UpdateColorBandMesh();
		}
	}
}
