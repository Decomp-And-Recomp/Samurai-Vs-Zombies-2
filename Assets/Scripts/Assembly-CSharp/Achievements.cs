using System.Collections.Generic;
using Glu.Plugins.ASocial;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Achievements : Singleton<Achievements>
{
	public enum AchievementType
	{
		CountAchievement = 0,
		ThresholdAchievement = 1,
		MaskAchievement = 2,
		MetaAchievement = 3,
		DummyCountAchievement = 4
	}

	private Dictionary<string, AchievementTracker> mAchievements = new Dictionary<string, AchievementTracker>();

	private List<string> achievements;

	private bool mSuppressPartialReporting;

	private bool mInitialized;

	private bool mUseInGameAlert = true;

	private bool mNeedToLoadAchievements;

	private List<AchievementTracker> mQueuedAchievements = new List<AchievementTracker>();

	private float mDisplayDelay;

	public void SetAchievementCompletionCount(string achievementId, int count)
	{
		AchievementTracker value;
		if (mAchievements.TryGetValue(achievementId, out value) && value.completedCount != count && value.completedCount < value.achievement.Data.CompletionCount && (value.unityAchievement == null || !value.unityAchievement.completed))
		{
			value.completedCount = count;
			float progress = 100f * (float)count / (float)Mathf.Max(1, value.achievement.Data.CompletionCount);
			ReportProgress(value, progress);
		}
	}

	public void SetAchievementCompletionCount(AchievementSchema achievement, int count)
	{
		SetAchievementCompletionCount(achievement.id, count);
	}

	public void IncrementAchievement(string achievementId, int count)
	{
		AchievementTracker value;
		if (mAchievements.TryGetValue(achievementId, out value) && value.completedCount < value.achievement.Data.CompletionCount && (value.unityAchievement == null || !value.unityAchievement.completed))
		{
			value.completedCount += count;
			float progress = 100f * (float)value.completedCount / (float)Mathf.Max(1, value.achievement.Data.CompletionCount);
			ReportProgress(value, progress);
		}
	}

	public void IncrementAchievement(AchievementSchema achievement, int count)
	{
		IncrementAchievement(achievement.id, count);
	}

	public void CheckThresholdAchievement(string achievementId, int count)
	{
		AchievementTracker value;
		if (mAchievements.TryGetValue(achievementId, out value) && value.completedCount < value.achievement.Data.CompletionCount && (value.unityAchievement == null || !value.unityAchievement.completed) && count >= value.achievement.Data.CompletionCount)
		{
			value.completedCount = value.achievement.Data.CompletionCount;
			ReportProgress(value, 100f);
		}
	}

	public void CheckThresholdAchievement(AchievementSchema achievement, int count)
	{
		CheckThresholdAchievement(achievement.id, count);
	}

	public void DoMaskAchievement(string achievementId, int index)
	{
		AchievementTracker value;
		if (index > 0 && index <= 32 && mAchievements.TryGetValue(achievementId, out value) && value.unityAchievement != null && !value.unityAchievement.completed)
		{
			int num = 1 << index - 1;
			if ((value.completedCount & num) == 0)
			{
				value.completedCount |= num;
				CheckMaskAchievement(value);
			}
		}
	}

	private void CheckMaskAchievement(AchievementTracker tracker)
	{
		int num = 0;
		for (int i = 0; i < tracker.achievement.Data.CompletionCount; i++)
		{
			int num2 = 1 << i;
			if ((tracker.completedCount & num2) != 0)
			{
				num++;
			}
		}
		float progress = (float)num / (float)tracker.achievement.Data.CompletionCount;
		ReportProgress(tracker, progress);
	}

	public void DoMaskAchievement(AchievementSchema achievement, int index)
	{
		DoMaskAchievement(achievement.id, index);
	}

	public void CheckMetaAchievement(string achievementId)
	{
		AchievementTracker value;
		if (!mAchievements.TryGetValue(achievementId, out value) || value.completedCount >= value.achievement.Data.CompletionCount || (value.unityAchievement != null && value.unityAchievement.completed))
		{
			return;
		}
		if (achievements == null)
		{
			achievements = DataBundleRuntime.Instance.GetRecordKeys(typeof(AchievementListSchema), achievementId, false);
		}
		if (achievements == null || achievements.Count <= 0)
		{
			return;
		}
		bool flag = true;
		foreach (string achievement in achievements)
		{
			if (!IsAchievementDone(achievement))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			value.completedCount = value.achievement.Data.CompletionCount;
			ReportProgress(value, 100f);
		}
	}

	public bool IsAchievementDone(string achievementId)
	{
		AchievementTracker value;
		if (mAchievements.TryGetValue(achievementId, out value) && (value.progress >= 100f || (value.unityAchievement != null && value.unityAchievement.completed)))
		{
			return true;
		}
		return false;
	}

	public void OnLogin()
	{
		if (mInitialized)
		{
			Social.LoadAchievements(OnLoadAchievements);
		}
		else
		{
			mNeedToLoadAchievements = true;
		}
		Initialize();
	}

	public void OnLoadAchievements(IAchievement[] achievements)
	{
		foreach (IAchievement achievement in achievements)
		{
			foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
			{
				AchievementTracker value = mAchievement.Value;
				if (value != null && value.achievement != null && value.achievement.Data.GameCenterID == achievement.id)
				{
					value.unityAchievement = achievement;
					if (achievement.percentCompleted >= 100.0 && value.completedCount < value.achievement.Data.CompletionCount)
					{
						value.completedCount = value.achievement.Data.CompletionCount;
						value.progress = 100f;
						Singleton<Profile>.Instance.saveData.SetDictionaryValue("achievements", value.achievement.Data.id, value.completedCount);
					}
					if (value.achievement.Data.achievementType == AchievementType.MetaAchievement)
					{
						CheckMetaAchievement(value.achievement.Data.id);
					}
					else if (value.achievement.Data.achievementType == AchievementType.MaskAchievement)
					{
						CheckMaskAchievement(value);
					}
					else if (!achievement.completed && value.completedCount >= value.achievement.Data.CompletionCount)
					{
						ReportProgress(value, 100f);
					}
					break;
				}
			}
		}
		Social.LoadAchievementDescriptions(OnLoadAchievementDescriptions);
	}

	public void OnLoadAchievementDescriptions(IAchievementDescription[] descriptions)
	{
		foreach (IAchievementDescription achievementDescription in descriptions)
		{
			AchievementTracker achievementTracker = null;
			foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
			{
				AchievementTracker value = mAchievement.Value;
				if (value == null || value.achievement == null || !(value.achievement.Data.GameCenterID == achievementDescription.id))
				{
					continue;
				}
				achievementTracker = value;
				if (value.unityAchievement == null)
				{
					value.unityAchievement = Social.CreateAchievement();
					value.unityAchievement.id = achievementDescription.id;
					value.unityAchievement.percentCompleted = 0.0;
					if (value.completedCount >= value.achievement.Data.CompletionCount)
					{
						ReportProgress(value, 100f);
					}
				}
				value.description = achievementDescription;
				if (value.achievement.Data.achievementType == AchievementType.MetaAchievement)
				{
					CheckMetaAchievement(value.achievement.Data.id);
				}
				break;
			}
			if (achievementTracker == null)
			{
			}
		}
	}

	private void ReportProgress(AchievementTracker achievement, float progress)
	{
		bool flag = achievement.progress >= 100f;
		Singleton<Profile>.Instance.saveData.SetDictionaryValue("achievements", achievement.achievement.Data.id, achievement.completedCount);
		achievement.progress = progress;
		if (!flag && achievement.progress >= 100f && achievement.achievement.Data.achievementType != AchievementType.DummyCountAchievement)
		{
			achievement.shared = false;
			if (PlayerPrefs.GetInt("ach_" + achievement.achievement.Data.GameCenterID, 0) != 1 && mUseInGameAlert)
			{
				mQueuedAchievements.Add(achievement);
				mDisplayDelay = 1f;
			}
		}
		if (achievement.unityAchievement != null)
		{
			if (!mSuppressPartialReporting || progress >= 100f)
			{
				UpdateServerAchievement(achievement);
			}
			else
			{
				achievement.dirty = true;
			}
		}
		if (AJavaTools.Properties.IsBuildAmazon())
		{
			Amazon.UpdateProgress(achievement.achievement.Data.GameCenterID, achievement.progress);
		}
		else if (PlayGameServices.IsSignedIn)
		{
			PlayGameServices.UpdateProgress(achievement.achievement.Data.GameCenterID, achievement.progress);
		}
		else
		{
			achievement.dirty = true;
		}
	}

	private void UpdateServerAchievement(AchievementTracker achievement)
	{
		if (achievement.unityAchievement == null)
		{
			achievement.dirty = false;
		}
		else
		{
			if (achievement.unityAchievement.completed || achievement.unityAchievement.percentCompleted == (double)achievement.progress)
			{
				return;
			}
			achievement.unityAchievement.percentCompleted = achievement.progress;
			if (achievement.progress >= 100f)
			{
				achievement.shared = false;
			}
			achievement.unityAchievement.ReportProgress(delegate(bool result)
			{
				if (result)
				{
					achievement.dirty = false;
				}
			});
		}
	}

	public void Update()
	{
		if (AJavaTools.Properties.IsBuildAmazon() && mQueuedAchievements.Count > 0 && AchievementNotification.Instance == null)
		{
			mDisplayDelay -= Time.deltaTime;
			if (mDisplayDelay <= 0f)
			{
				AchievementTracker achievementTracker = mQueuedAchievements[0];
				mQueuedAchievements.RemoveAt(0);
				AchievementNotification.AchievementDescription = StringUtils.GetStringFromStringRef(achievementTracker.achievement.Data.completedDescription);
				AchievementNotification.AchievementTitle = StringUtils.GetStringFromStringRef(achievementTracker.achievement.Data.displayName);
				AchievementNotification.AchievementIcon = achievementTracker.achievement.Data.Icon;
				GluiActionSender.SendGluiAction("ALERT_EMPTY", null, null);
				GluiDelayedAction.Create("ALERT_ACHIEVEMENT", 1f, null, true);
				mDisplayDelay = 2f;
				PlayerPrefs.SetInt("ach_" + achievementTracker.achievement.Data.GameCenterID, 1);
			}
		}
	}

	public void SuppressPartialReporting(bool suppress)
	{
		mSuppressPartialReporting = suppress;
		if (suppress)
		{
			return;
		}
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			if (mAchievement.Value.dirty)
			{
				UpdateServerAchievement(mAchievement.Value);
			}
		}
	}

	public void ReportOfflineProgress()
	{
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			float progress = 100f * (float)mAchievement.Value.completedCount / (float)Mathf.Max(1, mAchievement.Value.achievement.Data.CompletionCount);
			if (!AJavaTools.Properties.IsBuildAmazon() && PlayGameServices.IsSignedIn)
			{
				PlayGameServices.UpdateProgress(mAchievement.Value.achievement.Data.GameCenterID, progress);
			}
		}
	}

	public void Initialize()
	{
		mInitialized = true;
		mAchievements.Clear();
		Singleton<Profile>.Instance.achievementRating = 0;
		string table = "Achievements";
		foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys<AchievementSchema>(table))
		{
			DataBundleRecordHandle<AchievementSchema> dataBundleRecordHandle = new DataBundleRecordHandle<AchievementSchema>(table, item);
			AchievementTracker achievementTracker = new AchievementTracker();
			achievementTracker.achievement = dataBundleRecordHandle;
			mAchievements[dataBundleRecordHandle.Data.id] = achievementTracker;
			achievementTracker.completedCount = Singleton<Profile>.Instance.saveData.GetDictionaryValue<int>("achievements", dataBundleRecordHandle.Data.id);
			if (achievementTracker.completedCount >= achievementTracker.achievement.Data.CompletionCount)
			{
				achievementTracker.progress = 100f;
				Singleton<Profile>.Instance.achievementRating += dataBundleRecordHandle.Data.AchievementPoints;
			}
		}
		if (mNeedToLoadAchievements)
		{
			Social.LoadAchievements(OnLoadAchievements);
			mNeedToLoadAchievements = false;
		}
	}

	public void LoadFrontEndData()
	{
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			mAchievement.Value.achievement.Load(DataBundleResourceGroup.FrontEnd, false, null);
		}
	}

	public void UnloadData()
	{
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			mAchievement.Value.achievement.Unload();
		}
	}

	public AchievementTracker[] GetAchievements()
	{
		List<AchievementTracker> list = new List<AchievementTracker>();
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			if (mAchievement.Value.achievement.Data.achievementType != AchievementType.DummyCountAchievement)
			{
				list.Add(mAchievement.Value);
			}
		}
		return list.ToArray();
	}

	public void ShowUI()
	{
		if (Social.localUser.authenticated)
		{
			Social.ShowAchievementsUI();
		}
	}

	public AchievementTracker GetAchievement(string id)
	{
		AchievementTracker value = null;
		mAchievements.TryGetValue(id, out value);
		return value;
	}

	public void Reset()
	{
		foreach (KeyValuePair<string, AchievementTracker> mAchievement in mAchievements)
		{
			AchievementTracker value = mAchievement.Value;
			value.completedCount = 0;
			Singleton<Profile>.Instance.saveData.SetDictionaryValue("achievements", value.achievement.Data.id, value.completedCount);
		}
	}
}
