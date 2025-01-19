using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroControls : IDisposable
{
	public delegate void OnPlayerControlCallback();

	private const int topMargin = 120;

	private const int bottomMargin = 120;

	private Rect kMoveLeftTouchArea;

	private Rect kMoveRightTouchArea;

	public OnPlayerControlCallback onMoveLeft;

	public OnPlayerControlCallback onMoveRight;

	public OnPlayerControlCallback onDontMove;

	private bool alreadyDisposed;

	private List<int> activeInputs = new List<int>();

	public HeroControls()
	{
		SingletonMonoBehaviour<InputManager>.Instance.InputEventUnhandled += InputEventHandler;
		kMoveLeftTouchArea = new Rect(0f, 120f, Screen.width / 2, Screen.height - 240);
		kMoveRightTouchArea = new Rect(Screen.width / 2, 120f, Screen.width, Screen.height - 240);
	}

	private bool IsValidTouch(Vector2 pt)
	{
		return kMoveLeftTouchArea.Contains(pt) || kMoveRightTouchArea.Contains(pt);
	}

	public void Update()
	{
		HandInfo hand = SingletonMonoBehaviour<InputManager>.Instance.Hand;
		int num = 0;
		while (num < activeInputs.Count)
		{
			int num2 = activeInputs[num];
			if (!hand.fingers[num2].IsFingerDown)
			{
				activeInputs.RemoveAt(num);
				continue;
			}
			if (num == 0)
			{
				Vector2 cursorPosition = hand.fingers[num2].CursorPosition;
				if (onMoveLeft != null && kMoveLeftTouchArea.Contains(cursorPosition))
				{
					onMoveLeft();
				}
				else if (onMoveRight != null && kMoveRightTouchArea.Contains(cursorPosition))
				{
					onMoveRight();
				}
			}
			num++;
		}
		if (activeInputs.Count == 0 && onDontMove != null)
		{
			onDontMove();
		}
	}

	private void InputEventHandler(InputEvent inputEvent)
	{
		if (inputEvent.EventType == InputEvent.EEventType.OnCursorDown)
		{
			activeInputs.Remove(inputEvent.CursorIndex);
			activeInputs.Add(inputEvent.CursorIndex);
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (alreadyDisposed)
		{
			return;
		}
		if (isDisposing)
		{
		}
		if (SingletonMonoBehaviour<InputManager>.Exists)
		{
			UnityThreadHelper.CallOnMainThread(delegate
			{
				if (SingletonMonoBehaviour<InputManager>.Exists)
				{
					SingletonMonoBehaviour<InputManager>.Instance.InputEventUnhandled -= InputEventHandler;
				}
			});
		}
		alreadyDisposed = true;
	}

	~HeroControls()
	{
		Dispose(false);
	}
}
