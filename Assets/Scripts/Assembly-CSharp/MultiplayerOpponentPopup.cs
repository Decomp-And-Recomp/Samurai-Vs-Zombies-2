using UnityEngine;

public class MultiplayerOpponentPopup : MonoBehaviour
{
	public DataAdaptor_MultiplayerOpponent Adaptor;

	private void Start()
	{
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.MultiplayerData != null)
		{
			Adaptor.SetData(Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData);
		}
	}
}
