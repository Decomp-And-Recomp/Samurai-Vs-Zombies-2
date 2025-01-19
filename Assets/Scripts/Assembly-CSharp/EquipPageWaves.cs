using UnityEngine;

public class EquipPageWaves : EquipPage, UIHandlerComponent
{
	private const float kTimeBeforeCharacterReload = 0.75f;

	private const float kTimePushedBeforeChange = 1f;

	private const float kStartingWaveTicksPerSecond = 10f;

	private float mWaveTicksPerSecond;

	private float mTimePushed;

	private float mTimePressed;

	private int mSelectedWave = -1;

	private float mTimeSinceChange;

	private GluiStandardButtonContainer mButtonDec;

	private GluiStandardButtonContainer mButtonInc;

	private GluiStandardButtonContainer mButtonDec_10;

	private GluiStandardButtonContainer mButtonInc_10;

	private GluiText mTimesCompleted;

	private GluiText mLabel;

	public EquipPageWaves(GameObject uiParent)
	{
		if (WeakGlobalInstance<EnemiesShowCase>.Instance != null)
		{
			WeakGlobalInstance<EnemiesShowCase>.Instance.highlight = true;
		}
		mButtonDec = uiParent.FindChildComponent<GluiStandardButtonContainer>("Button_WaveDown");
		mButtonInc = uiParent.FindChildComponent<GluiStandardButtonContainer>("Button_WaveUp");
		mButtonDec_10 = uiParent.FindChildComponent<GluiStandardButtonContainer>("Button_WaveDown_10");
		mButtonInc_10 = uiParent.FindChildComponent<GluiStandardButtonContainer>("Button_WaveUp_10");
		mLabel = ((!Singleton<Profile>.Instance.inDailyChallenge) ? uiParent.FindChildComponent<GluiText>("SwapText_Digit") : uiParent.FindChildComponent<GluiText>("SwapText_Title"));
		mTimesCompleted = uiParent.FindChildComponent<GluiText>("Text_TimesCompleted");
		SelectWave(Singleton<Profile>.Instance.waveToPlay);
		SetDefaultHero(true);
		RefreshButtonStates();
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep11_Wave2Select");
	}

	private void CheckForWaveChange(int minChange)
	{
		float num = mTimeSinceChange * mWaveTicksPerSecond;
		if (num >= 1f)
		{
			int num2 = mSelectedWave - Singleton<Profile>.Instance.wave_SinglePlayerGame;
			if (num2 > 0 && num2 < minChange)
			{
				num = minChange;
			}
			int amount = (int)num;
			IncWave(amount);
			mTimeSinceChange = 0.001f;
		}
		else if (num <= -1f)
		{
			int num3 = mSelectedWave - Singleton<Profile>.Instance.wave_SinglePlayerGame;
			if (num3 < 0 && num3 > -minChange)
			{
				num = minChange;
			}
			int num4 = (int)num;
			DecWave(-num4);
			mTimeSinceChange = 0.001f;
		}
		else if ((float)minChange != 0f)
		{
			if (mWaveTicksPerSecond > 0f)
			{
				IncWave(minChange);
				mTimeSinceChange = 0.001f;
			}
			else if (mWaveTicksPerSecond < 0f)
			{
				DecWave(minChange);
				mTimeSinceChange = 0f;
			}
		}
	}

	public void Update(bool updateExpensiveVisuals)
	{
		if (mTimePushed > 0f)
		{
			mTimePushed += Time.deltaTime;
			if (mTimePushed >= 1f && mTimeSinceChange >= 0f)
			{
				mTimeSinceChange += Time.deltaTime;
				CheckForWaveChange(0);
			}
		}
		else
		{
			mTimePushed = Mathf.Max(mTimePushed - Time.deltaTime, -10f);
			if (mTimePushed < -0.75f && Singleton<Profile>.Instance.waveTypeToPlay == WaveManager.WaveType.Wave_SinglePlayer && Singleton<Profile>.Instance.wave_SinglePlayerGame != mSelectedWave)
			{
				Singleton<Profile>.Instance.wave_SinglePlayerGame = mSelectedWave;
				WeakGlobalInstance<EnemiesShowCase>.Instance.Reload(WaveManager.WaveType.Wave_SinglePlayer, Singleton<Profile>.Instance.wave_SinglePlayerGame);
			}
		}
	}

	private void StartPush(float waveIncModifier)
	{
		mWaveTicksPerSecond = waveIncModifier;
		mTimePushed = 0.001f;
		WeakGlobalInstance<EnemiesShowCase>.Instance.Clear();
		SetDefaultHero(false);
	}

	private void StartRelease(int minChange)
	{
		CheckForWaveChange(minChange);
		mWaveTicksPerSecond = 0f;
		mTimePushed = -0.001f;
	}

	public bool OnUIEvent(string eventID)
	{
		switch (eventID)
		{
		case "BUTTON_WAVE_INC_PRESSED":
			StartPush(10f);
			break;
		case "BUTTON_WAVE_DEC_PRESSED":
			StartPush(-10f);
			break;
		case "BUTTON_WAVE_INC_10_PRESSED":
			StartPush(100f);
			break;
		case "BUTTON_WAVE_DEC_10_PRESSED":
			StartPush(-100f);
			break;
		case "BUTTON_WAVE_INC":
			StartRelease(1);
			break;
		case "BUTTON_WAVE_DEC":
			StartRelease(1);
			break;
		case "BUTTON_WAVE_INC_10":
			StartRelease(10);
			break;
		case "BUTTON_WAVE_DEC_10":
			StartRelease(10);
			break;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	public void Save()
	{
	}

	private void SelectWave(int newWave)
	{
		if (newWave != mSelectedWave)
		{
			if (newWave < 1)
			{
				newWave = Singleton<Profile>.Instance.highestUnlockedWave;
			}
			else if (newWave > Singleton<Profile>.Instance.highestUnlockedWave)
			{
				newWave = 1;
			}
			mSelectedWave = newWave;
			if (Singleton<Profile>.Instance.inDailyChallenge)
			{
				mLabel.Text = StringUtils.GetStringFromStringRef(Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.waveDisplayName);
			}
			else
			{
				mLabel.Text = mSelectedWave.ToString();
			}
			if (mTimesCompleted != null)
			{
				mTimesCompleted.Text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings.Menu_MPCollectionTally"), Singleton<Profile>.Instance.GetWaveLevel(mSelectedWave) - 1);
			}
		}
	}

	private void SetDefaultHero(bool onlyIfForced)
	{
		WaveSchema waveData = WaveManager.GetWaveData(Singleton<Profile>.Instance.waveToPlay, Singleton<Profile>.Instance.waveTypeToPlay);
		if (!onlyIfForced || (onlyIfForced && waveData.recommendedHeroIsRequired))
		{
			Singleton<Profile>.Instance.heroID = waveData.recommendedHero.InitializeRecord<HeroSchema>().id;
		}
	}

	private void IncWave(int amount)
	{
		if (!mButtonInc.Locked)
		{
			SelectWave(mSelectedWave + amount);
		}
	}

	private void DecWave(int amount)
	{
		if (!mButtonDec.Locked)
		{
			SelectWave(mSelectedWave - amount);
		}
	}

	private void RefreshButtonStates()
	{
		if (!Singleton<Profile>.Instance.inDailyChallenge)
		{
			if (Singleton<Profile>.Instance.allNormalWavesBeaten)
			{
				mButtonDec.Locked = false;
				mButtonInc.Locked = false;
				mButtonDec_10.Locked = false;
				mButtonInc_10.Locked = false;
				mTimesCompleted.Enabled = true;
				mTimesCompleted.Visible = true;
			}
			else
			{
				mButtonDec.gameObject.SetActive(false);
				mButtonInc.gameObject.SetActive(false);
				mButtonDec_10.gameObject.SetActive(false);
				mButtonInc_10.gameObject.SetActive(false);
				mTimesCompleted.Enabled = false;
				mTimesCompleted.Visible = false;
			}
		}
	}
}
