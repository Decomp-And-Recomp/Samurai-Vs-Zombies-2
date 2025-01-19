using UnityEngine;

public class HUDLegendaryStrike : MonoBehaviour
{
	public GluiSprite TimerSprite;

	public GluiText TimerText;

	public static float sDecayRate = 0.1f;

	private static readonly float kLSCycleTime = 3f;

	private GluiFlipbook mIconRef;

	private GluiStandardButtonContainer mButtonRef;

	private ProgressMeterRadial mMeterRef;

	private ProgressMeterRadial mLockedMeterRef;

	private GluiProcess_OscillateTransition mMeterFullEffect;

	private float mChargedPercent;

	private float mLSCycleTimer;

	private int mIndex;

	private int mTutorialIndex;

	private float mTutorialTimer;

	public bool isAvailable
	{
		get
		{
			return mChargedPercent >= 1f && !WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.isInKnockback;
		}
		set
		{
			ResetCooldown();
		}
	}

	public int Index
	{
		get
		{
			return mIndex;
		}
	}

	public static HUDLegendaryStrike Create(GameObject parent)
	{
		HUDLegendaryStrike result = null;
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.inMultiplayerWave)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("UI/Prefabs/HUD/HUD_MultiplayerAddition")) as GameObject;
			result = ((!(gameObject != null)) ? null : gameObject.GetComponent<HUDLegendaryStrike>());
			gameObject.transform.parent = parent.transform;
		}
		return result;
	}

	public void Start()
	{
		mMeterRef = base.gameObject.FindChildComponent<ProgressMeterRadial>("Meter_LegendaryStrike");
		mLockedMeterRef = base.gameObject.FindChildComponent<ProgressMeterRadial>("Meter_LegendaryStrike_LockedIn");
		mButtonRef = base.gameObject.FindChildComponent<GluiStandardButtonContainer>("Button_Strike");
		mIconRef = base.gameObject.FindChildComponent<GluiFlipbook>("Flipbook_Strikes");
		mMeterFullEffect = GetComponentInChildren<GluiProcess_OscillateTransition>();
		if (WeakGlobalMonoBehavior<HUD>.Exists)
		{
			WeakGlobalMonoBehavior<HUD>.Instance.RegisterOnPressEvent(mButtonRef, "LEGEND_STRIKE");
		}
		SetLSAbility(0);
		mLockedMeterRef.Visible = true;
		sDecayRate = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("LegendaryStrikeDamageDecayRate", sDecayRate);
		mTutorialIndex = 0;
		mTutorialTimer = 3f;
		if (TimerSprite != null && TimerText != null)
		{
			TimerSprite.gameObject.SetActive(Singleton<PlayModesManager>.Instance.Attacking);
			TimerText.gameObject.SetActive(Singleton<PlayModesManager>.Instance.Attacking);
			if (Singleton<PlayModesManager>.Instance.Attacking)
			{
				UpdateTimer();
			}
		}
		mMeterFullEffect.ProcessPause(!isAvailable);
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
		float legendaryStrikeChargePercent = Singleton<PlayerWaveEventData>.Instance.LegendaryStrikeChargePercent;
		mChargedPercent = Mathf.MoveTowards(mChargedPercent, legendaryStrikeChargePercent, 0.8f * Time.deltaTime);
		float num = Mathf.Clamp(mChargedPercent, 0f, 1f);
		int num2 = (int)(num * 4f);
		float num3 = (float)num2 / 4f;
		mMeterRef.Value = num;
		mLockedMeterRef.Value = num3;
		Singleton<PlayerWaveEventData>.Instance.LegendaryStrikeChargePercent = Mathf.Max(num3, legendaryStrikeChargePercent - sDecayRate * Time.deltaTime);
		if (isAvailable == mButtonRef.Locked)
		{
			mButtonRef.Locked = !isAvailable;
			mMeterFullEffect.ProcessPause(!isAvailable);
			if (isAvailable)
			{
				mTutorialTimer = 0.01f;
				mTutorialIndex = 1;
			}
		}
		mLSCycleTimer += Time.deltaTime;
		if (mLSCycleTimer >= kLSCycleTime)
		{
			mLSCycleTimer -= kLSCycleTime;
			int num4 = mIndex + 1;
			if (num4 >= 4)
			{
				num4 = 0;
			}
			SetLSAbility(num4);
		}
		if (mTutorialTimer > 0f && !WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			mTutorialTimer -= Time.deltaTime;
			if (mTutorialTimer <= 0f && SingletonMonoBehaviour<TutorialMain>.Exists)
			{
				SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_LS_" + (mTutorialIndex + 1));
			}
		}
		UpdateTimer();
	}

	public void Activate()
	{
		if (isAvailable && !WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			Singleton<PlayerWaveEventData>.Instance.AddLegendaryStrike((ELegendaryStrike)mIndex);
			ResetCooldown();
			Singleton<PlayerWaveEventData>.Instance.ResetAccumulatedDamage();
			Hero.DoLegendaryStrike(PlayerWaveEventData.LegendaryStrikeID[mIndex], 0);
			mTutorialIndex = 2;
			mTutorialTimer = 3f;
		}
	}

	public void OnPause(bool pause)
	{
		mMeterFullEffect.ProcessPause(pause || !isAvailable);
	}

	private void SetLSAbility(int index)
	{
		if (index >= 0 && index < 4)
		{
			AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(PlayerWaveEventData.LegendaryStrikeID[index]);
			if (schema != null)
			{
				mIndex = index;
				mIconRef.frame = index;
			}
		}
	}

	public void ResetCooldown()
	{
		mChargedPercent = 0f;
	}
}
