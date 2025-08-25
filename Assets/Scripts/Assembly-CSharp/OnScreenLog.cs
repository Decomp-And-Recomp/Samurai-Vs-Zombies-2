using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class OnScreenLog : MonoBehaviour
{
	private sealed class OnScreenLogHandler : Logging.HandlerBase
	{
		private OnScreenLog log;

		public OnScreenLogHandler(OnScreenLog log)
		{
			this.log = log;
		}

		public override void Emit(ref Logging.LogRecord record)
		{
			string s = Format(ref record);
			log.Log(s);
		}
	}

	[SerializeField]
	private bool visible;

	[SerializeField]
	public Rect windowRect = new Rect(0f, 0.7f, 1f, 0.3f);

	[SerializeField]
	public bool showOpenButton = true;

	[SerializeField]
	public bool showControlButtons = true;

	[SerializeField]
	private Vector2 buttonSize = new Vector2(40f, 40f);

	[SerializeField]
	private bool autoScroll = true;

	[SerializeField]
	private int maxMessageCount = 100;

	[SerializeField]
	private string attachToLogger = string.Empty;

	[SerializeField]
	private bool dontDestroy;

	private Logging.ILogger loggerAttachedTo;

	private OnScreenLogHandler handler;

	private GUIStyle messageLabelStyle;

	private Vector2 scroll;

	private Queue<string> messages;

	private int id;

	private Func<Vector2> mouseDeltaDelegate;

	private static int uniqueID;

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			bool flag = visible != value;
			visible = value;
			if (flag && this.OnVisibilityChanged != null)
			{
				this.OnVisibilityChanged(this, value);
			}
		}
	}

	public Rect WindowRect
	{
		get
		{
			return windowRect;
		}
		set
		{
			windowRect = value;
		}
	}

	public bool ShowOpenButton
	{
		get
		{
			return showOpenButton;
		}
		set
		{
			showOpenButton = value;
		}
	}

	public bool ShowControlButtons
	{
		get
		{
			return showControlButtons;
		}
		set
		{
			showControlButtons = value;
		}
	}

	public bool AutoScroll
	{
		get
		{
			return autoScroll;
		}
		set
		{
			autoScroll = value;
		}
	}

	public int MaxMessageCount
	{
		get
		{
			return maxMessageCount;
		}
		set
		{
			maxMessageCount = value;
		}
	}

	public Vector2 ButtonSize
	{
		get
		{
			return buttonSize;
		}
		set
		{
			buttonSize = value;
		}
	}

	public bool DontDestroy
	{
		get
		{
			return dontDestroy;
		}
		set
		{
			dontDestroy = value;
		}
	}

	public string AttachToLogger
	{
		get
		{
			return attachToLogger;
		}
		set
		{
			Logging.ILogger logger = Logging.GetLogger(value);
			if (loggerAttachedTo != logger)
			{
				logger.AddHandler(handler);
				if (loggerAttachedTo != null)
				{
					loggerAttachedTo.RemoveHandler(handler);
				}
				loggerAttachedTo = logger;
				attachToLogger = value;
			}
		}
	}

	[method: MethodImpl(32)]
	public event Action<OnScreenLog, bool> OnVisibilityChanged;

	private void Awake()
	{
		uniqueID++;
		id = uniqueID + 3000000;
		messages = new Queue<string>(MaxMessageCount);
		handler = new OnScreenLogHandler(this);
		AttachToLogger = attachToLogger;
		switch (Application.platform)
		{
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			mouseDeltaDelegate = () => Event.current.delta;
			return;
		}
		mouseDeltaDelegate = delegate
		{
			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				return new Vector2(touch.deltaPosition.x, 0f - touch.deltaPosition.y);
			}
			return default(Vector2);
		};
	}

	private void Start()
	{
		if (dontDestroy)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
	}

	private void OnDestroy()
	{
		if (loggerAttachedTo != null)
		{
			loggerAttachedTo.RemoveHandler(handler);
		}
	}

	private void OnGUI()
	{
		if (messageLabelStyle == null)
		{
			InitializeGUI();
		}
		if (Visible)
		{
			GUILayout.Window(id, CalcWindowRect(), WindowProc, "Log");
		}
		else if (ShowOpenButton && GUILayout.Button("Log", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
		{
			Visible = true;
		}
	}

	private void DrawControlButtons()
	{
		GUIStyle window = GUI.skin.window;
		int num = (int)CalcWindowRect().width;
		num -= window.border.right + window.margin.right;
		GUIStyle verticalSlider = GUI.skin.verticalSlider;
		num -= (int)verticalSlider.fixedWidth + verticalSlider.margin.left + verticalSlider.margin.right + verticalSlider.border.left + verticalSlider.border.right;
		num -= (int)buttonSize.x;
		Rect position = new Rect(num, 2f, buttonSize.x, buttonSize.y);
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.red;
		if (GUI.Button(position, "X"))
		{
			Visible = false;
		}
		num -= (int)buttonSize.x + 8;
		position = new Rect(num, 2f, buttonSize.x, buttonSize.y);
		if (AutoScroll)
		{
			GUI.backgroundColor = Color.yellow;
		}
		else
		{
			GUI.backgroundColor = backgroundColor;
		}
		if (GUI.Button(position, "AS"))
		{
			AutoScroll = !AutoScroll;
		}
	}

	private void WindowProc(int id)
	{
		Event current = Event.current;
		if (current.type == EventType.MouseDrag)
		{
			scroll.y -= 5f * mouseDeltaDelegate().y;
			current.Use();
			AutoScroll = false;
		}
		else if (AutoScroll)
		{
			scroll.y = 2.1474836E+09f;
		}
		scroll = GUILayout.BeginScrollView(scroll);
		foreach (string message in messages)
		{
			GUILayout.Label(message, messageLabelStyle);
		}
		GUILayout.EndScrollView();
		if (ShowControlButtons)
		{
			DrawControlButtons();
		}
	}

	private void InitializeGUI()
	{
		messageLabelStyle = new GUIStyle(GUI.skin.label);
		messageLabelStyle.border.top = 0;
		messageLabelStyle.border.bottom = 0;
		messageLabelStyle.margin.top = 0;
		messageLabelStyle.margin.bottom = 0;
		messageLabelStyle.padding.top = 0;
		messageLabelStyle.padding.bottom = 0;
	}

	private void Log(string s)
	{
		if (messages.Count >= MaxMessageCount)
		{
			if (MaxMessageCount <= 1)
			{
				messages.Clear();
			}
			else
			{
				do
				{
					messages.Dequeue();
				}
				while (messages.Count >= MaxMessageCount);
			}
		}
		if (messages.Count < MaxMessageCount)
		{
			messages.Enqueue(s);
		}
	}

	private Rect CalcWindowRect()
	{
		float num = Screen.width;
		float num2 = Screen.height;
		Rect result = new Rect(num * WindowRect.x, num2 * WindowRect.y, num * WindowRect.width, num2 * WindowRect.height);
		return result;
	}
}
