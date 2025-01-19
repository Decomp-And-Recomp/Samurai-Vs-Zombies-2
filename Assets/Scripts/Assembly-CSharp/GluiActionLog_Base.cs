using UnityEngine;

public abstract class GluiActionLog_Base : GluiLog_Base
{
	public static GluiActionLog_Base Instance;

	public void Awake()
	{
		Instance = this;
	}

	public abstract void Add_Handled(string action, GameObject sender, string senderName, IGluiActionHandler handler);

	public abstract void Add_Unhandled(string action, GameObject sender, string senderName);

	public static void AddActionLog_Handled(string action, GameObject sender, string senderName, GameObject handler)
	{
		AddActionLog_Handled(action, sender, senderName, (IGluiActionHandler)handler.GetComponent(typeof(IGluiActionHandler)));
	}

	public static void AddActionLog_Handled(string action, GameObject sender, string senderName, IGluiActionHandler handler)
	{
		if (Instance != null)
		{
			Instance.Add_Handled(action, sender, senderName, handler);
		}
	}

	public static void AddActionLog_Unhandled(string action, GameObject sender, string senderName)
	{
		if (Instance != null)
		{
			Instance.Add_Unhandled(action, sender, senderName);
		}
	}
}
