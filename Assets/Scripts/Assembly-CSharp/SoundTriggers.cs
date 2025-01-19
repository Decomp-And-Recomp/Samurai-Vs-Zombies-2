using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Audio/Sound Trigger")]
public class SoundTriggers : MonoBehaviour
{
	public AudioClip ClipForTrigger1;

	public Vector3 PositionForTrigger1;

	public float VolumeForTrigger1;

	private AudioSource PlayAudioClip(AudioClip clip, Vector3 position, float volume)
	{
		GameObject gameObject = new GameObject("One shot audio");
		gameObject.transform.position = position;
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.Play();
		UnityEngine.Object.Destroy(gameObject, clip.length);
		return audioSource;
	}

	public void PlayTrigger1()
	{
		PlayAudioClip(ClipForTrigger1, PositionForTrigger1, VolumeForTrigger1);
	}
}
