using UnityEngine;

public class GluiActionSender
{
	public static void SendGluiAction(string action, GameObject sender, object data)
	{
		if (string.IsNullOrEmpty(action))
		{
			return;
		}
		string senderName = ((!(sender == null)) ? sender.name : "[Destroyed]");
		if (GluiGlobalActionHandler.Instance != null && GluiGlobalActionHandler.Instance.Filter(action, sender, data))
		{
			GluiActionLog_Base.AddActionLog_Handled(action, sender, senderName, GluiGlobalActionHandler.Instance.lastActionHandler);
			return;
		}
		bool flag = false;
		if (sender != null)
		{
			Transform nextObject = sender.transform;
			do
			{
				IGluiActionHandler[] array = FindNextListener(ref nextObject);
				if (array == null)
				{
					continue;
				}
				IGluiActionHandler[] array2 = array;
				foreach (IGluiActionHandler gluiActionHandler in array2)
				{
					if (gluiActionHandler.HandleAction(action, sender, data))
					{
						GluiActionLog_Base.AddActionLog_Handled(action, sender, senderName, gluiActionHandler);
						flag = true;
					}
				}
			}
			while (!flag && nextObject != null);
		}
		if (!flag && !(GluiGlobalActionHandler.Instance == null) && GluiGlobalActionHandler.Instance.HandleAction(action, sender, data))
		{
			GluiActionLog_Base.AddActionLog_Handled(action, sender, senderName, GluiGlobalActionHandler.Instance.lastActionHandler);
			flag = true;
		}
		if (!flag)
		{
			GluiActionLog_Base.AddActionLog_Unhandled(action, sender, senderName);
		}
	}

	private static IGluiActionHandler[] FindNextListener(ref Transform nextObject)
	{
		while (nextObject != null)
		{
			IGluiActionHandler[] array = ObjectUtils.FindComponents<IGluiActionHandler>(nextObject.gameObject);
			nextObject = nextObject.parent;
			if (array != null)
			{
				return array;
			}
		}
		return null;
	}
}
