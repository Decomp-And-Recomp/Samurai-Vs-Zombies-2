using System;
using System.Collections;
using System.IO;
using Glu.Plugins.ASocial;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
	public const string tipsTableName = "LoadingScreenTips";

	private const int startupProgressSteps = 21;

	private const int ingameProgressSteps = 10;

	private const int frontendProgressSteps = 9;

	private static string levelToLoad = "AllMenus";

	public static TaggedString[] tips;

	public GluiText text_tips;

	public GluiMeter meter_progress;

	private static int numLogSteps;

	private static int curLogStep;

	private static TypedWeakReference<GluiMeter> sProgressMeter;

	private static DateTime? lastLogTime;

	private static DateTime? logStartTime;

	public static void LoadLevel(string levelName)
	{
		if (!string.IsNullOrEmpty(levelName))
		{
			levelToLoad = levelName;
			Application.LoadLevel("LoadingScreen");
		}
	}

	private void Awake()
	{
		MemoryWarningHandler.CreateInstance();
	}

	private IEnumerator Start()
	{
		sProgressMeter = new TypedWeakReference<GluiMeter>(meter_progress);
		GameObject go = base.gameObject;
		UnityEngine.Object.DontDestroyOnLoad(go);
		AJavaTools.UI.StartIndeterminateProgress(17);
		if (!Singleton<Profile>.Exists)
		{
			Screen.sleepTimeout = -1;
			AJavaTools.Init(base.gameObject);
			AJavaTools.Util.VerifySignature();
			AJavaTools.Util.LogEventOBB();
			AJavaTools.Util.LogEventDataRestored();
			LogBegin(21);
			if (text_tips != null)
			{
				text_tips.gameObject.SetActive(false);
			}
			yield return null;
			LogStep("BEFORE Handheld.PlayFullScreenMovie");
			AJavaTools.UI.StopIndeterminateProgress();
			Handheld.PlayFullScreenMovie("SVZ2_intro_1280x720.3gp", Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
			LogStep("AFTER Handheld.PlayFullScreenMovie");
			yield return new WaitForSeconds(0.5f);
			LogStep("AFTER Handheld.PlayFullScreenMovie 1");
			AJavaTools.UI.StartIndeterminateProgress(17);
			InitializeAndroid();
			Coroutine profileInit = StartCoroutine(Singleton<Profile>.Instance.Init());
			yield return null;
			if (!Singleton<Profile>.Instance.Initialized)
			{
				yield return profileInit;
			}
			Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep1_AppStart");
		}
		else
		{
			int progressSteps = 9;
			if (levelToLoad != "AllMenus")
			{
				progressSteps = 10;
			}
			LogBegin(progressSteps);
			if (tips == null)
			{
				tips = DataBundleRuntime.Instance.InitializeRecords<TaggedString>("LoadingScreenTips");
			}
			if (tips != null && text_tips != null)
			{
				TaggedString tip = tips[UnityEngine.Random.Range(0, tips.Length - 1)];
				if (tip != null)
				{
					text_tips.Text = StringUtils.GetStringFromStringRef("LoadingScreenTips", tip.tag);
				}
			}
		}
		AsyncOperation loadOp = Application.LoadLevelAsync(levelToLoad);
		levelToLoad = null;
		if (!loadOp.isDone)
		{
			yield return loadOp;
		}
		yield return null;
		yield return StartCoroutine(MemoryWarningHandler.Instance.FreeMemory());
		yield return null;
		NUF.StopSpinner();
		UnityEngine.Object.Destroy(go);
		LogEnd();
	}

	private void InitializeAndroid()
	{
		verifyRestoreSaveData();
		bool flag = true;
		bool flag2 = AJavaTools.Util.IsFirstLaunchThisVersion();
		if (!flag2)
		{
			string systemLanguage = BundleUtils.GetSystemLanguage();
			string path = AssetBundleConfig.BundleDataPath + "/" + systemLanguage + "/" + AssetBundleConfig.BundleAssetInfoName;
			if (!File.Exists(path))
			{
				flag = false;
			}
		}
		if (flag2 || !flag || PlayerPrefs.GetInt("gameLoadedCorrectly", 0) == 0 || PlayerPrefs.GetString("gameTag", "0").CompareTo(AJavaTools.Properties.GetBuildTag()) != 0)
		{
			StartCoroutine(UpdateSystem.CopyDataBundleFiles());
		}
		PlayerPrefs.SetInt("gameLoadedCorrectly", 0);
		if (AJavaTools.Properties.IsBuildAmazon())
		{
			Amazon.Init((Amazon.GameCircleFeatures)6);
		}
	}

	private void verifyRestoreSaveData()
	{
		string path = AJavaTools.GameInfo.GetFilesPath() + "/local";
		string path2 = AJavaTools.GameInfo.GetFilesPath() + "/local.restore";
		try
		{
			byte[] array;
			using (FileStream fileStream = new FileStream(path2, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					array = binaryReader.ReadBytes((int)fileStream.Length);
				}
			}
			if (array.Length > 0)
			{
				lock (this)
				{
					using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
					{
						using (BinaryWriter binaryWriter = new BinaryWriter(output))
						{
							binaryWriter.Write(array);
						}
					}
				}
			}
			File.Delete(path2);
		}
		catch (Exception)
		{
		}
	}

	public static void LogBegin(int numSteps = 1)
	{
		ProgressBegin(numSteps);
		logStartTime = DateTime.Now;
		lastLogTime = logStartTime;
	}

	public static void LogEnd()
	{
		ProgressEnd();
		if (logStartTime.HasValue)
		{
		}
		lastLogTime = null;
		logStartTime = null;
	}

	public static void LogStep(string s)
	{
		ProgressStep();
		DateTime now = DateTime.Now;
		if (lastLogTime.HasValue)
		{
		}
		lastLogTime = now;
	}

	public static void ProgressBegin(int numSteps = 1)
	{
		if (sProgressMeter != null && sProgressMeter.ptr != null)
		{
			sProgressMeter.ptr.Value = 0f;
		}
		curLogStep = 0;
		numLogSteps = numSteps;
	}

	public static void ProgressStep()
	{
		curLogStep++;
		if (sProgressMeter != null && sProgressMeter.ptr != null)
		{
			sProgressMeter.ptr.Value = (float)curLogStep / (float)numLogSteps;
		}
	}

	public static void ProgressEnd()
	{
		if (sProgressMeter != null && sProgressMeter.ptr != null)
		{
			sProgressMeter.ptr.Value = 1f;
		}
	}
}
