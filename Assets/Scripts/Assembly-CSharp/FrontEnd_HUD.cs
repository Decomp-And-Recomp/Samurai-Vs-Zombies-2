using UnityEngine;

public class FrontEnd_HUD : MonoBehaviour
{
	public GluiText PowerRatingText;

	public GameObject DefenseRating;

	public GameObject AttackRating;

	private GluiElement_DecayTimerData timerComponent;

	private GluiText timerText;

	private static FrontEnd_HUD smInstance;

	private static bool smShowingDefenseRating;

	public static FrontEnd_HUD Instance
	{
		get
		{
			return smInstance;
		}
	}

	public static bool ShowingDefenseRating
	{
		get
		{
			return smShowingDefenseRating;
		}
	}

	private void Start()
	{
		timerComponent = GetComponentInChildren<GluiElement_DecayTimerData>();
		timerText = base.gameObject.FindChildComponent<GluiText>("SwapText_SoulsAlert");
		smInstance = this;
		UpdateDefenseMode();
	}

	private void Update()
	{
		bool flag = Singleton<Profile>.Instance.souls == Singleton<Profile>.Instance.GetMaxSouls();
		if (flag == timerComponent.enabled)
		{
			timerComponent.enabled = !flag;
			if (flag)
			{
				timerText.Text = StringUtils.GetStringFromStringRef("LocalizedStrings", "ZombieHead_name");
			}
		}
	}

	public void UpdatePowerRating()
	{
		if (PowerRatingText != null)
		{
			if (!smShowingDefenseRating)
			{
				PowerRatingText.Text = Singleton<Profile>.Instance.playerAttackRating.ToString();
			}
			else
			{
				PowerRatingText.Text = Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.defenseRating.ToString();
			}
		}
	}

	public static void SetDefenseRatingMode(bool defense)
	{
		smShowingDefenseRating = defense;
		if (smInstance != null)
		{
			smInstance.UpdateDefenseMode();
		}
	}

	private void UpdateDefenseMode()
	{
		if (DefenseRating != null)
		{
			DefenseRating.SetActive(smShowingDefenseRating);
		}
		if (AttackRating != null)
		{
			AttackRating.SetActive(!smShowingDefenseRating);
		}
		UpdatePowerRating();
	}
}
