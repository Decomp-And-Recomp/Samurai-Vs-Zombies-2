using UnityEngine;

[AddComponentMenu("Glui Action/Global Handler")]
public abstract class GluiGlobalActionHandler : MonoBehaviour, IGluiActionHandler
{
	public GameObject[] rebroadcastUnhandledToObjectsFirst;

	public string[] rebroadcastUnhandledToObjectsByNameSecond;

	[HideInInspector]
	public GameObject lastActionHandler;

	public GameObject[] filterAllActionsThroughFilterObjectsFirst;

	public string[] filterAllActionsThroughFilterByNameSecond;

	private static GluiGlobalActionHandler gGluiGlobalActionHandler;

	public static GluiGlobalActionHandler Instance
	{
		get
		{
			return gGluiGlobalActionHandler;
		}
		set
		{
			if (gGluiGlobalActionHandler == null)
			{
				gGluiGlobalActionHandler = value;
			}
		}
	}

	public GluiGlobalActionHandler()
	{
	}

	public virtual void Awake()
	{
		Instance = this;
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		bool flag = HandleGlobalAction(action, sender, data);
		if (!flag)
		{
			flag = Rebroadcast_Internal(action, sender, data);
		}
		else
		{
			lastActionHandler = base.gameObject;
		}
		return flag;
	}

	public abstract bool HandleGlobalAction(string action, GameObject sender, object data);

	private bool Rebroadcast_Internal(string action, GameObject sender, object data)
	{
		bool flag = SendGluiMessage(action, sender, data, rebroadcastUnhandledToObjectsFirst);
		if (!flag)
		{
			flag = SendGluiMessage(action, sender, data, rebroadcastUnhandledToObjectsByNameSecond);
		}
		return flag;
	}

	protected bool Rebroadcast(string action)
	{
		return Rebroadcast(action, base.gameObject, null);
	}

	protected bool Rebroadcast(string action, GameObject sender, object data)
	{
		bool flag = HandleAction(action, sender, data);
		if (flag)
		{
			GluiActionLog_Base.AddActionLog_Handled(action, sender, sender.name, this);
			return flag;
		}
		flag = Rebroadcast_Internal(action, sender, data);
		if (flag)
		{
			GluiActionLog_Base.AddActionLog_Handled(action, sender, sender.name, lastActionHandler);
		}
		else
		{
			GluiActionLog_Base.AddActionLog_Unhandled(action, sender, sender.name);
		}
		return flag;
	}

	private bool SendGluiMessage(string action, GameObject sender, object data, GameObject[] listOfListeners)
	{
		foreach (GameObject gameObject in listOfListeners)
		{
			if (!(gameObject != null))
			{
				continue;
			}
			IGluiActionHandler[] array = ObjectUtils.FindComponents<IGluiActionHandler>(gameObject);
			if (array.Length <= 0)
			{
				continue;
			}
			IGluiActionHandler[] array2 = array;
			foreach (IGluiActionHandler gluiActionHandler in array2)
			{
				if (gluiActionHandler.HandleAction(action, sender, data))
				{
					lastActionHandler = gameObject;
					return true;
				}
			}
		}
		return false;
	}

	private bool SendGluiMessage(string action, GameObject sender, object data, string[] listOfListenersByName)
	{
		foreach (string text in listOfListenersByName)
		{
			GameObject gameObject = GameObject.Find(text);
			if (gameObject != null)
			{
				IGluiActionHandler gluiActionHandler = (IGluiActionHandler)gameObject.GetComponent(typeof(IGluiActionHandler));
				if (gluiActionHandler != null && gluiActionHandler.HandleAction(action, sender, data))
				{
					lastActionHandler = gameObject;
					return true;
				}
			}
		}
		return false;
	}

	public bool Filter(string action, GameObject sender, object data)
	{
		bool flag = SendGluiMessage(action, sender, data, filterAllActionsThroughFilterObjectsFirst);
		if (!flag)
		{
			flag = SendGluiMessage(action, sender, data, filterAllActionsThroughFilterByNameSecond);
		}
		return flag;
	}
}
