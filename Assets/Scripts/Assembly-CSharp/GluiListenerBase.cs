using UnityEngine;

public abstract class GluiListenerBase : MonoBehaviour
{
	protected void CallMethod(string method, GameObject sender, object data)
	{
		GluiListenerMethodData gluiListenerMethodData = default(GluiListenerMethodData);
		gluiListenerMethodData.sender = sender;
		gluiListenerMethodData.data = data;
		SendMessage(method, gluiListenerMethodData);
	}

	protected virtual void ActionDone()
	{
	}
}
