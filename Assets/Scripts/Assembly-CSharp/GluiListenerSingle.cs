using UnityEngine;

[AddComponentMenu("Glui/Glui Listener Single")]
public abstract class GluiListenerSingle : GluiListenerBase, IGluiActionHandler
{
	public string actionToHandle = string.Empty;

	public bool actionPassthrough;

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == actionToHandle)
		{
			OnTrigger(sender, data);
			if (!actionPassthrough)
			{
				return true;
			}
		}
		return false;
	}

	protected abstract void OnTrigger(GameObject sender, object data);
}
