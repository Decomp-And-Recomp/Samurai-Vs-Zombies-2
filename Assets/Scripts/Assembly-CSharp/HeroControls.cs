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
    
    private KeyCode mCurrentKey = KeyCode.A;

    public HeroControls()
    {
        SingletonMonoBehaviour<InputManager>.Instance.InputEventUnhandled += InputEventHandler;
        kMoveLeftTouchArea = new Rect(0f, topMargin, Screen.width / 2, Screen.height - topMargin - bottomMargin);
        kMoveRightTouchArea = new Rect(Screen.width / 2, topMargin, Screen.width, Screen.height - topMargin - bottomMargin);
    }

    private bool IsValidTouch(Vector2 pt)
    {
        return kMoveLeftTouchArea.Contains(pt) || kMoveRightTouchArea.Contains(pt);
    }
    
    private void UpdatePCControls()
	{
		//flips the else statements depending on the key you last pressed
		//this is so if you're holding d and you press a, you'll go backwards. and vice versa. instead of d just taking priority over a.
		//I did this in brawlers as well. it makes the controls feel a lot less clunky.
		if (mCurrentKey == KeyCode.A)
		{
			if (Input.GetKey(KeyCode.D))
			{
				onMoveRight();

				if (!Input.GetKey(mCurrentKey))
				{
					mCurrentKey = KeyCode.D;
				}
			}
			else if (Input.GetKey(KeyCode.A))
			{
				onMoveLeft();
				
				if (!Input.GetKey(mCurrentKey))
				{
					mCurrentKey = KeyCode.A;
				}
			}
			else
			{
				onDontMove();
			}
		}
		else if (mCurrentKey == KeyCode.D)
		{
			if (Input.GetKey(KeyCode.A))
			{
				onMoveLeft();
				
				if (!Input.GetKey(mCurrentKey))
				{
					mCurrentKey = KeyCode.A;
				}
			}
			else if (Input.GetKey(KeyCode.D))
			{
				onMoveRight();

				if (!Input.GetKey(mCurrentKey))
				{
					mCurrentKey = KeyCode.D;
				}
			}
			else
			{
				onDontMove();
			}
		}
	}

    private void UpdateMobileControls()
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

    public void Update()
    {
        if (!Application.isMobilePlatform)
        {
            UpdatePCControls();
        }
        else
        {
            UpdateMobileControls();
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
        if (SingletonMonoBehaviour<InputManager>.Exists)
        {
            UnityThreadHelper.CallOnMainThread(() =>
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
