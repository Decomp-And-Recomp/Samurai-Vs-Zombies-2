using UnityEngine;

public class ConfirmPurchaseImpl : MonoBehaviour, IGluiActionHandler
{
	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "POPUP_POP":
			if (WeakGlobalMonoBehavior<ResultsMenuImpl>.Exists)
			{
				GluiActionSender.SendGluiAction("POPUP_EMPTY", base.gameObject, null);
				return true;
			}
			break;
		}
		return false;
	}
}
