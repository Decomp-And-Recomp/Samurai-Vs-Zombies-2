using UnityEngine;

public class MultipleMysteryBoxImpl : MonoBehaviour, IGluiActionHandler
{
	public string prefab;

	public float delayTimer;

	public GameObject instructionWidget;

	public string soundOnStart = "UI_PrizeStart";

	public GluiStandardButtonContainer buyMoreButton;

	public GluiStandardButtonContainer facebookButton;

	public GluiStandardButtonContainer closeButton;

	public Transform[] locators;

	private GameObject[] mBoxes;

	private float[] mUnwrapTimers;

	private bool mGiftTriggered;

	private float mTimer;

	private MultipleMysteryBoxContents mContentsRef = new MultipleMysteryBoxContents();

	private bool mPlayPopSoundOnNextGift = true;

	public static Cost MysteryBoxPackCost;

	private void Start()
	{
		mBoxes = new GameObject[locators.Length];
		int num = 0;
		Transform[] array = locators;
		foreach (Transform parent in array)
		{
			GameObject gameObject = Object.Instantiate(ResourceCache.GetCachedResource(prefab, 1).Resource) as GameObject;
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			mBoxes[num] = gameObject;
			MysteryBoxImpl componentInChildren = GetComponentInChildren<MysteryBoxImpl>();
			if ((bool)componentInChildren)
			{
				componentInChildren.FacebookButton = facebookButton;
				if (num == 0)
				{
					componentInChildren.cost = MysteryBoxPackCost;
				}
			}
			num++;
		}
		if (closeButton != null)
		{
			closeButton.Locked = true;
			closeButton.Enabled = false;
		}
		if (facebookButton != null)
		{
			facebookButton.gameObject.SetActive(false);
		}
		if (buyMoreButton != null)
		{
			buyMoreButton.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		mContentsRef = null;
	}

	private void Update()
	{
		if (mUnwrapTimers == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < mUnwrapTimers.Length; i++)
		{
			if (mUnwrapTimers[i] >= 0f)
			{
				flag = true;
				mUnwrapTimers[i] -= GluiTime.deltaTime;
				if (mUnwrapTimers[i] < 0f)
				{
					Unwrap(i);
				}
			}
		}
		if (!flag)
		{
			mUnwrapTimers = null;
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "BUTTON_UNWRAP_GIFTS":
			if (!mGiftTriggered)
			{
				mGiftTriggered = true;
				OpenAllGifts();
			}
			return true;
		case "BUY_MORE_MYSTERY_BOX":
			GluiActionSender.SendGluiAction("POPUP_POP", base.gameObject, null);
			return true;
		case "FACEBOOK_SHARE_PRIZE":
			if (mContentsRef.mGoldHelpers.Count > 0)
			{
				HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[mContentsRef.mGoldHelpers[0]];
				string empty = string.Empty;
				string caption = string.Empty;
				if (helperSchema != null)
				{
					empty = StringUtils.GetStringFromStringRef("LocalizedText", helperSchema.displayName);
					caption = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookMysteryBoxGoldenMessage"), empty);
				}
				FacebookInterface.FeedDialog(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookMysteryBoxTitle"), caption, null, null, MultiplayerData.FacebookLink, MultiplayerData.FacebookImageLink, onFeedPost);
				facebookButton.gameObject.SetActive(false);
			}
			return true;
		default:
			return false;
		}
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			facebookButton.gameObject.SetActive(false);
		}
	}

	private void OpenAllGifts()
	{
		instructionWidget.SetActive(false);
		mUnwrapTimers = new float[mBoxes.Length];
		for (int i = 0; i < mUnwrapTimers.Length; i++)
		{
			mUnwrapTimers[i] = delayTimer * (float)i;
		}
		for (int j = 0; j < mUnwrapTimers.Length; j++)
		{
			int num = Random.Range(0, mUnwrapTimers.Length - 1);
			float num2 = mUnwrapTimers[j];
			mUnwrapTimers[j] = mUnwrapTimers[num];
			mUnwrapTimers[num] = num2;
		}
		for (int k = 0; k < mUnwrapTimers.Length; k++)
		{
			if (mUnwrapTimers[k] == delayTimer * (float)(mUnwrapTimers.Length - 1))
			{
				MysteryBoxImpl component = mBoxes[k].GetComponent<MysteryBoxImpl>();
				component.closeButton = closeButton;
				component.buyMoreButton = buyMoreButton;
				component.FacebookButton = facebookButton;
				break;
			}
		}
		PlayStartSound();
	}

	private void Unwrap(int index)
	{
		GluiActionSender.SendGluiAction("BUTTON_GIFT_TOUCHED", mBoxes[index], mPlayPopSoundOnNextGift);
		mPlayPopSoundOnNextGift = false;
	}

	private void PlayStartSound()
	{
		GluiSoundSender.SendGluiSound(soundOnStart, base.gameObject);
	}
}
