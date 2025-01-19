using UnityEngine;

[RequireComponent(typeof(UdamanSoundThemePlayer))]
[AddComponentMenu("Glui Sound/Global Sound Handler")]
public class GluiGlobalSoundHandler : MonoBehaviour, IGluiSoundHandler
{
	private UdamanSoundThemePlayer soundPlayer;

	private static GluiGlobalSoundHandler gGluiGlobalSoundHandler;

	public static GluiGlobalSoundHandler Instance
	{
		get
		{
			return gGluiGlobalSoundHandler;
		}
		set
		{
			if (gGluiGlobalSoundHandler == null)
			{
				gGluiGlobalSoundHandler = value;
			}
		}
	}

	public void Awake()
	{
		Instance = this;
		soundPlayer = (UdamanSoundThemePlayer)base.gameObject.GetComponent(typeof(UdamanSoundThemePlayer));
	}

	public bool HandleSound(string sound, GameObject sender)
	{
		USoundThemeEventClip uSoundThemeEventClip = soundPlayer.PlaySoundEvent(sound);
		return uSoundThemeEventClip;
	}

	public USoundThemeEventClip PlaySoundEvent(string sound)
	{
		return soundPlayer.PlaySoundEvent(sound);
	}
}
