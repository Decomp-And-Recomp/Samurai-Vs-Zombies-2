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

    public void Update()
    {
        bool usedInput = false;

        bool aPressed = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool dPressed = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (mCurrentKey == KeyCode.A)
        {
            if (dPressed)
            {
                if (onMoveRight != null) onMoveRight();
                usedInput = true;

                if (!aPressed)
                    mCurrentKey = KeyCode.D;
            }
            else if (aPressed)
            {
                if (onMoveLeft != null) onMoveLeft();
                usedInput = true;

                if (!dPressed)
                    mCurrentKey = KeyCode.A;
            }
            else
            {
                if (onDontMove != null) onDontMove();
                usedInput = true;
            }
        }
        else if (mCurrentKey == KeyCode.D)
        {
            if (aPressed)
            {
                if (onMoveLeft != null) onMoveLeft();
                usedInput = true;

                if (!dPressed)
                    mCurrentKey = KeyCode.A;
            }
            else if (dPressed)
            {
                if (onMoveRight != null) onMoveRight();
                usedInput = true;

                if (!aPressed)
                    mCurrentKey = KeyCode.D;
            }
            else
            {
                if (onDontMove != null) onDontMove();
                usedInput = true;
            }
        }

        HandInfo hand = SingletonMonoBehaviour<InputManager>.Instance.Hand;
        int num = 0;
        while (num < activeInputs.Count)
        {
            int cursorIndex = activeInputs[num];
            if (!hand.fingers[cursorIndex].IsFingerDown)
            {
                activeInputs.RemoveAt(num);
                continue;
            }

            if (!usedInput && num == 0)
            {
                Vector2 cursorPosition = hand.fingers[cursorIndex].CursorPosition;
                if (kMoveLeftTouchArea.Contains(cursorPosition))
                {
                    if (onMoveLeft != null) onMoveLeft();
                    usedInput = true;
                }
                else if (kMoveRightTouchArea.Contains(cursorPosition))
                {
                    if (onMoveRight != null) onMoveRight();
                    usedInput = true;
                }
            }
            num++;
        }

        if (!usedInput)
        {
            if (onDontMove != null) onDontMove();
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
            return;

        if (isDisposing)
        {

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
