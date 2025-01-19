using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(InputTrace))]
[RequireComponent(typeof(InputGesture_Move))]
[RequireComponent(typeof(InputCrawler))]
[AddComponentMenu("Input/Input Manager")]
public class InputManager : SingletonMonoBehaviour<InputManager>
{
	public delegate void NewInputEventHandler(InputEvent inputEvent, ref List<InputRouter.InputResponse> response);

	public delegate void InputEventHandler(InputEvent inputEvent);

	public Stack<GluiStandardButtonContainer> backStack = new Stack<GluiStandardButtonContainer>();

	public bool tutorialPopupEnabled;

	protected IInputDriver inputDriver;

	private InputCrawler inputCrawler;

	private InputGestureStatus gestureStatus = new InputGestureStatus();

	private bool IsInitialized;

	private bool inputEnabled = true;

	public HandInfo Hand
	{
		get
		{
			return gestureStatus.Hand;
		}
	}

	public IInputDriver InputDevice
	{
		get
		{
			return inputDriver;
		}
	}

	public InputGestureStatus GestureStatus
	{
		get
		{
			return gestureStatus;
		}
	}

	public bool InputEnabled
	{
		get
		{
			return inputEnabled;
		}
		set
		{
			inputEnabled = value;
		}
	}

	[method: MethodImpl(32)]
	public event NewInputEventHandler NewInputEvent;

	[method: MethodImpl(32)]
	public event InputEventHandler InputEventUnhandled;

	protected override void Awake()
	{
		base.Awake();
		Initialize();
	}

	public void Initialize()
	{
		if (!IsInitialized)
		{
			inputCrawler = (InputCrawler)GetComponent(typeof(InputCrawler));
			if (inputCrawler == null)
			{
			}
			inputCrawler.inputTrace = (InputTrace)GetComponent(typeof(InputTrace));
			if (inputCrawler.inputTrace == null)
			{
			}
			CreateDefaultGestures();
			CreateInputDriver();
			GestureStatus.Reset();
			if ((bool)inputDriver)
			{
				inputDriver.Initialize();
			}
			IsInitialized = true;
		}
	}

	private void CreateDefaultGestures()
	{
		InputGesture_Move component = GetComponent<InputGesture_Move>();
		if (component == null)
		{
			base.gameObject.AddComponent(typeof(InputGesture_Move));
		}
	}

	private void CreateInputDriver()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.Android:
			inputDriver = base.gameObject.AddComponent<TouchInputDriver>();
			break;
		default:
			inputDriver = base.gameObject.AddComponent<MouseInputDriver>();
			break;
		}
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && SingletonSpawningMonoBehaviour<GluIap>.Instance.GetPurchaseTransactionStatus() != ICInAppPurchase.TRANSACTION_STATE.ACTIVE && !tutorialPopupEnabled && SingletonSpawningMonoBehaviour<GluIap>.Instance.restoreTransactionStatus == ICInAppPurchase.RESTORE_STATE.NONE)
		{
			while (backStack.Count > 0 && backStack.Peek() == null)
			{
				backStack.Pop();
			}
			if ((bool)GameObject.Find("StartScreen(Clone)") && backStack.Count == 0)
			{
				AJavaTools.UI.ShowExitPrompt(string.Empty, string.Empty);
			}
			else if ((bool)GameObject.Find("Button_Resume"))
			{
				GameObject gameObject = GameObject.Find("Button_Resume");
				GluiSoundSender.SendGluiSound("SOUND_GLUI_BUTTON_PRESS", gameObject.gameObject);
				GluiActionSender.SendGluiAction("BUTTON_PAUSEMENU_RESUME", gameObject.gameObject, null);
			}
			else if ((bool)GameObject.Find("Button_Pause") && backStack.Count == 0)
			{
				GameObject gameObject2 = GameObject.Find("Button_Pause");
				GluiSoundSender.SendGluiSound("SOUND_GLUI_BUTTON_PRESS", gameObject2.gameObject);
				GluiActionSender.SendGluiAction("POPUP_PAUSEMENU", gameObject2.gameObject, null);
			}
			else if (backStack.Count == 0 && (bool)GameObject.Find("Button_Back"))
			{
				GameObject gameObject3 = GameObject.Find("Button_Back");
				GluiSoundSender.SendGluiSound("SOUND_GLUI_BUTTON_PRESS", gameObject3.gameObject);
				GluiActionSender.SendGluiAction("BUTTON_BACK", gameObject3.gameObject, null);
			}
			else if (backStack.Count > 0)
			{
				GluiStandardButtonContainer gluiStandardButtonContainer = backStack.Pop();
				if ((bool)gluiStandardButtonContainer)
				{
					GluiSoundSender.SendGluiSound(gluiStandardButtonContainer.soundOnPress, gluiStandardButtonContainer.gameObject);
					GluiActionSender.SendGluiAction(gluiStandardButtonContainer.onReleaseActions[0], gluiStandardButtonContainer.gameObject, null);
				}
			}
		}
		if (!(inputDriver == null))
		{
			if (inputEnabled)
			{
				UpdateInputProcessing();
				UpdateGestures();
			}
			inputCrawler.Send();
			inputCrawler.SendUnhandledEvents();
		}
	}

	public void UpdateInputProcessing()
	{
		inputDriver.UpdateInputProcessing(ref gestureStatus);
	}

	private void UpdateGestures()
	{
		if (inputDriver.GetTouchCount() == 0)
		{
			gestureStatus.ClearOnNoTouch();
		}
		if (inputDriver.GetTouchCount() == 1)
		{
			gestureStatus.lastSingleTouchPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		}
		Component[] components = GetComponents(typeof(InputGestureBase));
		Component[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			InputGestureBase inputGestureBase = (InputGestureBase)array[i];
			InputEvent inputEvent = inputGestureBase.UpdateGesture(gestureStatus, this);
			if (inputEvent != null)
			{
				inputCrawler.Add(inputEvent);
			}
		}
	}

	public void OnCursorDown(int iFingerIndex)
	{
		FingerInfo fingerInfo = gestureStatus.Hand.fingers[iFingerIndex];
		fingerInfo.OldCursorPosition = fingerInfo.CursorPosition;
		fingerInfo.CursorDeltaMovement = Vector2.zero;
		Vector2 cursorPosition = gestureStatus.Hand.fingers[iFingerIndex].CursorPosition;
		inputCrawler.Add(new InputEvent(InputEvent.EEventType.OnCursorDown, cursorPosition, iFingerIndex));
	}

	public void OnCursorUp(int iFingerIndex)
	{
		FingerInfo fingerInfo = gestureStatus.Hand.fingers[iFingerIndex];
		fingerInfo.OldCursorPosition = fingerInfo.CursorPosition;
		fingerInfo.CursorDeltaMovement = Vector2.zero;
		Vector2 cursorPosition = gestureStatus.Hand.fingers[iFingerIndex].CursorPosition;
		inputCrawler.Add(new InputEvent(InputEvent.EEventType.OnCursorUp, cursorPosition, iFingerIndex));
	}

	public List<InputRouter.InputResponse> OnNewInputEvent(InputEvent inputEvent)
	{
		if (this.NewInputEvent != null)
		{
			List<InputRouter.InputResponse> response = new List<InputRouter.InputResponse>();
			this.NewInputEvent(inputEvent, ref response);
			return response;
		}
		return null;
	}

	public void OnInputEventUnhandled(InputEvent inputEvent)
	{
		if (this.InputEventUnhandled != null)
		{
			this.InputEventUnhandled(inputEvent);
		}
	}

	public void AddExclusiveInputContainer(GameObject container, int exclusiveLayer, InputLayerType layerType)
	{
		inputCrawler.inputExcluder.AddExclusiveInputContainer(container, exclusiveLayer, layerType);
	}

	public void RemoveExclusiveInputContainer(GameObject container, int exclusiveLayer)
	{
		inputCrawler.inputExcluder.RemoveExclusiveInputContainer(container, exclusiveLayer);
	}

	public void SetFocusedObject(GameObject focusObject)
	{
		inputCrawler.inputFocus.SetFocusedObject(focusObject);
	}

	public void ClearFocusedObject(GameObject focusObject)
	{
		inputCrawler.inputFocus.ClearFocusedObject(focusObject);
	}
}
