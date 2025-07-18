using System;
using System.Collections;
using UnityEngine;

public class DailyRewardsObserver : MonoBehaviour
{
	private static int kNumRewards = 5;

	private static double kTriggerTreshold = 24.0;

	private static double kTriggerRange = 48.0;

	private IEnumerator Start()
	{
		while (!Singleton<Profile>.Instance.Initialized)
		{
			yield return null;
		}
		ApplicationUtilities instance = SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance;
		instance.OnPause = (Action<bool>)Delegate.Combine(instance.OnPause, new Action<bool>(OnGamePause));
		Check();
	}

	private void OnDestroy()
	{
		if (SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance != null)
		{
			ApplicationUtilities instance = SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance;
			instance.OnPause = (Action<bool>)Delegate.Remove(instance.OnPause, new Action<bool>(OnGamePause));
		}
		if (Singleton<Profile>.Instance != null)
		{
			Singleton<Profile>.Instance.ScheduleNotifications();
		}
	}

	private void Update()
	{
	}

	private void OnGamePause(bool pausing)
	{
		if (!pausing)
		{
			Check();
			NUF.CancelAllLocalNotification();
		}
		else
		{
			Singleton<Profile>.Instance.ScheduleNotifications();
		}
	}

	private void Check()
	{
		if (CheckDailyRewards())
		{
			GluiActionSender.SendGluiAction("POPUP_DAILY_REWARDS", base.gameObject, null);
		}
	}

	public static bool CheckDailyRewards()
	{
		DateTime? lastDailyRewardDate = Singleton<Profile>.Instance.lastDailyRewardDate;
		DateTime now = DateTime.Now; //ApplicationUtilities.Now;
		if (!lastDailyRewardDate.HasValue)
		{
			RestartRewardsTracking();
		}
		else
		{
			double totalHours = (now - lastDailyRewardDate.Value).TotalHours;
			if (!(totalHours >= kTriggerTreshold))
			{
				return false;
			}
			if (totalHours <= kTriggerTreshold + kTriggerRange)
			{
				SetNextReward();
				return true;
			}
			RestartRewardsTracking();
		}
		Singleton<Profile>.Instance.Save();
		return false;
	}

	private static void RestartRewardsTracking()
	{
		Singleton<Profile>.Instance.lastDailyRewardIndex = 0;
		DateTime? lastDailyRewardDate = Singleton<Profile>.Instance.lastDailyRewardDate;
		DateTime now = ApplicationUtilities.Now;
		now = new DateTime(now.Year, now.Month, now.Day);
		if (!lastDailyRewardDate.HasValue || lastDailyRewardDate.Value < now)
		{
			Singleton<Profile>.Instance.lastDailyRewardDate = now;
		}
	}

	private static void SetNextReward()
	{
		int num = Singleton<Profile>.Instance.lastDailyRewardIndex + 1;
		if (num > kNumRewards)
		{
			num = 1;
		}
		Singleton<Profile>.Instance.lastDailyRewardIndex = num;
		DateTime now = DateTime.Now; //= ApplicationUtilities.Now;
		now = new DateTime(now.Year, now.Month, now.Day);
		Singleton<Profile>.Instance.lastDailyRewardDate = now;
		string stringFromStringRef = StringUtils.GetStringFromStringRef("LocalizedStrings", "Notification_DailyReward");
		string stringFromStringRef2 = StringUtils.GetStringFromStringRef("LocalizedStrings", "tapjoy_awarded_gems_button");
		if (!string.IsNullOrEmpty(stringFromStringRef))
		{
			NUF.ScheduleNotification((int)kTriggerTreshold * 60 * 60, stringFromStringRef, stringFromStringRef2, null);
		}
		Singleton<Profile>.Instance.Save();
	}
}
