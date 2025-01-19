using UnityEngine;

public class MultiplayerMenuImpl : MonoBehaviour
{
	private void Start()
	{
		if (Singleton<Profile>.Instance.souls < 50)
		{
			ApplicationUtilities.MakePlayHavenContentRequest("consumable_soul_sub_50");
		}
	}
}
