using UnityEngine;

public class HUDDailyChallenge : MonoBehaviour
{
	public GluiSprite TimerSprite;

	public GluiText TimerText;

	public static HUDDailyChallenge Create(GameObject parent)
	{
		HUDDailyChallenge result = null;
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.inDailyChallenge)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("UI/Prefabs/HUD/HUD_DailyChallengeAddition")) as GameObject;
			result = ((!(gameObject != null)) ? null : gameObject.GetComponent<HUDDailyChallenge>());
			gameObject.transform.parent = parent.transform;
		}
		return result;
	}

	public void Start()
	{
		if (TimerSprite != null && TimerText != null)
		{
			TimerSprite.gameObject.SetActive(true);
			TimerText.gameObject.SetActive(true);
			UpdateTimer();
		}
	}

	private void UpdateTimer()
	{
		if (TimerText != null && TimerText.gameObject.activeSelf)
		{
			float gameTimer = WeakGlobalMonoBehavior<InGameImpl>.Instance.GameTimer;
			int num = (int)(gameTimer + 0.9f);
			int num2 = num / 60;
			num -= num2 * 60;
			TimerText.Text = string.Format("{0}:{1:D2}", num2, num);
		}
	}

	public void Update()
	{
		UpdateTimer();
	}

	public void Activate()
	{
	}

	public void OnPause(bool pause)
	{
	}
}
