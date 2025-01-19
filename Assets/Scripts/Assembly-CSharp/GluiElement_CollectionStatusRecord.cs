using UnityEngine;

[AddComponentMenu("Samurai Data/GluiElement CollectionStatusRecord")]
public class GluiElement_CollectionStatusRecord : GluiElement_DataAdaptor<DataAdaptor_CollectionStatusRecord>, IGluiActionHandler
{
	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "PREPARE_TO_ATTACK_SELECTED_COLLECTIBLE":
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("SELECTED_MULTIPLAYER_POTENTIAL_CONFLICT", adaptor.statusRecord);
			break;
		case "RATING_DETAILS":
			if (adaptor.statusRecord.OwnerID == 0)
			{
				DefenseRatingImpl.loadoutToDisplay = adaptor.statusRecord.AIOpponent.loadout;
				GluiActionSender.SendGluiAction("POPUP_DEFENSE_RATING", base.gameObject, null);
				break;
			}
			SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = false;
			Singleton<Profile>.Instance.MultiplayerData.GetOtherUserData(adaptor.statusRecord.OwnerID, delegate(MultiplayerData.MultiplayerUserDataQueryResponse response)
			{
				if (response != null)
				{
					DefenseRatingImpl.loadoutToDisplay = response.loadout;
					GluiActionSender.SendGluiAction("POPUP_DEFENSE_RATING", base.gameObject, null);
				}
				SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
			});
			break;
		}
		return false;
	}
}
