using UnityEngine;

public interface IGluiActionHandler
{
	bool HandleAction(string action, GameObject sender, object data);
}
