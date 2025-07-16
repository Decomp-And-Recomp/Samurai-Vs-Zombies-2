using UnityEngine;

public class DebugMain : SingletonSpawningMonoBehaviour<DebugMain>
{
	private DebugStats debugStats = new DebugStats();

	private bool consoleMenuInput;

	private float kConsole_AmountOfScreenToUseVertically = 1f;

	public bool Allow4TouchActivate { get; set; }

	public bool ShowDebugStats
	{
		get
		{
			return debugStats.visible;
		}
		set
		{
			debugStats.visible = value;
		}
	}

	public DebugStats.DebugStatsDisplay DebugStatsDisplay
	{
		get
		{
			return debugStats.display;
		}
		set
		{
			debugStats.display = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(base.gameObject);
		if (GeneralConfig.IsLive)
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		Singleton<GrGui>.Instance.init();
		float scale = DebugScale.Scale();
		InitConsole(scale);
		RegisterConsoleMethods();
		debugStats.Start();
		Allow4TouchActivate = true;
	}

	private void RegisterConsoleMethods()
	{
		Singleton<GrConsole>.Instance.ClearConsoleFunctions();
		ConsoleFunctions.register();
		ConsoleFunctions_Game.register();
	}

	private void InitConsole(float scale)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		GrGui instance2 = Singleton<GrGui>.Instance;
		instance.Scale = scale;
		instance.setPosition(new Vector2(0f, instance2.getVirtualHeight() * (1f - kConsole_AmountOfScreenToUseVertically)));
		instance.setSize(new Vector2(instance2.getVirtualWidth() / scale, instance2.getVirtualHeight() * kConsole_AmountOfScreenToUseVertically / scale));
		instance.Visible = false;
	}

	private void Update()
	{
		UpdateConsoleShowControls();
		Singleton<GrGui>.Instance.update();
	}

	private void UpdateConsoleShowControls()
	{
		if ((PCInput.touchCount == 4 && (Allow4TouchActivate || Singleton<GrConsole>.Instance.Visible)) || (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.LeftAlt)) || (Input.GetKey(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightBracket)) || (AJavaTools.Properties.IsBuildAmazon() && PCInput.touchCount == 2))
		{
			if (!consoleMenuInput)
			{
				Singleton<GrConsole>.Instance.Visible = !Singleton<GrConsole>.Instance.Visible;
				consoleMenuInput = true;
				if (Singleton<GrConsole>.Instance.Visible)
				{
					RegisterConsoleMethods();
				}
			}
		}
		else
		{
			consoleMenuInput = false;
		}
		if (SingletonMonoBehaviour<InputManager>.Exists)
		{
			if (Singleton<GrConsole>.Instance.Visible)
			{
				SingletonMonoBehaviour<InputManager>.Instance.SetFocusedObject(base.gameObject);
			}
			else
			{
				SingletonMonoBehaviour<InputManager>.Instance.ClearFocusedObject(base.gameObject);
			}
		}
	}

	private void OnGUI()
	{
		Singleton<GrGui>.Instance.render();
		Singleton<GrConsole>.Instance.renderConsole();
		debugStats.OnGUI();
	}
}
