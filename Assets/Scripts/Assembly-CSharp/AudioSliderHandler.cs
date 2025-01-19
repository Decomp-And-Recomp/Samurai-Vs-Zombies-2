using UnityEngine;

public class AudioSliderHandler : MonoBehaviour
{
	public enum AudioChannelType
	{
		Music = 0,
		SFX = 1
	}

	public AudioChannelType audioType;

	private GluiSlider mSlider;

	private float mLastValue;

	public bool isMusic
	{
		get
		{
			return audioType == AudioChannelType.Music;
		}
	}

	public bool isSFX
	{
		get
		{
			return audioType == AudioChannelType.SFX;
		}
	}

	private void Start()
	{
		mSlider = base.gameObject.GetComponent<GluiSlider>();
		if (isMusic)
		{
			mLastValue = AudioUtils.MusicVolumePlayer;
		}
		else
		{
			mLastValue = AudioUtils.SoundThemeVolumePlayer;
		}
		mSlider.Value = mLastValue;
	}

	private void Update()
	{
		if (mSlider.Value != mLastValue)
		{
			mLastValue = mSlider.Value;
			if (isMusic)
			{
				AudioUtils.MusicVolumePlayer = mLastValue;
			}
			else
			{
				AudioUtils.SoundThemeVolumePlayer = mLastValue;
			}
		}
	}
}
