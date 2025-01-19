using UnityEngine;

[AddComponentMenu("Audio/Udaman Music Player")]
public class UdamanMusicPlayer : MonoBehaviour
{
	[HideInInspector]
	[DataBundleSchemaFilter(typeof(UMusicEventSchema), false)]
	public DataBundleRecordKey musicEvent;

	private void Start()
	{
		SingletonSpawningMonoBehaviour<UMusicManager>.Instance.PlayByKey(musicEvent);
		Object.Destroy(this);
	}
}
