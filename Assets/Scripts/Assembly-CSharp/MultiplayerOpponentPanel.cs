using UnityEngine;

public class MultiplayerOpponentPanel : MonoBehaviour, IGluiActionHandler
{
	public MultiplayerOpponentsData Adaptor;

	public GluiBouncyScrollList ScrollList;

	public void Start()
	{
		SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("MP_SoulDiscount");
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		bool flag = false;
		if (action == "SHOW_FRIENDS")
		{
			Adaptor.ShowingFriends = true;
			flag = true;
		}
		else if (action == "SHOW_STRANGERS")
		{
			Adaptor.ShowingFriends = false;
			flag = true;
		}
		if (flag)
		{
			if (ScrollList != null)
			{
				ScrollList.Redraw();
			}
			return true;
		}
		return false;
	}
}
