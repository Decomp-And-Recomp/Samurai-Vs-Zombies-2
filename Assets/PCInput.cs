using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCInput : MonoBehaviour
{
	private static Touch[] fakeTouches = new Touch[0];

	public static Touch GetTouch(int index)
	{
		if (Application.isMobilePlatform)
		{
			return Input.GetTouch(index);
		}

		return fakeTouches[index];
	}

	public static int touchCount
	{
		get
		{
			if (Application.isMobilePlatform)
			{
				return Input.touchCount;
			}

			return fakeTouches.Length;
		}
	}

	public static Touch[] touches
	{
		get
		{
			if (Application.isMobilePlatform)
			{
				return Input.touches;
			}

			return fakeTouches;
		}
	}

	private static Vector3 lastPosition;

	private static float deltaTime;

	private void Start()
	{
		Application.targetFrameRate = 240;

		if (Application.isMobilePlatform)
		{
			return;
		}

		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if (Application.isMobilePlatform)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.F1))
		{
			Screen.lockCursor = !Screen.lockCursor;
		}

		if (!Screen.lockCursor)
		{
			if (Input.GetMouseButtonDown(0))
			{
				lastPosition = Input.mousePosition;

				fakeTouches = new Touch[1]
				{
					new Touch()
					{
						position = Input.mousePosition,
						phase = TouchPhase.Began
					}
				};

				return;
			}

			if (Input.GetMouseButton(0))
			{
				deltaTime += Time.deltaTime;

				fakeTouches = new Touch[1]
				{
					new Touch()
					{
						position = Input.mousePosition,
						deltaPosition = Input.mousePosition - lastPosition,
						deltaTime = deltaTime,
						phase = TouchPhase.Moved
					}
				};

				lastPosition = Input.mousePosition;
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				fakeTouches = new Touch[1]
				{
					new Touch()
					{
						position = Input.mousePosition,
						deltaPosition = Input.mousePosition - lastPosition,
						deltaTime = deltaTime,
						phase = TouchPhase.Ended
					}
				};

				lastPosition = Vector2.zero;
				deltaTime = 0f;

				return;
			}
		}

		fakeTouches = new Touch[0];

		lastPosition = Vector2.zero;
		deltaTime = 0f;
	}
}
